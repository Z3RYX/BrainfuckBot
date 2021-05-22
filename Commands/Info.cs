using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck.Commands
{
    [Group("Ping")]
    [Alias("p", "latency")]
    [Summary("Returns the time it takes for a message to be send out and received again by the bot")]
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task PingAsync([Remainder] string args = null)
        {
            var start = DateTime.Now;

            var msg = await ReplyAsync("**Loading...**", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

            var time = DateTime.Now.Subtract(start).TotalMilliseconds;

            var embed = new EmbedBuilder()
                .WithTitle("Pong!")
                .WithDescription("**ACK:** " + time.ToString().Replace(',', '.') + " ms")
                .WithColor(Config.EmbedColor)
                .WithCurrentTimestamp()
                .Build();

            await msg.ModifyAsync(x => { x.Embed = embed; x.Content = ""; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }
    }

    [Group("Prefix")]
    [Alias("setprefix", "changeprefix")]
    [Summary("Sets the prefix for this guild")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task PrefixAsync([Remainder] string Prefix = null)
        {
            if (Prefix == null)
            {
                await ReplyAsync("**MISSING ARGUMENT**\nYou need to provide a new prefix", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }

            MongoHelper.UpdateGuildPrefix(Context.Guild.Id, Prefix);

            await ReplyAsync($"Changed this guild's prefix to: `{Prefix}`", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }
    }

    [Group("Invite")]
    [Alias("botlist", "vote", "listing", "links", "support")]
    [Summary("Lists all the bot listing sites on which you can find Brainfuck to invite or upvote it, as well as a link to join the support server where you can send feedback, suggestions, or bug reports")]
    public class InviteModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task InviteAsync([Remainder] string args = null)
        {
            var em = new EmbedBuilder()
                .WithColor(Config.EmbedColor)
                .WithCurrentTimestamp()
                .WithTitle("Bot Invite & Support Server")
                .WithDescription("Below you can find links to all the sites where Brainfuck is listed. From there you can invite it to your server or, if you feel generous and want to help me out, upvote the bot to make it easier for others to find.")
                .AddField("Bot Listings", "[top.gg](https://top.gg/bot/697253587113476096)\n" +
                                    "[Discord Bots](https://discord.bots.gg/bots/697253587113476096)\n" +
                                    "[discordbotlist.com](https://discordbotlist.com/bots/brainfuck)")
                .AddField("Support Server", "[Click this link](https://discord.gg/rVWjgnuzcZ) to get an invite to the support server.\n" +
                                    "There you can ask questions to the developer, give feedback & suggestions, and report bugs.");

            await ReplyAsync("", embed: em.Build(), messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }
    }

    [Group("BotStats")]
    [Alias("stats", "statistic")]
    [Summary("Shows a few interesting statistics about this bot")]
    public class BotStatsModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task BotStatsAsync([Remainder] string args = null)
        {
            var u = DateTime.UtcNow - Config.Startup;

            var em = new EmbedBuilder()
                .WithColor(Config.EmbedColor)
                .WithCurrentTimestamp()
                .WithTitle("Bot Statistics")
                .WithDescription("Here you can find a few interesting statistics about " + Context.Client.CurrentUser.Mention)
                .AddField("Uptime", $"{(u.TotalDays < 1 ? "" : $"{u.Days} Days and ")}{u.Hours}:{u.Minutes}:{u.Seconds}.{u.Milliseconds}")
                .AddField("Hosting", "[DigitalOcean Droplet](https://digitalocean.com/)\n[Ubuntu 18.04.3](https://ubuntu.com/)\n[.NET 5](https://dotnet.microsoft.com/)")
                .AddField("Dependencies", "[Discord.Net 2.3.1](https://www.nuget.org/packages/Discord.Net/)\n[Discord.Addons.Interactive 2.0.0](https://www.nuget.org/packages/Discord.Addons.Interactive/)\n[BrainfuckNET 1.2.2](https://www.nuget.org/packages/BrainfuckNET/)");

            await ReplyAsync("", embed: em.Build(), messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }
    }

    [Group("Help")]
    [Alias("command", "cmd", "commands", "cmds", "h")]
    [Summary("Shows the help menu containing all callable commands and their usage")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task HelpAsync([Remainder] string Command = null)
        {
            var modules = Config.DCommands.Modules;
            Embed cmdembed;

            string prefix = MongoHelper.GetPrefix(Context.Guild.Id);

            if (Command != null) {
                try {
                    Command = Command.ToLower();
                    cmdembed = HelpEmbed.Build(Command, prefix);
                }
                catch (Exception e) {
                    await ReplyAsync(e.Message);
                    return;
                }

                await ReplyAsync("", embed: cmdembed, messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

            }
            else {
                var embed = new EmbedBuilder()
                    .WithTitle("Brainfuck Help")
                    .WithDescription($"Use `{prefix}help [command]` to get more info about a certain command")
                    .AddField("Available Commands", $"```\n{string.Join(", ", modules.Select(x => x.Name.ToLower()))}```")
                    .WithColor(Config.EmbedColor)
                    .WithCurrentTimestamp()
                    .Build();

                await ReplyAsync("", embed: embed, messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }
        }
    }
    [Group("Info")]
    [Alias("i", "about", "brainfuck")]
    [Summary("Displays info about the esoteric programming language Brainfuck")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task InfoAsync([Remainder] string args = null)
        {
            string desc = "**Background**\n" +
                "Brainfuck is an esoteric programming language created in 1993 by Urban Müller.\n" +
                "The language contains only eight simple commands and an instruction pointer.\n" +
                "While it is fully Turing - complete, it is not intended for practical use, but to challenge and amuse programmers.\n\n" +
                "  **Diagram**\n  ```\n" +
                "  --------------------||---||--------\n" +
                "  | 0 | 0 | 0 | 0 | 0 || 0 || 0 | 0 |\n" +
                "  --------------------||---||--------\n" +
                "  tape ---^     ^        ^\n" +
                "    cell --------        |\n" +
                "      head(pointer) -----```\n" +
                "**Instructions**\n" +
                "  `>` - Move the pointer right\n" +
                "  `<` - Move the pointer left\n" +
                "  `+` - Increment the current cell\n" +
                "  `-` - Decrement the current cell\n" +
                "  `.` - Output the value of the current cell\n" +
                "  `,` - Replace the value of the current cell with input\n" +
                "  `[` - Jump to the matching `]` instruction if the value of the current cell is zero\n" +
                "  `]` - Jump to the matching `[` instruction if the value of the current cell is not zero\n\n" +
                "**Memory Layout**\n" +
                "The brainfuck tape is made of an infinite(in this case limited to 30,000) collection of 1 byte cells.\n" +
                "Each cell represents a single, unsigned 8 - bit number.\n" +
                "Cells start initialized at zero.\n\n" +
                "Since the numbers are unsigned, there is no need for any complex integer implementation.\n" +
                "If the upper limit of the cell is reached, it wraps back to zero.\n" +
                "If zero is decremented, it must wrap back to 11111111.\n\n" +
                "**Notes**\n" +
                "- Using the `,` operator prompts the bot to wait for user input through discord.\n" +
                "- The `.` operator stores the output in a list which is sent to the channel at the end of execution.\n" +
                "- To prevent the bot hanging, execution time is limited to 10 seconds and will be cancelled if this limit is exceeded.";

            var embed = new EmbedBuilder()
                .WithColor(Config.EmbedColor)
                .WithCurrentTimestamp()
                .WithDescription(desc)
                .Build();

            await ReplyAsync("", embed: embed, messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }
    }

    public static class HelpEmbed
    {
        public static Embed Build(string command, string prefix)
        {
            var modules = Config.DCommands.Modules;
            var commands = Config.DCommands.Commands;
            var args = command.Split(' ');

            if (args.Length > 1) {
                throw new Exception($"No {args.Length}-layered commands available");
            }

            var embed = new EmbedBuilder();
            string parameters = "";

            if (modules.Any(x => x.Group.ToLower() == args[0].ToLower() || x.Aliases.Contains(args[0].ToLower()))) {
                var module = modules.First(x => x.Group.ToLower() == args[0].ToLower() || x.Aliases.Contains(args[0].ToLower()));
                CommandInfo cmd;
                if (args.Length == 1) {
                    if (module.Commands.Count > 1) {
                        // Module Help
                        return embed.WithTitle($"{module.Group} Module Help")
                            .WithDescription(module.Summary ?? "No description available yet")
                            .AddField("Sub-Commands", string.Join(", ", module.Commands.Where(x =>!x.Name.EndsWith("Async")).Select(x => x.Name.ToLower())))
                            .WithColor(Config.EmbedColor)
                            .WithCurrentTimestamp()
                            .Build();
                    } else {
                        // Single Command Help
                        cmd = module.Commands.First();
                        parameters = string.Join(", ", cmd.Parameters.Where(x => x.Name != "args"));

                        return embed.WithTitle($"{module.Group} Help")
                            .WithDescription($"```\n{prefix}{module.Group.ToLower()}{(parameters != "" ? $" [{parameters}]" : "")}```")
                            .AddField("Description", module.Summary ?? "No description available yet")
                            .AddField("Aliases", string.Join(", ", cmd.Aliases).Replace($"{module.Group.ToLower()} ", "").ToLower())
                            .WithColor(Config.EmbedColor)
                            .WithCurrentTimestamp()
                            .Build();
                    }

                } else if (module.Commands.Any(x => x.Name.ToLower() == args[1])) {
                    // Sub-Command Help
                    cmd = module.Commands.First(x => x.Name.ToLower() == args[1]);
                    return embed.WithTitle($"{module.Group} {cmd.Name} Help")
                            .WithDescription($"```\n{prefix}{module.Group.ToLower()} {cmd.Name.ToLower()}{(parameters != "" ? $" [{parameters}]" : "")}```")
                            .AddField("Description", cmd.Summary ?? "No description available yet")
                            .AddField("Aliases", string.Join(", ", cmd.Aliases).Replace($"{module.Group.ToLower()} ", "").ToLower())
                            .WithColor(Config.EmbedColor)
                            .WithCurrentTimestamp()
                            .Build();

                } else {
                    throw new Exception("Command `" + prefix + command + "` not found");
                }
            } else {
                throw new Exception("Command `" + prefix + command + "` not found");
            }
        }
    }
}
