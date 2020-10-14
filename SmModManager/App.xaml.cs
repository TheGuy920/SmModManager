using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Enums;
using SmModManager.Core.Models;
using SmModManager.Graphics;

namespace SmModManager
{

    public partial class App
    {

        #region Variables

        public static App GetApp;
        private string ScrapMechanicFile;
        private string SurvivalFile;

        #endregion

        #region AppData

        internal static Configuration Settings { get; private set; }

        #endregion

        #region UserData

        public static string UserSteamId { get; set; }
        public static bool IsClosing { get; set; }
        public static bool HasFormattedAllMods { get; set; }

        #endregion
        
        #region GraphicVariables

        internal static WnManager WindowManager { get; private set; }
        internal static PgStore PageStore { get; private set; }
        internal static PgAdvanced PageAdvanced { get; private set; }
        internal static PgHome PageHome { get; private set; }
        internal static PgBackups PageBackups { get; private set; }
        internal static PgManage PageManage { get; private set; }
        internal static WnJoinFriend PageJoinFriend { get; private set; }
        internal static PgMultiplayer PageMultiplayer { get; private set; }

        #endregion

        #region Initialization

        public App()
        {
            GetApp = this;
            var settings = new CefSettings
            {
                CachePath = Path.Combine(Constants.CachePath, "UserDataCache")
            };
            settings.CefCommandLineArgs.Add("enable-media-stream");
            Cef.Initialize(settings, true, browserProcessHandler: null);
            IsClosing = false;
            HasFormattedAllMods = false;
        }
        
        private void Initialize(object sender, StartupEventArgs args)
        {
            AppCenter.Start("c818850e-34d5-4155-850b-348c823bed24", typeof(Analytics), typeof(Crashes));
            try
            {
                // throw new Exception("There was an error accessing the folder");
                // File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "configuration.smmm"), "intentional error");
                // Settings = Configuration.Load();
                Settings = new Configuration();
                SetAppCulture();
                if (!Directory.Exists(Constants.UsersDataPath))
                    MessageBox.Show("WARNING: Scrap Mechanic save file missing! Please launch the game once before using this app!", "SmModManager");
                SetSteamStuff();
                CheckForUpdates();
                SkipToStartup:
                if (!Directory.Exists(Constants.ArchivesPath))
                    Directory.CreateDirectory(Constants.ArchivesPath);
                if (!Directory.Exists(Constants.GameBackupsPath))
                    Directory.CreateDirectory(Constants.GameBackupsPath);
                if (!Directory.Exists(Constants.CachePath))
                    Directory.CreateDirectory(Constants.CachePath);
                if (!Directory.Exists(Constants.WorldBackupsPath))
                    Directory.CreateDirectory(Constants.WorldBackupsPath);
                Utilities.LoadDataFromFile();
                PageAdvanced = new PgAdvanced();
                PageBackups = new PgBackups();
                PageManage = new PgManage();
                PageStore = new PgStore();
                PageHome = new PgHome();
                PageJoinFriend = new WnJoinFriend();
                PageMultiplayer = new PgMultiplayer();
                WindowManager = new WnManager();
                WindowManager.Show();
                WnManager.GetWnManager.ShowHomePage(null, null);
                if (!Settings.HasTakenTutorial)
                    WnManager.GetWnManager.Notification("Looks like your new!\nHead over to Advanced section and check out the tutorial!");
                var formatmods = new Thread(FormatAllMods) { IsBackground = true, Priority = ThreadPriority.Highest };
                formatmods.Start();
            }
            catch (Exception error)
            {
                try
                {
                    WindowManager?.Close();
                }
                catch
                {
                    // nothing
                }
                var ErrorWindow = new WnStartupHandler();
                ErrorWindow.Show();
                WnStartupHandler.StartUpErrorWindow(error);
            }
        }

        #endregion

        #region InitializationMethods
        
        private void SetAppCulture()
        {
            var culture = new CultureInfo(Settings.AppLanguage switch
            {
                _ => "en-US" // English
            });
            var dictionary = Current.Resources.MergedDictionaries.FirstOrDefault(rd => rd.Source.OriginalString.Contains(culture.ToString()));
            if (dictionary != null)
            {
                Current.Resources.MergedDictionaries.Remove(dictionary);
                Current.Resources.MergedDictionaries.Add(dictionary);
            }
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void CheckForUpdates()
        {
            if (Settings.UpdatePreference == UpdateBehaviorOptions.DontCheckForUpdates)
                return;
            if (!Utilities.CheckForUpdates())
                return;
            if (Settings.UpdatePreference == UpdateBehaviorOptions.RemindForUpdates)
                if (MessageBox.Show("An update is available! Would you like to install the new update?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
            Utilities.InstallUpdate();
        }

        private void SetSteamStuff()
        {
            if (!string.IsNullOrEmpty(Settings.GameDataPath) && !string.IsNullOrEmpty(Settings.WorkshopPath) && !string.IsNullOrEmpty(Settings.UserDataPath))
                return;
            var steamPath = Utilities.GetSteamLocation();
            if (string.IsNullOrEmpty(steamPath))
                goto SkipToPrerequisites;
            if (Utilities.CheckSteamLocation(steamPath))
            {
                Settings.GameDataPath = Path.Combine(steamPath, "steamapps", "common", "Scrap Mechanic");
                Settings.WorkshopPath = Path.Combine(steamPath, "steamapps", "workshop", "content", Constants.GameId.ToString());
                Settings.UserDataPath = Directory.GetDirectories(Constants.UsersDataPath)[0];
                Settings.Save();
                return;
            }
            steamPath = Utilities.GetSteamAppsLocation(steamPath);
            if (string.IsNullOrEmpty(steamPath))
                goto SkipToPrerequisites;
            if (Utilities.CheckSteamLocation(steamPath))
            {
                Settings.GameDataPath = Path.Combine(steamPath, "steamapps", "common", "Scrap Mechanic");
                Settings.WorkshopPath = Path.Combine(steamPath, "steamapps", "workshop", "content", Constants.GameId.ToString());
                Settings.UserDataPath = Directory.GetDirectories(Constants.UsersDataPath)[0];
                Settings.Save();
                return;
            }
            SkipToPrerequisites:
            var dialog = new WnPrerequisites();
            if (dialog.ShowDialog() == true)
                return;
            Current.Shutdown();
        }
        
        #endregion

        #region TheGuyStuff
        
        private void FormatAllMods()
        {
            var StartDir = Settings.WorkshopPath;
            if (StartDir.Length <= 3)
                return;
            foreach (var SubDirectory in Directory.GetDirectories(StartDir))
            {
                var FormatFile = Path.Combine(SubDirectory, "format.smmm");
                if (File.Exists(FormatFile))
                    continue;
                var NewFolder = FormatMod(SubDirectory);
                var TopFolder = NewFolder.Split("\\")[^1];
                if (NewFolder != SubDirectory)
                    if (!Directory.Exists(Path.Combine(StartDir, "Scrap Mechanic")) && !Directory.Exists(Path.Combine(StartDir, "Survival")))
                    {
                        Directory.CreateDirectory(Path.Combine(SubDirectory, TopFolder));
                        Utilities.CopyDirectory(NewFolder, Path.Combine(SubDirectory, TopFolder));
                    }
                File.WriteAllText(FormatFile, "");
            }
            HasFormattedAllMods = true;
            Dispatcher.Invoke(() =>
            {
                PgManage.GetPgManage.RefreshAll();
            });
        }

        private string FormatMod(string StartDir)
        {
            if (Directory.Exists(Path.Combine(StartDir, "Scrap Mechanic")) || Directory.Exists(Path.Combine(StartDir, "Survival")))
                return StartDir;
            ScrapMechanicFile = "";
            SurvivalFile = "";
            FindScrapMechanicFile(StartDir);
            if (ScrapMechanicFile != "")
                return ScrapMechanicFile;
            FindSurvivalFile(StartDir);
            return SurvivalFile != "" ? SurvivalFile : StartDir;
        }

        private void FindScrapMechanicFile(string StartDir)
        {
            foreach (var SubDirectory in Directory.GetDirectories(StartDir))
            {
                if (ScrapMechanicFile != "")
                    return;
                if (SubDirectory.Split("\\")[^1] == "Scrap Mechanic")
                {
                    ScrapMechanicFile = SubDirectory;
                    return;
                }
                FindScrapMechanicFile(SubDirectory);
            }
            ScrapMechanicFile = "";
        }

        private void FindSurvivalFile(string StartDir)
        {
            foreach (var SubDirectory in Directory.GetDirectories(StartDir))
            {
                if (SurvivalFile != "")
                    return;
                if (SubDirectory.Split("\\")[^1] == "Survival")
                {
                    SurvivalFile = SubDirectory;
                    return;
                }
                FindSurvivalFile(SubDirectory);
            }
            SurvivalFile = "";
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            #if !DEBUG
            Crashes.TrackError(args.Exception);
            #endif
            if (!IsClosing)
                new WnErrorHandler(args.Exception).ShowDialog();
        }

        public void CleanFiles()
        {
            foreach (var str in Directory.GetFiles(Constants.AppUserData))
                File.Delete(str);
            File.Delete(Path.Combine(Environment.CurrentDirectory, "configuration.smmm"));
        }

        public void NewModAdded(IWebBrowser currentBrowser)
        {
            var randomInt = "0";
            var Address = "";
            Dispatcher.Invoke(() =>
            {
                Address = currentBrowser.Address;
            });
            foreach (var file in Directory.GetFiles(Constants.CachePath))
            {
                var CanOpenFile = false;
                var count = 0;
                while (!CanOpenFile)
                    try
                    {
                        if (count >= 20)
                            throw new Exception("Zip file load took too long. Zip file load timed out");
                        File.ReadAllText(file);
                        CanOpenFile = true;
                    }
                    catch
                    {
                        CanOpenFile = false;
                        count++;
                        Thread.Sleep(150);
                    }
                var rnd = new Random();
                randomInt = rnd.Next(999999, 999999999).ToString();
                var IsValidFileName = false;
                while (IsValidFileName)
                {
                    var valid = true;
                    foreach (var tmpFile in Directory.GetDirectories(Constants.ArchivesPath))
                        if (tmpFile.Split("\\")[^1] == randomInt)
                        {
                            valid = false;
                            IsValidFileName = false;
                            randomInt = rnd.Next(999999, 999999999).ToString();
                        }
                    if (valid)
                        IsValidFileName = true;
                }
                if (!file.Contains(".zip"))
                    continue;
                var name = file.Split(@"\")[^1].Replace(".zip", "");
                if (Address.Contains("smmods"))
                {
                    var indexBeg = name.LastIndexOf("(");
                    var indexEnd = name.LastIndexOf(")");
                    Address = "https://smmods.com/mod/" + name[(indexBeg + 1)..indexEnd];
                    name = name[..indexBeg];
                }
                Directory.CreateDirectory(Path.Combine(Constants.ArchivesPath, randomInt, "Scrap Mechanic"));
                ZipFile.ExtractToDirectory(file, Path.Combine(Constants.ArchivesPath, randomInt, "Scrap Mechanic"));
                File.Delete(file);
                var _data = new List<ModDescriptionModel>
                {
                    new ModDescriptionModel
                    {
                        Name = name,
                        Description = "Description Here",
                        Type = "Blocks and Parts",
                        Location = Address.Length.ToString().Length + Address.Length.ToString() + Address
                    }
                };
                var json = JsonConvert.SerializeObject(_data);
                File.WriteAllText(Path.Combine(Constants.ArchivesPath, randomInt, "description.json"), json.Replace("[", "").Replace("]", ""));
                File.Copy(Path.Combine(Constants.Resources, "Assets", "empty.png"), Path.Combine(Constants.ArchivesPath, randomInt, "preview.png"));
                Dispatcher.Invoke(() =>
                {
                    PgManage.GetPgManage.ArchivedModsList.Items.Add(ModItemBinding.Create(Path.Combine(Constants.ArchivesPath, randomInt)));
                    PgManage.GetPgManage.ArchivedModsList.SelectedItem = PgManage.GetPgManage.ArchivedModsList.Items[^1];
                    PgManage.GetPgManage.RefreshCompatibleMods(null, null);
                    WnManager.GetWnManager.ShowManagePage(null, null);
                    PgManage.GetPgManage.ArchivedModsTab.IsSelected = true;
                });
            }
        }

        #endregion

    }

}