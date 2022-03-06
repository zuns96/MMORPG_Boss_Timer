using System;
using System.Threading;
using System.Threading.Tasks;

namespace MMORPG_Boss_Timer
{
    class Program
    {
        private BossTimer m_discordClient = null;
        private Task m_timerTask;

        static void Main(string[] args)
        {
            var instance = new Program();
            instance.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
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

            if (!m_discordClient.Initialized)
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
        }

        void UpdateTime()
        {
            while (true)
            {
                m_discordClient.Tick(DateTime.Now);
                Thread.Sleep(10);
            }
        }
    }
}
