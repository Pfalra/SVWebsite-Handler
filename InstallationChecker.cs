using System;
using System.IO;

namespace SVWebsiteHandler
{
    class InstallationChecker
    {

        private static string user_save_path = string.Format("C:\\Users\\{0}\\Documents\\WebAccessor", Environment.UserName);
        private static string user_cfg = user_save_path + "\\cfg.txt";

        private static string userFirstName;
        private static string userLastName;

        InstallationChecker() { }


        private static void SaveUsername()
        {
            if (Directory.Exists(user_save_path))
            {
                return;
            }
            else
            {
                Console.WriteLine("Folgende Informationen werden benötigt um Änderungen an der Website besser nachvollziehen zu können. \r\n " +
                    "Bitte beantworten Sie diese wahrheitsgemäß. \r\n Die Informationen werden ausschließlich für die Versionierungssoftware verwendet.");
                string saveStr = "";
                bool approved = false;
                while (!approved)
                {
                    Console.WriteLine("Bitte tragen Sie Ihren Vornamen ein: ");
                    string first_name = Console.ReadLine();
                    Console.WriteLine("Bitte tragen Sie Ihren Nachnamen ein: ");
                    string last_name = Console.ReadLine();
                    saveStr = string.Format("{0},{1}", first_name, last_name);
                    Console.WriteLine("Der Name {0} wird nun hinterlegt", saveStr);
                    Console.WriteLine("Ist der Name richtig? (J/N)");
                    string input = Console.ReadLine();

                    if (input.StartsWith('J') || input.StartsWith('j'))
                    {
                        approved = true;
                    }
                }
                try
                {
                    Directory.CreateDirectory(user_save_path);
                    FileStream fs = File.Create(user_cfg);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(saveStr);
                    sw.Flush();
                    sw.Close();
                    Console.WriteLine("Nutzerkonfiguration wurde erfolgreich erstellt.");
                }
                catch (IOException e1)
                {
                    Console.WriteLine("Speichern der Nutzerkonfiguration fehlgeschlagen.");
                    Console.WriteLine(e1.ToString());
                }
            }
        }

        public bool checkSetup()
        {
            /* Check if username was set */
            if (!isUsernameSet())
            {
                setupUser();
            }


            /* Check if Git is installed */

            /* Check if CURL is available */
            return false;
        }

        private void setupUser()
        {

        }

        private bool isUsernameSet()
        {
            Console.WriteLine("Überprüfe ob der Nutzername gesetzt ist...");

            if (!File.Exists(user_cfg))
            {
                Console.WriteLine("Nutzerkonfiguration wurde noch nicht durchgeführt.");
                return false;
            }
            else return true;
        }
    }
}
