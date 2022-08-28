
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

public static class ClientExample {

    public static void Main (string[] args) {

        if (args.Length == 0) {

            Console.WriteLine("Please provide some arguments, silly!");
            Environment.Exit(-1);
        }
        if (args.Length == 1) {

            Console.WriteLine("At least enter a password also! You know for the Authentication..");
            Environment.Exit(-1);
        }

        if (args.Length == 2) RunInInteractive(args[0], args[1]);

        string message = "";
        if (args.Length > 5) for (int i = 4; i < args.Length; ++i)
                message += args[i] + " ";

        switch (args[2]) {

            case "-g": case "--get-channels": case "--get-channels-list": case "--get-channel-list":
                PrintChannelList(args[0], args[1]); break;

            case "-ss": case "--send-string-id":
                if (args.Length == 4) {

                    Console.WriteLine("Please provide a message!");
                    Environment.Exit(-1);
                }
                SendNameMessage(args[0], args[1], args[3], message); break;

            case "-si": case "--send-ulong-id":
                if (args.Length == 4) {

                    Console.WriteLine("Please provide a message!");
                    Environment.Exit(-1);
                }
                SendIdMessage(args[0], args[1], ulong.Parse(args[3]), message); break;

            case "-m": case "--get-messages":
                if (args.Length != 5) {

                    Console.WriteLine("Please provide a channelTarget and an amount!");
                    Environment.Exit(-1);
                }
                PrintMessagesFromChannel(args[0], args[1], args[3], int.Parse(args[4])); break;

            default: RunHelp(); break;
        }

        Environment.Exit(0);
    }

    private static void SendIdMessage (string address, string password, ulong channelId, string message) {

		TcpClient client = new TcpClient(address, 1122);
		NetworkStream stream = client.GetStream();
		byte[] sendBuffer;

        stream.Write(new byte[1]{0});

		//send useragent
		sendBuffer = Encoding.Unicode.GetBytes("nilfca-client-example");
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);

		//send password
		sendBuffer = Encoding.Unicode.GetBytes(Sha256Hash(password));
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);

		//send channelId
        stream.Write(BitConverter.GetBytes(channelId), 0, 8);

		//send message
		sendBuffer = Encoding.Unicode.GetBytes(message);
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);
    }

    private static void SendNameMessage (string address, string password, string channelName, string message) {

		TcpClient client = new TcpClient(address, 1122);
		NetworkStream stream = client.GetStream();
		byte[] sendBuffer;

        stream.Write(new byte[1]{1});

		//send useragent
		sendBuffer = Encoding.Unicode.GetBytes("nilfca-client-example");
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);

		//send password
		sendBuffer = Encoding.Unicode.GetBytes(Sha256Hash(password));
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);

		//send channelName
		sendBuffer = Encoding.Unicode.GetBytes(channelName);
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);

		//send message
		sendBuffer = Encoding.Unicode.GetBytes(message);
		stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
		stream.Write(sendBuffer, 0, sendBuffer.Length);
    }

    private static void PrintChannelList (string address, string password) {

		TcpClient client = new TcpClient(address, 1122);
		NetworkStream stream = client.GetStream();
		byte[] buffer;

        stream.Write(new byte[1]{2});

		//send useragent
		buffer = Encoding.Unicode.GetBytes("nilfca-client-example");
		stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
		stream.Write(buffer, 0, buffer.Length);

		//send password
		buffer = Encoding.Unicode.GetBytes(Sha256Hash(password));
		stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
		stream.Write(buffer, 0, buffer.Length);

        //recv channal count
        buffer = new byte[4];
        stream.Read(buffer, 0, 4);
        int channelCount = BitConverter.ToInt32(buffer, 0);

        ulong[] channelIds = new ulong[channelCount];
        string[] channelNames = new string[channelCount];

        //recv channel ids
        for (int i = 0; i < channelCount; ++i) {

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            channelIds[i] = BitConverter.ToUInt64(buffer, 0);
        }

        //recv channel names
        for (int i = 0; i < channelCount; ++i) {

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            stream.Read(buffer, 0, buffer.Length);

            channelNames[i] = Encoding.Unicode.GetString(buffer);
        }

        //print all channel names and ids
        for (int i = 0; i < channelCount; ++i) {

            Console.WriteLine(channelIds[i].ToString() + " " + channelNames[i]);
        }
    }

    private static void PrintMessagesFromChannel (string address, string password, string channelName, int messageCount) {

		TcpClient client = new TcpClient(address, 1122);
		NetworkStream stream = client.GetStream();
		byte[] buffer;

        stream.Write(new byte[1]{3});

		//send useragent
		buffer = Encoding.Unicode.GetBytes("nilfca-client-example");
		stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
		stream.Write(buffer, 0, buffer.Length);

		//send password
		buffer = Encoding.Unicode.GetBytes(Sha256Hash(password));
		stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
		stream.Write(buffer, 0, buffer.Length);

		//send channelName
		buffer = Encoding.Unicode.GetBytes(channelName);
		stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
		stream.Write(buffer, 0, buffer.Length);

        //send message count
		stream.Write(BitConverter.GetBytes(messageCount), 0, 4);

        //recv recved message count
        buffer = new byte[4];
        stream.Read(buffer, 0, 4);
        int recvMessageCount = BitConverter.ToInt32(buffer, 0);

        string[] messageSenders = new string[recvMessageCount];
        string[] messageContents = new string[recvMessageCount];

        //recv messages
        for (int i = 0; i < recvMessageCount; ++i) {

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            stream.Read(buffer, 0, buffer.Length);
            messageSenders[i] = Encoding.Unicode.GetString(buffer);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            stream.Read(buffer, 0, buffer.Length);
            messageContents[i] = Encoding.Unicode.GetString(buffer);
        }

        //print all messages
        for (int i = recvMessageCount - 1; i >= 0; ++i) {

            Console.WriteLine("<" + messageSenders[i].ToString() + ">: " + messageContents[i]);
        }
    }

    private static void RunHelp () {

        Console.WriteLine();
        Console.WriteLine("Nilfca client help page.");
        Console.WriteLine();
        Console.WriteLine(" nilfcacli [address] [password] [options] message");
        Console.WriteLine();
        Console.WriteLine("-g   --get-channels || --get-channels-list || --get-channel-list");
        Console.WriteLine("     gets and prints a list of all channels available on the server");
        Console.WriteLine();
        Console.WriteLine("-ss   --send-string-id");
        Console.WriteLine("     sends a message to a channel, string id");
        Console.WriteLine("     $(guild-name)::$(channel-name)");
        Console.WriteLine();
        Console.WriteLine("-si   --send-ulong-id");
        Console.WriteLine("     sends a message to a channel, ulong id");
        Console.WriteLine();
        Console.WriteLine("-m   --get-messages");
        Console.WriteLine("     gets messages from a channel");
    }

    private static void RunInInteractive (string serverAddress, string password) {

        Console.WriteLine("Welcome!");
        Console.WriteLine();
        Console.WriteLine("    _   _ _ _  __           ");
        Console.WriteLine("   | \\ | (_) |/ _| ___ __ _ ");
        Console.WriteLine("   |  \\| | | | |_ / __/ _` |");
        Console.WriteLine("   | |\\  | | |  _| (_| (_| |");
        Console.WriteLine("   |_| \\_|_|_|_|  \\___\\__,_|");
        Console.WriteLine("                             2022");
        Console.WriteLine();
        Console.WriteLine("A Networked Interface Layer for the Discord API.");
        Console.WriteLine("Running Nilfca client in interactive mode..");

        while (true) {

            Console.WriteLine();
            Console.Write("> ");

            string input = Console.ReadLine();

            string channelTarget = "";
            string message = "";
            if (input.Contains(" ")) {

                string[] args = input.Split(' ');
                input = args[0];
                channelTarget = args[1];
                if (args.Length > 1) for (int i = 2; i < args.Length; ++i)
                    message += args[i] + " ";
            }

            switch (input) {

                case "-g": case "--get-channels": case "--get-channels-list": case "--get-channel-list":
                    Console.WriteLine("Fetching and printing Channel List..");
                    PrintChannelList(serverAddress, password); break;

                case "-ss": case "--send-string-id":
                    if (channelTarget == "" || message == "") {

                        Console.WriteLine("Please input a channelTarget, and a channelName..");
                        break;
                    }
                    Console.WriteLine("Sending message with string id..");
                    SendNameMessage(serverAddress, password, channelTarget, message);
                    Console.WriteLine("Done!"); break;

                case "-si": case "--send-ulong-id":
                    if (channelTarget == "" || message == "") {

                        Console.WriteLine("Please input a channelTarget, and a channelName..");
                        break;
                    }
                    Console.WriteLine("Sending message with ulong id..");
                    SendIdMessage(serverAddress, password, ulong.Parse(channelTarget), message);
                    Console.WriteLine("Done!"); break;

                case "-m": case "--get-messages":
                    if (channelTarget == "" || message == "") {

                        Console.WriteLine("Please input a channelTarget, and an amount..");
                        break;
                    }
                    PrintMessagesFromChannel(serverAddress, password, channelTarget, int.Parse(message)); break;

                default: RunHelp(); break;

                case "exit": case "close": case "quit":
                    Console.WriteLine("Exiting interactive mode..");
                    Environment.Exit(0);
                    break;
            }
        }
    }

    //this is from https://github.com/harroo/Harasoft
    public static string Sha256Hash (string input) {

        HashAlgorithm algorithm = SHA256.Create();
        byte[] hashedData = algorithm.ComputeHash(Encoding.Unicode.GetBytes(input));

        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in hashedData)
            stringBuilder.Append(b.ToString("X2"));

        return stringBuilder.ToString();
    }
}
