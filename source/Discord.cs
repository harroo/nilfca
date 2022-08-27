using System;
using System.IO;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using System.Threading.Tasks;
using System.Threading;

public static class Program {

    private static bool shouldStop;

    public static void Main () {

    	shouldStop = false;

    	Program.MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void Stop () {

        shouldStop = true;
    }

    private static DiscordClient discord;

    public static async Task MainAsync () {

        shouldStop = false;

        discord = new DiscordClient(new DiscordConfiguration{

            Token = "YOUR_TOKEN_HERE",
            TokenType = TokenType.Bot,

            AutoReconnect = true
        });

        discord.Ready += Discord_OnReady;
        discord.GuildAvailable += Discord_OnGuildAvailable;
        discord.ClientErrored += Discord_OnClientError;
        discord.MessageCreated += Discord_OnMessageCreated;

        await discord.ConnectAsync();

        await Task.Delay(512);

        await discord.UpdateStatusAsync(

            new DiscordGame("anything you want to"),

            UserStatus.Online,

            null
        );

        while (!shouldStop) {

            await Task.Delay(512);
        }

        await discord.DisconnectAsync();

        return;
    }

    private static Task Discord_OnReady (ReadyEventArgs e) {

        return Task.CompletedTask;
    }

    private static Task Discord_OnGuildAvailable (GuildCreateEventArgs e) {

		foreach (var channel in e.Guild.Channels) {

			Console.WriteLine("Access To: " + e.Guild.Name + "::" + channel.Name);
		}

        return Task.CompletedTask;
    }

    private static Task Discord_OnClientError (ClientErrorEventArgs e) {

        return Task.CompletedTask;
    }
    private static Task Discord_OnMessageCreated (MessageCreateEventArgs e) {

    	if (e.Message.Content == "hello") {

    		e.Message.RespondAsync("hello discord!");
    	}

        return Task.CompletedTask;
    }
}
