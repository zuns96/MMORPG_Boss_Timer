using System;
using System.Timers;
using System.Threading.Tasks;
using ZLibrary.Google;
using ZLibrary.Debug;

namespace MMORPG_Boss_Timer
{
    class Program
    {
        private static Program s_instance = null;
        private BossTimer m_discordClient = null;
        private Timer m_timer;

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

            setTimer();

            if (!m_discordClient.SuccessInitialize)
                return;

            await m_discordClient.Start();

            await Task.Delay(-1);
        }

        void setTimer()
        {
            m_timer = new Timer(1000);
            m_timer.Elapsed += onTimeEvent;
            m_timer.AutoReset = true;
            m_timer.Enabled = true;
        }

        void onTimeEvent(Object source, ElapsedEventArgs e)
        {
            m_discordClient.Tick(e.SignalTime);
        }
    }
}