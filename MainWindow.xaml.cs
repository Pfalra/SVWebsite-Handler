using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebsiteHandlerBackend;
using System.Windows.Threading;
using WK.Libraries.BetterFolderBrowserNS;

namespace WebsiteHandler_GUI
{
    public partial class MainWindow : Window
    {
        private ConsoleTextBlockConnector ConsoleConnector;
        private string GuiVersion { get; } = "1.0.0";
        private string BackendVersion { get; set; }
        private WebsiteHandler HandlerBackend { get; set; }

        private Canvas ActiveCanvas;
        private string GitVersion { get; set; } = "Nicht installiert";
        private string MobiriseVersion { get; set; } = "Nicht installiert";
        private string UserConfig { get; set; } = "Nicht gesetzt";

        private string WebsiteWorkspace { get; set; } = "";


        public MainWindow() 
        { 
            InitializeComponent();

            ConsoleConnector = new ConsoleTextBlockConnector(ConsoleOutputTextBlock);
            HandlerBackend = new WebsiteHandler(ConsoleConnector);

            InitializeCanvasesContent();
            ActiveCanvas = HomeCanvas;
        }

        private void HomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(HomeCanvas);
        }

        private void ToolInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(ToolInstallCanvas);
        }

        private void BackupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(BackupCanvas);
        }

        private void UserConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(UserConfigCanvas);
        }

        private void LoadCurrentProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(GetCurrentProjectCanvas);
        }

        private void PublishMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(PublishCanvas);
        }

        private void ListFtpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(ListFtpFilesCanvas);
        }

        private void UpdateHandlerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(UpdateHandlerCanvas);
        }

        private void SwitchToCanvas(Canvas c)
        {
            if (c.Equals(ActiveCanvas))
            {
                return;
            }
            
            c.Visibility = Visibility.Visible;
            ActiveCanvas.Visibility = Visibility.Hidden;
            ActiveCanvas = c;
        }

        /************************/
        /* Buttons within cards */
        /************************/

        private void SubmitConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string namestr = FirstNameBox.Text + " " + LastNameBox.Text;
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.UserKey, namestr);
            ConsoleOutputTextBlock.Text += "\r\n--> Userkonfiguration übernommen: " + namestr;
            UserConfig = namestr; 
            UserConfigTextBlock.Text = "Nutzerkonfiguration: " + UserConfig;

            /* Use Init as update method */
            //InitializeHomeCanvas();
            //InitializeUserConfigCanvas();
        }

        private void GitInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HandlerBackend.Installer.InstallGit();
        }

        private void MobiriseInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HandlerBackend.Installer.InstallMobirise();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            BetterFolderBrowser browser = new BetterFolderBrowser();
            browser.Multiselect = false;
            browser.RootFolder = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName);
            browser.Title = "Ordner zur Sicherung";
            browser.ShowDialog();
            PathTextBox.Text = browser.SelectedPath;
        }

        private void WorkspaceBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            BetterFolderBrowser browser = new BetterFolderBrowser();
            browser.Multiselect = false;
            browser.RootFolder = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName);
            browser.Title = "Auswahl des Arbeitsbereichs";
            browser.ShowDialog();
            
            if (browser.SelectedPath != "")
            {
                WorkspaceTextBox.Text = browser.SelectedPath;
                WebsiteWorkspace = browser.SelectedPath;
                HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.WorkspaceKey, browser.SelectedPath);
            }

        }

        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: IMPLEMENT FTP DOWNLOAD */
        }

        private void RefreshFtpList_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: IMPLEMENT FTP PEEK */
        }



        private void ChangesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Set Button active
            if (ChangesTextBox.Text.Length > 15)
            {
                if (!PublishChangesButton.IsEnabled)
                {
                    PublishChangesButton.IsEnabled = true;
                }
            }
            else
            {
                if (PublishChangesButton.IsEnabled)
                {
                    PublishChangesButton.IsEnabled = false;
                }
            }
        }

        /***********************/
        /* Canvas Initializers */
        /***********************/

        private void InitializeCanvasesContent()
        {
            /* HomeCanvas */
            InitializeHomeCanvas();

            /* ToolInstall Canvas */
            InitializeToolInstallCanvas();

            /* Backup Canvas */
            InitializeBackupCanvas();

            /* UserConfig Canvas */
            InitializeUserConfigCanvas();

        }
        
        private void InitializeHomeCanvas()
        {
            WebsiteHandlerGuiVersionTextBlock.Text = "GUI-Version: " + GuiVersion;
            WebsiteHandlerVersionTextBlock.Text = "WebsiteHandler-Version: " + HandlerBackend.Version;

            if (HandlerBackend.UHandler.IsUsernameSet())
            {
                UserConfig = HandlerBackend.UHandler.GetFromConfig("USER");
            }

            UserConfigTextBlock.Text = "Nutzerkonfiguration: " + UserConfig;

            GitVersion = HandlerBackend.InstChecker.GetGitVersion();
            GitVersionTextBlock.Text = "Git-Version: " + GitVersion;

            MobiriseVersion = HandlerBackend.InstChecker.GetMobiriseVersion();
            MobiriseVersionTextBlock.Text = "Mobirise-Version: " + MobiriseVersion;

            WebsiteWorkspace = HandlerBackend.UHandler.GetFromConfig("WORKSPACE");
            WorkspaceTextBox.Text = WebsiteWorkspace;
        }

        private void InitializeToolInstallCanvas()
        {
            if (HandlerBackend.InstChecker.IsGitInstalled())
            {
                GitInstallStatusLabel.Content = "Git ist bereits installiert.";
            }
            else
            {
                GitInstallStatusLabel.Content = "Git ist noch nicht installiert.";
            }

            if (HandlerBackend.InstChecker.IsMobiriseInstalled())
            {
                MobiriseInstallStatusLabel.Content = "Mobirise ist bereits installiert.";
            }
            else
            {
                MobiriseInstallStatusLabel.Content = "Mobirise ist noch nicht installiert.";
            }
        }
    
        private void InitializeUserConfigCanvas()
        {
            if (UserConfig.Contains(" "))
            {
                string firstName = UserConfig.Substring(0, UserConfig.LastIndexOf(" "));
                string lastName = UserConfig.Substring(UserConfig.LastIndexOf(" ") + 1);
                FirstNameBox.Text = firstName;
                LastNameBox.Text = lastName;
            }
        }

        private void InitializeBackupCanvas()
        {
            BackupStatusLabel.Content = "Status: Nutzen Sie den Button um die Sicherung am angegebenen Pfad abzulegen.";
            PathTextBox.Text = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName);
        }

       
    }
}
