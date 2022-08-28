
//this module can be instert into any C# program and used to
//interface with a nilfca server, if u wana check it out,
// https://github.com/harroo/nilfca
//check it out :D

//usage is simple, just read the function names

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

public static class Nilfca {

    //tells the server to queue a message to be send to a channel, using the channel id, ulong
    //keep in mind this doesnt work with all ids because discord ids are now longer somehow, example:
    //works:  836381137357766705
    //!works: 1006762070160719882  <-- extra long??
    //works:  836381137781129327
    //and this doesnt work with users for some reason
    //so using the string one works the best, but this is still here for some compatibility ig
    public static void SendMessage (string address, string password, ulong channelId, string message) {

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

    //tells the server to queue a message to be send to a channel, using the channel name, $(guild.name)::$(channel.name)
    public static void SendMessage (string address, string password, string channelName, string message) {

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

    //returns a list of all available channel names and users
    //all channels the nilfca server can reach
    public static string[] GetChannelList (string address, string password) {

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

        return channelNames;
    }

    //gets a list of the latest x messages from a channel
    public static string[] GetMessages (string address, string password, string channelName, int messageCount) {

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

        return messageContents;
    }

    //gets a list of the latest x messages from a channel, including the users
    //usage: var messages = GetMessagesWithSenders(blah blah blah);
    //       messages[0].a // sender
    //       messages[0].b // message
    //see nilfca_d_string struct;
    public static nilfca_d_string[] GetMessagesWithSenders (string address, string password, string channelName, int messageCount) {

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

        nilfca_d_string[] messages = new nilfca_d_string[recvMessageCount];

        //recv messages
        for (int i = 0; i < recvMessageCount; ++i) {

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            stream.Read(buffer, 0, buffer.Length);
            messages[i].a = Encoding.Unicode.GetString(buffer);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            stream.Read(buffer, 0, buffer.Length);
            messages[i].b = Encoding.Unicode.GetString(buffer);
        }

        return messages;
    }
    public struct nilfca_d_string { public string a, b; }

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
