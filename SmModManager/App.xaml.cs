using System;
using System.IO.Compression;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SmModManager.Core;
using SmModManager.Core.Enums;
using SmModManager.Graphics;
using System.IO.IsolatedStorage;
using CefSharp.Wpf;
using CefSharp;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Linq;
using SmModManager.Core.Bindings;
using System.Security.Policy;
using System.Diagnostics;

namespace SmModManager
{

    public partial class App : Application
    {
        internal static Configuration Settings { get; private set; }
        internal static WnManager WindowManager { get; private set; }
        internal static PgStore PageStore { get; private set; }
        internal static PgAdvanced PageAdvanced { get; private set; }
        internal static PgHome PageHome { get; private set; }
        internal static PgBackups PageBackups { get; private set; }
        internal static PgManage PageManage { get; private set; }
        internal static JoinFriend PageJoinFriend { get; private set; }
        internal static PgMultiplayer PageMultiplayer { get; private set; }
        public static string UserSteamId { get; set; }
        public static bool IsClosing { get; set; }
        public static bool HasFormattedAllMods { get; set; }
        public enum ApplicationExitCode
        {
            Success = 0,
            Failure = 1,
            CantWriteToApplicationLog = 2,
            CantPersistApplicationState = 3
        }

        public static App GetApp;
        public App()
        {
            GetApp = this;
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Constants.CachePath, "UserDataCache")
            };

            //Example of setting a command line argument
            //Enables WebRTC
            settings.CefCommandLineArgs.Add("enable-media-stream");

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            IsClosing = false;
            HasFormattedAllMods = false;
        }
        public void OnAppExit(object sender, ExitEventArgs e)
        {
            // bruh
        }
        private void Initialize(object sender, StartupEventArgs args)
        {
            AppCenter.Start("c818850e-34d5-4155-850b-348c823bed24", typeof(Analytics), typeof(Crashes));
            try
            {
                //throw new Exception("There was an error accessing the folder");
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "configuration.smmm"), "eorororooror");
                Settings = Configuration.Load();
                if (!Directory.Exists(Constants.UsersDataPath))
                {
                    MessageBox.Show("WARNING: Scrap Mechanic save file missing! Please launch the game once before using this app!", "SmModManager");
                }
                if (string.IsNullOrEmpty(Settings.GameDataPath) || string.IsNullOrEmpty(Settings.WorkshopPath) || string.IsNullOrEmpty(Settings.UserDataPath))
                {
                    var steamPath = Utilities.GetSteamLocation();
                    if (string.IsNullOrEmpty(steamPath))
                        goto SkipToPrerequisites;
                    if (Utilities.CheckSteamLocation(steamPath))
                    {
                        Settings.GameDataPath = Path.Combine(steamPath, "steamapps", "common", "Scrap Mechanic");
                        Settings.WorkshopPath = Path.Combine(steamPath, "steamapps", "workshop", "content", Constants.GameId.ToString());
                        Settings.UserDataPath = Directory.GetDirectories(Constants.UsersDataPath)[0];
                        Settings.Save();
                        goto SkipToStartup;
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
                        goto SkipToStartup;
                    }
                SkipToPrerequisites:
                    var dialog = new WnPrerequisites();
                    if (dialog.ShowDialog() == true)
                        goto SkipToStartup;
                    Current.Shutdown();
                    return;
                }
                if (Settings.UpdatePreference != UpdateBehaviorOptions.DontCheckForUpdates)
                {
                    if (Utilities.CheckForUpdates())
                    {
                        if (Settings.UpdatePreference == UpdateBehaviorOptions.RemindForUpdates)
                            if (MessageBox.Show("An update is available! Would you like to install the new update?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                goto SkipToStartup;
                        Utilities.InstallUpdate();
                    }
                }
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
                PageJoinFriend = new JoinFriend();
                PageMultiplayer = new PgMultiplayer();
                WindowManager = new WnManager();
                WindowManager.Show();
                WnManager.GetWnManager.ShowHomePage(null, null);
                if (!Settings.HasTakenTutorial)
                    WnManager.GetWnManager.Notification("You look like your new\nHead over to Advanced and check out the tutorial");
                var formatmods = new Thread(FormatAllMods)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                formatmods.Start();
            }
            catch(Exception error)
            {
                try { if (WindowManager != null) { WindowManager.Close(); } } catch { }
                var ErrorWindow = new StartUpError();
                ErrorWindow.Show();
                StartUpError.StartUpErrorWindow(error);
            }
        }
        public void FormatAllMods()
        {
            string StartDir = Settings.WorkshopPath;
            if (StartDir.Length > 3)
            {
                foreach (string SubDirectory in Directory.GetDirectories(StartDir))
                {
                    var FormatFile = Path.Combine(SubDirectory, "format.smmm");
                    if (!File.Exists(FormatFile))
                    {
                        var NewFolder = FormatMod(SubDirectory);
                        var TopFolder = NewFolder.Split("\\")[^1];
                        if (NewFolder != SubDirectory)
                        {
                            if (!Directory.Exists(Path.Combine(StartDir, "Scrap Mechanic")) && !Directory.Exists(Path.Combine(StartDir, "Survival")))
                            {
                                Directory.CreateDirectory(Path.Combine(SubDirectory, TopFolder));
                                Utilities.CopyDirectory(NewFolder, Path.Combine(SubDirectory, TopFolder));
                            }
                        }
                        File.WriteAllText(FormatFile, "");
                    }
                }
                HasFormattedAllMods = true;
                Dispatcher.Invoke(() =>
                {
                    PgManage.GetPgManage.RefreshAll();
                });
            }
        }
        public string ScrapMechanicFile;
        public string SurvivalFile;
        public string FormatMod(string StartDir)
        {
            if (!Directory.Exists(Path.Combine(StartDir, "Scrap Mechanic")) && !Directory.Exists(Path.Combine(StartDir, "Survival")))
            {
                ScrapMechanicFile = "";
                SurvivalFile = "";
                FindScrapMechanicFile(StartDir);
                if (ScrapMechanicFile != "")
                    return ScrapMechanicFile;
                FindSurvivalFile(StartDir);
                if (SurvivalFile != "")
                    return SurvivalFile;
                return StartDir;
            }
            else 
            {
                return StartDir;
            }
        }
        public void FindScrapMechanicFile(string StartDir)
        {
            foreach (string SubDirectory in Directory.GetDirectories(StartDir))
            {
                if (ScrapMechanicFile != "")
                    return;
                if (SubDirectory.Split("\\")[^1] == "Scrap Mechanic")
                {
                    ScrapMechanicFile = SubDirectory;
                    return;
                }
                else
                    FindScrapMechanicFile(SubDirectory);
            }
            ScrapMechanicFile = "";
        }
        public void FindSurvivalFile(string StartDir)
        {
            foreach (string SubDirectory in Directory.GetDirectories(StartDir))
            {
                if (SurvivalFile != "")
                    return;
                if (SubDirectory.Split("\\")[^1] == "Survival")
                {
                    SurvivalFile = SubDirectory;
                    return;
                }
                else
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
            if(!IsClosing)
                new WnException(args.Exception).ShowDialog();
        }
        public void CleanFiles()
        {
            foreach (string str in Directory.GetFiles(Constants.AppUserData))
            {
                File.Delete(str);
            }
            File.Delete(Path.Combine(Environment.CurrentDirectory, "configuration.smmm"));
        }
        public void NewModAdded(IWebBrowser currentBrowser)
        {
            string randomInt = "0";
            string Address = "";
            Dispatcher.Invoke(() =>
            {
                Address = currentBrowser.Address;
            });
            foreach (string file in Directory.GetFiles(Constants.CachePath))
            {
                var CanOpenFile = false;
                var count = 0;
                while (!CanOpenFile)
                {
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
                }
                Random rnd = new Random();
                randomInt = rnd.Next(999999, 999999999).ToString();
                var IsValidFileName = false;
                while (IsValidFileName)
                {
                    var valid = true;
                    foreach(string tmpFile in Directory.GetDirectories(Constants.ArchivesPath))
                    {
                        if (tmpFile.Split("\\")[^1] == randomInt)
                        {
                            valid = false;
                            IsValidFileName = false;
                            randomInt = rnd.Next(999999, 999999999).ToString();
                        }
                    }
                    if (valid)
                    {
                        IsValidFileName = true;
                    }
                }
                if (file.Contains(".zip"))
                {
                    var name = file.Split(@"\")[^1].Replace(".zip", "");
                    if (Address.Contains("smmods"))
                    {
                        var indexBeg = name.LastIndexOf("(");
                        var indexEnd = name.LastIndexOf(")");
                        Address = "https://smmods.com/mod/" + name[(indexBeg+1)..indexEnd];
                        name = name[0..indexBeg];
                    }
                    Directory.CreateDirectory(Path.Combine(Constants.ArchivesPath, randomInt, "Scrap Mechanic"));
                    ZipFile.ExtractToDirectory(file, Path.Combine(Constants.ArchivesPath, randomInt, "Scrap Mechanic"));
                    File.Delete(file);
                    List<JsonData> _data = new List<JsonData>
                    {
                        new JsonData()
                        {
                            name = name,
                            description = "Description Here",
                            fileId = randomInt,
                            type = "Blocks and Parts",
                            Location = Address.Length.ToString().Length.ToString() + Address.Length.ToString() + Address,
                            version = 1
                        }
                    };
                    string json = JsonConvert.SerializeObject(_data);
                    File.WriteAllText(Path.Combine(Constants.ArchivesPath, randomInt, "description.json"), json.Replace("[", "").Replace("]", ""));
                    File.Copy(Path.Combine(Constants.Resources, "Assets", "empty.png"), Path.Combine(Constants.ArchivesPath, randomInt, "preview.png"));
                    Dispatcher.Invoke((Action)(() =>
                    {
                        PgManage.GetPgManage.ArchivedModsList.Items.Add(ModItemBinding.Create(Path.Combine(Constants.ArchivesPath, randomInt)));
                        PgManage.GetPgManage.ArchivedModsList.SelectedItem = PgManage.GetPgManage.ArchivedModsList.Items[^1];
                        PgManage.GetPgManage.RefreshCompatibleMods(null, null);
                        WnManager.GetWnManager.ShowManagePage(null, null);
                        PgManage.GetPgManage.ArchivedModsTab.IsSelected = true;
                    }));
                }
            }
        }
        public class JsonData
        {
            public string description { get; set; }
            public string fileId { get; set; }
            public string name { get; set; }
            public string Location { get; set; }
            public string type { get; set; }
            public int version { get; set; }
        }
    }

}
