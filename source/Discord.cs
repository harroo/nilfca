
using System;
using System.IO;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using System.Threading.Tasks;
using System.Threading;

namespace Nilfca {

    public static class Discord {

        private static bool shouldStop;

        private static string clientToken;

        public static void SetToken (string token) {

            clientToken = token;
        }

        public static void Start () {

        	shouldStop = false;

        	Discord.MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void Stop () {

            shouldStop = true;
        }

        private static DiscordClient discord;

        public static async Task MainAsync () {

            shouldStop = false;

            discord = new DiscordClient(new DiscordConfiguration{

                Token = clientToken,
                TokenType = TokenType.Bot,

                AutoReconnect = true
            });

            discord.GuildAvailable += Discord_OnGuildAvailable;

            await discord.ConnectAsync();

            await Task.Delay(512);

            await discord.UpdateStatusAsync(

                new DiscordGame("Nilfca"),

                UserStatus.Online,

                null
            );

            while (!shouldStop) {

                await Task.Delay(100);

                mutex.WaitOne(); try {

                    while (sendQueue.Count != 0) {

                        SendInfo sendInfo = sendQueue[0]; sendQueue.RemoveAt(0);

                        foreach (var channel in channels) {

                            if (channel.id == sendInfo.targetChannelId || channel.name == sendInfo.targetChannelName) {

                                await channel.reference.SendMessageAsync(sendInfo.message);

                                Console.WriteLine("Sent Message: " + channel.name + ": " + sendInfo.message);
                            }
                        }
                    }

                } finally { mutex.ReleaseMutex(); }
            }

            await discord.DisconnectAsync();

            return;
        }

        private static Mutex mutex = new Mutex();
        private static List<ChannelInfo> channels = new List<ChannelInfo>();
        private static List<SendInfo> sendQueue = new List<SendInfo>();

        private static Task Discord_OnGuildAvailable (GuildCreateEventArgs e) {

    		foreach (var channel in e.Guild.Channels) { mutex.WaitOne(); try {

    			Console.WriteLine("Access To: " + e.Guild.Name + "::" + channel.Name);

                ChannelInfo channelInfo = new ChannelInfo();
                channelInfo.name = e.Guild.Name + "::" + channel.Name;
                channelInfo.id = channel.Id;
                channelInfo.reference = channel;

                if (!channels.Contains(channelInfo)) channels.Add(channelInfo);

    		} finally { mutex.ReleaseMutex(); } }

            return Task.CompletedTask;
        }

        public static void QueueMessage (string channelName, string message) {

            SendInfo sendInfo = new SendInfo();
            sendInfo.targetChannelName = channelName;
            sendInfo.message = message;

            mutex.WaitOne(); try {

                sendQueue.Add(sendInfo);

            } finally { mutex.ReleaseMutex(); }
        }

        public static void QueueMessage (ulong channelId, string message) {

            SendInfo sendInfo = new SendInfo();
            sendInfo.targetChannelId = channelId;
            sendInfo.message = message;

            mutex.WaitOne(); try {

                sendQueue.Add(sendInfo);

            } finally { mutex.ReleaseMutex(); }
        }

        public static List<ChannelInfo> GetChannels () {

            mutex.WaitOne(); try {

                List<ChannelInfo> channelList = new List<ChannelInfo>();
                foreach (var channel in channels)
                    channelList.Add(new ChannelInfo{
                        name = channel.name,
                        id = channel.id
                    });

                return channelList;

            } finally { mutex.ReleaseMutex(); }
        }
    }

    public class ChannelInfo {

        public string name;
        public ulong id;
        public DiscordChannel reference;
    }

    public class SendInfo {

        public string message;
        public string targetChannelName;
        public ulong targetChannelId;
    }
}
