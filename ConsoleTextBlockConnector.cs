using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace WebsiteHandlerBackend
{
    class ConsoleTextBlockConnector
    {
        TextBlock TxtBlock;
        public ConsoleTextBlockConnector(TextBlock block)
        {
            TxtBlock = block;
        }

        public void WriteLine(string s)
        {
            TxtBlock.Text += "\r\n" + s;
        }

        public void WriteLine()
        {
            TxtBlock.Text += "\r\n";
        }

        public void ClearConsole()
        {
            TxtBlock.Text = "";
        }
    }
}
