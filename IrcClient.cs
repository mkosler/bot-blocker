using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace bot_banner
{
    public class PingSender
    {
        private readonly IrcClient _client;
        private readonly Thread _thread;

        public PingSender(IrcClient client)
        {
            _client = client;
            _thread = new Thread(new ThreadStart(Run));
        }

        public void Start()
        {
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Run()
        {
            while (true)
            {
                _client.SendIrcMessage("PING irc.twitch.tv");
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }

    public class IrcClient
    {
        public string UserName { get; set; }

        private readonly string _channel;
        private readonly TcpClient _tcp;
        private readonly StreamReader _input;
        private readonly StreamWriter _output;

        public IrcClient(string ip, int port, string userName, string password, string channel)
        {
            UserName = userName;
            _channel = channel;

            _tcp = new TcpClient(ip, port);
            _input = new StreamReader(_tcp.GetStream());
            _output = new StreamWriter(_tcp.GetStream());

            _output.WriteLine($"PASS {password}");
            _output.WriteLine($"NICK {userName}");
            _output.WriteLine($"USER {userName} 8 * :{userName}");
            _output.WriteLine($"JOIN #{channel}");
            _output.Flush();
        }

        public void SendIrcMessage(string message)
        {
            _output.WriteLine(message);
            _output.Flush();
        }

        public void SendPublicChatMessage(string message)
        {
            SendIrcMessage($"{UserName}!{UserName}@{UserName}.tmi.twitch.tv PRIVMSG #{_channel} :{message}");
        }

        public string ReadMessage()
        {
            return _input.ReadLine();
        }
    }
}