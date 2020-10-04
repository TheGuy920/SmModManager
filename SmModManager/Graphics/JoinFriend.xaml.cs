using SmModManager.Core;
using SmModManager.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using Nancy.Json;
using System.Text;
using System.IO.Compression;
using CefSharp;

namespace SmModManager.Graphics
{
    /// <summary>
    /// Interaction logic for JoinFriend.xaml
    /// </summary>
    public partial class JoinFriend : Window
    {
        public static JoinFriend GetJoinFriend;
        public string BrowserHTML;
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
        public bool ThreadStatus = false;
        public JoinFriend()
        {
            GetJoinFriend = this;
            InitializeComponent();
            WindowState = WindowState.Normal;
            var CursorPos = System.Windows.Forms.Cursor.Position;
            Top = CursorPos.Y - 80;
            Left = CursorPos.X - 140;
            var thread = new Thread(BringWindowIntoView)
            {
                IsBackground = true
            };
            thread.Start();
        }
        public void CloseWindow()
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
        }
        public void BringWindowIntoView()
        {
            var boolean = false;
            try
            {
                while (!boolean)
                {
                    Dispatcher.Invoke(() =>
                    {
                        boolean = IsActive;
                        BringIntoView();
                        Focus();
                        Activate();
                    });
                    Thread.Sleep(50);
                }
            }
            catch { }
        }
        public void RefreshCurrentModsStatus()
        {
            if(!ThreadStatus)
                UpdateCurrentMods();
            else
                Dispatcher.Invoke(() =>
                {
                    WnManager.GetWnManager.SendNotification("Please wait for your status update to finish!");
                });
        }
        public void UpdateCurrentMods(bool ShowEvenIfFail = true)
        {
            var thread = new Thread(StartThreading)
            {
                IsBackground = true
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
                var StatusQueueId = QueueMods.Count + 1;
                foreach (int item in QueueMods)
                    if (item >= StatusQueueId)
                        StatusQueueId = item + 1;
                if (QueueMods.Count > 2)
                    QueueMods.RemoveAt(1);
                else
                    QueueMods.Add(StatusQueueId);
                while (QueueMods.IndexOf(StatusQueueId) != 0)
                    if (QueueMods.IndexOf(StatusQueueId) < 0)
                        return;
                Thread.Sleep(250);
                var IsUsable = false;
                if (File.Exists(Path.Combine(BackBlazeFolder, SteamId) + ".txt"))
                    while (!IsUsable) { try { File.Delete(Path.Combine(BackBlazeFolder, SteamId) + ".txt"); IsUsable = true; } catch { Thread.Sleep(250); IsUsable = false; } }
                FileDetailsModel Contents = new FileDetailsModel();
                if (File.Exists(Path.Combine(BackBlazeFolder, "FileDetails.json")))
                {
                    try { Contents = FileDetailsModel.Load(Path.Combine(BackBlazeFolder, "FileDetails.json")); } catch { }
                    var IsNotBeingUsed = false;
                    while (!IsNotBeingUsed) { try { File.Delete(Path.Combine(BackBlazeFolder, "FileDetails.json")); IsNotBeingUsed = true; } catch { IsNotBeingUsed = false; Thread.Sleep(250); } }
                }
                string DeleteFile = "";
                if (Contents != null)
                    if (Contents.FileId != null)
                        DeleteFile = Contents.FileId;
                File.WriteAllText(Path.Combine(BackBlazeFolder, SteamId) + ".txt", ModInfo);
                ProcessStartInfo p2Info = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"" + RunBackBlaze + "\" " + SteamId + ".txt " + ServerListBucket + " " + BackBlazeApiKey + " " + DeleteFile,
                    CreateNoWindow = true
                };
                Process.Start(p2Info);
                var UploadCompleted = false;
                while (!UploadCompleted) { try { File.ReadAllText(Path.Combine(BackBlazeFolder, "FileDetails.json")); UploadCompleted = true; } catch { UploadCompleted = false; Thread.Sleep(250); } }
                var details = File.ReadAllText(Path.Combine(BackBlazeFolder, "FileDetails.json")).Replace("URL", "//URL");
                File.Delete(Path.Combine(BackBlazeFolder, "FileDetails.json"));
                File.WriteAllText(Path.Combine(BackBlazeFolder, "FileDetails.json"), details);
                var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + App.UserSteamId + ".txt";
                string htmlSite = "";
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        htmlSite = client.DownloadString(URL);
                        if (ModInfo != htmlSite.Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "").Replace("</pre></body></html>", ""))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                WnManager.GetWnManager.SendNotification("Online Status Failed To Update!\nPlease make sure your computers time is correctly synced");
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                WnManager.GetWnManager.SendNotification("Online Status Successfully Updated!");
                            });
                        }
                    }
                    catch
                    {
                        htmlSite = "404";
                    }
                }
                if (QueueMods.Count > 0)
                    QueueMods.RemoveAt(0);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    WnManager.GetWnManager.SendNotification("Please Sign In to enable multiplayer compatibility");
                });
            }
            ThreadStatus = false;
        }
        public string FormatCurrentMods()
        {
            string ModList = "";
            foreach(string mod in Utilities.CurrentMods)
            {
                var numbers = "0123456789";
                var subStrStartIndex = numbers.IndexOf(mod[0]);
                var numberList = "";
                for (var newIndex = 1; newIndex <= subStrStartIndex; newIndex++)
                {
                    numberList += mod[newIndex].ToString();
                }
                var URL = mod.Substring(subStrStartIndex + 1);
                var oldSub = subStrStartIndex + 1;
                subStrStartIndex = int.Parse(numberList) + int.Parse(mod[0].ToString()) + 1;
                URL = URL.Remove(subStrStartIndex- oldSub);
                var ModId = mod.Substring(subStrStartIndex);
                ModList += URL + "," + ModId + ";";
            }
            return "[" + ModList + "]";
        }
        public void JoinGame(object sender, RoutedEventArgs args)
        {
            if (Directory.Exists(TmpModFolder))
                Directory.Delete(TmpModFolder, true);
                Directory.CreateDirectory(TmpModFolder);
            this.Dispatcher.Invoke(() =>
            {
                PgMultiplayer.GetPgMultiplayer.HomePageSite.Load("");
            });
            Debug.WriteLine(PgMultiplayer.GetPgMultiplayer.SteamUserId);
            var URL = "https://f002.backblazeb2.com/file/SmPlayerList/" + PgMultiplayer.GetPgMultiplayer.SteamUserId + ".txt";
            string htmlSite = "";
            using (WebClient client = new WebClient())
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
                goto DownloadRequiredMods;
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    WnManager.GetWnManager.SendNotification("Online Status Not Found :(");
                });
                Close();
                return;
            }
        DownloadRequiredMods:
            foreach (string item in htmlSite.Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "").Replace("</pre></body></html>", "").Replace("[", "").Replace("]", "").Split(";"))
            {
                try
                {
                    URL = item.Split(",")[0];
                    var ItemId = item.Split(",")[1];
                    var isWorkshop = false;
                    if ( URL.Contains("smmods.com") )
                    {
                        URL += "/download";
                    }
                    else if ( URL.Contains("scrapmechanicmods.com") )
                    {
                        URL = URL.Replace("scrapmechanicmods.com/m=", "scrapmechanicmods.com/fmdl=");
                    }
                    else if ( URL == "ws" )
                    {
                        URL = "https://steamcommunity.com/workshop/filedetails/?id=" + ItemId;
                        isWorkshop = true;
                    }
                    if (!isWorkshop) {
                        using WebClient client = new WebClient();
                        if (!Directory.Exists(Path.Combine(TmpModFolder, ItemId)))
                            Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                        else
                        {
                            Directory.Delete(Path.Combine(TmpModFolder, ItemId), true);
                            Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                        }
                        client.DownloadFile(URL, Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                        ZipFile.ExtractToDirectory(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"), Path.Combine(TmpModFolder, ItemId), true);
                        File.Delete(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                    }
                    else
                    {
                        URL = PassComplexData(uint.Parse(ItemId));
                        using WebClient client = new WebClient();
                        if (!Directory.Exists(Path.Combine(TmpModFolder, ItemId)))
                            Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                        else
                        {
                            Directory.Delete(Path.Combine(TmpModFolder, ItemId), true);
                            Directory.CreateDirectory(Path.Combine(TmpModFolder, ItemId));
                        }
                        client.DownloadFile(URL, Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                        ZipFile.ExtractToDirectory(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"), Path.Combine(TmpModFolder, ItemId), true);
                        File.Delete(Path.Combine(TmpModFolder, ItemId, ItemId + ".zip"));
                    }
                }
                catch
                {
                    // last item causes error
                }
            }
            goto InstallRequiredMods;
        InstallRequiredMods:
            Dispatcher.Invoke(() =>
            {
                var CurrentMods = File.ReadAllText(Path.Combine(Constants.AppUserData, "CurrentMods.smmm"));
                File.WriteAllText(Path.Combine(Constants.AppUserData, "InstallOnBoot.smmm"), CurrentMods);
                PgManage.GetPgManage.InvokeRemoveAllCurrentMods(null, args);
                args.Source = "DontShowMessage";
                List<string> Mods = new List<string>();
                foreach (string item in Directory.GetDirectories(TmpModFolder))
                {
                    Mods.Add(item);
                }
                InjectModsInsideTMP(Mods);
            });
            goto LaunchGameAndJoin;
        LaunchGameAndJoin:
            var SteamId = PgMultiplayer.GetPgMultiplayer.SteamUserId;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + Path.Combine(Utilities.GetSteamLocation(), "steam.exe") + "\" " + "steam://run/387990//-connect_steam_id%20" + SteamId + "%20-friend_steam_id%20" + SteamId,
                CreateNoWindow = true
            };
            Process.Start(startInfo);
            Dispatcher.Invoke(() =>
            {
                WnManager.GetWnManager.CallMinimizeWindow();
            });
            Close();
        }
        public void InjectModsInsideTMP(List<string> BackUpModlist)
        {
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
            {
                Directory.CreateDirectory(Constants.ModInstallBackupsPath);
            }
            _ = !CreateBackUpFile(BackUpModlist);
            var tmpCurrentMods = new List<string>();
            foreach (var ModId in BackUpModlist)
            {
                var ModLocation = ModId;
                if (Directory.Exists(Constants.ModInstallBackupsPath))
                {
                    var newModId = ModLocation.Split(@"\")[^1];
                    var folder = App.Settings.GameDataPath;
                    if (Directory.Exists(Path.Combine(ModLocation, "Scrap Mechanic")))
                        folder.Replace("Scrap Mechanic", "");
                    Debug.WriteLine(Utilities.GetSubFilesOnly(ModLocation, folder, ModLocation, new List<string>()));
                    Debug.WriteLine(Path.Combine(Constants.ModInstallBackupsPath, "10" + newModId.ToString() + ".smmm"));
                    File.WriteAllLines(Path.Combine(Constants.ModInstallBackupsPath, "10" + newModId.ToString() + ".smmm"), Utilities.GetSubFilesOnly(ModLocation, folder, ModLocation, new List<string>()));
                }
                if (!tmpCurrentMods.Contains(ModId))
                {
                    tmpCurrentMods.Add(ModId);
                }
                SurvivalFolderInject(Path.Combine(ModLocation, "Survival"), Path.Combine(ModLocation, "Scrap Mechanic"));
            }
            Utilities.CurrentMods = tmpCurrentMods;
            PgManage.GetPgManage.InvokeRefreshCurrentModsList();
            WnManager.GetWnManager.SendNotification("Successfully installed required mods! Joining game now!");
        }
        public void SurvivalFolderInject(string TempModId, string TempModId2)
        {
            if (Directory.Exists(Path.Combine(TmpModFolder, TempModId2)))
            {
                Utilities.CopyDirectory(Path.Combine(TmpModFolder, TempModId2), App.Settings.GameDataPath);
            }
            else if (Directory.Exists(Path.Combine(TmpModFolder, TempModId)))
            {
                Utilities.CopyDirectory(Path.Combine(TmpModFolder, TempModId), Path.Combine(App.Settings.GameDataPath, "Survival"));
            }
        }
        public static bool CreateBackUpFile(List<string> ModIdList)
        {
            var InjectFailed = false;
            var NewFolderPath = App.Settings.GameDataPath;
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
            {
                Directory.CreateDirectory(Constants.ModInstallBackupsPath);
            }
            if (!Directory.Exists(Constants.ModInstallBackupsPath))
            {
                InjectFailed = true;
                return InjectFailed;
            }
            else
            {
                foreach (var tmpModId in ModIdList)
                {
                    var ModId = tmpModId.Split(@"\")[^1];
                    List<string> PathList = Utilities.GetSurvivalFolder(Path.Combine(TmpModFolder, ModId), true);
                    File.AppendAllText(Path.Combine(Constants.ModInstallBackupsPath, "Log.txt"), Path.Combine(TmpModFolder, ModId));
                    foreach (string tempString in PathList)
                    {
                        if (tempString.Contains("Survival") || tempString.Contains("Scrap Mechanic"))
                        {
                            var temp2 = tempString.Replace(Path.Combine(TmpModFolder, ModId), "");
                            var DirectoryList = temp2.Split('\\');
                            var partFull = "";
                            foreach (string Directory in DirectoryList)
                            {
                                    if (partFull.Length > 0)
                                    {
                                        partFull = partFull + @"\" + Directory;
                                    }
                                    else
                                    {
                                        partFull = Directory;
                                    }
                                    partFull = partFull.Replace(TmpModFolder, "");
                                    if (partFull.Contains("Scrap Mechanic"))
                                    {
                                        partFull = partFull.Substring(partFull.IndexOf("Scrap Mechanic"));
                                    }
                                    else if (partFull.Contains("Survival"))
                                    {
                                        partFull = partFull.Substring(partFull.IndexOf("Survival"));
                                    }
                                if (Directory != DirectoryList[0])
                                {
                                    //Debug.WriteLine(partFull);
                                    if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, partFull)) && !(partFull.Contains(".")))
                                    {
                                        partFull = partFull.Replace(@"Scrap Mechanic\", @"");
                                        if (System.IO.Directory.Exists(NewFolderPath + partFull))
                                        {
                                            System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, partFull));
                                        }
                                    }
                                    else
                                    {
                                        if (partFull.Contains("."))
                                        {
                                            var substr = partFull.Replace(@"Scrap Mechanic\", "");
                                            var file = substr.Split('\\')[^1];
                                            if (!System.IO.Directory.Exists(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, ""))))
                                            {
                                                System.IO.Directory.CreateDirectory(Path.Combine(Constants.ModInstallBackupsPath, substr.Replace(file, "")));
                                            }
                                            try
                                            {
                                                File.Copy(Path.Combine(NewFolderPath, substr), Path.Combine(Constants.ModInstallBackupsPath, substr), false);
                                            }
                                            catch { }
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
        public void Cancel(object sender, RoutedEventArgs args)
        {
            this.Close();
        }
        public string PassComplexData(uint FileId)
        {
            string ResponseString;
            HttpWebResponse response;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.steamworkshopdownloader.io/api/download/request");
                request.Accept = "application/json"; //"application/xml";
                request.Method = "POST";
                ModDataFormat obj = new ModDataFormat() { publishedFileId = FileId, collectionId = null, extract = true, hidden = true, direct = false };
                JavaScriptSerializer jss = new JavaScriptSerializer();
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
                    ResponseString = "Some error occured: " + response.StatusCode.ToString();
                }
                else
                {
                    ResponseString = "Some error occured: " + ex.Status.ToString();
                }
            }
            var uuid = ResponseString.Split("\"")[3];
            return "https://api.steamworkshopdownloader.io/api/download/transmit?uuid=" + uuid;
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
