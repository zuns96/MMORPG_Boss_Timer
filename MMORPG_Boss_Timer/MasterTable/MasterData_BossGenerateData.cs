using System;
using System.Collections.Generic;
using ZLibrary;
using ZLibrary.Google;

namespace Discord_Boss_Timer.MasterTable
{
    public class MasterTable_BossGenerateData
    {
        readonly string r_table_id = null;
        readonly string r_sheet_name = null;

        static MasterTable_BossGenerateData s_instnace = null;

        Dictionary<int, MasterData_BossGenerateData> m_dic = null;

        static public int Count { get { return s_instnace == null || s_instnace.m_dic == null ? 0 : s_instnace.m_dic.Count; } }

        static public void Init()
        {
            if (s_instnace == null)
            {
                s_instnace = new MasterTable_BossGenerateData();
            }
        }

        static public Dictionary<int, MasterData_BossGenerateData>.Enumerator GetEnumerator()
        {
            if (s_instnace != null && s_instnace.m_dic != null)
                return s_instnace.m_dic.GetEnumerator();
            return new Dictionary<int, MasterData_BossGenerateData>.Enumerator();
        }

        static public void LoadTable()
        {
            if (s_instnace != null)
            {
                s_instnace.loadTable();
            }
        }

        public MasterTable_BossGenerateData()
        {
            loadTable();
        }

        void loadTable()
        {
            try
            {
                m_dic = GoogleSpreadSheet.ConvertTableData<MasterData_BossGenerateData>(r_table_id, r_sheet_name);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    [Serializable]
    public class MasterData_BossGenerateData : MasterData
    {
        public enum EGEN_TYPE : byte
        {
            REPEAT = 1,             // 매 value값 만큼 반복
            TIME_NUMBER_TYPE,   // 0 : 짝수시, 1 : 홀수시
            FIXED_TIME,         // Time 컬럼에 적혀있는 시간에만 체크
        }

        public enum ENUMBER_TYPE : byte
        {
            EVEN_NUMBER,    // 짝수
            ODD_NUMBER,     // 홀수
        }

        public readonly string bossName;
        public readonly string bossName_Abbreviation;
        public readonly EGEN_TYPE genType;

        public readonly TimeSpan[] values;  // EGEN_TYPE이 REPEAT, FIXED_TIME일 때만 사용
        public readonly ENUMBER_TYPE numberType; // // EGEN_TYPE이 REPEAT, FIXED_TIME일 때만 사용

        public readonly bool isRandom;
        public readonly string[] itemList;

        public MasterData_BossGenerateData(IList<object> row) : base(row)
        {
            int x = 1;
            bossName = Convert.ToString(row[x++]);
            bossName_Abbreviation = Convert.ToString(row[x++]);
            genType = (EGEN_TYPE)Convert.ToByte(row[x++]);

            switch(genType)
            {
                case EGEN_TYPE.REPEAT:
                case EGEN_TYPE.FIXED_TIME:
                    {
                        string[] times = Convert.ToString(row[x++]).Split(',');
                        if (times == null)
                        {
                            values = new TimeSpan[0];
                        }
                        else
                        {
                            int len = times.Length;
                            values = new TimeSpan[len];
                            for (int i = 0; i < len; ++i)
                            {
                                string time = times[i].Trim();
                                DateTime dateTime = Convert.ToDateTime(time);
                                values[i] = dateTime.TimeOfDay;
                            }
                        }
                    }
                    break;
                case EGEN_TYPE.TIME_NUMBER_TYPE:
                    {
                        numberType = (ENUMBER_TYPE)Convert.ToByte(row[x++]);
                    }
                    break;
            }

            string random = Convert.ToString(row[x++]);
            isRandom = random != null && random.Equals("R");
            itemList = Convert.ToString(row[x++]).Split(',');
        }

        public override string ToString()
        {
            string log = "PID : " + PID +
                ", bossName : " + bossName +
                ", bossName_Abbreviation : " + bossName_Abbreviation +
                ", genType : " + genType;

            switch (genType)
            {
                case EGEN_TYPE.REPEAT:
                case EGEN_TYPE.FIXED_TIME:
                    {
                        log += ", Times : ";
                        int len1 = values.Length;
                        for (int i = 0; i < len1; ++i)
                        {
                            log += $"[{i + 1}]{values[i]}, ";
                        }
                    }
                    break;
                case EGEN_TYPE.TIME_NUMBER_TYPE:
                    {
                        log += $", numberType : {numberType}, ";
                    }
                    break;
            }

            log += "isRandom : " + isRandom
                + ", ItemList : ";

            int len2 = itemList.Length;
            for(int i = 0; i < len2; ++i)
            {
                log += $"[{i + 1}]{itemList[i]}";
            }

            return log;
        }
    }
}
