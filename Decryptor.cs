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
        public char Separator { get; set; }

        public Decryptor(string credentialsPath, string keyPath, string encryptionToolPath, char separator = '!') 
        {
            CredentialsPath = credentialsPath;
            KeyPath = keyPath;
            ToolPath = encryptionToolPath;
            Separator = separator;
        }

        // TODO: Fix issue with not finding the Encryptor.exe
        public string DecryptContent()
        {
            bool credsExist = File.Exists(CredentialsPath);
            bool keyExists = File.Exists(KeyPath);
            if (credsExist && keyExists)
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
