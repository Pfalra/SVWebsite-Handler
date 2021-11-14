using System;
using System.Collections.Generic;
using System.IO;

namespace WebsiteHandlerBackend
{
    class UserHandler
    {

        /*********************************************************************************************/
        /* Keys within the dictionary */
        /*********************************************************************************************/
        public string UserKey { get; } = "USER";
        public string WorkspaceKey { get; } = "WORKSPACE";
        public string BackupSpaceKey { get; } = "BACKUP";
        public string UserKeyPathKey { get; } = "USERKEYPATH";

        /*********************************************************************************************/
        /* Values within the dictionary */
        /*********************************************************************************************/
        public string UserName { get; set; } = "";
        public string WorkspacePath { get; set; } = "";
        public string BackupSpacePath { get; set; } = "";
        public string UserKeyPath { get; set; } = "";
        public string UserAccessKey { get; set; } = "";

        /*********************************************************************************************/
        /* General static values */
        /*********************************************************************************************/
        private const string separator = "!";
        private const string foldername = "WebsiteHandler";
        private const string filename = "cfg.txt";
        private readonly string savePath = String.Format("C:\\Users\\{0}\\Documents\\{1}", 
                                                         Environment.UserName, 
                                                         foldername);

        /*********************************************************************************************/
        /* List for all Keys within the dicitonary */
        /*********************************************************************************************/
        private List<string> KeyList { get; set; } = new List<string>();


        /*********************************************************************************************/
        /* Connector to print something out on a console */
        /*********************************************************************************************/
        ConsoleTextBlockConnector Connector;

        /*********************************************************************************************/
        /* Constructor */
        /*********************************************************************************************/
        public UserHandler(ConsoleTextBlockConnector c) 
        {
            Connector = c;
            KeyList.Add(UserKey);
            KeyList.Add(WorkspaceKey);
            KeyList.Add(BackupSpaceKey);
            KeyList.Add(UserKeyPathKey);

            if (!String.IsNullOrEmpty(UserKeyPath) && File.Exists(UserKeyPath))
            {
                UserAccessKey = ReadUserAccessKey(UserKeyPath);
            }
        }

        /*********************************************************************************************/
        /* Getters */
        /*********************************************************************************************/
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

        /*********************************************************************************************/
        /* User config validation */
        /*********************************************************************************************/
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

            string username = GetFromConfig(UserKey);

            if (username.Trim() == "")
            {
                Connector.WriteLine();
                Connector.WriteErrorLine("Ein leerer Nutzername wurde gesetzt. Bitte ändern Sie die Konfiguration!");
            }
            else
            {
                Connector.WriteLine();
                Connector.WriteLine("Valider Nutzername ist gesetzt: " + username);
            }

            return true;
        }

        public bool IsConfigExistent()
        {
            Dictionary<string, string> map = ReadConfig();
            if (map == null)
            {
                return false;
            }
            return true;
        }

        /*********************************************************************************************/
        /* Manipulation of the user config file and r/w operations */
        /*********************************************************************************************/
        public void DeleteUserInfo()
        {
            File.Delete(savePath  + "\\" + filename);
            Directory.Delete(savePath);
        }

        public void DisplayPathVar()
        {
            Connector.WriteLine(Environment.GetEnvironmentVariable("PATH"));
        }
    
        public string ReadUserAccessKey(string path)
        {
            if (path.Trim().Length == 0 || path == null || !Path.IsPathRooted(path))
            {
                /* Invalid Path */
                return null;
            }
            else if  (File.Exists(path))
            {
                /* File does not exist */
                return null;
            }
            else
            {
                string retval = File.ReadAllText(path);

                if (retval == null || retval.Trim().Length == 0)
                {
                    /* Invalid User key */
                    return null;
                }

                if (retval.Trim().Length > 64)
                {
                    /* Key to long */
                    return null;
                }

                return retval;
            }
        }

        public string GetFromConfig(string key)
        {
            Dictionary<string, string> map = ReadConfig();

            if (map == null)
            {
                return "";
            }

            if (map.TryGetValue(key, out string retVal))
            {
                return retVal;
            }

            return "";
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
                    if (key.CompareTo(kvp.Key) == 0)
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
