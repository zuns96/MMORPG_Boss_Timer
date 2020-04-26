using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord_Bot;
using ZLibrary.Debug;
using MMORPG_Boss_Timer.BossTimerData;
using MMORPG_Boss_Timer.MasterTable;

namespace MMORPG_Boss_Timer
{
    public class BossTimer : DiscordClient
    {
        Dictionary<int, BossGenerateData> m_dicBossData = null;
        Dictionary<string, int> m_dicBossNamePairPID = null;
        Dictionary<string, int> m_dicBossNameAbbreviationPairPID = null;

        static BossTimer s_intance = null;

        public BossTimer() : base()
        {
            m_dicBossData = new Dictionary<int, BossGenerateData>();
            m_dicBossNamePairPID = new Dictionary<string, int>();
            m_dicBossNameAbbreviationPairPID = new Dictionary<string, int>();

            MasterTable_BossGenerateData.Init();
            var itor = MasterTable_BossGenerateData.GetEnumerator();
            if (itor != null)
            {
                while (itor.MoveNext())
                {
                    var data = ((KeyValuePair<int, MasterData_BossGenerateData>)itor.Current).Value;
                    m_dicBossData.Add(data.PID, new BossGenerateData(data));
                    m_dicBossNamePairPID.Add(data.bossName, data.PID);
                    m_dicBossNameAbbreviationPairPID.Add(data.bossName_Abbreviation, data.PID);
                }
            }
        }

        protected override void onMessage(SocketMessage socketMessage)
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

        void saveJsonData()
        {

        }

        void loadJsonData()
        {

        }
    }
}
