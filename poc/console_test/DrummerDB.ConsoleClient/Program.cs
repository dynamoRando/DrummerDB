using Drummersoft.DrummerDB.Client;
using System;
using System.Threading.Tasks;
namespace DrummerDB.ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string clientUrl = "https://localhost";
            int clientPortNumber = 5017;
            string completeUrl = clientUrl + ":" + clientPortNumber.ToString();
            Console.WriteLine(completeUrl);

            Console.WriteLine("Testing SQL Service");
            var foo = new DrummerSQLClient(clientUrl, clientPortNumber);
            try
            {
                Console.WriteLine(foo.IsClientOnlineAsync().ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            Console.WriteLine("Testing Info Service");
            int infoPortNumber = 5018;

            var bar = new DrummerInfoClient(clientUrl, infoPortNumber);

            try
            {
                Console.WriteLine(bar.IsClientOnlineAsync().ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

            Console.WriteLine("Testing Db Service");
            int dbPortNumber = 5016;
            var baz = new DrummerDatabaseClient(clientUrl, dbPortNumber);
            try
            {
                Console.WriteLine(baz.IsClientOnlineAsync().ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}
