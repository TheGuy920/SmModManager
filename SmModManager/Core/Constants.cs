using System;
using System.IO;

namespace SmModManager.Core
{

    internal static class Constants
    {

        public static readonly string UsersDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axolot Games", "Scrap Mechanic", "User");

    }

}