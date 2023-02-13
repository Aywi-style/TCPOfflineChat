using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TCPChat.Client
{
    public class MainViewModel : ViewModelBase
    {
        public string IP { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5050;
        public string Nickname { get; set; } = "Bob";
        public string Chat
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Message { get => GetValue<string>(); set => SetValue(value); }

        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        private void Listener()
        {
            Task.Run(() => Listen());
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (_client?.Connected == true)
                    {
                        var line = _reader?.ReadLine();
                        if (line != null)
                        {
                            Chat += line + "\n";
                        }
                        else
                        {
                            _client.Close();
                            Chat += "Connection error.\n";
                        }
                    }
                    Task.Delay(1000).Wait();
                }
                catch (Exception exception)
                {
                    Chat += exception.Message + "\n";
                }
            }
        }

        public AsyncCommand ConnectCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Run(() => Connect());
                }, () => IsNotExistingClient());
            }
        }

        private void Connect()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(IP, Port);
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());
                Listener();
                _writer.AutoFlush = true;

                _writer.WriteLine($"Login: {Nickname}");
            }
            catch (Exception exeption)
            {
                MessageBox.Show(exeption.Message);
            }
        }

        private bool IsNotExistingClient()
        {
            return _client is null || _client?.Connected == false;
        }

        public AsyncCommand SendCommand
        {
            get
            {
                return new AsyncCommand(() =>
                {
                    return Task.Run(() => SendMessage());
                }, () => IsExistingClient(), !string.IsNullOrWhiteSpace(Message));
            }
        }

        private void SendMessage()
        {
            _writer?.WriteLine($"{Nickname}: {Message}");
            Message = "";
        }

        private bool IsExistingClient()
        {
            return _client?.Connected == true;
        }
    }
}
