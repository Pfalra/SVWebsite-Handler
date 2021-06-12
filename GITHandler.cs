using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace WebsiteHandlerBackend
{
    class GITHandler
    {
        string ProjectURL { get; set; } = "";
        Decryptor gitCredDecryptor;
               

        public GITHandler(string projectURL)
        {
            ProjectURL = projectURL;

        }

        public bool PullLatestChanges(string workspaceDir, out string stdOutput, out string stdError)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd";
            startInfo.Arguments = "/c \" cd /d \"" + workspaceDir + "\" && git pull \"";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            stdOutput = process.StandardOutput.ReadToEnd();
            stdError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return false;
        }

        public bool CommitPushLatestChanges(string workspaceDir, string message)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C cd /d " + workspaceDir + "&& git add -A && git commit -a -m \"" + message + "\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            return false;
        }

        public bool CloneProject(string destDir, out string stdOutput, out string stdError)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd";
            startInfo.Arguments = "/c \"cd /d \"" + destDir + "\" && git clone " + ProjectURL + "\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            stdOutput = process.StandardOutput.ReadToEnd();
            stdError = process.StandardError.ReadToEnd();

            return false;
        }
    
        public bool IsGitRepository(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return false;
            }

            if (Directory.GetFileSystemEntries(dir).Any(strVal => strVal.CompareTo(".git") == 0))
            {
                return true;
            }

            return false;
        }
    }
}
