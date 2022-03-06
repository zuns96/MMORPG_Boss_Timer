using System;
using System.Text;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord_Boss_Timer;
using Newtonsoft.Json;
using System.IO;
using Discord_Boss_Timer.BossTimerData;
using Discord_Boss_Timer.MasterTable;

namespace Discord_Boss_Timer
{
    public interface IDiscordMessageListener
    {
        void Tick(DateTime now);
        void RecvMessage(SocketMessage socketMessage);
    }

    public class DiscordBossTimer : IDiscordMessageListener
    {
        const string c_json_dir = "./Data";
        const string c_json_file_name = "boss_gen_data.json";

        int m_count = 0;
        List<BossGenerateData> _lstBossData = null;

        Dictionary<int, BossGenerateData> _dicBossData = null;
        Dictionary<string, int> _dicBossNameAbbreviationPairPID = null;

        public DiscordBossTimer()
        {
            _dicBossData = new Dictionary<int, BossGenerateData>();
            _dicBossNameAbbreviationPairPID = new Dictionary<string, int>();

            MasterTable_BossGenerateData.Init();
            var itor = MasterTable_BossGenerateData.GetEnumerator();
            _lstBossData = new List<BossGenerateData>();
            while (itor.MoveNext())
            {
                var data = itor.Current.Value;
                BossGenerateData bossGenData = new BossGenerateData(data);
                _lstBossData.Add(bossGenData);
                _dicBossData.Add(data.PID, bossGenData);
                _dicBossNameAbbreviationPairPID.Add(data.bossName_Abbreviation, data.PID);
            }

            m_count = _lstBossData.Count;
            loadJsonData();

            DiscordClient.INSTANCE.AddListenr(this);
        }

        public void Tick(DateTime now)
        {
            bool saveData = false;

            long nowSec = now.Ticks / TimeSpan.TicksPerSecond;
            //Log.WriteLog("Tick===============================================시작");
            for (int i = 0; i < m_count; ++i)
            {
                var data = _lstBossData[i];
                bool left5Min;
                bool alarm;
                bool isGen = data.CheckGenTime(now, nowSec, out left5Min, out alarm);
                saveData |= isGen;
                if (alarm)
                {
                    if (isGen)
                    {
                        DiscordClient.INSTANCE.SendMessage($"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 부활했습니다. 다음 젠 타임 : {data.NextGenTime}");
                    }
                    else if (left5Min)
                    {
                        DiscordClient.INSTANCE.SendMessage($"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 부활까지 5분 남았습니다! 다음 젠 타임 : {data.NextGenTime}");
                    }
                }
            }

            if (saveData)
            {
                saveJsonData();
            }

            //Log.WriteLog("Tick===============================================종료");
        }

        public void RecvMessage(SocketMessage socketMessage)
        {
            // 봇인지 아닌지
            if (socketMessage.Source == MessageSource.User)
            {
                if (socketMessage.Author is SocketGuildUser)    //채팅 그룹 유저한테만 반응하기
                {
                    var author = socketMessage.Author;
                    CheckCommand(socketMessage, socketMessage.Content, author.Username, author.Id);
                }
            }
        }
        
        void CheckCommand(SocketMessage socketMessage, string command, string userName, ulong userId)
        {
            if (string.IsNullOrEmpty(command) || command[0] != '.')
                return;

            var param = command.Split(' ');
            int len = param.Length;
            if (len < 1)
                return;

            switch(param[0])
            {
                case ".컷":
                    {
                        if (len < 2)
                            return;
                        
                        if (_dicBossNameAbbreviationPairPID.ContainsKey(param[1]))
                        {
                            int pid = _dicBossNameAbbreviationPairPID[param[1]];
                            if (_dicBossData.ContainsKey(pid))
                            {
                                var data = _dicBossData[pid];
                                data.SetGenTime(DateTime.Now);
                                DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 다음 젠 타임 : {data.NextGenTime}");

                                saveJsonData();
                            }
                        }
                    }
                    break;

                case ".멍":
                    {
                        if (len < 2)
                            return;
                        
                        if (_dicBossNameAbbreviationPairPID.ContainsKey(param[1]))
                        {
                            int pid = _dicBossNameAbbreviationPairPID[param[1]];
                            if (_dicBossData.ContainsKey(pid))
                            {
                                var data = _dicBossData[pid];
                                if (!data.m_masterData.isRandom)
                                    return;

                                //data.SetGenTime(DateTime.Now);
                                DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 다음 젠 타임 : {data.NextGenTime}");
                            }
                        }
                    }
                    break;

                case ".보탐":
                    {
                        StringBuilder sb = new StringBuilder();
                        _lstBossData.Sort((a, b) =>
                        {
                            if (!a.Alarm)
                                return 1;
                            else if (!b.Alarm)
                                return -1;
                            else
                                return a.NextGenTime.CompareTo(b.NextGenTime);
                        });

                        for (int i = 0; i < m_count; ++i)
                        {
                            var data = _lstBossData[i];
                            if (data.Alarm)
                                sb.AppendLine($"{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation}) : {data.NextGenTime}");
                            else
                                sb.AppendLine($"{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation}) : 정보 없음");
                        }
                        DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, sb.ToString());
                    }
                    break;

                case ".시간":
                    {
                        if (len < 2)
                            return;

                        if (_dicBossNameAbbreviationPairPID.ContainsKey(param[1]))
                        {
                            int pid = _dicBossNameAbbreviationPairPID[param[1]];
                            if (_dicBossData.ContainsKey(pid))
                            {
                                var data = _dicBossData[pid];
                                if (data.Alarm)
                                    DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 다음 젠 타임 : {data.NextGenTime}");
                                else
                                    DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 다음 젠 타임 : 정보 없음");
                            }
                        }
                    }
                    break;
                case ".숨김":
                    {
                        if (len < 2)
                            return;

                        if (_dicBossNameAbbreviationPairPID.ContainsKey(param[1]))
                        {
                            int pid = _dicBossNameAbbreviationPairPID[param[1]];
                            if (_dicBossData.ContainsKey(pid))
                            {
                                var data = _dicBossData[pid];
                                if (data.Alarm)
                                {
                                    data.EnableAlram(false);
                                    DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 숨김 처리 완료하였습니다.");
                                }
                                else
                                {
                                    DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"[{data.m_masterData.bossName}({data.m_masterData.bossName_Abbreviation})] 이미 숨김 처리됐습니다.");
                                }
                            }
                        }
                    }
                    break;
                default:
                    {
                        RecvUnknownCommand(socketMessage, command);
                    }
                    break;
            }
        }

        void RecvUnknownCommand(SocketMessage socketMessage, string command)
        {
            DiscordClient.INSTANCE.SendMessage(socketMessage.Channel, $"알 수 없는 명령어 입니다.({command})\n명령어 목록:\n.보탐 : 모든 보스의 젠 타임 확인\n.컷 보스이름(약자) : 해당 보스의 젠 타임을 현재 시간으로 갱신\n.시간 보스이름(약자) : 해당 보스의 젠 타임을 확인");
        }

        void saveJsonData()
        {
            if (!Directory.Exists(c_json_dir))
            {
                Directory.CreateDirectory(c_json_dir);
            }

            List<BossGenerateSaveData> lstSaveData = new List<BossGenerateSaveData>();
            var itor = _dicBossData.GetEnumerator();
            for(int i = 0; i < m_count; ++i)
            { 
                var data = _lstBossData[i];
                lstSaveData.Add(new BossGenerateSaveData()
                {
                    pid = data.m_masterData.PID,
                    lastGenTime = data.LastGenTime,
                    nextGenTime = data.NextGenTime,
                    alarmEnable = data.Alarm,
                });
            }

            string filePath = c_json_dir + c_json_file_name;
            if (File.Exists(filePath))
            {
                string newPath = filePath.Replace(".json", $"_{DateTime.Now.ToString("yyyy-MM-dd HH;mm;ss")}.json");
                File.Copy(filePath, newPath);
            }

            string jsonString = JsonConvert.SerializeObject(lstSaveData, Formatting.Indented);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            {
                streamWriter.Write(jsonString);
            }
        }

        void loadJsonData()
        {
            string filePath = Path.Combine(c_json_dir, c_json_file_name);
            if (!File.Exists(filePath))
                return;

            List<BossGenerateSaveData> lstSaveData = null;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                string jsonString = streamReader.ReadToEnd();
                lstSaveData = JsonConvert.DeserializeObject<List<BossGenerateSaveData>>(jsonString);
            }

            DateTime dtNow = DateTime.Now;
            int count = lstSaveData.Count;
            for (int i = 0; i < count; ++i)
            {
                if(_dicBossData.ContainsKey(lstSaveData[i].pid))
                {
                    _dicBossData[lstSaveData[i].pid].LoadData(lstSaveData[i], dtNow);
                }
            }
        }
    }
}