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
            PrintProductHead();
            PrintSeparator();
            PrintGreeting();

            //InstallationChecker checker = new InstallationChecker();
            //checker.checkSetup();

            /* Encryptor has been moved to separate tool. Only the normal usecase is now handled. */

        }
    }
}
