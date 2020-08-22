using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace SmModManager.Core
{

    internal static class Utilities
    {

        public static void RestartApp(string args = null)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (location.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                location = Path.Combine(Path.GetDirectoryName(location)!, Path.GetFileNameWithoutExtension(location) + ".exe");
            Process.Start(location, args ?? string.Empty);
            Application.Current.Shutdown();
        }

        public static bool IsMod(string path)
        {
            if (!File.Exists(Path.Combine(path, "description.json")))
                return false;
            return File.Exists(Path.Combine(path, "preview.png")) || File.Exists(Path.Combine(path, "preview.jpg"));
        }

        public static bool IsCompatibleMod(string path)
        {
            return IsMod(path) && Directory.Exists(Path.Combine(path, "Survival"));
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            var info = new DirectoryInfo(sourcePath);
            var directories = info.GetDirectories();
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);
            var files = info.GetFiles();
            foreach (var file in files)
                file.CopyTo(Path.Combine(destinationPath, file.Name), true);
            foreach (var subDirectories in directories)
                CopyDirectory(subDirectories.FullName, Path.Combine(destinationPath, subDirectories.Name));
        }

        public static string RetrieveResourceData(string resourceName)
        {
            var manifest = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (manifest == null)
                return null;
            var reader = new StreamReader(manifest);
            var result = reader.ReadToEnd();
            reader.Close();
            manifest.Close();
            return result;
        }

        public static string GetSteamLocation()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            if (key == null)
                return null;
            var installPath = (string)key.GetValue("InstallPath");
            return string.IsNullOrEmpty(installPath) ? null : installPath;
        }

        public static string GetSteamAppsLocation(string steamPath)
        {
            var deserialized = VdfConvert.Deserialize(File.ReadAllText(Path.Combine(steamPath, "steamapps", "libraryfolders.vdf")));
            var values = (VObject)deserialized.Value;
            var value = values["1"]?.Value<string>() ?? string.Empty;
            return value;
        }

        public static bool CheckSteamLocation(string steamPath)
        {
            if (!Directory.Exists(Path.Combine(steamPath, "steamapps", "workshop", "content", Constants.GameId.ToString())))
                return false;
            if (!File.Exists(Path.Combine(steamPath, "steamapps", "common", "Scrap Mechanic", "Release", "ScrapMechanic.exe")))
                return false;
            return true;
        }

        public static bool CheckForUpdates()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var fileVersion = Version.Parse(File.ReadAllText(Path.Combine(App.Settings.WorkshopPath, Constants.WorkshopId.ToString(), "sysVer.SMMM")));
            return fileVersion.CompareTo(currentVersion) > 0;
        }

        public static void InstallUpdate()
        {
            Process.Start(Path.Combine(App.Settings.WorkshopPath, Constants.WorkshopId.ToString(), "update.exe"));
            Application.Current.Shutdown();
        }

        public static string PathRemoveMatch(string path, string remove, string match)
        {
            var temp = path.Remove(0, remove.Length + 1);
            return Path.Combine(match, temp);
        }

        public static string GetDirectoryName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        public static void OpenBrowserUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static void OpenExplorerUrl(string url)
        {
            Process.Start("explorer.exe", url);
        }

        public static string GenerateAlphanumeric(int length)
        {
            var random = new Random();
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length).Select(index => index[random.Next(index.Length)]).ToArray());
        }

    }

}