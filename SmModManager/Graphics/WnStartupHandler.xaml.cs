using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmModManager.Core;
using SmModManager.Core.Bindings;

namespace SmModManager.Graphics
{

    /// <summary>
    ///     Interaction logic for StartUpError.xaml
    /// </summary>
    public partial class WnStartupHandler : Window
    {

        public static WnStartupHandler GetWnStartupHandler;
        public Exception exception;

        public WnStartupHandler()
        {
            GetWnStartupHandler = this;
            InitializeComponent();
        }

        public static void StartUpErrorWindow(Exception error)
        {
            GetWnStartupHandler.ErrorReport.Text = error.Message;
            GetWnStartupHandler.exception = error;
            GetWnStartupHandler.GeneratePossibleSolutions();
        }

        public void GeneratePossibleSolutions()
        {
            try
            {
                throw exception;
            }
            catch (FileLoadException e)
            {
            }
            catch (IOException e)
            {
                PossibleSolutions.Items.Add(ErrorDataBinding.Create(Application.Current.FindResource("problem") + " " + e.Message.Remove(e.Message.IndexOf("\'")) + e.Message.Substring(e.Message.LastIndexOf("\'") + 2) + "\n" + Application.Current.FindResource("problemkillall"), "KillAllAndRestart"));
                PossibleSolutions.Items.Add(ErrorDataBinding.Create(Application.Current.FindResource("problem") + " " + e.Message.Remove(e.Message.IndexOf("\'")) + e.Message.Substring(e.Message.LastIndexOf("\'") + 2) + "\n" + Application.Current.FindResource("problemdelete"), "DeleteCoruptObject"));
            }
            /*
            catch (FileLoadException e)
            {

            }
            */
        }

        public void UpdateSolutionSelection(object sender, SelectionChangedEventArgs e)
        {
        }

        public void KillAllAndRestart()
        {
            Console.WriteLine(Process.GetCurrentProcess().Id);
            foreach (var process in Process.GetProcessesByName("SmModManager"))
            {
                Console.WriteLine(process.Id);
                if (process.Id != Process.GetCurrentProcess().Id)
                    process.Kill();
            }
        }

        public void DeleteCoruptObject()
        {
            //MessageBox.Show("it works! DELETE CORRUPTED OBJECT");
        }

        public void DeleteCoruptFile()
        {
            //MessageBox.Show("it works! DELETE CORRUPTED FILE");
        }

        public void DeleteCoruptFolder()
        {
            //MessageBox.Show("it works! DELETE CORRUPTED FOLDER");
        }

        private void TakeAction(object sender, RoutedEventArgs e)
        {
            foreach (var item in PossibleSolutions.Items.OfType<ErrorDataBinding>())
                if (item.BoxIsChecked)
                    switch (item.Function)
                    {
                        case "DeleteCoruptObject":
                            DeleteCoruptObject();
                            break;
                        case "KillAllAndRestart":
                            KillAllAndRestart();
                            break;
                    }
            Utilities.RestartApp();
        }

    }

}