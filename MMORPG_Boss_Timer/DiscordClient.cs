using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ZLibrary;
using ZLibrary.Debug;

namespace Discord_Bot
{
    public class DiscordClient
    {
        protected DiscordSocketClient m_client = null;
        private RequestOptions m_requestOption = null;

        private string m_token = null;
        protected bool m_successInitialize = false;
        private List<SocketGuild> m_guild = null;

        public bool SuccessInitialize { get { return m_successInitialize; } }

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
            iniUtil ini = new iniUtil(Define.c_config_path);

            if (ini == null)
            {
                Log.WriteLog("config.ini 파일이 존재하지 않습니다.");
                return;
            }

            m_token = ini.GetIniValue("app_info", "token");
            if (string.IsNullOrEmpty(m_token))
            {
                Log.WriteLog("token이 존재하지 않습니다.");
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
            //m_requestOption.Timeout = 20 * 1000;    // 20초
            //m_requestOption.UseSystemClock = true;

            m_successInitialize = true;
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
            Log.WriteLog($"OnConnected 시작");
            if(m_client.Guilds != null)
            {
                int idx = 0;
                var itor = m_client.Guilds.GetEnumerator();
                while(itor.MoveNext())
                {
                    var guild = itor.Current;
                    m_guild.Add(guild);
                    Log.WriteLog($"[{idx++}] GuildID : {guild.Id}");
                }
            }
            Log.WriteLog($"OnConnected 끝");
            return Task.CompletedTask;
        }

        // 채널 입장
        private Task joinedGuild(SocketGuild socketChannel)
        {
            Log.WriteLog($"{socketChannel.ToString()}({socketChannel.Id}) 채널에 참가했습니다.");
            return Task.CompletedTask;
        }

        private Task leftGuild(SocketGuild socketChannel)
        {
            Log.WriteLog($"{socketChannel.ToString()}({socketChannel.Id}) 채널에서 퇴장했습니다.");
            return Task.CompletedTask;
        }

        virtual protected void onMessage(SocketMessage socketMessage)
        {
            // 봇인지 아닌지
            if (socketMessage.Source == MessageSource.User)
            {
                if (socketMessage.Author is SocketGuildUser)    //채팅 그룹 유저한테만 반응하기
                {
                    Log.WriteLog($"{socketMessage.Author.Username}({socketMessage.Author.Id}) {socketMessage.Content}");
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
            Log.WriteLog($"{logMessage.Source} {logMessage.Message}");
            return Task.CompletedTask;
        }

        protected void SendMessage(string msg)
        {
            int count = m_guild.Count;
            for(int i = 0; i < count; ++i)
            {
                m_guild[i].DefaultChannel.SendMessageAsync($"```{msg}```", false, null, m_requestOption).ContinueWith(task =>
                {
                    Log.WriteLog($"{m_client.CurrentUser.Username}({m_client.CurrentUser.Id}) {msg}");
                });
            }
        }

        protected void SendMessage(IMessageChannel channel, string msg)
        {
            channel.SendMessageAsync($"```{msg}```", false, null, m_requestOption).ContinueWith(task =>
            {
                Log.WriteLog($"{m_client.CurrentUser.Username}({m_client.CurrentUser.Id}) {msg}");
            });
        }
    }
}
