using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SVWebsiteHandler
{
    class UserHandler
    {
        private const string foldername = "WebsiteHandler";
        private const string filename = "cfg.txt";

        public UserHandler() { }

        /* Returns the path to the directory where the config file for the user is saved */
        public string getSavePath()
        {
            return string.Format("C:\\Users\\{0}\\Documents\\{1}", Environment.UserName, foldername);
        }

        /* Returns the full path to the config file */
        public string getConfigPath()
        {
            return string.Format("{0}\\{1}", getSavePath(), filename);
        }

        /* Reads the username from the config file */
        public string getUsername()
        {
            if (!File.Exists(getConfigPath())) return null;
            else return File.ReadAllText(getConfigPath());
        }

        /* Saves the Username in the config file */
        private void saveUsername(string namestr)
        {
            try
            {
                /* Check if the configuration directory already exists */
                if (!Directory.Exists(getSavePath()))
                {
                    Directory.CreateDirectory(getSavePath());
                }

                FileStream fs = File.Create(getConfigPath());
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(namestr);
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

        /* Handles the setup procedure of with the user */
        public void setupUser()
        {
            Console.WriteLine("Folgende Informationen werden benötigt um Änderungen an der Website besser nachvollziehen zu können.");
            Console.WriteLine("Bitte beantworten Sie diese wahrheitsgemäß. Die Informationen werden ausschließlich für die Versionierungssoftware verwendet.");

            string saveStr = "";
            bool approved = false;
            while (!approved)
            {
                Console.WriteLine();
                Console.WriteLine("Bitte tragen Sie Ihren Vornamen ein: ");
                string first_name = Console.ReadLine();
                Console.WriteLine();
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

            saveUsername(saveStr);
        }
    
        /* Returns true if username is set and valid */
        public bool isUsernameSet()
        {
            Console.WriteLine();
            Console.WriteLine("Überprüfe ob der Nutzername gesetzt ist...");

            if (!File.Exists(getConfigPath()))
            {
                Console.WriteLine();
                Console.WriteLine("Nutzerkonfiguration wurde noch nicht durchgeführt.");
                return false;
            }
            Console.WriteLine();
            Console.WriteLine("Nutzername ist gesetzt: {0}", getUsername());

            return true;
        }
    }
}
