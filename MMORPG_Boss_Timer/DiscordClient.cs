using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.WebSocket;

namespace Discord_Boss_Timer
{
    public class TimeOutLock : IDisposable
    {
        private Object _lockObject = null;
        private int _timeout = 20000;

        public TimeOutLock(Object obj)
        {
            _lockObject = obj;
        }

        public TimeOutLock Lock()
        {
            if (System.Threading.Monitor.TryEnter(_lockObject, _timeout))
                return new TimeOutLock(_lockObject);
            else
                throw new System.TimeoutException("failed to acquire the lock");
        }

        public void Dispose()
        {
            _lockObject = null;
        }
    }

    public class DiscordClient
    {

        public static readonly DiscordClient INSTANCE = new DiscordClient();

        protected DiscordSocketClient _client = null;
        private RequestOptions m_requestOption = null;

        private string _token = null;
        protected bool _initialized = false;
        private List<SocketGuild> _guilds = null;
        private ConcurrentBag<IDiscordMessageListener> _listeners = null;
        private TimeOutLock @lock = new TimeOutLock(new object());

        public bool Initialized { get { return _initialized; } }

        private DiscordClient()
        {
            XmlNodeList xmlNodes = null;
            using (var text = File.OpenText(Define.c_config_path))
            {
                if(text == null)
                {
                    return;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(text.ReadToEnd());
                xmlNodes = xmlDoc.GetElementsByTagName("config");
            }

            var e = xmlNodes[0].SelectSingleNode("app_info");
            _token = e.Attributes["token"].Value;
            if (string.IsNullOrEmpty(_token))
            {
                return;
            }

            _client = new DiscordSocketClient();
            _guilds = new List<SocketGuild>();
            _listeners = new ConcurrentBag<IDiscordMessageListener>();

            _client.Connected += connected;
            _client.JoinedGuild += joinedGuild;  // 채널 입장
            _client.LeftGuild += leftGuild;    // 채널 퇴장
            _client.MessageReceived += messageReceived;    // 메시지 받음
            _client.Log += onLog;  // 시스템 로그

            m_requestOption = RequestOptions.Default.Clone();
            m_requestOption.Timeout = 20 * 1000;    // 20초
            m_requestOption.UseSystemClock = true;

            _initialized = true;
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }

        private Task connected()
        {
            _guilds.Clear();

            if(_client.Guilds != null)
            {
                var itor = _client.Guilds.GetEnumerator();
                while(itor.MoveNext())
                {
                    var guild = itor.Current;
                    _guilds.Add(guild);
                }
            }
            return Task.CompletedTask;
        }

        // 채널 입장
        private Task joinedGuild(SocketGuild socketChannel)
        {
            using(@lock)
            {
                int idx = _guilds.FindIndex(item => item.Id == socketChannel.Id);
                if (idx >= 0)
                {
                    _guilds.RemoveAt(idx);
                }
                _guilds.Add(socketChannel);
            }

            return Task.CompletedTask;
        }

        private Task leftGuild(SocketGuild socketChannel)
        {
            using(@lock)
            {
                int idx = _guilds.FindIndex(item => item.Id == socketChannel.Id);
                if (idx >= 0)
                {
                    _guilds.RemoveAt(idx);
                }
            }

            return Task.CompletedTask;
        }
        
        // 메시지 받음
        private Task messageReceived(SocketMessage socketMessage)
        {
            onMessage(socketMessage);
            return Task.CompletedTask;
        }
        
        private Task onLog(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        public void SendMessage(string msg)
        {
            using (@lock)
            {
                foreach (var guild in _guilds)
                {
                    var task = guild.DefaultChannel.SendMessageAsync($"```{msg}```", false, null, m_requestOption);
                    task.Wait();
                }
            }
        }

        public void SendMessage(IMessageChannel channel, string msg)
        {
            var task = channel.SendMessageAsync($"```{msg}```", false, null, m_requestOption);
            task.Wait();
        }

        public void AddListenr(IDiscordMessageListener listener)
        {
            _listeners.Add(listener);
        }

        private void onMessage(SocketMessage socketMessage)
        {
            foreach(var entitiy in _listeners)
            {
                entitiy.RecvMessage(socketMessage);
            }
        }

        public void Tick(DateTime now)
        {
            foreach(var entitiy in _listeners)
            {
                entitiy.Tick(now);
            }
        }
    }
}
