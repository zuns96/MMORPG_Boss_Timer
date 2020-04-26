using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMORPG_Boss_Timer.MasterTable;

namespace MMORPG_Boss_Timer.BossTimerData
{
    public class BossGenerateData
    {
        private MasterData_BossGenerateData m_masterData = null;

        private DateTime lastGenTime;
        private DateTime nextGenTime;

        public BossGenerateData(MasterData_BossGenerateData masterData)
        {
            m_masterData = masterData;
        }
    }

    public class BossGenerateSaveData
    {
        private int PID;
        private DateTime lastGenTime;
        private DateTime nextGenTime;
    }
}
