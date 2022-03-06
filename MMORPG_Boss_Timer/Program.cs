using Discord_Boss_Timer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Boss_Timer
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var task = MainAsync();
            var awaiter = task.GetAwaiter();
            awaiter.GetResult();
        }

        public static async Task MainAsync()
        {
            Console.WriteLine("\t   +++++++++++++++++++++++++++   ");
            Console.WriteLine("\t :+++++++++++++++++++++++++++++: ");
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

            await DiscordClient.INSTANCE.Start();

            new DiscordBossTimer();

            UpdateTime();

            await Task.Delay(-1);
        }

        private static void UpdateTime()
        {
            while (true)
            {
                DiscordClient.INSTANCE.Tick(DateTime.UtcNow);
                Thread.Sleep(10);
            }
        }
    }
}
