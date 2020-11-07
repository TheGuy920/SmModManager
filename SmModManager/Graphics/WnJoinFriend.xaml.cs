using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CefSharp;
using Newtonsoft.Json;
using SmModManager.Core;
using SmModManager.Core.Bindings;
using SmModManager.Core.Models;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

namespace SmModManager.Graphics
{

    /// <summary>
    ///     Interaction logic for JoinFriend.xaml
    /// </summary>
    public partial class WnJoinFriend : Window
    {

        public static WnJoinFriend GetWnJoinFriend;
        public static string PastebinData = Path.Combine(Constants.Resources, "Api", "Pastebin", "pastebin.txt");
        public static string SteamCmd = Path.Combine(Constants.Resources, "Api", "SteamCMD", "steamcmd.exe");
        public static string RunBackBlaze = Path.Combine(Constants.Resources, "Api", "BackBlaze", "b2.bat");
        public static string GetCode = Path.Combine(Constants.Resources, "Api", "Pastebin", "PastebinGet.exe");
        public static string ShareCode = Path.Combine(Constants.Resources, "Api", "Pastebin", "PastebinShare.exe");
        public static string BackBlazeFolder = Path.Combine(Constants.Resources, "Api", "BackBlaze");
        public static string TmpModFolder = Path.Combine(Constants.CachePath, "TEMP");
        public static string PlayerListBucket = "SmServerList";
        public static string ServerListBucket = "SmPlayerList";
        public static string BackBlazeApiKey = "b2f43e974cff 0027e35a1f2600a7c06de4936ea8d91c7784266ab7";
        public static List<int> QueueMods = new List<int>();
        public string BrowserHTML;
        public string htmlSite = "";
        public bool IsLoadingInstall;
        public bool ThreadStatus;
        public RoutedEventArgs tmpArgs;

        public WnJoinFriend()
        {
            var CursorPos = System.Windows.Forms.Cursor.Position;
            GetWnJoinFriend = this;
            InitializeComponent();
            try
            {
                IsWindowOpen<WnJoinFriend>("Join Friend");
            }
            catch
            {
                WindowState = WindowState.Normal;
                var thread = new Thread(BringWindowIntoView)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                thread.Start();
                JoinGame();
            }
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Dispatcher.Invoke(() => { PgMultiplayer.GetPgMultiplayer.InvokeSetOverlay(); });
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke(() => { Close(); });
        }

        public void BringWindowIntoView()
        {
            try
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        BringIntoView();
                        Focus();
                        Activate();
                    });
                    Thread.Sleep(100);
                }
            }
            catch
            {
            }
        }

        public void RefreshCurrentModsStatus()
        {
            if (!ThreadStatus)
                UpdateCurrentMods();
            else
                Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("waitforupdatetofinish")); });
        }

        public void UpdateCurrentMods(bool ShowEvenIfFail = true)
        {
            var thread = new Thread(StartThreading)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            if (App.UserSteamId != null && !ShowEvenIfFail)
                thread.Start();
            else if (ShowEvenIfFail)
                thread.Start();
        }

        public void StartThreading()
        {
            UploadStatus(App.UserSteamId, FormatCurrentMods());
        }

        public void UploadStatus(string SteamId, string ModInfo)
        {
            ThreadStatus = true;
            if (SteamId != null)
            {
                Debug.WriteLine(QueueMods.Count);
                var StatusQueueId = QueueMods.Count + 1;
                foreach (var item in QueueMods)
                    if (item >= StatusQueueId)
                        StatusQueueId = item + 1;
                if (QueueMods.Count > 2)
                {
                    Debug.WriteLine("Queue Greater than 2");
                    QueueMods.RemoveAt(1);
                }
                else
                {
                    QueueMods.Add(StatusQueueId);
                }
                while (QueueMods.IndexOf(StatusQueueId) != 0)
                {
                    Thread.Sleep(250);
                    if (QueueMods.IndexOf(StatusQueueId) < 0)
                    {
                        Debug.WriteLine("Queue Removed");
                        return;
                    }
                }
                if (File.Exists(Path.Combine(BackBlazeFolder, SteamId) + ".txt"))
                    while (IsFileLocked(new FileInfo(Path.Combine(BackBlazeFolder, SteamId + ".txt"))))
                        Thread.Sleep(200);
                File.Delete(Path.Combine(BackBlazeFolder, SteamId) + ".txt");
                var Contents = new FileDetailsModel();
                if (File.Exists(Path.Combine(BackBlazeFolder, "FileDetails.json")))
                {
                    while (IsFileLocked(new FileInfo(Path.Combine(BackBlazeFolder, "FileDetails.json"))))
                        Thread.Sleep(200);
                    try
                    {
                        Contents = FileDetailsModel.Load(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                    }
                    catch
                    {
                        var details2 = File.ReadAllText(Path.Combine(BackBlazeFolder, "FileDetails.json")).Replace("URL", "//URL");
                        File.Delete(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                        File.WriteAllText(Path.Combine(BackBlazeFolder, "FileDetails.json"), details2);
                        Contents = FileDetailsModel.Load(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                    }
                    File.Delete(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                }
                var DeleteFile = "";
                if (Contents != null)
                    if (Contents.FileId != null)
                        DeleteFile = Contents.FileId;
                File.WriteAllText(Path.Combine(BackBlazeFolder, SteamId) + ".txt", ModInfo);
                var p2Info = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"" + RunBackBlaze + "\" " + SteamId + ".txt " + ServerListBucket + " " + BackBlazeApiKey + " " + DeleteFile,
                    CreateNoWindow = true
                };
                Process.Start(p2Info);
                while (IsFileLocked(new FileInfo(Path.Combine(BackBlazeFolder, "FileDetails.json"))))
                    Thread.Sleep(200);
                var details = File.ReadAllText(Path.Combine(BackBlazeFolder, "FileDetails.json")).Replace("URL", "//URL");
                File.Delete(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                File.WriteAllText(Path.Combine(BackBlazeFolder, "FileDetails.json"), details);
                var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + App.UserSteamId + ".txt";
                var htmlSite = "";
                using (var client = new WebClient())
                {
                    try
                    {
                        htmlSite = client.DownloadString(URL);
                    }
                    catch
                    {
                        htmlSite = "404";
                        Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("unabletoverifyupdate")); });
                    }
                    if (ModInfo != htmlSite.Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "").Replace("</pre></body></html>", ""))
                        Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("synctime")); });
                    else
                        Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("onlinestatusupdated")); });
                }
                if (QueueMods.Count > 0)
                    QueueMods.RemoveAt(0);
            }
            else
            {
                Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("pleasesignin")); });
            }
            ThreadStatus = false;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        public string FormatCurrentMods()
        {
            var ModList = "";
            foreach (var mod in Utilities.CurrentMods)
                if (!mod.Contains("Cache\\TEMP"))
                {
                    var numbers = "0123456789";
                    var subStrStartIndex = numbers.IndexOf(mod[0]);
                    var numberList = "";
                    for (var newIndex = 1; newIndex <= subStrStartIndex; newIndex++)
                        numberList += mod[newIndex].ToString();
                    var URL = mod.Substring(subStrStartIndex + 1);
                    var oldSub = subStrStartIndex + 1;
                    subStrStartIndex = int.Parse(numberList) + int.Parse(mod[0].ToString()) + 1;
                    URL = URL.Remove(subStrStartIndex - oldSub);
                    var ModId = mod.Substring(subStrStartIndex);
                    ModList += URL + "," + ModId + ";";
                }
                else
                {
                    var binding = ModItemBinding.Create(mod);
                    var URL = binding.Url;
                    var ModId = mod.Split("\\")[^1];
                    ModList += URL + "," + ModId + ";";
                }
            return "[" + ModList + "]";
        }

        public void JoinGame()
        {
            Width = 550;
            Height = 625;
            SizeToContent = SizeToContent.WidthAndHeight;
            Top = (Screen.PrimaryScreen.Bounds.Height - 625) / 2;
            Left = (Screen.PrimaryScreen.Bounds.Width - 550) / 2;
            PreLoadBox.Visibility = Visibility.Hidden;
            StatusBox.Visibility = Visibility.Visible;
            StatusTextBox.Text = (string)Application.Current.FindResource("loadinguserinformation");

            if (Directory.Exists(TmpModFolder))
                Directory.Delete(TmpModFolder, true);
            Directory.CreateDirectory(TmpModFolder);
            Dispatcher.Invoke(() => { PgMultiplayer.GetPgMultiplayer.HomePageSite.Load(""); });

            var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + PgMultiplayer.GetPgMultiplayer.SteamUserId + ".txt";
            using (var client = new WebClient())
            {
                try
                {
                    htmlSite = client.DownloadString(URL);
                }
                catch
                {
                    htmlSite = "404";
                }
            }
            if (htmlSite != "404")
            {
                Debug.WriteLine("Online Status Found");
            }
            else
            {
                Dispatcher.Invoke(() => { WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("onlinestatusnotfound")); });
                Close();
                return;
            }
            new Thread(StartUserDataCollection).Start();
            PgMultiplayer.GetPgMultiplayer.InvokeSetOverlay(true);
        }

        public void StartUserDataCollection()
        {
            var UserHtml = "";
            using (var client = new WebClient())
            {
                var source = "";
                Dispatcher.Invoke(() => { PgMultiplayer.GetPgMultiplayer.GotoAddress("https://steamcommunity.com/profiles/" + PgMultiplayer.GetPgMultiplayer.SteamUserId); });
                var boolean = true;
                while (boolean)
                    Dispatcher.Invoke(() => { boolean = PgMultiplayer.GetPgMultiplayer.GetAddress().Contains("friends") || !PgMultiplayer.GetPgMultiplayer.WebPageLoadFinished; });
                Dispatcher.Invoke(async () => { source = await PgMultiplayer.GetPgMultiplayer.HomePageSite.GetSourceAsync(); });
                while (source == "")
                {
                }
                UserHtml = source;
                Debug.WriteLine("https://steamcommunity.com/profiles/" + PgMultiplayer.GetPgMultiplayer.SteamUserId);
                var ImageUrl = "";
                var ImageUrl2 = "";
                if (UserHtml.Contains("profile_avatar_frame"))
                {
                    ImageUrl2 = UserHtml.Split("class=\"playerAvatarAutoSizeInner\"")[1].Split("</div>")[0].Split("img src=\"")[1].Split("\">")[0];
                    ImageUrl = UserHtml.Split("class=\"playerAvatarAutoSizeInner\"")[1].Split("</div>")[1].Split("img src=\"")[1].Split("\">")[0];
                }
                else
                {
                    ImageUrl = UserHtml.Split("class=\"playerAvatarAutoSizeInner\"")[1].Split("img src=\"")[1].Split("\">")[0];
                }
                Dispatcher.Invoke(() => { UserName.Text = UserHtml.Split("actual_persona_name\">")[1].Split("</span>")[0]; });
                var text = UserHtml.Split("responsive_status_info")[1].Split("responsive_count_link_area")[0].Replace(">", "").Replace("<", "").Replace("</", "").Replace("div", "").Replace("span", "").Replace("class", "").Replace("\"", "");
                if (text.Contains("profile_ban_status"))
                    text = text.Split("profile_ban_status")[0];
                text = text.Replace("profile_in_game", " ");
                text = text.Replace("=", " ");
                text = text.Replace("header", " ");
                text = text.Replace("name", " ");
                text = text.Replace("_", " ");
                text = text.Replace("\\", " ");
                text = text.Replace("/", " ");
                text = text.Replace("	", " ");
                text = text.Replace("persona online", " ");
                text = text.Replace("in-game", " ");
                text = text.Replace("persona", " ");
                text = text.Replace(Environment.NewLine, " ");
                text = text.Replace("\n", " ");
                text = text.Replace("offline", " ");
                while (text.Contains("  "))
                    text = text.Replace("  ", " ");
                while (text[0] == ' ')
                    text = text.Substring(1);
                text = text.Replace("btn green white innerfade btn small thin", "");
                text = text.Replace("joingame a href steam:", "");
                text = text.Replace("joingame", "");
                text = text.Replace("steam:", "");
                text = text.Replace("Join Game", "");
                text = text.Replace(" a ", "");
                text = text.Replace("Currently In-Game ", "Currently In-Game" + Environment.NewLine);
                text = text.Replace("Currently Offline ", "Currently Offline" + Environment.NewLine);
                Dispatcher.Invoke(() =>
                {
                    UserStatus.Text = text;
                    try
                    {
                        BorderImage.Source = new BitmapImage(new Uri(ImageUrl2, UriKind.Absolute));
                    }
                    catch
                    {
                    }
                    Image.Source = new BitmapImage(new Uri(ImageUrl, UriKind.Absolute));
                });
            }
            var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + PgMultiplayer.GetPgMultiplayer.SteamUserId + ".txt";
            var UserModList = htmlSite.Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "").Replace("</pre></body></html>", "").Replace("[", "").Replace("]", "").Split(";");
            var index = 0;
            foreach (var item in UserModList)
            {
                if (index != UserModList.Length - 1)
                    try
                    {
                        var ModName = "Unknown";
                        var PageUrl = "";
                        var ItemId = item.Split(",")[1];
                        if (item.Split(",")[0] == "ws")
                            PageUrl = "https://steamcommunity.com/workshop/filedetails/?id=" + ItemId;
                        else
                            PageUrl = item.Split(",")[0];
                        Debug.WriteLine(PageUrl);
                        using (var client = new WebClient())
                        {
                            var html = "";
                            try
                            {
                                html = client.DownloadString(PageUrl);
                            }
                            catch
                            {
                                html = "404";
                            }
                            if (html != "404")
                                try
                                {
                                    if (PageUrl.Contains("smmods.com"))
                                        ModName = html.Split("card-title\">")[1].Split("<")[0];
                                    else if (PageUrl.Contains("scrapmechanicmods.com"))
                                        ModName = html.Split("class=\"page-header\">")[1].Split("<")[0].Replace("\"", "");
                                    else if (PageUrl.Contains("https://steamcommunity.com/workshop/filedetails/?id="))
                                        ModName = html.Split("workshopItemTitle\">")[1].Split("<")[0];
                                    ModName = ModName.Replace("&amp;", "&");
                                }
                                catch
                                {
                                    html = "404";
                                    ModName = (string)Application.Current.FindResource("404cannotfindmod");
                                }
                            else
                                ModName = (string)Application.Current.FindResource("404cannotfindmod");
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var binding = ModListItemBinding.Create(ModName, PageUrl);
                            binding.IsVisible = false;
                            ModList.Items.Add(binding);
                        });
                    }
                    catch
                    {
                        // some error?
                    }
                index++;
            }
            Dispatcher.Invoke(() =>
            {
                PreLoadBox.Visibility = Visibility.Visible;
                StatusBox.Visibility = Visibility.Hidden;
            });
        }

        public void StartDownload(object sender, RoutedEventArgs args)
        {
            DownloadButton.IsEnabled = false;
            new Thread(ThreadDownload).Start();
        }

        public void ThreadDownload()
        {
            var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + PgMultiplayer.GetPgMultiplayer.SteamUserId + ".txt";
            var index = 0;
            var UserModList = htmlSite.Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "").Replace("</pre></body></html>", "").Replace("[", "").Replace("]", "").Split(";");
            foreach (var item in UserModList)
            {
                if (index != UserModList.Length - 1)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var binding = (ModListItemBinding)ModList.Items[index];
                        binding.IsLoading = true;
                        binding.IsVisible = true;
                    });
                    try
                    {
                        URL = item.Split(",")[0];
                        var ItemId = item.Split(",")[1];
                        var isWorkshop = false;
                        var DoesntHaveMod = true;
                        if (URL == "ws")
                        {
                            if (Utilities.CompatibleMods.Contains("12ws" + ItemId))
                            {
                                DoesntHaveMod = false;
                                Utilities.CopyDirectory(Path.Combine(App.Settings.WorkshopPath, ItemId), Path.Combine(TmpModFolder, ItemId));
                            }
                        }
                        else
                        {
                            if (Utilities.CompatibleMods.Contains(URL))
                            {
                                DoesntHaveMod = false;
                                Utilities.CopyDirectory(Path.Combine(Constants.ArchivesPath, ItemId), Path.Combine(TmpModFolder, ItemId));
                            }
                        }
                        var isValid = true;
                        var ModName = "";
                        Dispatcher.Invoke(() =>
                        {
                            var binding = (ModListItemBinding)ModList.Items[index];
                            isValid = binding.Name != "404, Could not find mod!";
                            ModName = binding.Name;
                        });
                        if (!isValid)
                            throw new Exception("InValid Mod");
                        var prevURL = URL;
                        if (URL.Contains("smmods.com"))
                        {
                            var tmpURL = URL;
                            var version = "";
                            if (URL.Contains("/version/"))
                            {
                                tmpURL = URL.Split("/version/")[0];
                                version = URL.Split("/version/")[1].Replace("/", "");
                            }
                            var modID = tmpURL.Split("/")[^1];
                            //fuck you tubo
                            URL = "https://smmods.com/api/v2/mods/" + modID;
                            using var client = new WebClient();
                            var html = client.DownloadString(URL);
                            var Modver = "";
                            if (version != "")
                                Modver = version;
                            else
                                Modver = html.Split("current_version")[1].Split("id\":\"")[1].Split("\",\"name\"")[0];
                            prevURL += "/version/" + Modver;
                            URL += "/version/" + Modver + "/download";
                        }
                        else if (URL.Contains("scrapmechanicmods.com"))
                        {
                            URL = URL.Replace("scrapmechanicmods.com/m=", "scrapmechanicmods.com/fmdl=");
                        }
                        else if (URL == "ws")
                        {
                            URL = "https://steamcommunity.com/workshop/filedetails/?id=" + ItemId;
                            isWorkshop = true;
                        }
                        if (DoesntHaveMod)
                        {
                            if (!isWorkshop)
                            {
                                using var client = new WebClient();
                                if (!Directory.Exists(Path.Combine(TmpModFolder, ItemId)))
                                {
                                    Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                                }
                                else
                                {
                                    Directory.Delete(Path.Combine(TmpModFolder, ItemId), true);
                                    Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                                }
                                client.DownloadFile(URL, Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                                ZipFile.ExtractToDirectory(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"), Path.Combine(TmpModFolder, ItemId), true);
                                File.Delete(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                                var _data = new List<ModDescriptionModel>
                                {
                                    new ModDescriptionModel
                                    {
                                        Name = ModName,
                                        Description = "Description Here",
                                        Type = "Blocks and Parts",
                                        Location = prevURL.Length.ToString().Length + prevURL.Length.ToString() + prevURL
                                    }
                                };
                                var json = JsonConvert.SerializeObject(_data);
                                File.WriteAllText(Path.Combine(TmpModFolder, ItemId, "description.json"), json.Replace("[", "").Replace("]", ""));
                                File.Copy(Path.Combine(Constants.Resources, "Assets", "empty.png"), Path.Combine(TmpModFolder, ItemId, "preview.png"));
                                Utilities.CopyDirectory(Path.Combine(TmpModFolder, ItemId), Path.Combine(Constants.ArchivesPath, ItemId));
                            }
                            else
                            {
                                var DownloadUrl = "https://steamworkshopdownloader.io/extension/embedded/" + ItemId;
                                Dispatcher.Invoke(() => { PgMultiplayer.GetPgMultiplayer.GotoAddress(DownloadUrl); });
                                var HasCompletedDownload = false;
                                App.Settings.NewFileName = ItemId + ".zip";
                                while (!HasCompletedDownload)
                                    HasCompletedDownload = App.Settings.LatestDownloadComplete;
                                App.Settings.LatestDownloadComplete = false;
                                ZipFile.ExtractToDirectory(Path.Combine(Constants.CachePath, ItemId + ".zip"), Path.Combine(TmpModFolder, ItemId), true);
                                File.Delete(Path.Combine(Constants.CachePath, ItemId + ".zip"));
                                Utilities.CopyDirectory(Path.Combine(TmpModFolder, ItemId), Path.Combine(App.Settings.WorkshopPath, ItemId));
                                Dispatcher.Invoke(() =>
                                {
                                    App.GetApp.ForceFormatAllMods();
                                    PgManage.GetPgManage.InvokeRefreshAll();
                                });
                            }
                        }
                        else
                        {
                            var count = Utilities.CompatibleMods.IndexOf(ItemId);
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var binding = (ModListItemBinding)ModList.Items[index];
                            binding.IsLoading = false;
                            binding.CurrentImage = 1;
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        Dispatcher.Invoke(() =>
                        {
                            var binding = (ModListItemBinding)ModList.Items[index];
                            binding.IsLoading = false;
                            binding.CurrentImage = 2;
                        });
                    }
                }
                index++;
            }
            Dispatcher.Invoke(() =>
            {
                DownloadButton.Content = "Install";
                DownloadButton.Click -= StartDownload;
                DownloadButton.Click += StartInstall;
                DownloadButton.IsEnabled = true;
            });
        }

        public void StartInstall(object sender, RoutedEventArgs args)
        {
            DownloadButton.IsEnabled = false;
            ScrollView.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Visible;
            IsLoadingInstall = true;
            tmpArgs = args;
            new Thread(LoadingGridImage).Start();
            new Thread(InstallThread).Start();
        }

        public void LoadingGridImage()
        {
            var deg = "";
            while (IsLoadingInstall)
            {
                if (deg.Length > 4)
                    deg = "";
                Dispatcher.Invoke(() => { InstallingText.Text = "Loading" + deg; });
                deg += ".";
                Thread.Sleep(350);
            }
        }

        public void InstallThread()
        {
            var CurrentMods = File.ReadAllText(Path.Combine(Constants.AppUserData, "CurrentMods.smmm"));
            PgManage.GetPgManage.InvokeRemoveAllCurrentMods(null, tmpArgs);
            var boolean = true;
            while (boolean)
                boolean = PgManage.GetPgManage.IsInvokingRemove;
            Debug.WriteLine("Done");
            tmpArgs = null;
            App.GetApp.FormatAllMods_StartDir = TmpModFolder;
            App.GetApp.FormatAllMods();
            App.GetApp.FormatAllMods_StartDir = Constants.ArchivesPath;
            App.GetApp.FormatAllMods();
            Dispatcher.Invoke(() => { WnManager.GetWnManager.ShowMultiplayerPage(null, null); });
            var Mods = new List<string>();
            foreach (var item in Directory.GetDirectories(TmpModFolder))
                Mods.Add(item);
            InjectModsInsideTMP(Mods);
        }

        public void LaunchGame(object sender, RoutedEventArgs args)
        {
            var SteamId = PgMultiplayer.GetPgMultiplayer.SteamUserId;
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + Path.Combine(Utilities.GetSteamLocation(), "steam.exe") + "\" " + "steam://run/387990//-connect_steam_id%20" + SteamId + "%20-friend_steam_id%20" + SteamId,
                CreateNoWindow = true
            };
            Process.Start(startInfo);
            Dispatcher.Invoke(() => { WnManager.GetWnManager.CallMinimizeWindow(); });
            bringToFront("Scrap Mechanic - Steam");
            Close();
        }

        public void InjectModsInsideTMP(List<string> BackUpModlist)
        {
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
                Directory.CreateDirectory(Constants.ModInstallBackupsPath);
            Dispatcher.Invoke(() => { Utilities.RemoveMods(false); });
            var BackupCompleted = !Utilities.CreateBackUpFile(BackUpModlist, false, TmpModFolder);
            var tmpCurrentMods = new List<string>();
            foreach (var ModId in BackUpModlist)
            {
                var ModLocation = ModId;
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
                            return;
                        File.WriteAllLines(Path.Combine(Constants.ModInstallBackupsPath, "10" + newModId + ".smmm"), Utilities.GetSubFilesOnly(ModLocation, folder, ModLocation, new List<string>()));
                    }
                    catch (Exception e)
                    {
                        ErrorCount++;
                        if (ErrorCount < 5)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Utilities.RemoveMods(false);
                                App.GetApp.CleanFiles();
                                Utilities.ArchivedMods.Clear();
                                Utilities.CompatibleMods.Clear();
                                Utilities.CurrentMods.Clear();
                                PgManage.GetPgManage.RefreshAll();
                            });
                            goto ReTry;
                        }
                        throw new Exception((string)Application.Current.FindResource("filesarecorrupt"), e);
                    }
                }
                if (!tmpCurrentMods.Contains(ModId))
                    tmpCurrentMods.Add(ModId);
                FolderInject(Path.Combine(ModLocation, "Scrap Mechanic"));
            }
            if (!BackupCompleted)
                tmpCurrentMods.Clear();
            Dispatcher.Invoke(() =>
            {
                IsLoadingInstall = false;
                var rotateTransform = new RotateTransform(0);
                InstallingText.Text = (string)Application.Current.FindResource("Completed");
                DownloadButton.Content = (string)Application.Current.FindResource("JoinGame");
                DownloadButton.Click += LaunchGame;
                DownloadButton.Click -= StartInstall;
                DownloadButton.IsEnabled = true;
                Utilities.CurrentMods = tmpCurrentMods;
                Utilities.SaveDataToFile();
                PgManage.GetPgManage.RefreshCurrentModsList();
                App.PageJoinFriend.UpdateCurrentMods(false);
                PgManage.GetPgManage.InvokeRefreshCurrentModsList();
                WnManager.GetWnManager.SendNotification((string)Application.Current.FindResource("successfullyinstalledrequiredmods"));
            });
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")] public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void bringToFront(string title)
        {
            // Get a handle to the Calculator application.
            var handle = FindWindow(null, title);

            // Verify that Calculator is a running process.
            if (handle == IntPtr.Zero)
                return;

            // Make Calculator the foreground application
            SetForegroundWindow(handle);
        }

        public void FolderInject(string TempModId)
        {
            if (Directory.Exists(Path.Combine(TmpModFolder, TempModId)))
                Utilities.CopyDirectory(Path.Combine(TmpModFolder, TempModId), App.Settings.GameDataPath);
        }

        public static bool CreateBackUpFile(List<string> ModIdList)
        {
            var InjectFailed = false;
            var NewFolderPath = App.Settings.GameDataPath;
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
                Directory.CreateDirectory(Constants.ModInstallBackupsPath);
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
            {
                InjectFailed = true;
                return InjectFailed;
            }
            foreach (var tmpModId in ModIdList)
            {
                var ModId = tmpModId.Split(@"\")[^1];
                var PathList = Utilities.GetSurvivalFolder(Path.Combine(TmpModFolder, ModId), true);
                File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), Path.Combine(TmpModFolder, ModId));
                foreach (var tempString in PathList)
                    if (tempString.Contains("Survival") || tempString.Contains("Scrap Mechanic"))
                    {
                        var temp2 = tempString.Replace(Path.Combine(TmpModFolder, ModId), "");
                        var DirectoryList = temp2.Split('\\');
                        var partFull = "";
                        foreach (var Directory in DirectoryList)
                        {
                            if (partFull.Length > 0)
                                partFull = partFull + @"\" + Directory;
                            else
                                partFull = Directory;
                            partFull = partFull.Replace(TmpModFolder, "");
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
            return InjectFailed;
        }

        public void Cancel(object sender, RoutedEventArgs args)
        {
            Close();
        }

        public string PassComplexData(uint FileId)
        {
            /*
            string ResponseString;
            HttpWebResponse response;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.steamworkshopdownloader.io/api/download/request");
                request.Accept = "application/json"; //"application/xml";
                request.Method = "POST";
                var obj = new ModDataFormat { publishedFileId = FileId, collectionId = null, extract = true, hidden = true, direct = false };
                var jss = new JavaScriptSerializer();
                // serialize into json string
                var myContent = jss.Serialize(obj);

                var data = Encoding.ASCII.GetBytes(myContent);

                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                response = (HttpWebResponse)request.GetResponse();

                ResponseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)ex.Response;
                    ResponseString = "Some error occured: " + response.StatusCode;
                }
                else
                {
                    ResponseString = "Some error occured: " + ex.Status;
                }
            }
            Debug.WriteLine(ResponseString);
            var uuid = ResponseString.Split("\"")[3];
            */
            //return "https://api.steamworkshopdownloader.io/api/download/transmit?uuid=" + uuid;
            return "";
        }

        public void OpenBrowser(object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            var link = ((TextBlock)button.Content).Text.Replace("System.Windows.Controls.Button: ", "");
            Utilities.OpenBrowserUrl(link);
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

    }

    public class ModDataFormat
    {

        public uint publishedFileId { get; set; }
        public object collectionId { get; set; }
        public bool extract { get; set; }
        public bool hidden { get; set; }
        public bool direct { get; set; }

    }

    public class ModStatusDataFormat
    {

        public string uuids { get; set; }

    }

    public class ResponseFormat
    {

        public string key { get; set; }
        public string value { get; set; }

    }

}