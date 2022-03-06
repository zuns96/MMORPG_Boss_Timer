using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.WebSocket;

namespace Discord_Bot
{
    public class DiscordClient
    {
        protected DiscordSocketClient m_client = null;
        private RequestOptions m_requestOption = null;

        private string m_appName = null;
        private string m_token = null;
        protected bool m_initialized = false;
        private List<SocketGuild> m_guild = null;

        public bool Initialized { get { return m_initialized; } }

        static public void Release(ref DiscordClient discordClient)
        {
            if(discordClient != null)
            {
                discordClient.release();
                discordClient = null;
            }
        }

        void release()
        {
            m_guild.Clear();
            m_guild = null;

            m_client.LogoutAsync();
            m_client = null;
        }

        public DiscordClient()
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
            m_appName = e.Attributes["AppName"].Value;
            m_token = e.Attributes["token"].Value;
            if (string.IsNullOrEmpty(m_token))
            {
                return;
            }

            m_client = new DiscordSocketClient();
            m_guild = new List<SocketGuild>();

            m_client.Connected += connected;
            m_client.JoinedGuild += joinedGuild;  // 채널 입장
            m_client.LeftGuild += leftGuild;    // 채널 퇴장
            m_client.MessageReceived += messageReceived;    // 메시지 받음
            m_client.Log += onLog;  // 시스템 로그

            m_requestOption = RequestOptions.Default.Clone();
            m_requestOption.Timeout = 20 * 1000;    // 20초
            m_requestOption.UseSystemClock = true;

            m_initialized = true;
        }

        public async Task Start()
        {
            await m_client.LoginAsync(TokenType.Bot, m_token);
            await m_client.StartAsync();
        }

        public virtual void Tick(DateTime dateTime)
        {
            
        }

        private Task connected()
        {
            m_guild.Clear();

            if(m_client.Guilds != null)
            {
                var itor = m_client.Guilds.GetEnumerator();
                while(itor.MoveNext())
                {
                    var guild = itor.Current;
                    m_guild.Add(guild);
                }
            }
            return Task.CompletedTask;
        }

        // 채널 입장
        private Task joinedGuild(SocketGuild socketChannel)
        {
            return Task.CompletedTask;
        }

        private Task leftGuild(SocketGuild socketChannel)
        {
            return Task.CompletedTask;
        }

        protected virtual void onMessage(SocketMessage socketMessage)
        {
            // 봇인지 아닌지
            if (socketMessage.Source == MessageSource.User)
            {
                if (socketMessage.Author is SocketGuildUser)    //채팅 그룹 유저한테만 반응하기
                {
                    SendMessage(socketMessage.Channel, "Hello");
                }
            }
        }
        
        // 메시지 받음
        protected Task messageReceived(SocketMessage socketMessage)
        {
            onMessage(socketMessage);
            return Task.CompletedTask;
        }
        
        private Task onLog(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        protected void SendMessage(string msg)
        {
            int count = m_guild.Count;
            for(int i = 0; i < count; ++i)
            {
                var task = m_guild[i].DefaultChannel.SendMessageAsync($"```{msg}```", false, null, m_requestOption);
                task.Start();
            }
        }

        protected void SendMessage(IMessageChannel channel, string msg)
        {
            var task = channel.SendMessageAsync($"```{msg}```", false, null, m_requestOption);
            task.Start();
        }
    }
}
