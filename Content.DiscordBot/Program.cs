using System.Text.Json;
using Content.DiscordBot;
using Content.Server.Database;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
client.Log += Logger.Log;

Console.Out.WriteLine($"[INIT] Working Directory: {Directory.GetCurrentDirectory()}");

var config = new Config();
if (File.Exists("link-config.json"))
{
    Console.Out.WriteLine("[INIT] link-config.json found. Deserializing.");
    config = await JsonSerializer.DeserializeAsync<Config>(File.OpenRead("link-config.json")) ?? new Config();
}

#if DEBUG
if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") is { } envToken)
    config.Token = envToken;

if (Environment.GetEnvironmentVariable("DATABASE_STRING") is { } dbString)
    config.DatabaseString = dbString;

if (Environment.GetEnvironmentVariable("DATABASE_CONTEXT") is { } context)
    config.DatabaseContext = context;

if (Environment.GetEnvironmentVariable("GUILD") is { } guildId)
{
    try
    {
        config.Guild = ulong.Parse("guildId");
    }
    catch (Exception ex)
    {
        Console.Out.WriteLine($"[INIT] Unable to parse guild ID. Defaulting to {config.Guild}. Exception: {ex.GetType()} {ex.Message}");
    }
}
#endif

if (string.IsNullOrWhiteSpace(config.Token))
    throw new ArgumentException("[INIT] No Discord Bot token found.");


if (string.IsNullOrWhiteSpace(config.DatabaseString))
    throw new ArgumentException("[INIT] No database connection string found.");

// Override for local sqlite development
var usePostgres = true;
if (!string.IsNullOrWhiteSpace(config.DatabaseContext) && config.DatabaseContext.Equals("sqlite"))
    usePostgres = false;

Console.Out.WriteLine($"[INIT] Guild ID: {config.Guild}");

await client.LoginAsync(TokenType.Bot, config.Token);
await client.StartAsync();

ServerDbContext db;
if (usePostgres)
{
    Console.Out.WriteLine("[INIT] Connecting to postgres...");
    var builder = new DbContextOptionsBuilder<PostgresServerDbContext>();
    builder.UseNpgsql(config.DatabaseString);
    db = new PostgresServerDbContext(builder.Options);
    // await db.Database.MigrateAsync();
}
else
{
    Console.Out.WriteLine("[INIT] Connecting to sqlite...");
    var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
    builder.UseSqlite(new SqliteConnection(config.DatabaseString));
    db = new SqliteServerDbContext(builder.Options);
}
Console.Out.WriteLine("[INIT] Database connection successful.");

var interaction = new InteractionService(client);
var handler = new CommandHandler(client, new CommandService(), interaction, db, config);

AppDomain.CurrentDomain.ProcessExit += (_, _) => Interlocked.Decrement(ref handler.Running);

await handler.InstallCommandsAsync();

// Block this task until the program is closed.
await Task.Delay(-1);
