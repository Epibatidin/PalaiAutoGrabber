using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using PalaiAutoGrabber.Pages;

namespace PalaiAutoGrabber
{
    public class Program
    {

        public static async Task Main()
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json", false);

            var config = configBuilder.Build();

            var palaiConfig = config.Get<Configuration>();

            bool justEndWhenDone = palaiConfig.Silent;
            var responseHelper = new ResponseHelper();
            foreach (var account in palaiConfig.Accounts)
            {
                try
                {
                    await DoForAccount(account, responseHelper);
                }
                catch (Exception e)
                {
                    justEndWhenDone = false;
                    Console.WriteLine("Error : " + e);
                }
            };
            Console.WriteLine("Nothing more todo");
            if (!justEndWhenDone)
            {
                Console.WriteLine("Press the \"Any Key\" - Key to close");
                Console.ReadLine();
            }
        }


        private static async Task DoForAccount(Account account, ResponseHelper responseHelper)
        {
            Console.WriteLine("Grabbing for " + account.UserName);

            var palaiClient = new PalaiClient(responseHelper);
            var loginPage = new LoginPage(palaiClient);
            await loginPage.Navigate();
            var dashBoard = await loginPage.Login(account);
            await dashBoard.Navigate();
            var recertification = dashBoard.RequiresRecertification();
            if (recertification != null)
            {
                Console.WriteLine("===============================================");
                Console.WriteLine("The Palai Account is outdated");
                Console.WriteLine("Trying to get a new code");
                await recertification.Navigate();
                await recertification.IssueRecertification(account);
                Console.WriteLine("The process ends here for today");
                Console.WriteLine("===============================================");
                return;
            }
            var incomePage = new IncomePage(loginPage.AuthToken, palaiClient);
            await incomePage.Navigate();
            if (!await incomePage.GrabTheCash())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("In that specific case that means that there is just no basic income available");
                Console.ResetColor();
            }

            await dashBoard.Navigate();
            var cash = dashBoard.getCashAmountFromDashBoard();

            Console.Write("Current Balance : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(cash);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}