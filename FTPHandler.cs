using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace WebsiteHandlerBackend
{
    class FTPHandler
    {
        private FtpWebRequest request;
        private NetworkCredential cred;
        private ConsoleTextBlockConnector connector;


        public FTPHandler(ConsoleTextBlockConnector conn, string server, string username, string pwd)
        {
            request = (FtpWebRequest)WebRequest.Create(String.Format("ftp://{0}", server));
            cred = new NetworkCredential(username, pwd);

            request.Credentials = cred;
            request.KeepAlive = true; // Keep the connection alive
            request.UseBinary = true;
            request.EnableSsl = false;
            request.UsePassive = true;
            connector = conn;
        }

        public FTPHandler(ConsoleTextBlockConnector conn, string server, Decryptor credDecryptor)
        {
            request = (FtpWebRequest)WebRequest.Create(String.Format("ftp://{0}", server));
            //cred = new NetworkCredential(credDecryptor.GetUsername(), credDecryptor.GetPassword());
            connector = conn;
        }

        public void HandleDownload(string destinationPath) 
        {

        }

        public void DisplayFtpDirectoryContent()
        {
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader myReader = new StreamReader(responseStream);

            Console.WriteLine(myReader.ReadToEnd());
        }

        public List<string> GetFtpDirectoryContent(bool displayInConsole)
        {
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader myReader = new StreamReader(responseStream);

            string responseContent = myReader.ReadToEnd();
            
            if (displayInConsole)
            {
                Console.WriteLine(responseContent);
            }

            myReader.Close();
            response.Close();

            return null;
        }

        public void HandleUpload() 
        {

        }

        public void Kill()
        {
            request.Abort();
            request.KeepAlive = false;
            cred = null;
            request = null;
        }

    }
}
