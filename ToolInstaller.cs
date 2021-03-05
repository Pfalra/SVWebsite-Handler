using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SVWebsiteHandler
{
    class ToolInstaller
    {
        private const string relPathToLinks = "./ToolLinks.txt";
        private const string gitKey = "<GIT_URL>";
        private const string downloadFolderName = "WebsiteHandler_Tools";
        private const string gitFileName = "GitInstaller.exe";

        public ToolInstaller() { }

        public bool InstallGit()
        {
            Console.WriteLine("Darf die Versionsverwaltungssoftware \'Git\' installiert werden. Bitte bestätigen Sie mit \'J\' oder lehnen Sie mit \'N\' ab.");

            while (true)
            {
                string input = Console.ReadLine();

                if (input.StartsWith('j') || input.StartsWith('J'))
                {
                    break;
                }
                else if (input.StartsWith('n') || input.StartsWith('N'))
                {
                    Console.WriteLine("Sie haben die Installation abgebrochen.");
                    return false;
                }
                else
                {
                    Console.WriteLine("Ungültige Eingabe. Bitte versuchen Sie es erneut.");
                }
            }

            Dictionary<string, string> urlList = readToolLinks();

            if (urlList.Equals(null))
            {
                Console.WriteLine("Es wurde eine leere Tool-Liste gefunden. Installation fehlgeschlagen!");
                return false;
            }

            if (urlList.ContainsKey(gitKey))
            {
                bool success = urlList.TryGetValue(gitKey, out string gitUrl);
                if (success && (gitUrl != null) && (gitUrl != ""))
                {
                    string downloadUrl = ReadGitApi(gitUrl);

                    if (downloadUrl.Equals("") || downloadUrl == null)
                    {
                        return false;
                    }

                    WebClient webClient = new WebClient();
                    webClient.Headers.Add("User-Agent: Other");
                    string savePath = String.Format("C:\\Users\\{0}\\Downloads\\{1}", Environment.UserName, downloadFolderName);

                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    try
                    {
                        Console.WriteLine("Das Tool wird jetzt heruntergerladen. Dies kann einen Moment dauern.");
                        webClient.DownloadFile(downloadUrl, String.Concat(savePath, "\\", gitFileName));
                        Console.WriteLine("Der Download war erfolgreich und wurde im Downloads-Ordner abgelegt.");
                    }
                    catch (WebException we)
                    {
                        Console.WriteLine("Fehler beim Download des Git Installers.");
                        we.ToString();
                        return false;
                    }


                    /* Invoke installation procedure */
                    InvokeToolInstaller(String.Concat(savePath, "\\", gitFileName), "");
                }
            }

            return true;
        }

        private bool InvokeToolInstaller(string pathToExe, string parameters)
        {
            Console.WriteLine("Es wird nun versucht die Installation durchzuführen.");
            Process installer = new Process();
            installer.StartInfo.FileName = pathToExe;
            installer.StartInfo.Arguments = "" + parameters;
            installer.Start();
            /* Process start successfull, now try with parameters */
            /* READ CONSOLE WHILE EXECUTING */
            return true;
        }

        private string ReadGitApi(string url)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent: Other");
            List<string> downloadUrls = new List<string>();

            try
            {
                if ((url != null) && (url != ""))
                {
                    /* Read in the json */
                    string githubApiStr = webClient.DownloadString(url);
                    webClient.Dispose();

                    JsonTextReader reader = new JsonTextReader(new StringReader(githubApiStr));

                    bool downloadUrlPropertyFound = false;
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            string jsonToken = reader.TokenType.ToString();
                            string jsonValue = reader.Value.ToString();

                            if (downloadUrlPropertyFound)
                            {
                                downloadUrls.Add(jsonValue);
                            }

                            if (jsonValue.Contains("browser_download_url"))
                            {
                                downloadUrlPropertyFound = true;
                            } 
                            else
                            {
                                downloadUrlPropertyFound = false;
                            }


                        }
                    }

                    string installUrl = GetGitInstallExe(downloadUrls);

                    return installUrl;
                }

            }
            catch (WebException we)
            {
                Console.WriteLine("Verbindung zur Github API nicht möglich.");
                we.ToString();
            }
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("Der Download der Github Git-API ergab eine leere Zeichenkette");
                ae.ToString();
            }

            return null;
        }


        private string GetGitInstallExe(List<string> urls)
        {
            foreach (string s in urls) {
                if ((s.Contains("windows")) && 
                    (s.Contains("release")) &&
                    (s.Contains("Git-")) &&
                    (s.EndsWith("64-bit.exe")))
                {
                    return s;
                }
            }
            return null;
        }


        private Dictionary<string, string> readToolLinks()
        {
            try
            {
                Dictionary<string, string> retDict = new Dictionary<string, string>();
                string line;
                StreamReader reader = new StreamReader(relPathToLinks);
                
                while((line = reader.ReadLine()) != null)
                {
                    string[] splitLine = line.Split('|');
                    retDict.Add(splitLine[0], splitLine[1]);
                }
                reader.Close();
                return retDict;
            } 
            catch (IOException e)
            {
                Console.WriteLine("Die Textdatei mit den URLs für den Tool-Download konnte nicht gefunden werden!");
                Console.WriteLine(e.ToString());
            }
            return null;
        }
    }
}
