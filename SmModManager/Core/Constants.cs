using System;
using System.IO;

namespace SmModManager.Core
{

    internal static class Constants
    {

        public const uint GameId = 387990;
        public const uint WorkshopId = 2122179067;

        public static readonly string UsersDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axolot Games", "Scrap Mechanic", "User");
        public static readonly string ArchivesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "Archives");
        public static readonly string WorldBackupsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Worlds");
        public static readonly string GameBackupsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Games");
        public static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        public static readonly string ModInstallBackupsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unins");
        public static readonly string AppUserData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        public static readonly string Resources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
    }

}