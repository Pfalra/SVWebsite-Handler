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
        /* Note: To use token authentication we can use commands like 
         * "git push https://<GITHUB_ACCESS_TOKEN>@github.com/<GITHUB_USERNAME>/<REPOSITORY_NAME>.git" 
         */

        private const string GithubUname = "SGEdelweiss";
        private const string RepoName = "Website";

        private string ProjectURL { get; set; } = "";
        private Decryptor GitCredDecryptor { get; set; } = null;

        /*********************************************************************************************/
        /* Constructors */
        /*********************************************************************************************/
        public GITHandler(string projectURL, Decryptor decryptor = null)
        {
            ProjectURL = projectURL;
            GitCredDecryptor = decryptor;

            if (GitCredDecryptor != null)
            {
                GitCredDecryptor.DecryptContent();
                InsertCredsInUrl(ProjectURL, GitCredDecryptor);
            }
        }


        public GITHandler(string projectURL, UserHandler userHandler)
        {
            ProjectURL = projectURL;

            ProjectURL = InsertPatInUrl(ProjectURL, userHandler.UserAccessKey);
        }

        /*********************************************************************************************/
        /* Basic GIT stuff */
        /*********************************************************************************************/
        public bool PullLatestChanges(string workspaceDir, out string stdOutput, out string stdError)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd";
            startInfo.Arguments = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git pull \"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true; 
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
            startInfo.Arguments = "/C cd /d " + workspaceDir + "\\" + RepoName + "&& git add -A && git commit -a -m \"" + message + "\"";
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

        /*********************************************************************************************/
        /* Validation functions */
        /*********************************************************************************************/
        public bool IsGitRepository(string dir)
        {
            if (!Directory.Exists(dir + "\\" + RepoName))
            {
                return false;
            }

            string[] fileNames = Directory.GetFileSystemEntries(dir + "\\" + RepoName);
            if (Directory.GetFileSystemEntries(dir + "\\" + RepoName).Any(strVal => strVal.EndsWith(".git")))
            {
                return true;
            }

            return false;
        }

        public string GetLastCommitDate(string workspaceDir, out string stdOutput, out string stdError)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd";
            startInfo.Arguments = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git log -1 --format=%ci \"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            stdOutput = process.StandardOutput.ReadToEnd();
            stdError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return stdOutput;
        }

        public string GetLastRemoteCommitDate(string workspaceDir, out string stdOutput, out string stdError)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd";
            startInfo.Arguments = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git log origin -1 --format=%ci \"";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            stdOutput = process.StandardOutput.ReadToEnd();
            stdError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return stdOutput;

        }
        /*********************************************************************************************/
        /* URL manipulation */
        /*********************************************************************************************/
        // TODO: TEST THIS FUNCTION
        private string InsertCredsInUrl(string url, Decryptor dec)
        {
            if (String.IsNullOrEmpty(dec.DecryptedContent))
            {
                throw new Exception("Decryption of content returned null or empty string. \r\n" + dec.CredentialsPath);
            } 
            else
            {
                string protocolStr = ProjectURL.Substring(0, ProjectURL.IndexOf("//")-1);
                string siteStr = ProjectURL.Substring(ProjectURL.IndexOf("//"));
                string credStr = dec.DecryptedContent.Replace(dec.Separator, ':');

                return String.Format("{0}//{1}@{2}", protocolStr, credStr, siteStr);
            }
        }
    

        private string InsertPatInUrl(string url, string pat)
        {
            return String.Format("https://{0}@github.com/{1}/{2}.git", pat, GithubUname, RepoName) ;
        }
    }
}
