using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using WebsiteHandlerBackend;
//using WK.Libraries.BetterFolderBrowserNS;

namespace WebsiteHandler_GUI
{
    public partial class MainWindow : Window
    {
        private ConsoleTextBlockConnector ConsoleConnector;
        private string GuiVersion { get; } = "1.0.0";
        private string BackendVersion { get; set; } = "1.0.0";
        private WebsiteHandler HandlerBackend { get; set; }

        private Canvas ActiveCanvas;
        private string GitVersion { get; set; } = "Nicht installiert";
        private string MobiriseVersion { get; set; } = "Nicht installiert";


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

        // UserConfig Canvas
        private void SubmitConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstNameBox.Text.Trim() == "" || LastNameBox.Text.Trim() == "")
            {
                AppendLineToConsole("Bitte geben Sie einen gültigen Namen ein.");
            }

            if (!Path.IsPathRooted(UserKeyTextBox.Text))
            {
                AppendLineToConsole("Wählen Sie eine gültige User-Key Datei aus.");
            }

            if (!Path.IsPathRooted(BackupPathTextBox.Text))
            {
                AppendLineToConsole("Wählen Sie ein gültiges Backup-Verzeichnis aus.");
            }

            if (!Path.IsPathRooted(HandlerBackend.UHandler.WorkspacePath))
            {
                AppendLineToConsole("Wählen Sie ein gültiges Arbeitsverzeichnis aus.");
            }


            /* Read from TextBoxes */
            HandlerBackend.UHandler.UserName = FirstNameBox.Text + " " + LastNameBox.Text;
            HandlerBackend.UHandler.UserKeyPath = UserKeyTextBox.Text;
            HandlerBackend.UHandler.BackupSpacePath = BackupPathTextBox.Text;

            /* Update on other Canvases */
            UserConfigTextBlock.Text = "Nutzerkonfiguration: " + HandlerBackend.UHandler.UserName;

            /* Edit the Config-File */
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.UserKey, HandlerBackend.UHandler.UserName);
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.UserKeyPathKey, HandlerBackend.UHandler.UserKeyPath);
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.BackupSpaceKey, HandlerBackend.UHandler.BackupSpacePath);

            /* Output on console */
            AppendLineToConsole("--> Userkonfiguration übernommen: " + HandlerBackend.UHandler.UserName);
            AppendLineToConsole("User-Key Pfad: " + HandlerBackend.UHandler.UserKeyPath);
            AppendLineToConsole("Backup Pfad: " + HandlerBackend.UHandler.BackupSpacePath);
            AppendLineToConsole("Arbeitsbereich: " + HandlerBackend.UHandler.WorkspacePath);
            AppendLineToConsole();
        }

        private void UserKey_BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Auswahl der User-Key Datei",
                InitialDirectory = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName),
                Multiselect = false,
                ShowHelp = false,
                Filter = "User-Key Dateien (*.cryp)|*.cryp",
                CheckFileExists = true,
                CheckPathExists = true
            };
            fileDialog.ShowDialog();
            UserKeyTextBox.Text = fileDialog.FileName;
        }
        
        private void BackupBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Auswahl des Sicherungsordners";
            dialog.ShowDialog();

            if (dialog.SelectedPath != "")
            {
                BackupPathTextBox.Text = dialog.SelectedPath;
                HandlerBackend.UHandler.BackupSpacePath = dialog.SelectedPath;
                HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.BackupSpaceKey, dialog.SelectedPath);

            }
        }

        // ToolInstall Canvas
        private void GitInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HandlerBackend.Installer.InstallGit();
        }

        private void MobiriseInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HandlerBackend.Installer.InstallMobirise();
        }

        // Home Canvas 
        private void WorkspaceBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Auswahl des Arbeitsbereichs"
            };
            dialog.ShowDialog();
            
            if (dialog.SelectedPath != "")
            {
                WorkspaceTextBox.Text = dialog.SelectedPath;
                HandlerBackend.UHandler.WorkspacePath = dialog.SelectedPath;
                HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.WorkspaceKey, dialog.SelectedPath);
            }
        }

        // Backup Canvas
        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: IMPLEMENT FTP DOWNLOAD */
            // Retrieve User-Key from File

            // Decrypt Server Credentials

        }

        // Show FTP Files Canvas
        private void RefreshFtpList_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: IMPLEMENT FTP PEEK */
        }

        // Publish Canvas
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

            GitVersion = HandlerBackend.InstChecker.GetGitVersion();
            GitVersionTextBlock.Text = "Git-Version: " + GitVersion;

            MobiriseVersion = HandlerBackend.InstChecker.GetMobiriseVersion();
            MobiriseVersionTextBlock.Text = "Mobirise-Version: " + MobiriseVersion;


            string tmpUsername = HandlerBackend.UHandler.UserName;
            if (HandlerBackend.UHandler.IsConfigExistent())
            {
                if (HandlerBackend.UHandler.IsUsernameSet())
                {
                    HandlerBackend.UHandler.UserName = HandlerBackend.UHandler.GetFromConfig(HandlerBackend.UHandler.UserKey);
                    UserConfigTextBlock.Text = "Nutzerkonfiguration: " + HandlerBackend.UHandler.UserName;

                    HandlerBackend.UHandler.WorkspacePath = HandlerBackend.UHandler.GetFromConfig(HandlerBackend.UHandler.WorkspaceKey);
                    WorkspaceTextBox.Text = HandlerBackend.UHandler.WorkspacePath;
                }
            }
            else
            {
                ConsoleOutputTextBlock.Text += "\r\nEs konnte keine Nutzerkonfiguration gefunden werden\r\nBitte erstellen Sie eine.";
                UserConfigTextBlock.Text = "Nutzerkonfiguration: Nicht gesetzt";
            }

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
            if (HandlerBackend.UHandler.UserName.Contains(" "))
            {
                string firstName = HandlerBackend.UHandler.UserName.Substring(0, HandlerBackend.UHandler.UserName.LastIndexOf(" "));
                string lastName = HandlerBackend.UHandler.UserName.Substring(HandlerBackend.UHandler.UserName.LastIndexOf(" ") + 1);
                FirstNameBox.Text = firstName;
                LastNameBox.Text = lastName;
            } else
            {
                FirstNameBox.Text = HandlerBackend.UHandler.UserName;
            }

            if (HandlerBackend.UHandler.IsConfigExistent())
            {
                HandlerBackend.UHandler.UserKeyPath = HandlerBackend.UHandler.GetFromConfig(HandlerBackend.UHandler.UserKeyPathKey);
                HandlerBackend.UHandler.BackupSpacePath = HandlerBackend.UHandler.GetFromConfig(HandlerBackend.UHandler.BackupSpaceKey);

                UserKeyTextBox.Text = HandlerBackend.UHandler.UserKeyPath;
                BackupPathTextBox.Text = HandlerBackend.UHandler.BackupSpacePath;
            }
        }

        private void InitializeBackupCanvas()
        {
            BackupStatusLabel.Content = "Status: Nutzen Sie den Button um die Sicherung am angegebenen Pfad abzulegen.";
            BackupPathTextBox.Text = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName);
        }


        /* OTHERS */
        private void AppendLineToConsole()
        {
            ConsoleOutputTextBlock.Text += "\r\n";
        }

        private void AppendLineToConsole(string s)
        {
            ConsoleOutputTextBlock.Text += "\r\n" + s;
        }

        private void AppendToConsole(string s)
        {
            ConsoleOutputTextBlock.Text += s;
        }
    }
}
