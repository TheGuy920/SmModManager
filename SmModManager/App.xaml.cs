﻿using System.IO;
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
            if (!Directory.Exists(Constants.UsersDataPath))
            {
                MessageBox.Show("Run the game at least once before using this program!", "SmModManager");
                Current.Shutdown();
            }
            if (string.IsNullOrEmpty(Settings.GameDataPath) || string.IsNullOrEmpty(Settings.WorkshopPath) || string.IsNullOrEmpty(Settings.UserDataPath))
            {
                var dialog = new WnPrerequisites();
                if (dialog.ShowDialog() == false)
                    Current.Shutdown();
                Utilities.RestartApp();
            }
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
