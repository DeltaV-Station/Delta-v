# Content.DiscordBot


### Configuration (Debug)
* `DISCORD_TOKEN`: Discord application token provided @ `https://discord.com/developers/applications/<APP_ID>/bot`
* `DATABASE_STRING`: Connection string to the database
  * For local development, you'll want it to be `Data Source=./bin/Content.Server/data/preferences.db` if you're using sqlite.
* `DATABASE_CONTEXT`: Default is `postgres`. If testing locally with sqlite, use `sqlite`.
* `GUILD`: The Discord Server ID. DeltaV's discord is `1513310351490289834`.
    * You can find this by turning Developer mode on in Discord, right-clicking on a server, and copying the server ID.

### Configuration (Production)
Requires a `link-config.json` file in the working directory.
```json
{
    "Token": "DISCORD_BOT_TOKEN",
    "DatabaseString": "DB_CONNECTION_STRING"
}
```

Here's a quick bash command to create it in your current working directory. Windows users will have to fend for themselves.
```bash
cat << EOF > link-config.json
{
    "Token": "DISCORD_BOT_TOKEN",
    "DatabaseString": "DB_CONNECTION_STRING"
}
EOF
```

You can also override `Guild` and `DatabaseContext` if you wanted to by including those JSON properties.

