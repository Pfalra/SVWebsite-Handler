using System;
using System.IO;
using System.Collections.Generic;

namespace WebsiteHandlerBackend
{
    class UserHandler
    {
        public string UserKey { get; } = "USER";
        public string WorkspaceKey { get; } = "WORKSPACE";


        private const string separator = "!";
        private const string foldername = "WebsiteHandler";
        private const string filename = "cfg.txt";
        private readonly string savePath = string.Format("C:\\Users\\{0}\\Documents\\{1}", Environment.UserName, foldername);

        private List<string> KeyList { get; set; } = new List<string>();

        ConsoleTextBlockConnector Connector;

        public UserHandler(ConsoleTextBlockConnector c) 
        {
            Connector = c;
            KeyList.Add(UserKey);
            KeyList.Add(WorkspaceKey);
        }


        /* Returns the path to the directory where the config file for the user is saved */
        public string GetSavePath()
        {
            return savePath;
        }

        /* Returns the full path to the config file */
        public string GetConfigPath()
        {
            return string.Format("{0}\\{1}", GetSavePath(), filename);
        }
    
        /* Returns true if username is set and valid */
        public bool IsUsernameSet()
        {
            Connector.WriteLine();
            Connector.WriteLine("Überprüfe ob der Nutzername gesetzt ist...");

            if (!File.Exists(GetConfigPath()))
            {
                Connector.WriteLine();
                Connector.WriteLine("Nutzerkonfiguration wurde noch nicht durchgeführt.");
                return false;
            }
            Connector.WriteLine();
            Connector.WriteLine("Nutzername ist gesetzt: " + GetFromConfig("USER"));

            return true;
        }

        public void DeleteUserInfo()
        {
            File.Delete(savePath  + "\\" + filename);
            Directory.Delete(savePath);
        }

        public void DisplayPathVar()
        {
            Connector.WriteLine(Environment.GetEnvironmentVariable("PATH"));
        }
    
        public string GetFromConfig(string key)
        {
            Dictionary<string, string> map = ReadConfig();
            string retVal;
            if (map.TryGetValue(key, out retVal))
            {
                return retVal;
            }

            return "Wert nicht gesetzt";
        }

        public void EditConfig(string key, string value)
        {

            Dictionary<string, string> map = ReadConfig();

            if (map == null)
            {
                CreateConfig();
                map = ReadConfig();
            }

            try
            {
                /* Check if the configuration directory already exists */
                if (!Directory.Exists(GetSavePath()))
                {
                    Directory.CreateDirectory(GetSavePath());
                }

                FileStream fs = File.Create(GetConfigPath());
                StreamWriter sw = new StreamWriter(fs);

                foreach (KeyValuePair<string, string> kvp in map)
                {
                    string tmp = "";
                    if (key.Contains(kvp.Key))
                    {
                        tmp = kvp.Key + separator + value;
                    } 
                    else
                    {
                        tmp = kvp.Key + separator + kvp.Value;
                    }
                    sw.WriteLine(tmp);
                }
                sw.Flush();
                sw.Close();
                Console.WriteLine("Konfiguration wurde erfolgreich geändert.");
            }
            catch (IOException e1)
            {
                Connector.WriteLine("Speichern der Nutzerkonfiguration fehlgeschlagen.");
                Console.WriteLine(e1.ToString());
            }
        }

        public void CreateConfig()
        {
            try
            {
                /* Check if the configuration directory already exists */
                if (!Directory.Exists(GetSavePath()))
                {
                    Directory.CreateDirectory(GetSavePath());
                }

                FileStream fs = File.Create(GetConfigPath());
                StreamWriter sw = new StreamWriter(fs);

                foreach (string key in KeyList)
                {
                    sw.WriteLine(key + separator);
                }
                sw.Flush();
                sw.Close();
                Console.WriteLine("Konfiguration wurde erfolgreich erstellt.");
            }
            catch (IOException e1)
            {
                Connector.WriteLine("Speichern der Nutzerkonfiguration fehlgeschlagen.");
                Console.WriteLine(e1.ToString());
            }
        }

        private Dictionary<string, string> ReadConfig()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!File.Exists(GetConfigPath()))
            {
                return null;
            }
            else
            {
                List<string> tmp = new List<string>(File.ReadLines(GetConfigPath()));

                foreach (string s in tmp)
                {
                    string[] arr = s.Split('!');
                    dict.Add(arr[0], arr[1]);
                }
                return dict;
            }
        }
    }
}
