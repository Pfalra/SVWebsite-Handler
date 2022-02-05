using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;


namespace WebsiteHandlerBackend
{
    class ToolInstaller
    {
        ConsoleTextBlockConnector Connector { get; set; } = null;

        private const string relPathToLinks = "./ToolLinks.txt";

        private const string gitKey = "<GIT_URL>";
        private const string mobiKey = "<MOBIRISE_URL>";

        private const string downloadFolderName = "WebsiteHandler_Tools";

        private const string mobiInstallerName = "MobiriseInstaller.exe";

        private const string gitFileName = "GitInstaller.exe";
        private const string gitInfName = "gitconfig.inf";

        public ToolInstaller(ConsoleTextBlockConnector c) 
        {
            Connector = c;
        }

        /* Mobirise things */
        public bool InstallMobirise()
        {
            Dictionary<string, string> toolLinks = ReadToolLinks();

            if (toolLinks.Equals(null))
            {
                Connector.WriteLine("Es wurde eine leere Tool-Liste gefunden. Installation fehlgeschlagen!");
                return false;
            }

            if (toolLinks.ContainsKey(mobiKey))
            {
                bool success = toolLinks.TryGetValue(mobiKey, out string mobiUrl);
                if (success && (mobiUrl != null) && (mobiUrl != ""))
                {
                    WebClient downloader = new WebClient();
                    downloader.Headers.Add("User-Agent: Other");
                    string downloadPath = String.Format("C:\\Users\\{0}\\Downloads\\{1}", Environment.UserName, downloadFolderName);

                    if (!Directory.Exists(downloadPath))
                    {
                        Directory.CreateDirectory(downloadPath);
                    }

                    try
                    {
                        Connector.WriteLine("Das Tool wird jetzt heruntergeladen. Dies kann einen Moment dauern.");
                        downloader.DownloadFile(mobiUrl, String.Concat(downloadPath, "\\", mobiInstallerName));
                        Connector.WriteLine("Der Download war erfolgreich und wurde im Downloads-Ordner abgelegt.");
                    }
                    catch (WebException we)
                    {
                        Connector.WriteLine("Fehler beim Download des Installers.\r\n");
                        Connector.WriteLine(we.ToString());
                        return false;
                    }

                    /* Invoke installation procedure */
                    bool installSuccess = InvokeToolInstaller(downloadPath + "\\" + mobiInstallerName);

                    if (installSuccess)
                    {
                        return true;
                    } 
                }
            }

            return false;
        }


        /* Git things */
        public bool InstallGit()
        {
            Dictionary<string, string> urlList = ReadToolLinks();

            if (urlList == null)
            {
                Connector.WriteLine("Es wurde eine leere Tool-Liste gefunden. Installation fehlgeschlagen!");
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
                    string downloadPath = String.Format("C:\\Users\\{0}\\Downloads\\{1}", Environment.UserName, downloadFolderName);

                    if (!Directory.Exists(downloadPath))
                    {
                        Directory.CreateDirectory(downloadPath);
                    }

                    try
                    {
                        Thread.Sleep(1000);
                        webClient.DownloadFile(downloadUrl, String.Concat(downloadPath, "\\", gitFileName));
                        Connector.WriteLine("Der Download war erfolgreich und wurde im Downloads-Ordner abgelegt.");
                    }
                    catch (WebException we)
                    {
                        Connector.WriteLine("Fehler beim Download des Installers.");
                        Connector.WriteLine(we.ToString());
                        return false;
                    }


                    /* Invoke installation procedure */
                    string execPath = System.Reflection.Assembly.GetExecutingAssembly().Location; // Get the path of the current executable
                    string execDirectory = System.IO.Path.GetDirectoryName(execPath);
                    string gitInfPath = execDirectory + "\\" + gitInfName;


                    bool installSuccess = InvokeToolInstaller(
                        String.Concat(downloadPath, "\\", gitFileName), 
                        String.Format("/SP- /VERYSILENT /SUPPRESSMSGBOXES /NOCANCEL /NORESTART /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS /LOADINF={0}", gitInfPath)
                        );

                    if (installSuccess)
                    {
                        // CLEAN-UP: Remove the installer
                        foreach (string filename in Directory.GetFiles(downloadPath + "\\"))
                        {
                            File.Delete(filename);
                        }
                        Directory.Delete(downloadPath);
                        return true;
                    }
                }
            }
            else
            {
                Connector.WriteLine("Es wurde kein Link zum Download von Git gefunden.");
                return false;
            }

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
                Connector.WriteLine("Verbindung zur Github API nicht möglich.");
                Connector.WriteLine(we.ToString());
            }
            catch (ArgumentNullException ae)
            {
                Connector.WriteLine("Der Download der Github Git-API ergab eine leere Zeichenkette");
                Connector.WriteLine(ae.ToString());
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


        private bool InvokeToolInstaller(string pathToExe, string parameters)
        {
            Connector.WriteLine("Es wird nun versucht die Installation durchzuführen.");
            Process installer = new Process();
            installer.StartInfo.FileName = pathToExe;
            installer.StartInfo.Arguments = parameters;
            installer.StartInfo.RedirectStandardOutput = true;
            installer.StartInfo.RedirectStandardError = true;
            installer.StartInfo.UseShellExecute = false;

            installer.Start();

            StreamReader outReader = installer.StandardOutput;
            StreamReader errReader = installer.StandardError;
            string stdoutput = outReader.ReadToEnd();
            string erroutput = errReader.ReadToEnd();


            Connector.WriteLine("\r\n---------- Ausgaben des Installers ----------");

            Connector.WriteLine("----> Standardausgabe: ");
            Connector.WriteLine(stdoutput);

            Connector.WriteLine("----> Fehlerausgabe: ");
            Connector.WriteLine(erroutput);

            Connector.WriteLine("---------------------------------------------\r\n");
            outReader.Close();
            errReader.Close();
            installer.WaitForExit();
            return true;
        }
        
        private bool InvokeToolInstaller(string pathToExe)
        {
            Connector.WriteLine("Es wird nun versucht die Installation durchzuführen.");
            Process installer = new Process();
            installer.StartInfo.FileName = pathToExe;
            installer.StartInfo.RedirectStandardOutput = true;
            installer.StartInfo.RedirectStandardError = true;
            installer.StartInfo.UseShellExecute = false;
            installer.Start();

            StreamReader outReader = installer.StandardOutput;
            StreamReader errReader = installer.StandardError;
            string stdoutput = outReader.ReadToEnd();
            string erroutput = errReader.ReadToEnd();


            Connector.WriteLine("\r\n---------- Ausgaben des Installers ----------");

            Connector.WriteLine("----> Standardausgabe: ");
            Connector.WriteLine(stdoutput);

            Connector.WriteLine("----> Fehlerausgabe: ");
            Connector.WriteLine(erroutput);

            Connector.WriteLine("---------------------------------------------\r\n");
            outReader.Close();
            errReader.Close();
            installer.WaitForExit();
            return true;
        }

        private Dictionary<string, string> ReadToolLinks()
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
                Connector.WriteLine("Die Textdatei mit den URLs für den Tool-Download konnte nicht gefunden werden!");
                Connector.WriteLine(e.ToString());
            }
            return null;
        }
    
        public void CleanUp()
        {
            if (Directory.Exists(downloadFolderName))
            {
                Directory.Delete(downloadFolderName);
            }
        }
    }
}
