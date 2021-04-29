using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebsiteHandlerBackend
{
    class WatermarkRemover
    {
        private string[] MobiriseWatermarkStrings { get; } = { "Visit site", "Mobirise website maker" };
        private string[] WordpressWatermarkStrings { get; } = { };
        private ConsoleTextBlockConnector Connector { get; set; }

        public WatermarkRemover(ConsoleTextBlockConnector c)
        {
            Connector = c;
        }

        public void RemoveWatermarks(string pathToFile)
        {
            Connector.WriteLine("Wasserzeichen werden entfernt.");
            if (!File.Exists(pathToFile))
            {
                return;
            }

            FileStream fs = new FileStream(pathToFile, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
                // TODO: WATCH OUT. CANT READ AND WRITE AT THE SAME TIME
            string readLine = "";

            while(readLine != null)
            {
                readLine = sr.ReadLine();
            }

        }
    }
}
