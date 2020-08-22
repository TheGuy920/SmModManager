﻿using System;
using System.IO;

namespace SmModManager.Core
{

    internal static class Constants
    {

        public static readonly string UsersDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axolot Games", "Scrap Mechanic", "User");
        public static readonly string ArchivesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Archives");
        public static readonly string WorldBackupsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Worlds");
        public static readonly string GameBackupsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Game");
        public static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");

        public const uint GameId = 387990;
        public const uint WorkshopId = 2122179067;

    }

}