using System;
using System.IO;

namespace SVWebsiteHandler
{
    class InstallationChecker
    {
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
                }
                else
                {
                    Console.WriteLine("Bei der Installation von Git ist ein Fehler aufgetreten. Bitte starten Sie den WebsiteHandler neu.");
                    return false;
                }

                
            }


            return false;
        }


        private bool IsGitInstalled()
        {
            /* If Git is installed it should be found in the PATH */
            string myPATH = Environment.GetEnvironmentVariable("PATH");
            Console.WriteLine();
            Console.Error.WriteLine("Aktuelle PATH-Variable: {0}", myPATH);
            Console.Error.WriteLine();
            if (myPATH.Contains("git") || myPATH.Contains("Git"))
            {
                Console.WriteLine("Git ist bereits installiert.");
                return true;
            }
            Console.WriteLine("Git ist nocht nicht installiert!");
            return false;
        }


    }
}
