using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
        private const string DevMail = "raphael.pfaller.dev@googlemail.com"; 
        private string GuiVersion { get; } = "1.0.0";
        private string BackendVersion { get; set; } = "1.0.0";
        private string HandlerVersion { get; set; } = "1.0.0";

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
        private bool CommitPossible { get; set; }


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
            InitializeLocalProjectCanvas();
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


        private void ClearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ClearConsole();
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
            InitializeLocalProjectCanvas();
        }


        // Publish Canvas
        private void ChangesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ChangesTextBox.Text.Length > 200)
            {
                if (CommitChangesButton.IsEnabled)
                {
                    CommitChangesButton.IsEnabled = false;
                }
            }
            else if (ChangesTextBox.Text.Length > 15 && CommitPossible)
            {
                if (!CommitChangesButton.IsEnabled)
                {
                    CommitChangesButton.IsEnabled = true;
                }
            }
            else
            {
                if (CommitChangesButton.IsEnabled)
                {
                    CommitChangesButton.IsEnabled = false;
                }
            }
        }


        private void PublishChangesButton_Click(object sender, RoutedEventArgs e)
        {
            PublishChanges();
        }


        // Update Canvas
        private void UpdateHandlerButton_Click(object sender, RoutedEventArgs e)
        {
            GITHandler tempHandler = new GITHandler(HandlerBackend.HandlerRepoLink, HandlerBackend.UHandler);
            tempHandler.OpenRepoInBrowser();
        }


        // Update Local
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


        // Commit Changes 
        private void CommitChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Änderungen vorbereiten? Änderungskommentar: " + ChangesTextBox.Text, "Vorbereiten der Änderungen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
                tempHandler.CommitLatestChanges(HandlerBackend.UHandler.WorkspacePath, HandlerBackend.UHandler.UserName + ":" + ChangesTextBox.Text, out string stdout, out string stderr);

                if (!String.IsNullOrWhiteSpace(stdout))
                {
                    AppendLineToConsole(">> OUTPUT: " + stdout);
                }

                if (!String.IsNullOrWhiteSpace(stderr))
                {
                    AppendLineToConsole(">> ERROR: " + stderr);
                }

                ChangesTextBox.Text = "";
                InitializeLocalProjectCanvas();
            }
        }


        // Remove/Unstage Changes
        private void RemoveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Änderungen verwerfen?", "Verwerfen der Änderungen", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
                tempHandler.RemoveChanges(HandlerBackend.UHandler.WorkspacePath, out string stdout, out string stderr);
                AppendLineToConsole("Änderungen verworfen.");
                AppendLineToConsole(">> OUTPUT: " + stdout);
                AppendLineToConsole(">> ERROR: " + stderr);

                AppendLineToConsole("Reinitialisierung des Handlers...");
                InitializeCanvasesContent();
            }
        }


        // Send problem message / Open Mailing Programme
        private void SendIssueButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + DevMail + "?subject=WebsiteHandlerProblem &body=ConsoleOutput " + 
                ConsoleOutputTextBlock.Text.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine));
        }


        // Refresh View
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ClearConsole();
            InitializeCanvasesContent();
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
            InitializeLocalProjectCanvas();

            /* Publishing Canvas */
            InitializePublishCanvas();

            /* Update HandlerCanvas */
            InitializeUpdateHandlerCanvas();
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


        private void InitializeLocalProjectCanvas()
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

            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            
            if(tempHandler.ChangesAvailable(HandlerBackend.UHandler.WorkspacePath, out string stdout, out string stderr))
            {
                ChangesAvailableLabel.Content = "Ja";
                CommitPossible = true;
            } else
            {
                ChangesAvailableLabel.Content = "Nein";
                CommitPossible = false;
                CommitChangesButton.IsEnabled = false;
            }

        }


        private void InitializeUpdateHandlerCanvas()
        {
            VersionLabel.Content = HandlerVersion;

        }


        private void InitializePublishCanvas()
        {
            string workspace = HandlerBackend.UHandler.WorkspacePath;
            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            int commitDiff = tempHandler.GetCommitCountDifference(out string stdout, out string stderr, workspace);

            string localDate = tempHandler.GetLastCommitDate(workspace, out stdout, out stderr);
            string remoteDate = tempHandler.GetLastRemoteCommitDate(workspace, out stdout, out stderr);

            LocalVersionLabel.Content = localDate;
            PublicVersionLabel.Content = remoteDate;
            CommitDifferenceLabel.Content = commitDiff;

            if (commitDiff == 0)
            {
                AppendLineToConsole("Keine Änderungen zum Veröffentlichen vorhanden.");
                PublishStatusLabel.Content = "Gleich";

            } else if (commitDiff < 0)
            {
                PublishStatusLabel.Content = "Das lokale Projekt ist hinterher. Erst aktualisieren.";
            } else
            {
                PublishStatusLabel.Content = "Änderungen können veröffentlicht werden.";
            }
        }


        /*********************************************************************************************/
        /* Publishing */
        /*********************************************************************************************/

        private void PublishChanges()
        {
            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            tempHandler.PushLatestChanges(HandlerBackend.UHandler.WorkspacePath, out string stdout, out string stderr);

            AppendLineToConsole(stdout);
            AppendLineToConsole(stderr);
        }


        /*********************************************************************************************/
        /* Update Handler */
        /*********************************************************************************************/
        

        /*********************************************************************************************/
        /* OTHERS */
        /*********************************************************************************************/
        private void UpdateReadRepoProperties()
        {
            /* All entries valid so read properties of the repo */
            GITHandler tempHandler = new GITHandler(HandlerBackend.WebsiteRepoLink, HandlerBackend.UHandler);
            AppendLineToConsole("Ermittle lokalen Stand...");
            string localTimeStr = tempHandler.GetLastCommitDate(HandlerBackend.UHandler.WorkspacePath, out string stdout, out string stderr);

            if (!String.IsNullOrEmpty(stdout))
            {
                AppendLineToConsole(">> OUTPUT: " + stdout);
            }

            if (!String.IsNullOrEmpty(stderr))
            {
                AppendLineToConsole(">> ERROR: " + stderr);
            }

            AppendLineToConsole("Lokaler Stand ermittelt.\r\n");

            if (String.IsNullOrEmpty(stdout) || String.IsNullOrEmpty(localTimeStr))
            {
                AppendLineToConsole("Fehler beim Abholen der letzten Änderungen!");
                AppendLineToConsole("Ist der Projektpfad richtig gesetzt?");
                return;
            }


            AppendLineToConsole("Ermittle Stand auf dem Server...");
            string remoteTimeStr = tempHandler.GetLastRemoteCommitDate(HandlerBackend.UHandler.WorkspacePath, out stdout, out stderr);

            if (!String.IsNullOrEmpty(stdout))
            {
                AppendLineToConsole(">> OUTPUT: " + stdout);
            }

            if (!String.IsNullOrEmpty(stderr))
            {
                AppendLineToConsole(">> ERROR: " + stderr);
            }


            if (String.IsNullOrEmpty(stdout) || String.IsNullOrEmpty(localTimeStr))
            {
                AppendLineToConsole("Fehler bei der Ermittlung des Server-Standes!");
                return;
            } else
            {
                AppendLineToConsole("Server-Stand ermittelt.\r\n");
            }

             
            if (localTimeStr != null && remoteTimeStr != null)
            {
                DateTime dtLocal = DateTime.Parse(localTimeStr);
                DateTime dtRemote = DateTime.Parse(remoteTimeStr);
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

            LocalRepoLabel.Content = localTimeStr;
            LatestRepoLabel.Content = remoteTimeStr;
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
            if (s == null || s == "")
            {
                ConsoleOutputTextBlock.Text += "\r\n";
            }

            if (ConsoleOutputTextBlock.Text.EndsWith("\r\n") || ConsoleOutputTextBlock.Text.EndsWith("\n"))
            {
                ConsoleOutputTextBlock.Text += s;
                return;
            }

            ConsoleOutputTextBlock.Text += "\r\n" + s;
        }

        private void AppendToConsole(string s)
        {
            ConsoleOutputTextBlock.Text += s;
        }

        private void ClearConsole()
        {
            ConsoleOutputTextBlock.Text = "";
        }

    }
}
