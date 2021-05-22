using BrainfuckNET;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck.Commands
{
    [Group("run")]
    [Alias("r", "execute, compile")]
    [Summary("Runs Brainfuck code and returns the output")]
    public class Run : InteractiveBase<SocketCommandContext>
    {
        [Command(RunMode = RunMode.Async)]
        public async Task RunAsync([Remainder] string Code = null)
        {
            if (Code == null)
            {
                var he = HelpEmbed.Build("run", MongoHelper.GetPrefix(Context.Guild.Id));
                await ReplyAsync("", embed: he, messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                return;
            }

            string input = null;

            if (Code.Contains(","))
            {
                await ReplyAsync("Please send a message containing the ASCII inputs for your code", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                var msg = await NextMessageAsync();

                if (msg == null)
                {
                    await ReplyAsync("**Timeout**, please send the run command again to retry", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                    return;
                }

                input = msg.Content;
            }

            Interpreter.ConsoleOutput = false;

            int steps = 0; double time = 0; string output = "";

            try
            {
                var t = Task.Run(() => Interpreter.Execute(Code, input));

                if (!t.Wait(10000))
                {
                    await ReplyAsync("**TIMEOUT**\nExecution took too long", messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                    return;
                }

                (steps, time, output) = t.Result;
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message, messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                return;
            }

            EmbedBuilder e = new EmbedBuilder()
                .WithColor(Config.EmbedColor)
                .WithCurrentTimestamp()
                .WithAuthor($"Steps: {steps} | Execution Time: {time}ms")
                .WithTitle("Finished Execution");

            if (output.Length > 1500 && output.Length <= 1_000_000)
            {
                e.AddField("Output", "Output exceeded 1500 characters, so it will be included as an attachment");
                await ReplyAsync(embed: e.Build(), messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                await Context.Channel.SendFileAsync(GenerateStreamFromString(output), "output.txt");
            } else if (output.Length > 1_000_000)
            {
                e.AddField("Output", "Output exceeded 1MB and will not be sent");
                await ReplyAsync(embed: e.Build(), messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }

            if (output == "") e.AddField("Output", "Your code didn't produce any output"); else e.AddField("Output", "```\n" + output.Replace("`", "`\u200b") + "```");

            await ReplyAsync(embed: e.Build(), messageReference: new MessageReference(Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
        }

        // Courtesy of Cameron MacFarland on StackOverflow
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
