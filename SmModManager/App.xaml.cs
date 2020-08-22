using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SmModManager.Core;
using SmModManager.Core.Options;
using SmModManager.Graphics;

namespace SmModManager
{

    public partial class App
    {

        internal static Configuration Settings { get; private set; }

        internal static WnManager WindowManager { get; private set; }

        internal static PgAdvanced PageAdvanced { get; private set; }
        internal static PgArchives PageArchives { get; private set; }
        internal static PgBackups PageBackups { get; private set; }
        internal static PgManage PageManage { get; private set; }
        internal static PgMultiplayer PageMultiplayer { get; private set; }

        private void Initialize(object sender, StartupEventArgs args)
        {
            AppCenter.Start("c818850e-34d5-4155-850b-348c823bed24", typeof(Analytics), typeof(Crashes));
            Settings = Configuration.Load();
            if (!Directory.Exists(Constants.UsersDataPath))
            {
                MessageBox.Show("Run the game at least once before using this program!", "SmModManager");
                Current.Shutdown();
            }
            if (string.IsNullOrEmpty(Settings.GameDataPath) || string.IsNullOrEmpty(Settings.WorkshopPath) || string.IsNullOrEmpty(Settings.UserDataPath))
            {
                var steamPath = Utilities.GetSteamLocation();
                if (string.IsNullOrEmpty(steamPath))
                    goto SkipToPrerequisites;
                if (Utilities.CheckSteamLocation(steamPath))
                {
                    Settings.GameDataPath = Path.Combine(steamPath, "steamapps" , "common", "Scrap Mechanic");
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
                    Settings.GameDataPath = Path.Combine(steamPath, "steamapps" , "common", "Scrap Mechanic");
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
            if (Settings.UpdatePreference != UpdatePreferenceOptions.DontCheckForUpdates)
            {
                var isUpdateAvailable = Utilities.CheckForUpdates();
                if (isUpdateAvailable)
                {
                    if (Settings.UpdatePreference == UpdatePreferenceOptions.RemindForUpdates)
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
            PageAdvanced = new PgAdvanced();
            PageArchives = new PgArchives();
            PageBackups = new PgBackups();
            PageManage = new PgManage();
            PageMultiplayer = new PgMultiplayer();
            WindowManager = new WnManager();
            WindowManager.Show();
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            Crashes.TrackError(args.Exception);
            new WnException(args.Exception).ShowDialog();
        }

    }

}