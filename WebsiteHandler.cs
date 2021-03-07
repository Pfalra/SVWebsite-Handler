using System;

namespace SVWebsiteHandler
{
    class WebsiteHandler
    {
        const int major = 0;
        const int minor = 1;
        const int patch = 0;
        
        private static void PrintProductHead()
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"Website Accessor {major}.{minor}.{patch}");
            Console.WriteLine("--------------------------------------------");
        }

        private static void PrintGreeting()
        {
            Console.WriteLine("Hallo! Dieses Programm verwaltet alle Aktionen für den Zugriff auf die Webseite des Schützenvereins.");
            Console.WriteLine("Hierfür wird eine Verbindung mit dem Internet benötigt. Downloads und Uploads können dabei ein wenig dauern.");
        }

        private static void PrintSeparator()
        {
            Console.WriteLine("--------------------------------------------");
        }

        static void Main(string[] args)
        {
            Console.Title = "WebsiteHandler";
            Console.TreatControlCAsInput = false;
            Console.ForegroundColor = ConsoleColor.Green;
            PrintProductHead();
            PrintSeparator();
            PrintGreeting();
            Console.ForegroundColor = ConsoleColor.White;


            InstallationChecker checker = new InstallationChecker();

            if (checker.CheckSetup())
            {
                Console.WriteLine("\r\nEs liegt eine valide Installation aller notwendigen Tools vor.");

                if (checker.InstallCounter > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Es wurde Software installiert. Nun ist ein Neustart erforderlich um Änderungen zu Übernehmen");
                }
            } 
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nEine Installation aller Tools ist nicht möglich. Wenden Sie sich an den Ersteller dieses Tools.");
                Console.WriteLine("\r\nE-Mail: ba11i5t0.dev@gmail.com");
            }



            /* Encryptor has been moved to separate tool. Only the normal usecase is now handled. */

        }
    }
}
