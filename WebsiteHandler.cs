using System;
using System.IO;

namespace WebsiteHandlerBackend
{
    class WebsiteHandler
    {
        public string ServerName { get; set; }

        public string Version{ get; } = "1.0.0";
        public UserHandler UHandler { get; }
        public InstallationChecker InstChecker { get; }
        public ToolInstaller Installer { get; }

        public FTPHandler FtpHandler { get; }
        
        public Decryptor Decrypt { get; set; }
        public HandlerLogger Logger { get; }

        public ConsoleTextBlockConnector Connector { get; set; }

        public WebsiteHandler(ConsoleTextBlockConnector c) 
        {
            Connector = c;
            UHandler = new UserHandler(Connector);
            InstChecker = new InstallationChecker(Connector);
            Installer = new ToolInstaller(Connector);

            /* Read in configuration */
            ServerName = UHandler.GetFromConfig("SERVER-NAME");

            /* During Debugging use clear text credentials */
            FtpHandler = new FTPHandler(Connector, ServerName, Decrypt);
            
            Logger = new HandlerLogger();
        }


        

    }
}