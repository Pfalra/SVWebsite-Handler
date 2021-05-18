using System.Windows.Controls;
using System.Windows.Media;

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

        public void WriteErrorLine(string s)
        {
            Brush tmp = TxtBlock.Foreground;
            TxtBlock.Foreground = new SolidColorBrush(Colors.Red);
            WriteLine(s);
            TxtBlock.Foreground = tmp;

        }
    }
}
