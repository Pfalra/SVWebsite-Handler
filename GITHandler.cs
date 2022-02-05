using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebsiteHandlerBackend
{
    class GITHandler
    {
        /* Note: To use token authentication we can use commands like 
         * "git push https://<GITHUB_ACCESS_TOKEN>@github.com/<GITHUB_USERNAME>/<REPOSITORY_NAME>.git" 
         */

        private const string GithubUname = "SGEdelweiss";
        public string RepoName = "";

        private string ProjectURLAndPAT { get; set; } = "";
        private string ProjectURL { get; set; } = "";
        private Decryptor GitCredDecryptor { get; set; } = null;

        /*********************************************************************************************/
        /* Constructors */
        /*********************************************************************************************/
        public GITHandler(string projectURL, Decryptor decryptor = null)
        {
            ProjectURLAndPAT = projectURL;
            GitCredDecryptor = decryptor;

            if (GitCredDecryptor != null)
            {
                GitCredDecryptor.DecryptContent();
                InsertCredsInUrl(ProjectURLAndPAT, GitCredDecryptor);
            }
        }


        public GITHandler(string projectURL, UserHandler userHandler)
        { 
            ProjectURL = projectURL;
            ProjectURLAndPAT = projectURL;

            if (!projectURL.Contains(".git") || !projectURL.Contains('/'))
            {
                /* repos file is broken */
                // TODO: Find a proper way to handle such thing

                ProjectURLAndPAT = "REPOS FILE BROKEN";
            }
            else
            {
                RepoName = projectURL;
                int start = RepoName.LastIndexOf('/')+1;
                int end = RepoName.LastIndexOf(".git");
                int len = RepoName.Length;
                RepoName = RepoName.Substring(start, end-start);
                ProjectURLAndPAT = InsertPatInUrl(ProjectURLAndPAT, userHandler.UserAccessKey);
            }

        }

        /*********************************************************************************************/
        /* Basic GIT stuff */
        /*********************************************************************************************/
        public void PullLatestChanges(string workspaceDir, out string stdOutput, out string stdError)
        {
            string argStr = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git pull \"";
            StartGitProcess(argStr, out stdOutput, out stdError);
        }


        public void PushLatestChanges(string workspaceDir, out string stdOutput, out string stdError)
        {
            string argStr = "/C cd /d " + workspaceDir + "\\" + RepoName + "&& git push";
            StartGitProcess(argStr, out stdOutput, out stdError);
        }


        public void CloneProject(string destDir, out string stdOutput, out string stdError)
        {
            string argStr = "/c \"cd /d \"" + destDir + "\" && git clone " + ProjectURLAndPAT + "\"";
            StartGitProcess(argStr, out stdOutput, out stdError);
        }


        public void CommitLatestChanges(string workspaceDir, string message, out string stdOutput, out string stdError)
        {
            string argStr = "/C cd /d " + workspaceDir + "\\" + RepoName + "&& git add -A && git commit -a -m \"" + message + "\"";
            StartGitProcess(argStr, out stdOutput, out stdError);
        }
        

        public void RemoveChanges(string workspaceDir, out string stdOutput, out string stdError)
        {
            string argStr = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git clean -f\"";
            StartGitProcess(argStr, out stdOutput, out stdError);
        }
        
        
        public bool ChangesAvailable(string workspaceDir, out string stdOutput, out string stdError)
        {
            string argStr = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git status --short \"";
            StartGitProcess(argStr, out stdOutput, out stdError);
            string[] outArr = stdOutput.Split('\n');
            if (outArr.Length >= 1 && outArr[0].Length > 1)
            {
                return true;
            }

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
            string argStr = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git log -1 --format=%ci \"";
            StartGitProcess(argStr, out stdOutput, out stdError);
            return FormatCommitDate(stdOutput);
        }


        public string GetLastRemoteCommitDate(string workspaceDir, out string stdOutput, out string stdError)
        {
            string argStr = "/c \" cd /d \"" + workspaceDir + "\\" + RepoName + "\" && git fetch && git log origin -1 --format=%ci \"";
            StartGitProcess(argStr, out stdOutput, out stdError);
            return FormatCommitDate(stdOutput);
        }


        public int GetLocalNumberOfCommits(out string stdOutput, out string stdError, string workspace = "")
        {
            int result = -1;
            string executableDirName = Assembly.GetExecutingAssembly().Location;

            string argStr;

            if (workspace == "")
            {
                argStr = "/c \" cd /d \"" + executableDirName + "\\" + RepoName + "\" && git rev-list --count --all\"";
            } else
            {
                argStr = "/c \" cd /d \"" + workspace + "\\" + RepoName + "\" && git rev-list --count --all\"";
            }

            StartGitProcess(argStr, out stdOutput, out stdError);

            if (!String.IsNullOrEmpty(stdOutput) && int.TryParse(stdOutput, out result))
            {
                return result;
            }

            return -1;
        }


        public int GetRemoteNumberOfCommits(out string stdOutput, out string stdError, string workspace = "")
        {
            int result = -1;
            string executableDirName = Assembly.GetExecutingAssembly().Location;

            string argStr;
            if (workspace == "")
            {
                argStr = "/c \" cd /d \"" + executableDirName + "\\" + RepoName + "\" && git fetch && git rev-list origin --count --all\"";
            }
            else
            {
                argStr = "/c \" cd /d \"" + workspace + "\\" + RepoName + "\" && git fetch && git rev-list origin --count --all\"";
            }

            StartGitProcess(argStr, out stdOutput, out stdError);

            if (!String.IsNullOrEmpty(stdOutput) && int.TryParse(stdOutput, out result))
            {
                return result;
            }

            return -1;
        }


        /* Count the amount of commits first locally and then on the remote 
         * and calculates local commits - remote commits.
         * Returns by how many commits the local repo is ahead or behind the remote.
         */
        public int GetCommitCountDifference(out string stdOutput, out string stdError, string workspace = "")
        {
            string executableDirName = Assembly.GetExecutingAssembly().Location;

            string argStr;

            if (workspace == "")
            {
                argStr = "/c \" cd /d \"" + executableDirName + "\\" + RepoName + "\" && git rev-list --count --all && git rev-list origin --count --all\"";
            }
            else
            {
                argStr = "/c \" cd /d \"" + workspace + "\\" + RepoName + "\" && git rev-list --count --all && git rev-list origin --count --all\"";
            }

            StartGitProcess(argStr, out stdOutput, out stdError);

            if (String.IsNullOrEmpty(stdOutput))
            {
                stdError += "\r\n>> Commit Anzahl konnte nicht ermittelt werden.";
                return -1;
            }

            /* Split the output into lines and read the commit count */
            string[] temp = Regex.Split(stdOutput, "\r\n|\r|\n");
            int.TryParse(temp[0], out int localCommitCount);
            int.TryParse(temp[1], out int remoteCommitCount);

            return localCommitCount - remoteCommitCount;
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
                string protocolStr = ProjectURLAndPAT.Substring(0, ProjectURLAndPAT.IndexOf("//")-1);
                string siteStr = ProjectURLAndPAT.Substring(ProjectURLAndPAT.IndexOf("//"));
                string credStr = dec.DecryptedContent.Replace(dec.Separator, ':');

                return String.Format("{0}//{1}@{2}", protocolStr, credStr, siteStr);
            }
        }
    

        private string InsertPatInUrl(string url, string pat)
        {
            return String.Format("https://{0}@github.com/{1}/{2}.git", pat, GithubUname, RepoName) ;
        }


        /*********************************************************************************************/
        /* Date manipulation */
        /*********************************************************************************************/
        private string FormatCommitDate(string dateStr)
        {
            if (String.IsNullOrEmpty(dateStr))
            {
                return "";
            }

            string[] tmpStr = dateStr.Split(' '); /* Get each entity from the git output */
            string date = tmpStr[0];   
            string time = tmpStr[1];
            string timezone = tmpStr[2];

            /* Time is represented the right way. Date must be formatted to dd.MM.yyyy*/
            tmpStr = date.Split('-');
            date = String.Format("{0}.{1}.{2}", tmpStr[2], tmpStr[1], tmpStr[0]);

            return String.Format("{0} {1}", date, time);
        }

        /*********************************************************************************************/
        /* Path manipulation */
        /*********************************************************************************************/
        private string FormatPathForGit(string path)
        {
            string retVal = path.Replace('\\', '/');
            retVal = retVal.Remove(':');
            return retVal;
        }

        /*********************************************************************************************/
        /* Process generation and starting */
        /*********************************************************************************************/
        private void StartGitProcess(string argStr, out string stdout, out string stderr)
        {
            if (!String.IsNullOrEmpty(argStr))
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.FileName = "cmd";
                startInfo.Arguments = argStr;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                process.StartInfo = startInfo;
                process.Start();

                stdout = process.StandardOutput.ReadToEnd();
                stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
            } else
            {
                stdout = "";
                stderr = "";
            }

        }


        /*********************************************************************************************/
        /* Browser things */
        /*********************************************************************************************/
        public void OpenRepoInBrowser()
        {
            Process.Start(ProjectURL);
        }
    }
}
