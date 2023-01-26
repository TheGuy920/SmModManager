using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CefSharp;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Handlers;
using SmModManager.Core.Models;
using Image = System.Drawing.Image;

namespace SmModManager.Graphics
{

    public partial class PgManage
    {

        public static PgManage GetPgManage;
        public static object NewestItem;
        public double Imageratio = 100;
        public double previousHeight;
        public object previousItemA;
        public object previousItemAA;
        public object previousItemC;
        public object previousItemCC;
        public double previousWidth;
        public bool IsInvokingRemove;

        public PgManage()
        {
            GetPgManage = this;
            InitializeComponent();
            CompatibleModsListWebPage.MenuHandler = new MenuHandler();
            CurrentModsListWebPage.MenuHandler = new MenuHandler();
            AvailableModsListWebPage.MenuHandler = new MenuHandler();
            ArchivedModsListWebPage.MenuHandler = new MenuHandler();
            if (App.HasFormattedAllMods)
            {
                RefreshCompatibleMods(null, null);
                RefreshAvailableMods(null, null);
                RefreshCurrentModsList();
                RefreshArchivedMods(null, null);
            }
        }

        public void RefreshAll()
        {
            RefreshCompatibleMods(null, null);
            RefreshAvailableMods(null, null);
            RefreshCurrentModsList();
            RefreshArchivedMods(null, null);
        }

        public void InjectMod(object sender, RoutedEventArgs args)
        {
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
                Directory.CreateDirectory(Constants.ModInstallBackupsPath);
            var ArchivedMods = Utilities.ArchivedMods;
            var CompatibleMods = Utilities.CompatibleMods;
            var BackUpModlist = new List<string>();
            BackUpModlist.AddRange(Utilities.CurrentMods);
            var ShowMessage = true;
            try
            {
                ShowMessage = (string)args.Source != "DontShowMessage";
            }
            catch { }

            if (ShowMessage)
            {
                if (MessageBox.Show("Injecting this mod might conflict with the already injected mods, it is recommended to backup before injecting. Do you want to continue?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;
                var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
                var ModIdLoc = binding.ModIdLocation;
                if (!BackUpModlist.Contains(ModIdLoc))
                    BackUpModlist.Add(ModIdLoc);
            }
            Utilities.RemoveMods(false);
            var BackupCompleted = !Utilities.CreateBackUpFile(BackUpModlist, ShowMessage);
            var tmpCurrentMods = new List<string>();
            foreach (var ModId in BackUpModlist)
            {
                Debug.WriteLine(ModId);
                var numbers = "0123456789";
                var subStrStartIndex = numbers.IndexOf(ModId[0]);
                var numberList = "";
                for (var newIndex = 1; newIndex <= subStrStartIndex; newIndex++)
                    numberList += ModId[newIndex].ToString();
                subStrStartIndex = int.Parse(numberList) + int.Parse(ModId[0].ToString()) + 1;
                var ModLocation = ModId.Substring(subStrStartIndex);
                if (ModId[2] + ModId[3].ToString() == "ws")
                {
                    ModLocation = Path.Combine(App.Settings.WorkshopPath, ModLocation);
                    if(!Directory.Exists(ModLocation))
                        ModLocation = Path.Combine(Constants.ArchivesPath, ModLocation);
                }
                else
                {
                    ModLocation = ModId.Substring(subStrStartIndex);
                    ModLocation = Path.Combine(Constants.ArchivesPath, ModLocation);
                }
                if (Directory.Exists(Constants.ModInstallBackupsPath))
                {
                    var ErrorCount = 0;
                    ReTry:
                    var newModId = ModLocation.Split(@"\")[^1];
                    try
                    {
                        var folder = App.Settings.GameDataPath;
                        if (Directory.Exists(Path.Combine(ModLocation, "Scrap Mechanic")))
                            folder.Replace("Scrap Mechanic", "");
                        if (!Directory.Exists(ModLocation) && ErrorCount > 0)
                        {
                            MessageBox.Show("The mod you have selected cannot be found", "Error Not Found");
                            return;
                        }
                        File.WriteAllLines(Path.Combine(Constants.ModInstallBackupsPath, "10" + newModId + ".smmm"), Utilities.GetSubFilesOnly(ModLocation, folder, ModLocation, new List<string>()));
                    }
                    catch (Exception e)
                    {
                        ErrorCount++;
                        if (ErrorCount < 5)
                        {
                            Utilities.RemoveMods(false);
                            App.GetApp.CleanFiles();
                            Utilities.ArchivedMods.Clear();
                            Utilities.CompatibleMods.Clear();
                            Utilities.CurrentMods.Clear();
                            RefreshAll();
                            goto ReTry;
                        }
                        throw new Exception("It appears some files are corrupt we are attempting to repair them please restart the app", e);
                    }
                }
                if (CompatibleMods.Contains(ModId) && BackupCompleted && !ArchivedMods.Contains(ModId))
                {
                    var newModId = ModLocation.Split(@"\")[^1];
                    var binding = Path.Combine(App.Settings.WorkshopPath, newModId);
                    Utilities.CopyDirectory(binding, Path.Combine(Constants.ArchivesPath, Utilities.GetDirectoryName(binding)));
                    ArchivedModsList.Items.Add(ModItemBinding.Create(binding));
                }
                if (ArchivedMods.Contains(ModId) && BackupCompleted)
                {
                    if (!tmpCurrentMods.Contains(ModId))
                        tmpCurrentMods.Add(ModId);
                    SurvivalFolderInjectArchive(Path.Combine(ModLocation, "Survival"), Path.Combine(ModLocation, "Scrap Mechanic"));
                }
                else if (CompatibleMods.Contains(ModId) && BackupCompleted)
                {
                    if (!tmpCurrentMods.Contains(ModId))
                        tmpCurrentMods.Add(ModId);
                    SurvivalFolderInject(Path.Combine(ModLocation, "Survival"), Path.Combine(ModLocation, "Scrap Mechanic"));
                }
            }
            if (!BackupCompleted)
            {
                if (ShowMessage)
                    WnManager.GetWnManager.Notification("Warning: These Mods Conflict With Each Other, Injection Failed");
                tmpCurrentMods.Clear();
            }
            else
            {
                var message = "";
                var height = 50;
                if (App.UserSteamId == null)
                {
                    message = "\nPlease Sign in to enable the multiplayer feature";
                    height = 75;
                }
                if (ShowMessage)
                    WnManager.GetWnManager.Notification("Injection Completed " + tmpCurrentMods.ToArray().Length + " Mods Injected" + message, height);
            }
            Utilities.CurrentMods = tmpCurrentMods;
            Utilities.SaveDataToFile();
            RefreshCurrentModsList();
            App.PageJoinFriend.UpdateCurrentMods(false);
        }

        private void SurvivalFolderInject(string TempModId, string TempModId2)
        {
            if (Directory.Exists(Path.Combine(App.Settings.WorkshopPath, TempModId2)))
                Utilities.CopyDirectory(Path.Combine(App.Settings.WorkshopPath, TempModId2), App.Settings.GameDataPath);
            else if (Directory.Exists(Path.Combine(App.Settings.WorkshopPath, TempModId)))
                Utilities.CopyDirectory(Path.Combine(App.Settings.WorkshopPath, TempModId), Path.Combine(App.Settings.GameDataPath, "Survival"));
        }

        public void SurvivalFolderInjectArchive(string TempModId, string TempModId2)
        {
            if (Directory.Exists(Path.Combine(Constants.ArchivesPath, TempModId2)))
                Utilities.CopyDirectory(Path.Combine(Constants.ArchivesPath, TempModId2), App.Settings.GameDataPath);
            else if (Directory.Exists(Path.Combine(Constants.ArchivesPath, TempModId)))
                Utilities.CopyDirectory(Path.Combine(Constants.ArchivesPath, TempModId), Path.Combine(App.Settings.GameDataPath, "Survival"));
        }

        private void ArchiveCompatibleMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            Utilities.CopyDirectory(binding.Path, Path.Combine(Constants.ArchivesPath, Utilities.GetDirectoryName(binding.Path)));
            ArchivedModsList.Items.Add(ModItemBinding.Create(binding.Path));
            ArchivedModsList.SelectedItem = ArchivedModsList.Items[^1];
            WnManager.GetWnManager.ShowManagePage(null, null);
            ArchivedModsTab.IsSelected = true;
        }

        private void DeleteCompatibleMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            try
            {
                if (binding.Path == "" || binding.Path == null)
                    throw new Exception("Path is empty");
                Directory.Delete(binding.Path, true);
            }
            catch (Exception error)
            {
                WnManager.GetWnManager.Notification("Warning: Possible error while deleting mod\n" + error.Message);
            }
            RefreshCompatibleMods(null, null);
        }

        public void RefreshCompatibleMods(object sender, RoutedEventArgs args)
        {
            if (CompatibleModsList.Items.Count > 0)
                CompatibleModsList.Items.Clear();
            Utilities.CompatibleMods.Clear();
            var tmpList = new List<string>();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Utilities.IsCompatibleMod(path) && !CompatibleModsList.Items.Contains(ModItemBinding.Create(path)) && !tmpList.Contains(ModItemBinding.Create(path).ModId.ToString()))
                    try { CompatibleModsList.Items.Add(ModItemBinding.Create(path)); tmpList.Add(ModItemBinding.Create(path).ModId.ToString()); } catch { /* nothing*/ }
            foreach (var path in Directory.GetDirectories(Path.Combine(App.Settings.UserDataPath, "Mods")))
                if (Utilities.IsCompatibleMod(path) && !CompatibleModsList.Items.Contains(ModItemBinding.Create(path)) && !tmpList.Contains(ModItemBinding.Create(path).ModId.ToString()))
                    try { CompatibleModsList.Items.Add(ModItemBinding.Create(path)); tmpList.Add(ModItemBinding.Create(path).ModId.ToString()); } catch { /* nothing*/ }
            foreach (var path in Directory.GetDirectories(Path.Combine(Constants.ArchivesPath)))
                if (Utilities.IsCompatibleMod(path) && !CompatibleModsList.Items.Contains(ModItemBinding.Create(path)) && !tmpList.Contains(ModItemBinding.Create(path).ModId.ToString()))
                    try { CompatibleModsList.Items.Add(ModItemBinding.Create(path)); tmpList.Add(ModItemBinding.Create(path).ModId.ToString()); } catch { /* nothing*/ }
            
            foreach (var mod in CompatibleModsList.Items)
            {
                var ModId = (ModItemBinding)mod;
                if(!Utilities.CompatibleMods.Contains(ModId.ModIdLocation))
                    Utilities.CompatibleMods.Add(ModId.ModIdLocation);
            }
            Utilities.SaveDataToFile();
            try
            {
                CompatibleModsList.SelectedItem = CompatibleModsList.Items[0];
            }
            catch
            {  }
        }

        private void OpenCompatibleMod(object sender, MouseButtonEventArgs args)
        {
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            if (!string.IsNullOrEmpty(binding.Url))
                Utilities.OpenBrowserUrl(binding.Url);
            else
                Process.Start(binding.Path);
        }

        private void UpdateCompatibleSelection(object sender, SelectionChangedEventArgs args)
        {
            UpdateUrl();
            var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            if (binding != null)
            {
                if (binding.Url != null)
                {
                    CompatibleModsListManualView.Visibility = Visibility.Hidden;
                    Compatible_CurrentUrl.Text = binding.Url;
                    CompatibleModsListWebPage.Address =  binding.Url;
                }
                else
                {
                    CompatibleModsListPreview.Source = binding.Preview;
                    CompatibleModsListName.Text = binding.Name;
                    CompatibleModsListDescription.Document = Utilities.ParseToFlowDocument(binding.Description);
                }
            }
        }

        public void UpdateTabSelection(object sender, SelectionChangedEventArgs e)
        {
            if (App.WindowManager != null) { App.WindowManager.RunVoidList(null, null); }
        }

        public void UpdatePreviewImages()
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            if (previousWidth != App.WindowManager.ActualWidth || previousHeight != App.WindowManager.ActualHeight)
            {
                if (App.WindowManager.ActualWidth < App.WindowManager.ActualHeight + 550)
                    Imageratio = Math.Clamp(Math.Clamp(App.WindowManager.ActualWidth + 100, 500, 1650) - 800, 100, 1650);
                else
                    Imageratio = Math.Clamp(Math.Clamp(App.WindowManager.ActualHeight + 500, 500, 1200) - 500, 100, 1200);
            }
            if (binding != null && (previousWidth != App.WindowManager.ActualWidth || previousHeight != App.WindowManager.ActualHeight || previousItemA != binding))
            {
                var img = Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                AvailableModsListScroll.Height = App.WindowManager.ActualHeight;
                AvailableModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 500, 10, 700);
                AvailableModsListDescription.Height = Count(binding.Description, '\n', Math.Clamp((int)AvailableModsListDescription.Width - 50, 1, 999)) * 30 + 50;
                AvailableModsListPreview.Height = Math.Clamp(height / width * Imageratio, 10, 400);
                AvailableModsListPreview.Width = Math.Clamp(width / height * Imageratio, 10, 800);
                ArchiveAvailableButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                DeleteAvailableButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                AvailableModsListWebPage.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                AvailableModsListWebPage.Width = Math.Clamp(App.WindowManager.ActualWidth - 450, 10, 2000);
            }
            previousItemA = binding;
            binding = (ModItemBinding)CompatibleModsList.SelectedItem;
            if (binding != null && (previousWidth != App.WindowManager.ActualWidth || previousHeight != App.WindowManager.ActualHeight || previousItemC != binding))
            {
                var img = Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                CompatibleModsListScroll.Height = App.WindowManager.ActualHeight;
                CompatibleModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 500, 10, 700);
                CompatibleModsListDescription.Height = Count(binding.Description, '\n', Math.Clamp((int)CompatibleModsListDescription.Width - 50, 1, 999)) * 30 + 50;
                CompatibleModsListPreview.Height = Math.Clamp(height / width * Imageratio, 10, 400);
                CompatibleModsListPreview.Width = Math.Clamp(width / height * Imageratio, 10, 800);
                InjectButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                ArchiveCompatibleButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                DeleteCompatibleButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                CompatibleModsListWebPage.Width = Math.Clamp(App.WindowManager.ActualWidth - 450, 10, 2000);
            }
            previousItemC = binding;
            binding = (ModItemBinding)CurrentModsList.SelectedItem;
            if (binding != null && (previousWidth != App.WindowManager.ActualWidth || previousHeight != App.WindowManager.ActualHeight || previousItemCC != binding))
            {
                var img = Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                CurrentModsListScroll.Height = App.WindowManager.ActualHeight;
                CurrentModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 500, 10, 700);
                CurrentModsListDescription.Height = Count(binding.Description, '\n', Math.Clamp((int)CurrentModsListDescription.Width - 50, 1, 999)) * 30 + 50;
                CurrentModsListPreview.Height = Math.Clamp(height / width * Imageratio, 10, 400);
                CurrentModsListPreview.Width = Math.Clamp(width / height * Imageratio, 10, 800);
                DeleteCurrentButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                CurrentModsListWebPage.Width = Math.Clamp(App.WindowManager.ActualWidth - 450, 10, 2000);
            }
            previousItemCC = binding;
            binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            if (binding != null && (previousWidth != App.WindowManager.ActualWidth || previousHeight != App.WindowManager.ActualHeight || previousItemAA != binding))
            {
                var img = Image.FromFile(binding.ImagePath);
                var width = (double)img.Width - img.Width / 7;
                var height = (double)img.Height;
                ArchivedModsListScroll.Height = App.WindowManager.ActualHeight;
                ArchivedModsListDescription.Width = Math.Clamp(App.WindowManager.ActualWidth - 500, 10, 700);
                ArchivedModsListDescription.Height = Count(binding.Description, '\n', Math.Clamp((int)ArchivedModsListDescription.Width - 50, 1, 999)) * 30 + 50;
                ArchivedModsListPreview.Height = Math.Clamp(height / width * Imageratio, 10, 400);
                ArchivedModsListPreview.Width = Math.Clamp(width / height * Imageratio, 10, 800);
                DeleteArchivedButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                UnarchiveButton.Width = Math.Clamp(width / height * Imageratio + 250, 300, 600) / 2 - 100;
                ArchivedModsListWebPage.Width = Math.Clamp(App.WindowManager.ActualWidth - 450, 10, 2000);
            }
            previousItemAA = binding;
            if (CompatibleModsTab.IsSelected)
                CompatibleModsTab.Foreground = new SolidColorBrush(Colors.Black);
            else
                CompatibleModsTab.Foreground = new SolidColorBrush(Colors.White);
            if (AvailableModsTab.IsSelected)
                AvailableModsTab.Foreground = new SolidColorBrush(Colors.Black);
            else
                AvailableModsTab.Foreground = new SolidColorBrush(Colors.White);
            if (CurrentModsTab.IsSelected)
                CurrentModsTab.Foreground = new SolidColorBrush(Colors.Black);
            else
                CurrentModsTab.Foreground = new SolidColorBrush(Colors.White);
            if (ArchivedModsTab.IsSelected)
                ArchivedModsTab.Foreground = new SolidColorBrush(Colors.Black);
            else
                ArchivedModsTab.Foreground = new SolidColorBrush(Colors.White);
            if (CompatibleModsList.SelectedItem == null && CompatibleModsList.Items.Count > 0)
                CompatibleModsList.SelectedItem = CompatibleModsList.Items[0];
            if (AvailableModsList.SelectedItem == null && AvailableModsList.Items.Count > 0)
                AvailableModsList.SelectedItem = AvailableModsList.Items[0];
            if (CurrentModsList.SelectedItem == null && CurrentModsList.Items.Count > 0)
                CurrentModsList.SelectedItem = CurrentModsList.Items[0];
            if (ArchivedModsList.SelectedItem == null && ArchivedModsList.Items.Count > 0)
                ArchivedModsList.SelectedItem = ArchivedModsList.Items[0];
            previousWidth = App.WindowManager.ActualWidth;
            previousHeight = App.WindowManager.ActualHeight;
        }

        public int Count(string str, char Char, int limit)
        {
            var count = 0;
            var previousPos = 0;
            foreach (var NewChar in str)
                if (NewChar == Char)
                    count += 1 * (1 + (int)Math.Floor((decimal)previousPos / limit));

            return count;
        }

        private void ArchiveAvailableMod(object sender, RoutedEventArgs args)
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            Utilities.CopyDirectory(binding.Path, Path.Combine(Constants.ArchivesPath, Utilities.GetDirectoryName(binding.Path)));
            ArchivedModsList.Items.Add(ModItemBinding.Create(binding.Path));
            ArchivedModsList.SelectedItem = ArchivedModsList.Items[^1];
            WnManager.GetWnManager.ShowManagePage(null, null);
            ArchivedModsTab.IsSelected = true;
        }

        private void DeleteAvailableMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            try
            {
                if (string.IsNullOrEmpty(binding.Path))
                    throw new Exception("Path is empty");
                Directory.Delete(binding.Path, true);
            }
            catch { WnManager.GetWnManager.SendNotification("Possible error while deleting mod\nMaybe it does not exist?"); }
            RefreshAvailableMods(null, null);
        }

        public void RefreshAvailableMods(object sender, RoutedEventArgs args)
        {
            AvailableModsList.Items.Clear();
            foreach (var path in Directory.GetDirectories(App.Settings.WorkshopPath))
                if (Utilities.IsMod(path))
                    try { AvailableModsList.Items.Add(ModItemBinding.Create(path)); }
                    catch { }
            foreach (var path in Directory.GetDirectories(Path.Combine(App.Settings.UserDataPath, "Mods")))
                if (Utilities.IsMod(path))
                    try { AvailableModsList.Items.Add(ModItemBinding.Create(path)); }
                    catch { }
            try
            {
                AvailableModsList.SelectedItem = AvailableModsList.Items[0];
            }
            catch { }
        }

        private void OpenAvailableMod(object sender, MouseButtonEventArgs args)
        {
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            if (!string.IsNullOrEmpty(binding.Url))
                Utilities.OpenBrowserUrl(binding.Url);
            else
                Utilities.OpenExplorerUrl(binding.Path);
        }

        private void UpdateAvailableModSelection(object sender, SelectionChangedEventArgs args)
        {
            UpdateUrl();
            var binding = (ModItemBinding)AvailableModsList.SelectedItem;
            if (binding != null)
            {
                if (binding.Url != null)
                {
                    AvailableModsListManualView.Visibility = Visibility.Hidden;
                    Available_CurrentUrl.Text = binding.Url;
                    AvailableModsListWebPage.Address = binding.Url;
                }
                else
                {
                    AvailableModsListManualView.Visibility = Visibility.Visible;
                    AvailableModsListPreview.Source = binding.Preview;
                    AvailableModsListName.Text = binding.Name;
                    AvailableModsListDescription.Document = Utilities.ParseToFlowDocument(binding.Description);
                }
            }
        }

        public void UpdateCurrentModSelection(object sender, SelectionChangedEventArgs args)
        {
            UpdateUrl();
            var binding = (ModItemBinding)CurrentModsList.SelectedItem;
            if (binding != null)
            {
                if (binding.Url != null)
                {
                    CurrentModsListManualView.Visibility = Visibility.Hidden;
                    Current_CurrentUrl.Text = binding.Url;
                    CurrentModsListWebPage.Address = binding.Url;
                }
                else
                {
                    CurrentModsListManualView.Visibility = Visibility.Visible;
                    CurrentModsListPreview.Source = binding.Preview;
                    CurrentModsListName.Text = binding.Name;
                    CurrentModsListDescription.Document = Utilities.ParseToFlowDocument(binding.Description);
                }
            }
            else
            {
                CurrentModsListPreview.Source = null;
                CurrentModsListName.Text = null;
                CurrentModsListDescription.Document = new FlowDocument();
            }
        }

        public void InvokeRemoveAllCurrentMods(object sender, RoutedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                IsInvokingRemove = true;
                args.Source = "DontShowMessage";
                RemoveAllCurrentMods(sender, args);
            });
        }

        public void RemoveAllCurrentMods(object sender, RoutedEventArgs args)
        {
            //...
            try
            {
                if ((string)args.Source == "DontShowMessage")
                    Utilities.RemoveMods(false);
                else
                    Utilities.RemoveMods(true);
            }
            catch
            {
                Utilities.RemoveMods(true);
            }
            CurrentModsList.Items.Clear();
            Utilities.CurrentMods.Clear();
            Utilities.SaveDataToFile();
            CurrentModsList.SelectedItem = null;
            UpdateCurrentModSelection(null, null);
            CurrentModsListWebPage.Visibility = Visibility.Hidden;
            DeleteCurrentButton.Visibility = Visibility.Hidden;
            App.PageJoinFriend.UpdateCurrentMods(false);
            IsInvokingRemove = false;
        }

        public void OpenCurrentMod(object sender, MouseButtonEventArgs args)
        {
            var binding = (ModItemBinding)CurrentModsList.SelectedItem;
            if (binding != null)
            {
                if (!string.IsNullOrEmpty(binding.Url))
                    Utilities.OpenBrowserUrl(binding.Url);
                else
                    Utilities.OpenExplorerUrl(binding.Path);
            }
        }

        public void DeleteCurrentMod(object sender, RoutedEventArgs args)
        {
            var mod = (ModItemBinding)CurrentModsList.SelectedItem;
            Utilities.CurrentMods.Remove(mod.ModIdLocation);
            var obj = new RoutedEventArgs
            {
                RoutedEvent = args.RoutedEvent,
                Source = "DontShowMessage"
            };
            InjectMod(null, obj);
            WnManager.GetWnManager.Notification("Sucessfully Removed " + mod.Name);
        }

        public void InvokeRefreshCurrentModsList()
        {
            Dispatcher.Invoke(() =>
            {
                RefreshCurrentModsList();
            });
        }
        public void InvokeRefreshAll()
        {
            Dispatcher.Invoke(() =>
            {
                RefreshAll();
            });
        }

        public void RefreshCurrentModsList()
        {
            Dispatcher.Invoke(()=> { 
            CurrentModsList.Items.Clear();
            if (Directory.Exists(Constants.ModInstallBackupsPath))
                foreach (var file in Directory.GetFiles(Constants.ModInstallBackupsPath))
                {
                    DeleteCurrentButton.Visibility = Visibility.Visible;
                    var ModId = file.Split('\\')[^1];
                    if (!ModId.Contains(".txt"))
                    {
                        ModId = ModId.Substring(2);
                        ModId = ModId.Replace(".smmm", "");
                        if (Directory.Exists(Path.Combine(Constants.ArchivesPath, ModId)))
                        {
                            if (Utilities.IsMod(Path.Combine(Constants.ArchivesPath, ModId)))
                                CurrentModsList.Items.Add(ModItemBinding.Create(Path.Combine(Constants.ArchivesPath, ModId)));
                        }
                        else if (Directory.Exists(Path.Combine(App.Settings.WorkshopPath, ModId)))
                        {
                            if (Utilities.IsMod(Path.Combine(App.Settings.WorkshopPath, ModId)))
                                CurrentModsList.Items.Add(ModItemBinding.Create(Path.Combine(App.Settings.WorkshopPath, ModId)));
                        }
                    }
                }
            if (CurrentModsList.Items.Count <= 0)
            {
                CurrentModsListWebPage.Visibility = Visibility.Hidden;
                DeleteCurrentButton.Visibility = Visibility.Hidden;
            }
            else
            {
                CurrentModsListWebPage.Visibility = Visibility.Visible;
                DeleteCurrentButton.Visibility = Visibility.Visible;
            }
            });
            try
            {
                CurrentModsList.SelectedItem = CurrentModsList.Items[0];
            }
            catch { }
        }

        private void UnarchiveMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to unarchive this mod?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            try
            {
                Directory.Move(binding.Path, Path.Combine(App.Settings.UserDataPath, "Mods", Utilities.GetDirectoryName(binding.Path)));
            }
            catch
            {
                Utilities.CopyDirectory(binding.Path, Path.Combine(App.Settings.UserDataPath, "Mods", Utilities.GetDirectoryName(binding.Path)));
                Directory.Delete(binding.Path, true);
            }
            RefreshArchivedMods(null, null);
            RefreshCompatibleMods(null, null);
            RefreshAvailableMods(null, null);
        }

        private void DeleteArchivedMod(object sender, RoutedEventArgs args)
        {
            if (MessageBox.Show("Are you sure that you want to delete this archive?", "SmModManager", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            try
            {
                if (binding.Path == "" || binding.Path == null)
                    throw new Exception("Path is empty");
                if (Directory.Exists(Path.Combine(Constants.ArchivesPath, binding.ModId.ToString())))
                    Directory.Delete(Path.Combine(Constants.ArchivesPath, binding.ModId.ToString()), true);
                else
                    throw new Exception("Cannot find path to Arhived Mod");
            }
            catch { WnManager.GetWnManager.SendNotification("Possible error while deleting archived mod\nMaybe it is being used by another process?"); }
            RefreshArchivedMods(null, null);
        }

        public void RefreshArchivedMods(object sender, RoutedEventArgs args)
        {
            ArchivedModsList.Items.Clear();
            if (!Directory.Exists(Constants.ArchivesPath))
            {
                Directory.CreateDirectory(Constants.ArchivesPath);
                return;
            }
            foreach (var path in Directory.GetDirectories(Constants.ArchivesPath))
                if (Utilities.IsMod(path))
                    try
                    {
                        ArchivedModsList.Items.Add(ModItemBinding.Create(path));
                        NewestItem = ModItemBinding.Create(path);
                    }
                    catch { }
            Utilities.ArchivedMods.Clear();
            foreach (var mod in ArchivedModsList.Items)
            {
                var ModId = (ModItemBinding)mod;
                if (!Utilities.ArchivedMods.Contains(ModId.ModIdLocation))
                    Utilities.ArchivedMods.Add(ModId.ModIdLocation);
            }
            Utilities.SaveDataToFile();
            try
            {
                ArchivedModsList.SelectedItem = ArchivedModsList.Items[0];
            }
            catch { }
        }

        private void UpdateArchivedSelection(object sender, SelectionChangedEventArgs args)
        {
            UpdateUrl();
            var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
            if (binding != null)
            {
                if (binding.Url != null)
                {
                    ArchivedModsListManualView.Visibility = Visibility.Hidden;
                    Archived_CurrentUrl.Text = binding.Url;
                    ArchivedModsListWebPage.Address = binding.Url;
                }
                else
                {
                    ArchivedModsListManualView.Visibility = Visibility.Visible;
                    ArchivedModsListPreview.Source = binding.Preview;
                    ArchivedModsListName.Text = binding.Name;
                    ArchivedModsListDescription.Document = Utilities.ParseToFlowDocument(binding.Description);
                }
            }
        }

        public void MoveForward(object sender, RoutedEventArgs args)
        {
            if (CompatibleModsTab.IsSelected)
                if (CompatibleModsListWebPage.CanGoForward)
                    CompatibleModsListWebPage.Forward();
            if (AvailableModsTab.IsSelected)
                if (AvailableModsListWebPage.CanGoForward)
                    AvailableModsListWebPage.Forward();
            if (ArchivedModsTab.IsSelected)
                if (ArchivedModsListWebPage.CanGoForward)
                    ArchivedModsListWebPage.Forward();
            if (CurrentModsTab.IsSelected)
                if (CurrentModsListWebPage.CanGoForward)
                    CurrentModsListWebPage.Forward();
        }

        public void MoveBackward(object sender, RoutedEventArgs args)
        {
            if (CompatibleModsTab.IsSelected)
                if (CompatibleModsListWebPage.CanGoBack)
                    CompatibleModsListWebPage.Back();
            if (AvailableModsTab.IsSelected)
                if (AvailableModsListWebPage.CanGoBack)
                    AvailableModsListWebPage.Back();
            if (ArchivedModsTab.IsSelected)
                if (ArchivedModsListWebPage.CanGoBack)
                    ArchivedModsListWebPage.Back();
            if (CurrentModsTab.IsSelected)
                if (CurrentModsListWebPage.CanGoBack)
                    CurrentModsListWebPage.Back();
        }

        public void GoHome(object sender, RoutedEventArgs args)
        {
            try
            {
                if (CompatibleModsTab.IsSelected)
                {
                    var binding = (ModItemBinding)CompatibleModsList.SelectedItem;
                    CompatibleModsListWebPage.Load(binding.Url);
                }
                if (AvailableModsTab.IsSelected)
                {
                    var binding = (ModItemBinding)AvailableModsList.SelectedItem;
                    AvailableModsListWebPage.Load(binding.Url);
                }
                if (ArchivedModsTab.IsSelected)
                {
                    var binding = (ModItemBinding)ArchivedModsList.SelectedItem;
                    ArchivedModsListWebPage.Load(binding.Url);
                }
                if (CurrentModsTab.IsSelected)
                {
                    var binding = (ModItemBinding)CurrentModsList.SelectedItem;
                    CurrentModsListWebPage.Load(binding.Url);
                }
            }
            catch { }
        }

        public void RemoveText(object sender, EventArgs e)
        {
            var myTxtbx = (TextBox)sender;
            if (myTxtbx.Text == (string)System.Windows.Application.Current.FindResource("searchfilter"))
            {
                myTxtbx.Text = (string)System.Windows.Application.Current.FindResource("searchfilter");
                myTxtbx.Foreground = Brushes.Black;
            }
        }

        public void AddText(object sender, EventArgs e)
        {
            var myTxtbx = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(myTxtbx.Text))
            {
                myTxtbx.Text = "";
                myTxtbx.Foreground = Brushes.Gray;
            }
        }

        public void UpdateUrl()
        {
            if (CompatibleModsTab.IsSelected)
            {
                if (CompatibleModsList.Items.Count <= 0)
                    CompatibleModsListScroll.Visibility = Visibility.Hidden;
                else
                    CompatibleModsListScroll.Visibility = Visibility.Visible;
            }
            if (AvailableModsTab.IsSelected)
            {
                if (AvailableModsList.Items.Count <= 0)
                    AvailableModsListScroll.Visibility = Visibility.Hidden;
                else
                    AvailableModsListScroll.Visibility = Visibility.Visible;
            }
            if (ArchivedModsTab.IsSelected)
            {
                if (ArchivedModsList.Items.Count <= 0)
                    ArchivedModsListScroll.Visibility = Visibility.Hidden;
                else
                    ArchivedModsListScroll.Visibility = Visibility.Visible;
            }
            if (CurrentModsTab.IsSelected)
            {
                if (CurrentModsList.Items.Count <= 0)
                    CurrentModsListScroll.Visibility = Visibility.Hidden;
                else
                    CurrentModsListScroll.Visibility = Visibility.Visible;
            }
        }

        public void SearchTextChanged(object sender, TextChangedEventArgs e)
        {

            var TextBoxItem = (TextBox)sender;
            if (CompatibleModsTab != null)
                if (CompatibleModsTab.IsSelected)
                    if (TextBoxItem.Text.Length > 0)
                        Sort(CompatibleModsList, TextBoxItem.Text);
            if (AvailableModsTab != null)
                if (AvailableModsTab.IsSelected)
                    if (TextBoxItem.Text.Length > 0)
                        Sort(AvailableModsList, TextBoxItem.Text);
            if (ArchivedModsTab != null)
                if (ArchivedModsTab.IsSelected)
                    if (TextBoxItem.Text.Length > 0)
                        Sort(ArchivedModsList, TextBoxItem.Text);
            if (CurrentModsTab != null)
                if (CurrentModsTab.IsSelected)
                    if(TextBoxItem.Text.Length > 0)
                        Sort(CurrentModsList, TextBoxItem.Text);
        }

        public void Sort(ListBox list, string values)
        {
            if (values != "")
            {
                var NewList = new List<Vector2>();
                var CurrentIndex = 0;
                foreach (var item in list.Items)
                {
                    var binding = (ModItemBinding)item;
                    var score = 0;
                    var previousMatch = 1;
                    var CharacterIndex = 0;
                    foreach (var TopVal in binding.Name.ToLower())
                    {
                        CharacterIndex++;
                        var IsEqual = false;
                        foreach (var value in values.ToLower())
                            if (value == TopVal)
                            {
                                score = score + previousMatch;
                                IsEqual = true;
                                break;
                            }
                        if (IsEqual)
                            previousMatch += Math.Clamp(10 / CharacterIndex, 1, 10) * 2;
                        else
                            previousMatch = 1;
                    }
                    var vector = new Vector2(CurrentIndex, score);
                    NewList.Add(vector);
                    CurrentIndex++;
                }
                NewList.Sort((v1, v2) => (1000 - v1.Y).CompareTo(1000 - v2.Y));
                var newList = new List<object>();
                foreach (var item in NewList)
                    newList.Add(list.Items[(int)item.X]);
                list.Items.Clear();
                foreach (var item in newList)
                    list.Items.Add(item);
                list.SelectedItem = list.Items[0];
            }
            else
            {
                RefreshAll();
            }
        }
    }

}