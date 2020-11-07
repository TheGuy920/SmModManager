using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using SmModManager.Graphics;

namespace SmModManager.Core
{

    internal static class Utilities
    {

        public static List<string> CurrentMods = new List<string>();
        public static List<string> ArchivedMods = new List<string>();
        public static List<string> CompatibleMods = new List<string>();
        public static string AppVersion = "8.08";
        public static List<string> PathList = new List<string>();

        public static void SaveDataToFile()
        {
            if (!Directory.Exists(Constants.AppUserData))
                Directory.CreateDirectory(Constants.AppUserData);
            var thread = new Thread(SaveDataThread)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public static void SaveDataThread()
        {
            var isValidSave = false;
            var count = 0;
            while (!isValidSave)
                try
                {
                    if (count >= 100)
                        throw new Exception("Data save took too long. Save timed out");
                    File.WriteAllLines(Path.Combine(Constants.AppUserData, "CurrentMods.smmm"), CurrentMods);
                    File.WriteAllLines(Path.Combine(Constants.AppUserData, "ArchivedMods.smmm"), ArchivedMods);
                    File.WriteAllLines(Path.Combine(Constants.AppUserData, "CompatibleMods.smmm"), CompatibleMods);
                    isValidSave = true;
                }
                catch
                {
                    isValidSave = false;
                    count++;
                    Thread.Sleep(50);
                }
        }

        public static void LoadDataFromFile()
        {
            if (File.Exists(Path.Combine(Constants.AppUserData, "CurrentMods.smmm")))
                foreach (var item in File.ReadAllLines(Path.Combine(Constants.AppUserData, "CurrentMods.smmm")))
                    CurrentMods.Add(item.Replace("\n", ""));
            if (File.Exists(Path.Combine(Constants.AppUserData, "ArchivedMods.smmm")))
                foreach (var item in File.ReadAllLines(Path.Combine(Constants.AppUserData, "ArchivedMods.smmm")))
                    ArchivedMods.Add(item.Replace("\n", ""));
            if (File.Exists(Path.Combine(Constants.AppUserData, "CompatibleMods.smmm")))
                foreach (var item in File.ReadAllLines(Path.Combine(Constants.AppUserData, "CompatibleMods.smmm")))
                    CompatibleMods.Add(item.Replace("\n", ""));
        }

        public static void RestartApp(string args = null)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (location.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                location = Path.Combine(Path.GetDirectoryName(location)!, Path.GetFileNameWithoutExtension(location) + ".exe");
            Process.Start(location, args ?? string.Empty);
            Application.Current.Shutdown();
        }

        public static void RestartAppIfNotAdmin()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(windowsIdentity);
            var isRunningAsAdmin = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (isRunningAsAdmin)
                return;
            var location = Assembly.GetExecutingAssembly().Location;
            if (location.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                location = Path.Combine(Path.GetDirectoryName(location)!, Path.GetFileNameWithoutExtension(location) + ".exe");
            Process.Start(new ProcessStartInfo
            {
                FileName = location,
                UseShellExecute = true,
                Verb = "runas"
            });
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
            return IsMod(path) && (Directory.Exists(Path.Combine(path, "Survival")) || Directory.Exists(Path.Combine(path, "Scrap Mechanic")));
        }

        public static bool CreateBackUpFile(List<string> ModIdList, bool showMessage, string SourceFolder = "")
        {
            try
            {
                var PathList = new List<string>();
                var InjectFailed = false;
                var NewFolderPath = App.Settings.GameDataPath;
                if (!Directory.Exists(Constants.ModInstallBackupsPath))
                    Directory.CreateDirectory(Constants.ModInstallBackupsPath);
                if (!Directory.Exists(Constants.ModInstallBackupsPath))
                {
                    InjectFailed = true;
                    MessageBox.Show("Fatal Error", "SmModManager", MessageBoxButton.OK);
                    return InjectFailed;
                }
                var HasClickedOverWrite = !showMessage;
                foreach (var tmpModId in ModIdList)
                {
                    var ModId = tmpModId;
                    ModId = ModId.Replace(App.Settings.WorkshopPath, "");
                    ModId = ModId.Replace(Constants.ArchivesPath, "");
                    if (SourceFolder != "")
                    {
                        ModId = ModId.Replace(SourceFolder, "").Replace("\\", "");
                        PathList = GetSurvivalFolder(Path.Combine(SourceFolder, ModId), true);
                        File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), Path.Combine(SourceFolder, ModId));
                        HasClickedOverWrite = true;
                    }
                    else
                    {
                        ModId = ModId.Replace(@"\", "");
                        var numbers = "0123456789";
                        var subStrStartIndex = numbers.IndexOf(ModId[0]);
                        var numberList = "";
                        for (var newIndex = 1; newIndex <= subStrStartIndex; newIndex++)
                            numberList += ModId[newIndex].ToString();
                        subStrStartIndex = int.Parse(numberList) + int.Parse(ModId[0].ToString()) + 1;
                        ModId = ModId.Substring(subStrStartIndex);
                        Debug.WriteLine(ModId);
                        if (Directory.Exists(Path.Combine(Constants.ArchivesPath, ModId)))
                        {
                            File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), Path.Combine(Constants.ArchivesPath, ModId));
                            PathList = GetSurvivalFolder(Path.Combine(Constants.ArchivesPath, ModId), true);
                        }
                        else if (Directory.Exists(Path.Combine(App.Settings.WorkshopPath, ModId)))
                        {
                            File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), Path.Combine(App.Settings.WorkshopPath, ModId));
                            PathList = GetSurvivalFolder(Path.Combine(App.Settings.WorkshopPath, ModId), true);
                        }
                    }
                    foreach (var tempString in PathList)
                    {
                        bool archivebool;
                        if (tempString.Contains("Archives"))
                            archivebool = true;
                        else
                            archivebool = false;
                        if (tempString.Contains("Survival") || tempString.Contains("Scrap Mechanic"))
                        {
                            var temp2 = tempString.Replace(Path.Combine(App.Settings.WorkshopPath, ModId), "");
                            temp2 = temp2.Replace(Path.Combine(Constants.ArchivesPath, ModId), "");
                            temp2 = temp2.Replace(Path.Combine(SourceFolder, ModId), "");
                            var DirectoryList = temp2.Split('\\');
                            var partFull = "";
                            foreach (var Directory in DirectoryList)
                                if (archivebool)
                                {
                                    if (partFull.Length > 0)
                                        partFull = partFull + @"\" + Directory;
                                    else
                                        partFull = Directory;
                                    partFull = partFull.Replace(Constants.ArchivesPath, "");
                                    if (partFull.Contains("Scrap Mechanic"))
                                        partFull = partFull.Substring(partFull.IndexOf("Scrap Mechanic"));
                                    else if (partFull.Contains("Survival"))
                                        partFull = partFull.Substring(partFull.IndexOf("Survival"));
                                    if (Directory != DirectoryList[0])
                                    {
                                        if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, partFull)) && !partFull.Contains("."))
                                        {
                                            partFull = partFull.Replace(@"Scrap Mechanic\", @"");
                                            if (System.IO.Directory.Exists(NewFolderPath + partFull))
                                                System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, partFull));
                                        }
                                        else
                                        {
                                            if (partFull.Contains("."))
                                            {
                                                var substr = partFull.Replace(@"Scrap Mechanic\", "");
                                                var file = substr.Split('\\')[^1];
                                                var Overwrite = false;
                                                if (File.Exists(Path.Combine(Constants.ModInstallBackupsPath, substr)))
                                                {
                                                    var IsInTestMode = false;
                                                    if (partFull.Contains(".json") && IsInTestMode)
                                                    {
                                                    }
                                                    else
                                                    {
                                                        if (HasClickedOverWrite == false)
                                                        {
                                                            if (MessageBox.Show("WARNING: These mods conflict with each other, would you like to install them anyway?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                                            {
                                                                InjectFailed = true;
                                                                File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), "Warning: These Mods Conflict With Each Other, Injection Failed");
                                                                RemoveMods(false);
                                                                MessageBox.Show("Warning: These Mods Conflict With Each Other, Injection Failed", "SmModManager", MessageBoxButton.OK);
                                                                return InjectFailed;
                                                            }
                                                            HasClickedOverWrite = true;
                                                            Overwrite = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Overwrite = true;
                                                }
                                                if (Overwrite || HasClickedOverWrite)
                                                {
                                                    if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, ""))))
                                                        System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, "")));
                                                    try
                                                    {
                                                        File.Copy(Path.Combine(NewFolderPath, substr), Path.Combine(Constants.ModInstallBackupsPath, substr), false);
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (partFull.Length > 0)
                                        partFull = partFull + @"\" + Directory;
                                    else if (Directory != DirectoryList[0])
                                        partFull = Directory;
                                    if (Directory != DirectoryList[0])
                                    {
                                        if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, partFull)) && !partFull.Contains("."))
                                        {
                                            partFull = partFull.Replace(@"Scrap Mechanic\", @"");
                                            if (System.IO.Directory.Exists(NewFolderPath + partFull))
                                                System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, partFull));
                                        }
                                        else
                                        {
                                            if (partFull.Contains("."))
                                            {
                                                var substr = partFull.Replace(@"Scrap Mechanic\", "");
                                                var file = substr.Split('\\')[^1];
                                                var Overwrite = false;
                                                if (File.Exists(Path.Combine(Constants.ModInstallBackupsPath, substr)))
                                                {
                                                    var IsInTestMode = false;
                                                    if (partFull.Contains(".json") && IsInTestMode)
                                                    {
                                                    }
                                                    else
                                                    {
                                                        if (HasClickedOverWrite == false)
                                                        {
                                                            if (MessageBox.Show("WARNING: These mods conflict with each other, would you like to install them anyway?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                                            {
                                                                InjectFailed = true;
                                                                File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), "Warning: These Mods Conflict With Each Other, Injection Failed");
                                                                RemoveMods(false);
                                                                MessageBox.Show("Warning: These Mods Conflict With Each Other, Injection Failed", "SmModManager", MessageBoxButton.OK);
                                                                return InjectFailed;
                                                            }
                                                            HasClickedOverWrite = true;
                                                            Overwrite = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Overwrite = true;
                                                }
                                                if (Overwrite || HasClickedOverWrite)
                                                {
                                                    if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, ""))))
                                                        System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, "")));
                                                    try
                                                    {
                                                        File.Copy(Path.Combine(NewFolderPath, substr), Path.Combine(Constants.ModInstallBackupsPath, substr), false);
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                        }
                    }
                }
                return InjectFailed;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return true;
            }
        }

        public static List<string> GetSubFilesOnly(string MasterDir, string ScrapFolder, string sourceDirName, List<string> FilePaths)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(sourceDirName, file.Name).Replace(MasterDir, ScrapFolder);
                FilePaths.Add(temppath);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(sourceDirName, subdir.Name);
                GetSubFilesOnly(MasterDir, ScrapFolder, temppath, FilePaths);
            }
            return FilePaths;
        }

        public static void RemoveMods(bool ShowMessage)
        {
            if (Directory.Exists(Constants.ModInstallBackupsPath))
                foreach (var file in Directory.GetFiles(Constants.ModInstallBackupsPath))
                    if (file.EndsWith(".smmm"))
                        foreach (var line in File.ReadAllLines(file))
                        {
                            if (File.Exists(line))
                                File.Delete(line);
                            var topDir = "";
                            foreach (var Directory in line.Split(@"\"))
                            {
                                topDir = Path.Combine(topDir, Directory);
                                try
                                {
                                    if (!topDir.Contains(".") && System.IO.Directory.Exists(topDir))
                                        if (System.IO.Directory.GetFiles(topDir).Length == 0 && System.IO.Directory.GetDirectories(topDir).Length == 0)
                                            System.IO.Directory.Delete(topDir);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                }
                            }
                        }
            foreach (var item in CurrentMods)
            {
                var numbers = "0123456789";
                var subStrStartIndex = numbers.IndexOf(item[0]);
                var numberList = "";
                for (var newIndex = 1; newIndex <= subStrStartIndex; newIndex++)
                    numberList += item[newIndex].ToString();
                try
                {
                    subStrStartIndex = int.Parse(numberList) + int.Parse(item[0].ToString()) + 1;
                }
                catch
                {
                    subStrStartIndex = item.IndexOf("TEMP\\") + 5;
                }
                var ModLocation = "10" + item.Substring(subStrStartIndex);
                if (Directory.Exists(Constants.ModInstallBackupsPath) && File.Exists(Path.Combine(Constants.ModInstallBackupsPath, ModLocation) + ".smmm"))
                    File.Delete(Path.Combine(Constants.ModInstallBackupsPath, ModLocation) + ".smmm");
            }
            CurrentMods.Clear();
            if (ShowMessage)
            {
                var message = "";
                if (App.UserSteamId == null)
                    message = "\nPlease Sign in to enable the multiplayer feature";
                WnManager.GetWnManager.SendNotification("Sucessfully Removed All Mods" + message);
            }
            if (File.Exists(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt")))
                File.Delete(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"));
            if (Directory.Exists(Constants.ModInstallBackupsPath))
            {
                CopyDirectory(Constants.ModInstallBackupsPath, App.Settings.GameDataPath);
                Directory.Delete(Constants.ModInstallBackupsPath, true);
            }
        }

        public static List<string> GetSurvivalFolder(string sourceDirName, bool copySubDirs)
        {
            File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), sourceDirName + "\n");
            PathList.Clear();
            if (Directory.Exists(Path.Combine(sourceDirName, "Scrap Mechanic")))
            {
                DirectoryCopyToList(Path.Combine(sourceDirName, "Scrap Mechanic"), copySubDirs);
                return PathList;
            }
            if (Directory.Exists(Path.Combine(sourceDirName, "Survival")))
            {
                DirectoryCopyToList(Path.Combine(sourceDirName, "Survival"), copySubDirs);
                return PathList;
            }
            return PathList;
        }

        public static void DirectoryCopyToList(string sourceDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            var dirs = dir.GetDirectories();
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(sourceDirName, file.Name);
                PathList.Add(temppath);
            }
            if (copySubDirs)
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(sourceDirName, subdir.Name);
                    DirectoryCopyToList(temppath, copySubDirs);
                }
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

        public static FlowDocument ParseToFlowDocument(string description)
        {
            description = description.Replace("&", "&amp;");
            description = description.Replace("<", "&lt;");
            description = description.Replace(">", "&gt;");
            description = description.Replace("\"", "&quot;");
            description = description.Replace("[b]", "<Bold>");
            description = description.Replace("[/b]", "</Bold>");
            description = description.Replace("[u]", "<Underline>");
            description = description.Replace("[/u]", "</Underline>");
            description = description.Replace("[i]", "<Italic>");
            description = description.Replace("[/i]", "</Italic>");
            description = description.Replace("\n", "<LineBreak/>");
            description = FormatColor(description);
            description = description.Replace("[h1]", "<Bold FontSize=\"16\" Foreground =\"#0374ff\">");
            description = description.Replace("[/h1]", "</Bold>");
            description = description.Replace("[h2]", "<Bold FontSize=\"14\" Foreground=\"#0374ff\">");
            description = description.Replace("[/h2]", "</Bold>");
            description = description.Replace("[h3]", "<Bold FontSize=\"12\" Foreground =\"#0374ff\">");
            description = description.Replace("[/h3]", "</Bold>");
            return (FlowDocument)XamlReader.Parse($"<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><Paragraph>{description}</Paragraph></FlowDocument>");
        }

        public static string FormatColor(string str)
        {
            var ReturnString = str;
            str = str.ToLower();
            var alphebet = "abcdefghijklmnopqrstuvwxyz";
            var numbers = "0123456789";
            var newString = str;
            while (newString.Contains("#"))
            {
                var Start = newString.LastIndexOf("#");
                var IsValidColor = true;
                var intList = "";
                for (var i = Start + 1; i < Start + 7; i++)
                    if (IsValidColor)
                    {
                        intList += str[i];
                        IsValidColor = IsAnyOf(str[i], alphebet) || IsAnyOf(str[i], numbers);
                    }
                if (IsValidColor)
                {
                    ReturnString = ReturnString.Remove(Start, 7);
                    ReturnString = ReturnString.Insert(Start, "<Span Foreground=\"#" + intList.ToUpper() + "\">");
                    var index = ReturnString.Length;
                    if (ReturnString.LastIndexOf("\n") > 0)
                        index = ReturnString.LastIndexOf("\n");
                    ReturnString = ReturnString.Insert(index, "</Span>");
                }
                newString = newString.Remove(Start, 1);
                newString = newString.Insert(Start, "-");
            }
            return ReturnString;
        }

        public static bool IsAnyOf(char value, string list)
        {
            foreach (var character in list)
                if (character == value)
                    return true;
            return false;
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

        public static string PathRemoveMatch(string path, string remove, string match)
        {
            var temp = path.Remove(0, remove.Length + 1);
            return Path.Combine(match, temp);
        }

        public static bool CheckForUpdates()
        {
            if (File.Exists(Path.Combine(App.Settings.WorkshopPath, Constants.WorkshopId.ToString(), "sysVer.SMMM")))
                try
                {
                    //string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    var currentVersion = AppVersion;
                    var fileVersion = File.ReadAllText(Path.Combine(App.Settings.WorkshopPath, Constants.WorkshopId.ToString(), "sysVer.SMMM"));
                    return fileVersion != currentVersion;
                }
                catch
                {
                    return false;
                }
            return false;
        }

        public static void InstallUpdate()
        {
            try
            {
                Process.Start(Path.Combine(App.Settings.WorkshopPath, Constants.WorkshopId.ToString(), "update.exe"));
                Application.Current.Shutdown();
            }
            catch
            {
                MessageBox.Show("Update Failed :(", "MM-Updater");
            }
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

        public static void SetFolderPermission(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var directorySecurity = directoryInfo.GetAccessControl();
            var currentUserIdentity = WindowsIdentity.GetCurrent();
            var fileSystemRule = new FileSystemAccessRule(currentUserIdentity.Name,
                FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit |
                InheritanceFlags.ContainerInherit,
                PropagationFlags.None,
                AccessControlType.Allow);

            directorySecurity.AddAccessRule(fileSystemRule);
            directoryInfo.SetAccessControl(directorySecurity);
        }

    }

}