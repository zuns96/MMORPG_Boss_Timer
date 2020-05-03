using System;
//using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using ZLibrary.Debug;

namespace MMORPG_Boss_Timer
{
    class Program
    {
        private static Program s_instance = null;
        private BossTimer m_discordClient = null;
        //private Timer m_timer;
        private Task m_timerTask;

        static void Main(string[] args)
        {
            s_instance = new Program();
            s_instance.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Log.Create();

            Console.WriteLine("\t   +++++++++++++++++++++++++++");
            Console.WriteLine("\t :+++++++++++++++++++++++++++++:");
            Console.WriteLine("\t-+++++++++++++++++++++++++++++++-");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:++++++++/:---:--------:++++++++:");
            Console.WriteLine("\t:+++++++`               `/++++++:");
            Console.WriteLine("\t:++++++.                 `++++++:");
            Console.WriteLine("\t:+++++:     .-`   `--     -+++++:");
            Console.WriteLine("\t:+++++`    :+++   +++:     +++++:");
            Console.WriteLine("\t:+++++     `--`   `--`     /++++:");
            Console.WriteLine("\t:+++++`   `..``    `...`  `/++++:");
            Console.WriteLine("\t:++++++/-.../++++++++.`.-/++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t:+++++++++++++++++++++++++++++++:");
            Console.WriteLine("\t-+++++++++++++++++++++++:+++++++:");
            Console.WriteLine("\t -/+++++++++++++++++++++: -/++++:");
            Console.WriteLine("\t                            ./++:");
            Console.WriteLine("\t                              .::");

            m_discordClient = new BossTimer();

            if (!m_discordClient.SuccessInitialize)
                return;

            await m_discordClient.Start();

            setTimer();

            await Task.Delay(-1);
        }

        void setTimer()
        {
            if (m_timerTask == null)
            {
                m_timerTask = new Task(UpdateTime);
                m_timerTask.Start();
            }
            //m_timer = new Timer(1000);
            //m_timer.Elapsed += onTimeEvent;
            //m_timer.AutoReset = true;
            //m_timer.Enabled = true;
        }

        void UpdateTime()
        {
            while(true)
            {
                m_discordClient.Tick(DateTime.Now);
                Thread.Sleep(1000);
            }
        }

        //void onTimeEvent(Object source, ElapsedEventArgs e)
        //{
        //    m_discordClient.Tick(e.SignalTime);
        //}
    }
}