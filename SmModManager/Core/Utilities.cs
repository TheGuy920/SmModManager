using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

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

        public static string GenerateTempFile()
        {
            return Path.Combine(Constants.CachePath, new Random().Next(int.MaxValue) + ".tmp");
        }

        public static string GetDirectoryName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

    }

}