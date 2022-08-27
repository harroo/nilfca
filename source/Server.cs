
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
// using System.Collections.Generic;

namespace Nilfca {

    public static class Server {

        private static TcpListener listener;

        public static void Start () {

            listener = new TcpListener(IPAddress.Any, 1122);

            try {

                listener.Start();

                Console.WriteLine("Server started successfully!");

                Loop();

            } catch (Exception ex) {

                Console.WriteLine("Failed to start Server!");
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

        private static void Loop () {

            while (true) {

                try {

    				TcpClient client = listener.AcceptTcpClient();
    				NetworkStream stream = client.GetStream();

    				Console.WriteLine("Client received! " + client.Client.RemoteEndPoint.ToString());

                    byte[] recvBuffer = new byte[1];
                    stream.Read(recvBuffer, 0, 1);

                    switch (recvBuffer[0]) {

                        case 0: RecvIdMessage(stream); break;
                        case 1: RecvNameMessage(stream); break;
                        case 2: GetChannelList(stream); break;
                    }

                    client.Close();
                    stream.Close();


                } catch (Exception ex) {

                    Console.WriteLine("Server.Loop() Error:");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void RecvIdMessage (NetworkStream stream) {

            //recv useragent
            byte[] recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string useragent = Encoding.Unicode.GetString(recvBuffer);

            //recv channel id
            recvBuffer = new byte[8];
            stream.Read(recvBuffer, 0, 8);

            ulong channelId = BitConverter.ToUInt64(recvBuffer, 0);

            //recv message
            recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string message = Encoding.Unicode.GetString(recvBuffer);


            Discord.QueueMessage(channelId, message);

            Console.WriteLine(useragent + ": send to channelId: " + channelId + " message length: " + message.Length);
        }

        private static void RecvNameMessage (NetworkStream stream) {

            //recv useragent
            byte[] recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string useragent = Encoding.Unicode.GetString(recvBuffer);

            //recv channel name
            recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string channelName = Encoding.Unicode.GetString(recvBuffer);

            //recv message
            recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string message = Encoding.Unicode.GetString(recvBuffer);


            Discord.QueueMessage(channelName, message);

            Console.WriteLine(useragent + ": send to channelName: " + channelName + " message length: " + message.Length);
        }

        private static void GetChannelList (NetworkStream stream) {

            //recv useragent
            byte[] recvBuffer = new byte[4];
            stream.Read(recvBuffer, 0, 4);
            recvBuffer = new byte[BitConverter.ToInt32(recvBuffer, 0)];
            stream.Read(recvBuffer, 0, recvBuffer.Length);

            string useragent = Encoding.Unicode.GetString(recvBuffer);

            var channels = Discord.GetChannels();
            byte[] sendBuffer = new byte[4];

            //amount of channels
            stream.Write(BitConverter.GetBytes(channels.Count), 0, 4);

            //each individual channel id
            foreach (var channel in channels) stream.Write(BitConverter.GetBytes(channel.id), 0, 8);

            //each individual channel name
            foreach (var channel in channels) {

                sendBuffer = Encoding.Unicode.GetBytes(channel.name);

                stream.Write(BitConverter.GetBytes(sendBuffer.Length), 0, 4);
                stream.Write(sendBuffer, 0, sendBuffer.Length);
            }

            Console.WriteLine(useragent + ": requested, annd was sent, the channel-list");
        }
    }
}
