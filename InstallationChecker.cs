using System;
using System.IO;

namespace WebsiteHandlerBackend
{
    class InstallationChecker
    {
        public int ErrorCounter { set; get; }
        public int InstallCounter { set; get; } 
        public ConsoleTextBlockConnector Connector;

        private string MobiriseAppDataPath;

        public InstallationChecker(ConsoleTextBlockConnector c) 
        {
            Connector = c; 
            MobiriseAppDataPath = String.Format("C:\\Users\\{0}\\AppData\\Roaming\\Mobirise", Environment.UserName);
        }

        public bool IsGitInstalled()
        {
            /* If Git is installed it should be found in the PATH */
            string myPATH = Environment.GetEnvironmentVariable("PATH");

            if (myPATH.Contains("git") || myPATH.Contains("Git"))
            {
                return true;
            }
            return false;
        }

        public string GetGitVersion()
        {
            if (!IsGitInstalled())
            {
                return "Nicht installiert";
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C git --version";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            StreamReader outReader = process.StandardOutput;
            string gitReply = outReader.ReadLine();
            outReader.Close();

            return gitReply.Substring(gitReply.LastIndexOf(" "));
        }

        public bool IsMobiriseInstalled()
        { 
            if (Directory.Exists(MobiriseAppDataPath))
            {
                return true;
            }

            return false;
        }

        public string GetMobiriseVersion()
        {

            if (!IsMobiriseInstalled())
            {
                return "Nicht installiert";
            }
            return Directory.GetCreationTime(MobiriseAppDataPath).ToString();
        }
    }
}
