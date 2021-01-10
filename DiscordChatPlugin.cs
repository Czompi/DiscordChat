using DiscordChat.Commands;
using DiscordChat.Configs;
using DiscordChat.Settings;
using DiscordChat.Settings.Lang;
using DSharpPlus;
using DSharpPlus.Entities;
using Obsidian.API;
using Obsidian.API.Events;
using Obsidian.API.Plugins;
using Obsidian.API.Plugins.Services;
using Obsidian.CommandFramework.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordChat.Plugin
{
    [Plugin(Name = "DiscordChat", Version = "0.0.1",
            Authors = "Czompi", Description = "A communication layer between players on the Discord and on the Minecraft server.",
            ProjectUrl = "https://github.com/Czompi/DiscordChat-Obsidian")]
    public class DiscordChatPlugin : PluginBase
    {
        // Any interface from Obsidian.Plugins.Services can be injected into properties
        [Inject] public ILogger Logger { get; set; }
        [Inject] public IFileReader IFileReader { get; set; }
        [Inject] public IFileWriter IFileWriter { get; set; }
        // One of server messages, called when an event occurs
        public async Task OnLoad(IServer server)
        {

            #region InitializeComponents
            Globals.PluginInfo = Info;
            Logger.Log($"DiscordChat §9{Globals.VersionFull}{ChatColor.Reset} loading...");

            Logger.Log($"§7[Global]{ChatColor.Reset} Global things are §9loading{ChatColor.Reset}...");
            Globals.Server = server;
            Globals.Logger = Logger;
            Globals.FileReader = IFileReader;
            Globals.FileWriter = IFileWriter;
            Logger.Log($"§7[Global]{ChatColor.Reset} Global things {ChatColor.BrightGreen}successfully{ChatColor.Reset} assigned.");

            Logger.Log($"§7[Language]{ChatColor.Reset} §9Detecting{ChatColor.Reset} language...");
            Globals.Language = new LanguageManager();
            Logger.Log($"§7[Language]{ChatColor.Reset} Language loaded {ChatColor.BrightGreen}successfully{ChatColor.Reset}.");

            Logger.Log($"§7[Config]{ChatColor.Reset} Config files are §9loading{ChatColor.Reset}...");
            Globals.Configs = new ConfigManager();
            Logger.Log($"§7[Config]{ChatColor.Reset} Config files are loaded {ChatColor.BrightGreen}successfully{ChatColor.Reset}.");

            Logger.Log($"§7[Commands]{ChatColor.Reset} Registering §9commands{ChatColor.Reset}...");
            server.RegisterCommandClass<DiscordCommandModule>();
            Logger.Log($"§7[Commands]{ChatColor.Reset} Command module {ChatColor.BrightGreen}DiscordCommandModule{ChatColor.Reset} registered.");
            Logger.Log($"§7[Commands]{ChatColor.Reset} Commands {ChatColor.BrightGreen}successfully{ChatColor.Reset} registered...");
            #endregion

            #region InitializeDiscord
            Logger.Log($"§7[DSharpPlus]{ChatColor.Reset} Initializing Discord connection. Please wait...");
            try
            {
                Globals.Discord = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Globals.Configs.Config.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true
                });
                await Globals.Discord.ConnectAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = "Obsidian Server" }, UserStatus.Online);
                Globals.DiscordChannel = await Globals.Discord.GetChannelAsync(Globals.Configs.Config.ChannelId);
                DiscordMessageDispatcher.Initialize();
                await DiscordMessageDispatcher.Start();

                Globals.Discord.MessageCreated += async (sender, e) =>
                {
                    if (e.Channel.Id == Globals.Configs.Config.ChannelId)
                    {
                        if (!e.Author.IsBot)
                        {
                            var msg = e.Message.Content;
                            foreach (var user in e.MentionedUsers)
                            {
                                var userInfo = await Globals.Discord.GetUserAsync(user.Id);
                                msg = msg.Replace($"<@{user.Id}>", $"{ChatColor.Blue}@{userInfo.Username}:{userInfo.Discriminator}{ChatColor.Reset}");
                                msg = msg.Replace($"<@!{user.Id}>", $"{ChatColor.Blue}@{userInfo.Username}:{userInfo.Discriminator}{ChatColor.Reset}");
                            }
                            await server.BroadcastAsync(IChatMessage.Simple(Globals.Configs.Config.ChatFormat.Minecraft.Replace("{username}", $"{ChatColor.Blue}@{e.Author.Username}:{e.Author.Discriminator}").Replace("{message}", msg)));
                        }
                    }
                };
            }
            catch (System.Exception ex)
            {
                Logger.LogWarning($"{ChatColor.Gray}[DSharpPlus]{ChatColor.White} Further configuration needed. The error was the following:\r\n{ChatColor.Gray}[DSharpPlus] {ChatColor.White}{ex.ToString().Replace("\n", $"\n{ChatColor.Gray}[DSharpPlus] {ChatColor.White}")}");
            }
            #endregion

            Logger.Log($"DiscordChat {ChatColor.BrightGreen}{Globals.VersionFull}{ChatColor.Reset} loaded!");

            await Task.CompletedTask;
        }

        public async Task OnIncomingChatMessage(IncomingChatMessageEventArgs e)
        {
            var player = e.Player;
            var message = e.Message;
            Globals.Messages.Add(player, message);
            await Task.CompletedTask;
        }

    }
}
