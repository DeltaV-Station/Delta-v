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

string? token = null;
string? connectionString = null;
if (File.Exists("config.json"))
{
    var config = await JsonSerializer.DeserializeAsync<Config>(File.OpenRead("config.json")) ?? new Config();
    token = config.Token;
}

#if DEBUG
if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") is { } envToken)
    token = envToken;

if (Environment.GetEnvironmentVariable("DATABASE_STRING") is { } dbString)
    connectionString = dbString;
#endif

if (string.IsNullOrWhiteSpace(token))
    throw new ArgumentException("No token found.");

var usePostgres = true;
if (string.IsNullOrWhiteSpace(connectionString)) 
{
    usePostgres = false;
    //throw new ArgumentException("No database connection string found. Assuming local development.");
}

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

ServerDbContext db;
if (usePostgres)
{
    var builder = new DbContextOptionsBuilder<PostgresServerDbContext>();
    builder.UseNpgsql(connectionString);
    db = new PostgresServerDbContext(builder.Options);
    // await db.Database.MigrateAsync();
}
else
{
    var relativePath = "FIX"; // Gross but whatever, I'm just testing
    var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
    builder.UseSqlite(new SqliteConnection($"Data Source={relativePath}"));
    db = new SqliteServerDbContext(builder.Options);
}

var interaction = new InteractionService(client);
var handler = new CommandHandler(client, new CommandService(), interaction, db);

AppDomain.CurrentDomain.ProcessExit += (_, _) => Interlocked.Decrement(ref handler.Running);

await handler.InstallCommandsAsync();

// Block this task until the program is closed.
await Task.Delay(-1);
