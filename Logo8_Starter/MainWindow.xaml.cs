using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;

namespace Logo8_Starter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            try
            {
                uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            }
            catch (Exception exp)
            {

            }
        }
    }

    public partial class MainWindow : Window
    {

        string ProjektName = "";
        string ProjektPfad = "h:\\Logo8_V81";

        public MainWindow()
        {
            InitializeComponent();
            ProjekteLesen();
        }

        public void ProjekteLesen()
        {

            /*
             * Aufbau der Projektnamen (Ordner)
             * LOGO_PLC_WEB_FUP_Linearachse
             * 
             * _PLC_ oder  _BUG_    
             * + _NC_
             * + _HMI
             * + _VISU_
             * + _FIO_
             * + _WEB_
             * 
             * _AWL_ oder _AS_ oder _FUP_ oder _KOP_ oder _SCL_ oder _ST_
             * 
             * */

            var Sprachen = new List<Tuple<string, string>>
                                {
                                    Tuple.Create("AWL", "(AWL)"),
                                    Tuple.Create("AS", "(AS)"),
                                    Tuple.Create("FUP", "(FUP)"),
                                    Tuple.Create("CFC", "(CFC)"),
                                    Tuple.Create("SCL", "(SCL)"),
                                    Tuple.Create("ST", "(ST)"),
                                    Tuple.Create("KOP", "(KOP)")
                                };

            string[] EigenschaftenArray = { "_NC_", "_HMI_", "_VISU_", "_FIO_", "_WEB_" };

            var Eigenschaften = new List<Tuple<string, string>>
                                {
                                    Tuple.Create("PLC", "PLC"),
                                    Tuple.Create("BUG", "Bug"),
                                    Tuple.Create("NC", "NC"),
                                    Tuple.Create("HMI", "HMI"),
                                    Tuple.Create("VISU", "Visu"),
                                    Tuple.Create("WEB", "Web"),
                                    Tuple.Create("FIO", "Factory I/O")
                                };




            var ProjekteListe = new Dictionary<string, List<string>>();


            List<string> ProjektVerzeichnis = new List<string>();
            List<string> Projekte_PLC = new List<string>();
            List<string> Projekte_BUG = new List<string>();






            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");

            foreach (System.IO.DirectoryInfo d in ParentDirectory.GetDirectories())
                ProjektVerzeichnis.Add(d.Name);

            ProjektVerzeichnis.Sort();


            foreach (string Projekt in ProjektVerzeichnis)
            {       

                string Sprache = "";
                int StartBezeichnung = 0;

                if (Projekt.Contains("KOP"))
                {
                    Sprache = " (KOP)";
                    StartBezeichnung = 4 + Projekt.IndexOf("KOP");
                }
                else
                {
                    if (Projekt.Contains("FUP"))
                    {
                        Sprache = " (FUP)";
                        StartBezeichnung = 4 + Projekt.IndexOf("FUP");
                    }
                }
                RadioButton rdo = new RadioButton();
                rdo.GroupName = "Logo8!";
                rdo.VerticalAlignment = VerticalAlignment.Top;
                rdo.Checked += new RoutedEventHandler(radioButton_Checked);

                if (Projekt.Contains("PLC_"))
                {
                    rdo.Content = Projekt.Substring(StartBezeichnung).Replace("_", " ") + Sprache;
                    rdo.Name = Projekt;
                    StackPanel_PLC_NONE.Children.Add(rdo);
                }
                if (Projekt.Contains("BUG_"))
                {
                    rdo.Content = Projekt.Substring(StartBezeichnung).Replace("_", " ") + Sprache;
                    rdo.Name = Projekt;
                    StackPanel_BUG_NONE.Children.Add(rdo);
                }
            }

        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");

            EigenschaftenAendern(ProjektStarten_BUG_NONE, ProjektStarten_PLC_NONE, "Enable", "-");

            ProjektName = rb.Name;

            string DateiName = ParentDirectory.FullName + "\\" + rb.Name + "\\index.html";
            string HtmlSeite = System.IO.File.ReadAllText(DateiName);
            string LeereHtmlSeite = "<!doctype html>   </html >";

            if (rb.Name.Contains("PLC_NONE"))
                Web_PLC_NONE.NavigateToString(HtmlSeite);
            else
                Web_PLC_NONE.NavigateToString(LeereHtmlSeite);

            if (rb.Name.Contains("BUG_NONE"))
                Web_BUG_NONE.NavigateToString(HtmlSeite);
            else
                Web_BUG_NONE.NavigateToString(LeereHtmlSeite);

        }

        private void ProjektStarten(object sender, RoutedEventArgs e)
        {

            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");
            string sourceDirectory = ParentDirectory.FullName + "\\" + ProjektName;

            EigenschaftenAendern(ProjektStarten_BUG_NONE, ProjektStarten_PLC_NONE, "Start", "Ordner " + ProjektPfad + " löschen");
            if (System.IO.Directory.Exists(ProjektPfad)) System.IO.Directory.Delete(ProjektPfad, true);

            EigenschaftenAendern(ProjektStarten_BUG_NONE, ProjektStarten_PLC_NONE, "Start", "Ordner " + ProjektPfad + " erstellen");
            System.IO.Directory.CreateDirectory(ProjektPfad);

            EigenschaftenAendern(ProjektStarten_BUG_NONE, ProjektStarten_PLC_NONE, "Start", "Alle Dateien kopieren");
            Copy(sourceDirectory, ProjektPfad);

            Process proc = new Process();
            proc.StartInfo.FileName = ProjektPfad + "\\start.cmd";
            proc.StartInfo.WorkingDirectory = ProjektPfad;
            proc.Start();
        }


        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


        private void TabControl_SelectionChanged(object sender, RoutedEventArgs e)
        {

            EigenschaftenAendern(ProjektStarten_BUG_NONE, ProjektStarten_PLC_NONE, "Disable", "-");

            string LeereHtmlSeite = "<!doctype html>   </html >";
            Web_PLC_NONE.NavigateToString(LeereHtmlSeite);
            Web_BUG_NONE.NavigateToString(LeereHtmlSeite);
        }

        private void EigenschaftenAendern(Button Knopf1, Button Knopf2, String ToDo, string Text)
        {
            switch (ToDo)
            {
                case "Enable":
                    Knopf1.IsEnabled = true;
                    Knopf2.IsEnabled = true;

                    Knopf1.Background = new SolidColorBrush(Colors.Green);
                    Knopf2.Background = new SolidColorBrush(Colors.Green);

                    Knopf1.Refresh();
                    Knopf2.Refresh();
                    break;

                case "Disable":
                    Knopf1.Background = new SolidColorBrush(Colors.Gray);
                    Knopf2.Background = new SolidColorBrush(Colors.Gray);

                    Knopf1.IsEnabled = false;
                    Knopf2.IsEnabled = false;

                    Knopf1.Refresh();
                    Knopf2.Refresh();
                    break;

                case "Start":
                    Knopf1.Background = new SolidColorBrush(Colors.Yellow);
                    Knopf2.Background = new SolidColorBrush(Colors.Yellow);

                    Knopf1.Content = Text;
                    Knopf2.Content = Text;

                    Knopf1.Refresh();
                    Knopf2.Refresh();
                    break;

                default:
                    break;
            }

        }





    }
}

