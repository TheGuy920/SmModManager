using System.Windows;
using SmModManager.Core;
using SmModManager.Graphics;

namespace SmModManager
{

    public partial class App
    {

        internal static Configuration Settings { get; private set; }

        internal static WnManager WindowManager { get; private set; }

        internal static PgAdvanced PageAdvanced { get; private set; }
        internal static PgArchive PageArchive { get; private set; }
        internal static PgBackup PageBackup { get; private set; }
        internal static PgManage PageManage { get; private set; }
        internal static PgMultiplayer PageMultiplayer { get; private set; }

        private void Initialize(object sender, StartupEventArgs args)
        {
            Settings = Configuration.Load();
            PageAdvanced = new PgAdvanced();
            PageArchive = new PgArchive();
            PageBackup = new PgBackup();
            PageManage = new PgManage();
            PageMultiplayer = new PgMultiplayer();
            WindowManager = new WnManager();
            WindowManager.Show();
        }

    }
}
