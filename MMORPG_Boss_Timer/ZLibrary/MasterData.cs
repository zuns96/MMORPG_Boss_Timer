using System;
using System.Collections.Generic;

namespace ZLibrary
{
    public abstract class MasterData
    {
        public readonly int PID;

        public MasterData(IList<object> row)
        {
            int x = 0;
            PID = Convert.ToInt32(row[x]);
        }
    }
}
