using System;
using ZLibrary.Debug;
using MMORPG_Boss_Timer.MasterTable;

namespace MMORPG_Boss_Timer.BossTimerData
{
    public class BossGenerateData
    {
        const int c_5minToSec = 60 * 5;

        public readonly MasterData_BossGenerateData m_masterData = null;

        int m_timeIdx = 0;
        private DateTime m_lastGenTime;
        private DateTime m_nextGenTime;
        private long m_targetTime;
        private bool m_alarmOn;

        public DateTime LastGenTime { get { return m_lastGenTime; } }
        public DateTime NextGenTime { get { return m_nextGenTime; } }
        public bool Alarm { get { return m_alarmOn; } }

        readonly object m_criticalSec = null;

        public BossGenerateData(MasterData_BossGenerateData masterData)
        {
            m_criticalSec = new object();
            m_masterData = masterData;
            m_lastGenTime = DateTime.Now;
            m_alarmOn = true;
            SetNetxGenTime(DateTime.Now);
        }

        public bool CheckGenTime(DateTime dtNow, long now, out bool left5Min, out bool alarm)
        {
            bool gen = now == m_targetTime;
            long delta = (m_targetTime - now);

            left5Min = delta == c_5minToSec;
            alarm = m_alarmOn;


            if (gen)
            {
                m_lastGenTime = dtNow;
                SetNetxGenTime(dtNow);
            }

            //if(left5Min)
            //    Log.WriteLog($"[{m_masterData.bossName}({m_masterData.bossName_Abbreviation}) remain time : {delta}]");

            return gen;
        }

        public void EnableAlram(bool enable)
        {
            m_alarmOn = enable;
        }

        public void SetGenTime(DateTime dateTime)
        {
            m_lastGenTime = dateTime;
            m_alarmOn = true;
            SetNetxGenTime(dateTime);
        }

        public void SetNetxGenTime(DateTime dtNow)
        {
            var now = dtNow.TimeOfDay;
            lock (m_criticalSec)
            {
                switch (m_masterData.genType)
                {
                    case MasterData_BossGenerateData.EGEN_TYPE.REPEAT:
                        {
                            m_nextGenTime = m_lastGenTime;
                            do
                            {
                                m_nextGenTime += m_masterData.values[0];
                            }
                            while (m_nextGenTime < dtNow);
                        }
                        break;
                    case MasterData_BossGenerateData.EGEN_TYPE.FIXED_TIME:
                        {
                            bool bSetGenTime = false;
                            int len = m_masterData.values.Length;
                            for (int i = 0; i < len; ++i)
                            {
                                int hour = now.Hours;
                                int min = now.Minutes;

                                int targetHour = m_masterData.values[i].Hours;
                                int targetMin = m_masterData.values[i].Minutes;

                                if ((hour < targetHour || (hour == targetHour && min < targetMin)))
                                {
                                    bSetGenTime = true;
                                    m_timeIdx = i;
                                    m_nextGenTime = dtNow - now + m_masterData.values[i];
                                    break;
                                }
                            }

                            if (!bSetGenTime)
                            {
                                m_timeIdx = 0;
                                m_nextGenTime = dtNow - now + m_masterData.values[0];
                                m_nextGenTime = m_nextGenTime.AddDays(1);
                            }
                        }
                        break;
                    case MasterData_BossGenerateData.EGEN_TYPE.TIME_NUMBER_TYPE:
                        {
                            dtNow = dtNow.AddMinutes(-now.Minutes).AddSeconds(-now.Seconds);
                            int pivot = (int)m_masterData.numberType;

                            m_nextGenTime = dtNow.AddHours(1 + (now.Hours & pivot));
                        }
                        break;
                }

                m_targetTime = m_nextGenTime.Ticks / TimeSpan.TicksPerSecond;
            }
        }

        public void LoadData(BossGenerateSaveData saveData, DateTime dtNow)
        {
            m_lastGenTime = saveData.lastGenTime;
            m_nextGenTime = saveData.nextGenTime;
            m_alarmOn = saveData.alarmEnable;

            if (dtNow > m_nextGenTime)
                SetNetxGenTime(dtNow);
            else
                m_targetTime = m_nextGenTime.Ticks / TimeSpan.TicksPerSecond;
        }
    }

    public class BossGenerateSaveData
    {
        public int pid;
        public DateTime lastGenTime;
        public DateTime nextGenTime;
        public bool alarmEnable;
    }
}
