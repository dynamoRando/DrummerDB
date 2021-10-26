using Drummersoft.DrummerDB.Core.Systems;
using System;

namespace Drummersoft.DrummerDB.Console
{
    class Program
    {
        private static Process _process;

        static void Main(string[] args)
        {
            string input = string.Empty;
            System.Console.WriteLine("Wecome to DrummerDB Console. This application hosts a DrummerDB process in a terminal.");
            System.Console.WriteLine("Press enter to start a DrummerDB Process in the current folder. Type 'q' to exit process or close the terminal.");
            input = System.Console.ReadLine();
            StartupProcess();

            while (!string.Equals(input, "q", System.StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine("Waiting for input. Press 'q' to quit");
                input = System.Console.ReadLine();
            }

            System.Console.WriteLine("Press any key to quit");
            System.Console.ReadLine();
        }

        static void StartupProcess()
        {
            if (_process is null)
            {
                _process = new Process();
            }

            _process.Start();
            _process.StartSQLServer();
            _process.StartInfoServer();
            _process.StartDbServer();

            // TO DO: Should ermove this!
            // WARN: This is for testing purposes only and should be removed
            _process.Test_SetupAdminLogin("Test0", "Test0", Guid.NewGuid());

            System.Console.WriteLine("Running...");
        }
    }
}
