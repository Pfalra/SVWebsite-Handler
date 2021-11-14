using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Globalization;
using WebsiteHandlerBackend;
using System.Windows.Media;

namespace WebsiteHandler_GUI
{
    public partial class MainWindow : Window
    {
        /*********************************************************************************************/
        /* Variables and Attributes */
        /*********************************************************************************************/
        /* ConsoleConnector makes a connection from another object to the console within this window */
        private ConsoleTextBlockConnector ConsoleConnector;

        /* Static version information */
        private string GuiVersion { get; } = "1.0.0";
        private string BackendVersion { get; set; } = "1.0.0";

        /* User dependent version information */
        private string GitVersion { get; set; } = "Nicht installiert";
        private string MobiriseVersion { get; set; } = "Nicht installiert";

        /* Backend Object */
        private WebsiteHandler HandlerBackend { get; set; }

        /* Active canvas object for switching */
        private Canvas ActiveCanvas;

        /* Project information */
        private string ProjectStatus { get; set; } = "Kein Projekt gefunden!";
        private string DefaultDateString { get; } = "dd.mm.yyyy hh:mm";


        /*********************************************************************************************/
        /* Constructor */
        /*********************************************************************************************/
        public MainWindow() 
        { 
            InitializeComponent();
            ConsoleOutputTextBlock.TextTrimming = TextTrimming.None;
            ConsoleOutputTextBlock.TextWrapping = TextWrapping.Wrap;

            ConsoleConnector = new ConsoleTextBlockConnector(ConsoleOutputTextBlock);
            HandlerBackend = new WebsiteHandler(ConsoleConnector);

            InitializeCanvasesContent();
            ActiveCanvas = HomeCanvas;
        }

        /*********************************************************************************************/
        /* Button Actions */
        /*********************************************************************************************/
        private void HomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(HomeCanvas);
        }

        private void ToolInstallMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(ToolInstallCanvas);
        }


        private void UserConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(UserConfigCanvas);
        }

        private void LoadCurrentProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InitializeGetCurrentProjectCanvas();
            SwitchToCanvas(GetCurrentProjectCanvas);
        }

        private void PublishMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(PublishCanvas);
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


        /*********************************************************************************************/
        /* Buttons within cards */
        /*********************************************************************************************/

        // UserConfig Canvas
        private void SubmitConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitUserConfig();
        }


        private void UserKey_BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserKeyBrowse();
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
            HandleWorkspaceBrowse();

            /* Reinitialize */
            InitializeGetCurrentProjectCanvas();
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


        // TODO: UpdateLocalProject
        private void GetLatestProjectVersionButton_Click(object sender, RoutedEventArgs e)
        {
            string stdout = "";
            string stderr = "";

            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            
            if (tempHandler.IsGitRepository(HandlerBackend.UHandler.WorkspacePath))
            {
                AppendLineToConsole("PULL:");
                tempHandler.PullLatestChanges(HandlerBackend.UHandler.WorkspacePath, out stdout, out stderr);

            } else
            {
                AppendLineToConsole("CLONE:");
                AppendLineToConsole("Im Arbeitsverzeichnis liegt noch kein Projekt ab.");
                AppendLineToConsole("Das aktuelle Projekt wird nun heruntergeladen und alle Verzeichnisse erstellt.");
                tempHandler.CloneProject(HandlerBackend.UHandler.WorkspacePath, out stdout, out stderr);
            }

            if (!String.IsNullOrWhiteSpace(stdout))
            {
                AppendLineToConsole(">> OUTPUT: " + stdout);
            }

            if (!String.IsNullOrWhiteSpace(stderr))
            {
                AppendLineToConsole(">> ERROR: " + stderr);
            }
        }


        /*********************************************************************************************/
        /* Canvas Initializers */
        /*********************************************************************************************/

        private void InitializeCanvasesContent()
        {
            /* HomeCanvas */
            InitializeHomeCanvas();

            /* ToolInstall Canvas */
            InitializeToolInstallCanvas();


            /* UserConfig Canvas */
            InitializeUserConfigCanvas();

            /* Update Current Project Canvas */
            InitializeGetCurrentProjectCanvas();

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

                if (String.IsNullOrEmpty(HandlerBackend.UHandler.UserKeyPath))
                {
                    AppendLineToConsole("Es ist kein valider User-Key Pfad gesetzt.");
                }

                UserKeyTextBox.Text = HandlerBackend.UHandler.UserKeyPath;


            }
        }


        private void InitializeGetCurrentProjectCanvas()
        {
            /* Check installations */
            if (!HandlerBackend.InstChecker.IsGitInstalled() || !HandlerBackend.InstChecker.IsMobiriseInstalled())
            {
                ProjectStatus = "Tools sind noch nicht installiert.";
                ProjectStatusLabel.Content = ProjectStatus;
                LocalRepoLabel.Content = DefaultDateString;
                LatestRepoLabel.Content = DefaultDateString;
                return;
            }

            /* Check for valid workspace */
            if (!Path.IsPathRooted(HandlerBackend.UHandler.WorkspacePath))
            {
                ProjectStatus = "Kein gültiger Arbeitsbereich gewählt";
                ProjectStatusLabel.Content = ProjectStatus;
                LocalRepoLabel.Content = DefaultDateString;
                LatestRepoLabel.Content = DefaultDateString;
                return;
            }

            UpdateReadRepoProperties();
        }

        /*********************************************************************************************/
        /* OTHERS */
        /*********************************************************************************************/
        private void UpdateReadRepoProperties()
        {
            DateTime dtLocal = new DateTime();
            DateTime dtRemote = new DateTime();

            /* All entries valid so read properties of the repo */
            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            AppendLineToConsole("Ermittle lokalen Stand...");
            tempHandler.GetLastCommitDate(HandlerBackend.UHandler.WorkspacePath, out string stdout, out string stderr);

            if (!String.IsNullOrEmpty(stdout))
            {
                AppendLineToConsole(">> OUTPUT: " + stdout);
            }

            if (!String.IsNullOrEmpty(stderr))
            {
                AppendLineToConsole(">> ERROR: " + stderr);
            }

            AppendLineToConsole("Lokaler Stand ermittelt.\r\n");

            if (stdout.Length < 22)
            {
                LocalRepoLabel.Content = stdout;
                dtLocal = DateTime.ParseExact(stdout, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
            }


            AppendLineToConsole("Ermittle Stand auf dem Server...");
            tempHandler.GetLastRemoteCommitDate(HandlerBackend.UHandler.WorkspacePath, out stdout, out stderr);

            if (!String.IsNullOrEmpty(stdout))
            {
                AppendLineToConsole(">> OUTPUT: " + stdout);
            }

            if (!String.IsNullOrEmpty(stderr))
            {
                AppendLineToConsole(">> ERROR: " + stderr);
            }

            AppendLineToConsole("Server-Stand ermittelt.\r\n");

            if (stdout.Length < 22)
            {
                dtRemote = DateTime.ParseExact(stdout, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
            }
 
            if (dtLocal != null && dtRemote != null)
            {
                int comp = DateTime.Compare(dtLocal, dtRemote);

                if (comp > 0)
                {
                    AppendLineToConsole("Der lokale Stand ist neuer als der auf dem Server.");
                    ProjectStatusLabel.Foreground = new SolidColorBrush(Colors.YellowGreen);
                    ProjectStatusLabel.Content = "Lokal voraus";
                } else if (comp < 0)
                {
                    AppendLineToConsole("Der lokale Stand ist älter als der auf dem Server.");
                    ProjectStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    ProjectStatusLabel.Content = "Lokal hinterher";
                } else
                {
                    AppendLineToConsole("Der Arbeitsbereich ist auf dem aktuellen Stand.");
                    ProjectStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    ProjectStatusLabel.Content = "Gleich";
                }
            }

            LocalRepoLabel.Content = dtLocal.ToString("dd.MM.yyyy hh:mm:ss");
            LatestRepoLabel.Content = dtRemote.ToString("dd.MM.yyyy hh:mm:ss");
            



        }

        /*********************************************************************************************/
        /* File/Folder Browsing */
        /*********************************************************************************************/
        private void HandleWorkspaceBrowse()
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

        private void HandleUserKeyBrowse()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Auswahl der User-Key Datei",
                InitialDirectory = String.Format("C:\\Users\\{0}\\Desktop", Environment.UserName),
                Multiselect = false,
                ShowHelp = false,
                Filter = "User-Key Dateien (*.uk)|*.uk",
                CheckFileExists = true,
                CheckPathExists = true
            };
            fileDialog.ShowDialog();
            UserKeyTextBox.Text = fileDialog.FileName;
        }


        /*********************************************************************************************/
        /* User Config Submission */
        /*********************************************************************************************/
        private void SubmitUserConfig()
        {
            if (FirstNameBox.Text.Trim() == "" || LastNameBox.Text.Trim() == "")
            {
                AppendLineToConsole("Bitte geben Sie einen gültigen Namen ein.");
            }

            if (!Path.IsPathRooted(UserKeyTextBox.Text))
            {
                AppendLineToConsole("Wählen Sie eine gültige User-Key Datei aus.");
            }

            if (!Path.IsPathRooted(HandlerBackend.UHandler.WorkspacePath))
            {
                AppendLineToConsole("Wählen Sie ein gültiges Arbeitsverzeichnis aus.");
            }


            /* Read from TextBoxes */
            HandlerBackend.UHandler.UserName = FirstNameBox.Text + " " + LastNameBox.Text;
            HandlerBackend.UHandler.UserKeyPath = UserKeyTextBox.Text;


            // TODO: Remove Encryption/Decryption as we now use PATs from Github
            //HandlerBackend.ServerCredDecryptor = new Decryptor(HandlerBackend.DefaultServerCredsPath, UserKeyTextBox.Text, HandlerBackend.DefaultEncryptorFile);
            //HandlerBackend.GitCredDecryptor = new Decryptor(HandlerBackend.DefaultGitCredsPath, UserKeyTextBox.Text, HandlerBackend.DefaultEncryptorFile);

            /* Update on other Canvases */
            UserConfigTextBlock.Text = "Nutzerkonfiguration: " + HandlerBackend.UHandler.UserName;

            /* Edit the Config-File */
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.UserKey, HandlerBackend.UHandler.UserName);
            HandlerBackend.UHandler.EditConfig(HandlerBackend.UHandler.UserKeyPathKey, HandlerBackend.UHandler.UserKeyPath);

            /* Output on console */
            AppendLineToConsole("--> Userkonfiguration übernommen: " + HandlerBackend.UHandler.UserName);
            AppendLineToConsole("User-Key Pfad: " + HandlerBackend.UHandler.UserKeyPath);
            AppendLineToConsole("Arbeitsbereich: " + HandlerBackend.UHandler.WorkspacePath);
            AppendLineToConsole();
        }




        /*********************************************************************************************/
        /* Console Handling */
        /*********************************************************************************************/
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
