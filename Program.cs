using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class Program
{
    public static void Main()
    {
        var bot = new Brainfuck.Bot();
        bot.MainAsync().GetAwaiter().GetResult();
    }
}

namespace Brainfuck
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
            });

            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            if (!File.Exists("config.json"))
            {
                Console.WriteLine("No config file found.\nCreate config.json and restart the bot.");
                return;
            }

            var cfg = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));

            Config.DClient      = _client;
            Config.DCommands    = _commands;
            Config.Prefix       = cfg.Prefix;
            Config.Token        = cfg.Token;

            Config.DB = new MongoClient(cfg.DBConnection).GetDatabase(cfg.DBName);

            // Events
            _client.Log += Log;
            _client.Ready += OnReady;
            _client.LeftGuild += OnGuildLeft;
            _client.JoinedGuild += OnGuildJoin;

            await PrepareMessageHandler();

            await _client.LoginAsync(TokenType.Bot, Config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task OnGuildLeft(SocketGuild arg)
        {
            _client.SetGameAsync($"in {_client.Guilds.Count} Guilds | Brainfuck has undergone a rewrite! | {Config.Prefix}help");
            return Task.CompletedTask;
        }

        private Task OnGuildJoin(SocketGuild arg)
        {
            _client.SetGameAsync($"in {_client.Guilds.Count} Guilds | Brainfuck has undergone a rewrite! | {Config.Prefix}help");
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            Log(new LogMessage(LogSeverity.Info, "Bot", $"Successfully connected to {_client.CurrentUser} ({_client.CurrentUser.Id})"));
            Log(new LogMessage(LogSeverity.Info, "Bot", $"Bot is in {_client.Guilds.Count} Guilds"));

            _client.SetGameAsync($"in {_client.Guilds.Count} Guilds | {(Config.ActivityMessage == "" ? "" : Config.ActivityMessage + " | ")}{Config.Prefix}help");

            return Task.CompletedTask;
        }

        private async Task PrepareMessageHandler()
        {
            _client.MessageReceived += OnMessage;

            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
        }

        private async Task OnMessage(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;

            //TODO Things that should happen when someone sends a message, regardless if it is a commands or not

            await HandleCommands(message);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine($"[{arg.Source} @ {DateTime.UtcNow}] {arg.Severity} - {arg.Message}");
            return Task.CompletedTask;
        }

        private async Task HandleCommands(SocketUserMessage message)
        {
            var ctx = new SocketCommandContext(_client, message);

            var prefix = MongoHelper.GetPrefix(ctx.Guild.Id);

            if (message.Content == _client.CurrentUser.Mention)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Welcome to Brainfuck")
                    .WithColor(Config.EmbedColor)
                    .WithFooter("Created by Z3RYX#1079 and inspired by thomm.o#8637")
                    .AddField("About Me", "This bot allows you to run Brainfuck scripts easily in Discord.")
                    .AddField("Other Info", $"**Prefix:** `{prefix}`\n**Help:** - `{prefix}help`\n**Info:** - `{prefix}info`\n**Invite & Voting** - `{prefix}invite`")
                    .Build();

                await ctx.Channel.SendMessageAsync("", embed: embed, messageReference: new MessageReference(message.Id, ctx.Channel.Id, ctx.Guild.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                return;
            }

            int argpos = 0;

            if (!(message.HasStringPrefix(prefix, ref argpos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argpos)) ||
                message.Author.IsBot)
                return;


            var result = await _commands.ExecuteAsync(
                context: ctx,
                argPos: argpos,
                services: _services);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await ctx.Channel.SendMessageAsync("**ERROR**\n" + result.ErrorReason, messageReference: new MessageReference(ctx.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }
    }

    public static class Config
    {
        public static DiscordSocketClient DClient { get; set; }
        public static CommandService DCommands { get; set; }
        public static string Token { get; set; }
        public static string Prefix { get; set; }
        public static IMongoDatabase DB { get; set; }
        public static Color EmbedColor = new Color(0x2f, 0x31, 0x36);
        public static DateTime Startup = DateTime.UtcNow;
        public static string ActivityMessage = "Brainfuck has undergone a rewrite!";
    }

    public class ConfigFile
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("db_connection")]
        public string DBConnection { get; set; }

        [JsonProperty("db_name")]
        public string DBName { get; set; }
    }
}
