using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebsiteHandlerBackend
{
    class Decryptor
    {
        public string DecryptedContent { get; set; }
        public string CredentialsPath { get; set; }
        public string KeyPath { get; set; }
        public string ToolPath { get; set; }

        public Decryptor(string credentialsPath, string keyPath, string encryptionToolPath) 
        {
            CredentialsPath = credentialsPath;
            KeyPath = keyPath;
            ToolPath = encryptionToolPath;            
        }


        public string DecryptContent()
        {
            if (File.Exists(CredentialsPath) && File.Exists(KeyPath))
            {
                string key = File.ReadAllText(KeyPath); // Read the key

                using (System.Diagnostics.Process decryptionProcess = new System.Diagnostics.Process())
                {
                    decryptionProcess.StartInfo.FileName = ToolPath;
                    decryptionProcess.StartInfo.Arguments = "-i " + CredentialsPath + " -k \"" + key + "\"";
                    decryptionProcess.StartInfo.UseShellExecute = false;
                    decryptionProcess.StartInfo.RedirectStandardOutput = true;
                    decryptionProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    decryptionProcess.StartInfo.CreateNoWindow = true;
                    decryptionProcess.Start();
                    string output = decryptionProcess.StandardOutput.ReadToEnd();
                    decryptionProcess.WaitForExit();

                    // Parse output
                    output = output.Substring(output.IndexOf("<DEC-START>"), output.IndexOf("<DEC-END>") - output.IndexOf("<DEC-START>"));
                    DecryptedContent = output;
                }
            }
            else throw new FileNotFoundException("Either the credentials or the key-file cannot be found.");
            return DecryptedContent;
        }

        public string GetParameterValue(string key)
        {
            return "";
        }
    }
}
