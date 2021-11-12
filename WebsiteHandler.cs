using System;
using System.IO;

namespace WebsiteHandlerBackend
{
    class WebsiteHandler
    {

        /*********************************************************************************************/
        /* WebsiteHandler variables (purpose: making updates of the handler and store general 
         * config-filepaths ) */
        /*********************************************************************************************/
        private string handlerKey = "HANDLER";
        private string websiteKey = "WEBSITE";

        public string Version { get; } = "1.0.0";
        public string DefaultServerCredsPath { get; } = ".\\sc.enc";
        public string DefaultGitCredsPath { get; } = ".\\gc.enc";
        public string DefaultEncryptorFile { get; } = ".\\Encryptor.exe";
        public string WebsiteRepoLink { get; set; } = "";
        public string HandlerRepoLink { get; set; } = "";

        public string ServerName { get; set; } = "";

        /*********************************************************************************************/
        /* Handlers */
        /*********************************************************************************************/
        public UserHandler UHandler { get; }
        public InstallationChecker InstChecker { get; }
        public ToolInstaller Installer { get; }

        public FTPHandler FtpHandler { get; }
        public GITHandler GitHandler { get; }

        /*********************************************************************************************/
        /* DEPRECATED: Decryptors for usage of credentials stored locally */
        /*********************************************************************************************/
        public Decryptor ServerCredDecryptor { get; set; }
        public Decryptor GitCredDecryptor { get; set; }

        /*********************************************************************************************/
        /* Logger for the case of debugging */
        /*********************************************************************************************/
        public HandlerLogger Logger { get; }

        /*********************************************************************************************/
        /* Connector to print something to the main window */
        /*********************************************************************************************/
        public ConsoleTextBlockConnector Connector { get; set; }

        /*********************************************************************************************/
        /* Constructor */ 
        /*********************************************************************************************/
        public WebsiteHandler(ConsoleTextBlockConnector c) 
        {
            Connector = c;
            UHandler = new UserHandler(Connector);
            InstChecker = new InstallationChecker(Connector);
            Installer = new ToolInstaller(Connector);

            if (UHandler.IsConfigExistent())
            {
                string userKeyPath = UHandler.GetFromConfig(UHandler.UserKeyPathKey);

                if (userKeyPath == null || userKeyPath == "")
                {
                    Connector.WriteErrorLine(">> Es wurden noch nicht genügend Informationen angegeben, um die Anmeldedaten für den Server zu entschlüsseln!");
                } else
                {
                    ServerCredDecryptor = new Decryptor(DefaultServerCredsPath, userKeyPath, DefaultEncryptorFile);
                    GitCredDecryptor = new Decryptor(DefaultGitCredsPath, userKeyPath, DefaultEncryptorFile);
                }

            }


            /* Read in configuration */
            //ServerName = UHandler.GetFromConfig("SERVER-NAME");

            //FtpHandler = new FTPHandler(Connector, ServerName, Decrypt);
            
            Logger = new HandlerLogger();


            ReadRepositoryLinks();
        }


        private void ReadRepositoryLinks()
        {
            FileInfo fi = new FileInfo(".\\repos");
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

            StreamReader sr = new StreamReader(fs);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] kvp = line.Split('!');
                
                if (kvp[0].CompareTo(handlerKey) == 0)
                {
                    HandlerRepoLink = kvp[1];
                }

                if (kvp[0].CompareTo(websiteKey) == 0)
                {
                    WebsiteRepoLink = kvp[1];
                }
            }
        }
    }
}