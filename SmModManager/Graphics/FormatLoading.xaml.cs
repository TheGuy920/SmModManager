using SmModManager.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmModManager.Graphics
{
    /// <summary>
    /// Interaction logic for FormatLoading.xaml
    /// </summary>
    public partial class FormatLoading : Page
    {
        public string message = (string)System.Windows.Application.Current.FindResource("ohnoerror");
        public FormatLoading()
        {
            InitializeComponent();
        }
        public void UpdateStatus()
        {
            if (App.RevalidateMods)
            {
                FormattingStatus.Visibility = Visibility.Hidden;
                DeleteAndValidate.Visibility = Visibility.Visible;
                Status.Visibility = Visibility.Hidden;
                RichTextBox.Document = Utilities.ParseToFlowDocument(message);
            }
            else
            {
                FormattingStatus.Visibility = Visibility.Visible;
                DeleteAndValidate.Visibility = Visibility.Hidden;
                Status.Visibility = Visibility.Hidden;
            }
        }
        public int CurrentFileIndex = 0;
        public void DeleteAllAndValidate(object sender, RoutedEventArgs args)
        {
            Status.Visibility = Visibility.Visible;
            DeleteAndValidate.Visibility = Visibility.Hidden;
            new Thread(ThreadDeleteThenValidate).Start(); 
        }
        public void ThreadDeleteThenValidate()
        {
            var totalNumOfFiles = Directory.GetDirectories(App.Settings.WorkshopPath).Length;
            foreach (var SubDirectory in Directory.GetDirectories(App.Settings.WorkshopPath))
            {
                CurrentFileIndex++;
                Dispatcher.Invoke(()=>
                {
                    Progress.Value = CurrentFileIndex / totalNumOfFiles;
                    TextBox.Text = SubDirectory;
                });
                Directory.Delete(SubDirectory, true);
            }
            Dispatcher.Invoke(() =>
            {
                Progress.Value = 100;
                TextBox.Text = (string)System.Windows.Application.Current.FindResource("completednowvalidating");
            });
            var startInfo = new ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(Utilities.GetSteamLocation(), "steam.exe"),
                Arguments = @"steam://validate/387990/"
            };
            Process.Start(startInfo);
        }
        public void Exit(object sender, RoutedEventArgs args)
        {
            App.Settings.DontAskMeToReValidate = (bool)RemindMe.IsChecked;
            App.Settings.Save();
            WnManager.GetWnManager.CanNavigate = true;
            WnManager.GetWnManager.ShowHomePage(null, null);
        }
        public void Restart(object sender, RoutedEventArgs args)
        {
            Utilities.RestartApp();
        }
    }
}
