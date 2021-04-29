using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebsiteHandlerBackend
{
    class HandlerLogger
    {
        private static readonly string foldername = "WebsiteHandler";
        private static readonly string savePath = string.Format("C:\\Users\\{0}\\Documents\\{1}", Environment.UserName, foldername);
        private static string logFileName = "LOG";
        private string logFilePath = savePath + "\\" + logFileName;
        private string backupString = "<BU-DATE>";

        public HandlerLogger() { }

        public void AppendLog(string s)
        {
            DateTime date = new DateTime();
            date = DateTime.Now;
            if (!File.Exists(savePath))
            {
                File.Create(logFilePath);
            }

            string tmp = File.ReadAllText(logFilePath);
            string textToAdd = tmp + "\r\n" + backupString + date.ToString() + "-->" + s;
            File.WriteAllText(logFilePath, textToAdd);
        }

        public string GetAllContent()
        {
            return File.ReadAllText(logFilePath);
        }

        public string GetLastEntry()
        {
            List<string> lines = new List<string>(File.ReadLines(logFilePath));

            return lines.ElementAt(lines.Count);
        }

        public string GetLastBackupDate()
        {
            List<string> lines = new List<string>(File.ReadLines(logFilePath));

            string latest = "";
            foreach (string line in lines)
            {
                if (line.Contains(backupString))
                {
                    latest = line;
                }
            }

            return latest.Substring(latest.IndexOf(">"), latest.IndexOf("-->"));
        }
    }
}
