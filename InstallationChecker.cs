using System;
using System.IO;

namespace SVWebsiteHandler
{
    class InstallationChecker
    {
        public int ErrorCounter { set; get; }
        public int InstallCounter { set; get; }

        public InstallationChecker() { }

        public bool CheckSetup()
        {

            UserHandler uHandler = new UserHandler();
            /* Check if username was set */
            if (!uHandler.isUsernameSet())
            {
                uHandler.setupUser();
            }

            /* Check if Git is installed */
            if (!IsGitInstalled())
            {
                ToolInstaller toolInstaller = new ToolInstaller();
                bool installSuccess = toolInstaller.InstallGit();

                if (installSuccess)
                {
                    Console.WriteLine("Git wurde erfolgreich installiert.");
                    InstallCounter++;
                }
                else
                {
                    ErrorCounter++;
                    Console.WriteLine("Bei der Installation von Git ist ein Fehler aufgetreten.");
                }   
            }

            /* Check if Mobirise is installed */
            if (!IsMobiriseInstalled())
            {
                ToolInstaller toolInstaller = new ToolInstaller();
                bool installSuccess = toolInstaller.InstallMobirise();

                if (installSuccess)
                {
                    Console.WriteLine("Mobirise wurde erfolgreich installiert.");
                    InstallCounter++;
                }
                else
                {
                    ErrorCounter++;
                    Console.WriteLine("Bei der Installation von Mobirise ist ein Fehler aufgetreten");
                }
            } 
            else
            {
                Console.WriteLine("Mobirise ist bereits installiert");
            }

            if (ErrorCounter > 0)
            {
                return false;
            }

            return true;
        }


        private bool IsGitInstalled()
        {
            /* If Git is installed it should be found in the PATH */
            string myPATH = Environment.GetEnvironmentVariable("PATH");

            // FOR DEBUG PURPOSES
            //Console.WriteLine();
            //Console.Error.WriteLine("Aktuelle PATH-Variable: {0}", myPATH);
            //Console.Error.WriteLine();
            if (myPATH.Contains("git") || myPATH.Contains("Git"))
            {
                Console.WriteLine("Git ist bereits installiert.");
                return true;
            }
            Console.WriteLine("Git ist noch nicht installiert!");
            return false;
        }

        private bool IsMobiriseInstalled()
        {
            //C: \Users\Raphael\AppData\Roaming\Mobirise

            string mobiriseAppDataPath = String.Format("C:\\Users\\{0}\\AppData\\Roaming\\Mobirise", Environment.UserName);

            if (Directory.Exists(mobiriseAppDataPath))
            {
                return true;
            }

            return false;
        }


    }
}
