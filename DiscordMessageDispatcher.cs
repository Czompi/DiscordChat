using DiscordChat.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordChat.Plugin
{
    internal class DiscordMessageDispatcher
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static CancellationToken ct;
        private static bool isSyncronising = false;
        public static void Initialize()
        {
            ct = cts.Token;
        }
        public static readonly Task SyncTask = new Task(async () =>
        {
            while (!ct.IsCancellationRequested)
            {

                if (Globals.Messages.Count > 0)
                {
                    foreach (var msg in Globals.Messages)
                    {
                        await Globals.Discord.SendMessageAsync(Globals.DiscordChannel, Globals.Configs.Config.ChatFormat.Discord.Replace("{username}", msg.Key.Username).Replace("{message}", msg.Value));
                    }
                    Globals.Messages.Clear();
                }
                await Task.Delay(Globals.Configs.Config.MessageSyncInterval * 1000);
            }
        });
        public async static Task Start()
        {
            if (!isSyncronising)
            {
                try
                {
                    //Globals.Logger.Log("Trying to start task");
                    SyncTask.Start();
                    isSyncronising = true;
                }
                catch (OperationCanceledException e)
                {
                    isSyncronising = false;
                }

            }

        }
        public static void Stop()
        {
            cts.Cancel();
        }
    }
}