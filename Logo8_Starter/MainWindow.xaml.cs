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
                Console.WriteLine("{0} Exception 1 caught.", exp);
            }
        }
    }

    public partial class MainWindow : Window
    {

        string ProjektName = "";
        string ProjektPfad = "h:\\Logo8V81";
        List<RadioButton> RadioButtonList = new List<RadioButton>();
        List<Button> ButtonListe = new List<Button>();

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
            ButtonListe.Add(ProjektStarten_BUG);
            ButtonListe.Add(ProjektStarten_PLC);

            // Name Komplett, kurz, Sprache, Anfang
            List<Tuple<string, string, string>> TupleList_PLC = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> TupleList_BUG = new List<Tuple<string, string, string>>();

            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");

            foreach (System.IO.DirectoryInfo d in ParentDirectory.GetDirectories())
            {
                string OrdnerName = d.Name;
                string Sprache = "";
                int StartBezeichnung = 0;

                if (OrdnerName.Contains("FUP"))
                {
                    Sprache = "FUP";
                    StartBezeichnung = 4 + OrdnerName.IndexOf("FUP");
                }
                if (OrdnerName.Contains("KOP"))
                {
                    Sprache = "KOP";
                    StartBezeichnung = 4 + OrdnerName.IndexOf("KOP");
                }


                if (d.Name.Contains("PLC"))
                {
                    // nur PLC und sonst nichts
                    Tuple<string, string, string> TplEintrag = new Tuple<string, string, string>(OrdnerName.Substring(StartBezeichnung), Sprache, OrdnerName);
                    TupleList_PLC.Add(TplEintrag);
                }
                else
                {
                    // Es gibt momentan noch keine Gruppe bei den Bugs
                    Tuple<string, string, string> TplEintrag = new Tuple<string, string, string>(OrdnerName.Substring(StartBezeichnung), Sprache, OrdnerName);
                    TupleList_BUG.Add(TplEintrag);
                }

            } // Ende foreach

            TupleList_PLC.Sort();
            TupleList_BUG.Sort();

            TabMitInhaltFuellen(TupleList_PLC, StackPanel_PLC);
            TabMitInhaltFuellen(TupleList_BUG, StackPanel_BUG);
        }

        private void TabMitInhaltFuellen(List<Tuple<string, string, string>> Projekte, System.Windows.Controls.StackPanel StackPanel)
        {
            foreach (Tuple<string, string, string> Projekt in Projekte)
            {
                RadioButton rdo = new RadioButton();
                rdo.GroupName = "Logo8!";
                rdo.VerticalAlignment = VerticalAlignment.Top;
                rdo.Checked += new RoutedEventHandler(radioButton_Checked);
                rdo.FontSize = 14;

                // nur PLC und sonst nichts
                rdo.Content = Projekt.Item1 + " (" + Projekt.Item2 + ")";
                rdo.Name = Projekt.Item3;
                StackPanel.Children.Add(rdo);
            }
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");

            DarstellungAendernListe(ButtonListe, true, Colors.Green, "Projekt starten");
            ProjektName = rb.Name;

            string DateiName = ParentDirectory.FullName + "\\" + rb.Name + "\\index.html";
            string HtmlSeite = System.IO.File.ReadAllText(DateiName);
            string LeereHtmlSeite = "<!doctype html>   </html >";

            Web_PLC.NavigateToString(LeereHtmlSeite);
            Web_BUG.NavigateToString(LeereHtmlSeite);

            if (rb.Name.Contains("PLC"))
            {
                Web_PLC.NavigateToString(HtmlSeite);
            }
            else
            {
                if (rb.Name.Contains("BUG")) Web_BUG.NavigateToString(HtmlSeite);
            }
        }

        private void ProjektStarten(object sender, RoutedEventArgs e)
        {

            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo("Projekte");
            string sourceDirectory = ParentDirectory.FullName + "\\" + ProjektName;

            try
            {
                DarstellungAendernListe(ButtonListe, true, Colors.Yellow, "Ordner " + ProjektPfad + " löschen");
                if (System.IO.Directory.Exists(ProjektPfad)) System.IO.Directory.Delete(ProjektPfad, true);
            }
            catch (Exception exp)
            {
                Console.WriteLine("{0} Exception 2 caught.", exp);
            }

            try
            {
                DarstellungAendernListe(ButtonListe, true, Colors.Yellow, "Ordner " + ProjektPfad + " erstellen");
                System.IO.Directory.CreateDirectory(ProjektPfad);
            }
            catch (Exception exp)
            {
                Console.WriteLine("{0} Exception 3 caught.", exp);
            }

            try
            {
                DarstellungAendernListe(ButtonListe, true, Colors.Yellow, "Alle Dateien kopieren");
                Copy(sourceDirectory, ProjektPfad);
            }
            catch (Exception exp)
            {
                Console.WriteLine("{0} Exception 4 caught.", exp);
            }

            try
            {
                DarstellungAendernListe(ButtonListe, true, Colors.LawnGreen, "Projekt mit TwinCAT V3 öffnen");
                Process proc = new Process();
                proc.StartInfo.FileName = ProjektPfad + "\\start.cmd";
                proc.StartInfo.WorkingDirectory = ProjektPfad;
                proc.Start();
            }
            catch (Exception exp)
            {
                Console.WriteLine("{0} Exception 5 caught.", exp);
            }

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
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void TabControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            DarstellungAendernListe(ButtonListe, false, Colors.Gray, "Projekt auswählen");
            AlleRadioButtonsDeaktivieren();

            string LeereHtmlSeite = "<!doctype html>   </html >";
            Web_PLC.NavigateToString(LeereHtmlSeite);
            Web_BUG.NavigateToString(LeereHtmlSeite);
        }


        private void DarstellungAendernListe(List<Button> KnopfListe, bool Enable, Color Farbe, string Text)
        {
            foreach (Button Knopf in KnopfListe)
            {
                Knopf.IsEnabled = Enable;
                Knopf.Background = new SolidColorBrush(Farbe);
                Knopf.Content = Text;
                Knopf.Refresh();
            }
        }


        private void AlleRadioButtonsDeaktivieren()
        {
            foreach (RadioButton R_Button in RadioButtonList)
            {
                if (R_Button.IsChecked == true) R_Button.IsChecked = false;
            }
        }

    }
}
