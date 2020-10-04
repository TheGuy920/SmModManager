using SmModManager.Core.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmModManager.Graphics
{
    /// <summary>
    /// Interaction logic for StartUpError.xaml
    /// </summary>
    public partial class StartUpError : Window
    {
        public static StartUpError GetStartUpError;
        public Exception exception;
        public StartUpError()
        {
            GetStartUpError = this;
            InitializeComponent();
        }
        public static void StartUpErrorWindow(Exception error)
        {
            GetStartUpError.ErrorReport.Text = error.Message;
            GetStartUpError.exception = error;
            GetStartUpError.GeneratePossibleSolutions();
        }
        public void GeneratePossibleSolutions()
        {
            try
            {
                throw exception;
            }
            catch(FileLoadException e)
            {

            }
            catch(IOException e)
            {
                PossibleSolutions.Items.Add(ErrorDataBinding.Create("Problem: " + e.Message.Remove(e.Message.IndexOf("\'")) + e.Message.Substring(e.Message.LastIndexOf("\'")+2) + "\nSolution: Kill all SmModManager.exe processes and restart", "KillAllAndRestart"));
                PossibleSolutions.Items.Add(ErrorDataBinding.Create("Problem: " + e.Message.Remove(e.Message.IndexOf("\'")) + e.Message.Substring(e.Message.LastIndexOf("\'") + 2) + "\nSolution: Delete corrupt file/folder and restart", "DeleteCoruptObject"));
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
                {
                    process.Kill();
                }
            }
        }
        public void DeleteCoruptObject()
        {
            MessageBox.Show("it works! DELETE CORRUPTED OBJECT");
        }
        public void DeleteCoruptFile()
        {
            MessageBox.Show("it works! DELETE CORRUPTED FILE");
        }
        public void DeleteCoruptFolder()
        {
            MessageBox.Show("it works! DELETE CORRUPTED FOLDER");
        } 
        private void TakeAction(object sender, RoutedEventArgs e)
        {
            foreach (var item in PossibleSolutions.Items.OfType<ErrorDataBinding>())
            {
                if (item.BoxIsChecked)
                {
                    switch (item.Function)
                    {
                        case "DeleteCoruptObject":
                            DeleteCoruptObject();
                            break;
                        case "KillAllAndRestart":
                            KillAllAndRestart();
                            break;
                    }
                }
            }

        }
    }
}
