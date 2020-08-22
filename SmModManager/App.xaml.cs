using System.IO;
using System.Windows;
using System.Windows.Threading;
using SmModManager.Core;
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
                    Settings.WorkshopPath = Path.Combine(steamPath, "steamapps", "workshop", "content", "387990");
                    Settings.UserDataPath = Directory.GetDirectories(Constants.UsersDataPath)[0];
                    Settings.Save();
                    goto SkipToLoading;
                }
                else
                {
                    steamPath = Utilities.GetSteamAppsLocation(steamPath);
                    if (Utilities.CheckSteamLocation(steamPath))
                    {
                        Settings.GameDataPath = Path.Combine(steamPath, "steamapps" , "common", "Scrap Mechanic");
                        Settings.WorkshopPath = Path.Combine(steamPath, "steamapps", "workshop", "content", "387990");
                        Settings.UserDataPath = Directory.GetDirectories(Constants.UsersDataPath)[0];
                        Settings.Save();
                        goto SkipToLoading;
                    }
                }
                SkipToPrerequisites:
                var dialog = new WnPrerequisites();
                if (dialog.ShowDialog() == false)
                    Current.Shutdown();
                Utilities.RestartApp();
            }
            SkipToLoading:
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
            new WnException(args.Exception).ShowDialog();
        }

    }
}
