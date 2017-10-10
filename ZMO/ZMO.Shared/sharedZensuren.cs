using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Store;

namespace ZMO
{
    public class fach
    {
        public string Name { get; set; }
    }
    
    public partial class zensuren : Page
    {
        #region Deklarationen
        public string[] artArray = { "  Tests: ", "  Stunden- + Hausarbeiten: ", "  Mitarbeit: ", "  Sonstige Noten: ", "  Klausur + Leistungsnachweis: " };
        public SolidColorBrush blueBrush = new SolidColorBrush { Color = Windows.UI.Color.FromArgb(255, 27, 161, 226) };
        public StorageFolder data;
        public StorageFile zensurenFile;
        public StorageFile faecherFile;

        public IList<string> zensurenIList;
        public IList<string> alleNoten;

        public IList<fach> faecherIList = new List<fach>();

        public string[] tests;
        public string[] stunde;
        public string[] mitarbeit;
        public string[] sonstige;
        public string[] klausur;

        public int fachNummer = 0;
        public int semester = 1;

        public FrameworkElement selection;
        public bool close = true;
        #endregion

        private async void startUp()
        {
            semester = Convert.ToInt16(ApplicationData.Current.RoamingSettings.Values["Semester"]);
            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + semester);

            faecherFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey");

            foreach (string n in await FileIO.ReadLinesAsync(faecherFile))
            {
                faecherIList.Add(new fach { Name = n });
            }

            fachSelector.ItemsSource = faecherIList;

            try
            {
                int fach = Convert.ToInt32(ApplicationData.Current.RoamingSettings.Values["Temp"]);
                if (fach != -1)
                {
                    fachSelector.SelectedIndex = fach;
                    ApplicationData.Current.RoamingSettings.Values["Fach"] = null;
                    mainGrid.Visibility = Visibility.Visible;
                    deleteButton.IsEnabled = true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                mainGrid.Visibility = Visibility.Collapsed;
                deleteButton.IsEnabled = false;
            }
            await Task.Delay(50);
            chooseSemester(semester, false);
        }

        private async void loadData()
        {
            if (fachSelector.SelectedIndex != -1)
            {
                zensurenFile = await data.GetFileAsync("zensuren.rey");
                zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);

                string[] alleNotenArray = zensurenIList.ElementAt(fachNummer).Split(',');
                alleNoten = alleNotenArray.ToList();
                tests = alleNoten.ElementAt(0).Split('|');
                stunde = alleNoten.ElementAt(1).Split('|');
                mitarbeit = alleNoten.ElementAt(2).Split('|');
                sonstige = alleNoten.ElementAt(3).Split('|');
                klausur = alleNoten.ElementAt(4).Split('|');

                testNoten.Text = "";
                stundenNoten.Text = "";
                mitarbeitNoten.Text = "";
                sonstigeNoten.Text = "";
                klausurNoten.Text = "";

                foreach (string i in tests)
                {
                    testNoten.Text += i + "   ";
                }
                foreach (string i in stunde)
                {
                    stundenNoten.Text += i + "   ";
                }
                foreach (string i in mitarbeit)
                {
                    mitarbeitNoten.Text += i + "   ";
                }
                foreach (string i in sonstige)
                {
                    sonstigeNoten.Text += i + "   ";
                }
                foreach (string i in klausur)
                {
                    klausurNoten.Text += i + "   ";
                }

                durchschnittRechner();
                mainGrid.Visibility = Visibility.Visible;
                deleteButton.IsEnabled = true;
                fachName.Text = " " + faecherIList.ElementAt(fachSelector.SelectedIndex).Name.Trim();
            }
        }
        private void durchschnittRechner()
        {
            int testSumme = 0;
            int stundenSumme = 0;
            int mitarbeitSumme = 0;
            int sonstigesSumme = 0;
            int klausurSumme = 0;

            int summeOhneKlausur = 0;
            int anzahlOhneKlausur = 0;
            int anzahlKlausur = 0;
            float durchschnittOhneKlausur = 0;
            float durchschnittGesamt = 0;

            #region extrahieren
            if (tests.ElementAt(0) != "keine")
            {
                for (int i = 0; i < tests.Count(); i++)
                {
                    testSumme += Convert.ToInt32(tests[i]);
                }
                testDurchschnitt.Text = Math.Round(((float)testSumme / (float)tests.Count()), 2).ToString();
                summeOhneKlausur += testSumme;
                anzahlOhneKlausur += tests.Count();
            }
            else
            {
                testDurchschnitt.Text = "/";
            }
            if (stunde.ElementAt(0) != "keine")
            {
                for (int i = 0; i < stunde.Count(); i++)
                {
                    stundenSumme += Convert.ToInt32(stunde[i]);
                }
                stundenDurchschnitt.Text = Math.Round(((float)stundenSumme / (float)stunde.Count()), 2).ToString();
                summeOhneKlausur += stundenSumme;
                anzahlOhneKlausur += stunde.Count();
            }
            else
            {
                stundenDurchschnitt.Text = "/";
            }
            if (mitarbeit.ElementAt(0) != "keine")
            {
                for (int i = 0; i < mitarbeit.Count(); i++)
                {
                    mitarbeitSumme += Convert.ToInt32(mitarbeit[i]);
                }
                mitarbeitDurchschnitt.Text = Math.Round(((float)mitarbeitSumme / (float)mitarbeit.Count()), 2).ToString();
                summeOhneKlausur += mitarbeitSumme;
                anzahlOhneKlausur += mitarbeit.Count();
            }
            else
            {
                mitarbeitDurchschnitt.Text = "/";
            }
            if (sonstige.ElementAt(0) != "keine")
            {
                for (int i = 0; i < sonstige.Count(); i++)
                {
                    sonstigesSumme += Convert.ToInt32(sonstige[i]);
                }
                sonstigeDurchschnitt.Text = Math.Round(((float)sonstigesSumme / (float)sonstige.Count()), 2).ToString();
                summeOhneKlausur += sonstigesSumme;
                anzahlOhneKlausur += sonstige.Count();
            }
            else
            {
                sonstigeDurchschnitt.Text = "/";
            }

            if (klausur.ElementAt(0) != "keine")
            {
                for (int i = 0; i < klausur.Count(); i++)
                {
                    klausurSumme += Convert.ToInt32(klausur[i]);
                }
                klausurDurchschnitt.Text = Math.Round(((float)klausurSumme / (float)klausur.Count()), 2).ToString();
                anzahlKlausur += klausur.Count();
            }
            else
            {
                klausurDurchschnitt.Text = "/";
            }
#endregion

            if (anzahlOhneKlausur == 0)
            {
                durchschnittGesamt = ((float)klausurSumme / (float)anzahlKlausur);
            }

            if (anzahlKlausur == 0)
            {
                durchschnittGesamt = ((float)summeOhneKlausur / (float)anzahlOhneKlausur);
            }
            if (anzahlKlausur == 1)
            {
                durchschnittGesamt = (((float)summeOhneKlausur / (float)anzahlOhneKlausur) / 3 * 2) + ((float)klausurSumme / 3);
            }
            else if (anzahlKlausur > 1)
            {
                durchschnittGesamt = (((float)summeOhneKlausur / (float)anzahlOhneKlausur) / 3) + (((float)klausurSumme / (float)klausur.Count()) / 3 * 2);
            }

            durchschnittOhneKlausur = ((float)summeOhneKlausur / (float)anzahlOhneKlausur);
            durchschnittGesamtTB.Text = Math.Round((float)durchschnittGesamt, 2).ToString().Replace("NaN", "/");
            durchschnittGesamtNoteTB.Text = Math.Round((17 - (float)durchschnittGesamt) / 3, 2).ToString("0.00").Replace("NaN", "/");

        }

        private void fachSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fachSelector.SelectedIndex != -1)
            {
                mainGrid.Visibility = Visibility.Visible;
                fachNummer = fachSelector.SelectedIndex;
                loadData();
                editZensuren.Text = "";
                deleteButton.IsEnabled = true;
            }
            else
            {
                mainGrid.Visibility = Visibility.Collapsed;
                deleteButton.IsEnabled = false;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement deleteAll = sender as FrameworkElement;
            if (deleteAll != null)
            {
                FlyoutBase.ShowAttachedFlyout(deleteAll);
            }
        }
        private async void deleteAllZensuren_Click(object sender, RoutedEventArgs e)
        {
            zensurenIList.RemoveAt(fachNummer);
            zensurenIList.Insert(fachNummer, "keine,keine,keine,keine,keine");
            await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);
            loadData();
            bottomBar.IsOpen = false;
        }

        private async void speichern()
        {
            try
            {
                string newLine = "";
                if (editZensuren.Text.Trim() != "keine," && editZensuren.Text.Trim() != "" && editZensuren.Text.Replace(',', ' ').Replace('.', ' ').Trim() != "")
                {
                    foreach (string eingabe in editZensuren.Text.Trim().Replace(' ', ',').Replace('.', ',').Split(','))
                    {
                        if (eingabe.Trim() != "")
                        {
                            int np = Convert.ToInt32(eingabe);
                            if (np < 0 || np > 15)
                            {
                                throw new Exception();
                            }
                            newLine += np.ToString() + " ";
                        }
                    }
                }

                if (editZensuren.Text.Trim() == "" || editZensuren.Text.Trim().ToLower() == "keine" || editZensuren.Text.Replace(',', ' ').Replace('.', ' ').Trim() == "")
                {
                    newLine = "keine";
                }

                alleNoten.RemoveAt(kategorie);
                alleNoten.Insert(kategorie, newLine.Trim().Replace(' ', '|'));

                zensurenIList.RemoveAt(fachNummer);
                zensurenIList.Insert(fachNummer, alleNoten.ElementAt(0) + "," + alleNoten.ElementAt(1) + "," + alleNoten.ElementAt(2) + "," + alleNoten.ElementAt(3) + "," + alleNoten.ElementAt(4));
                await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);
                loadData();
                editFlyout.Hide();

                fachSelector.Focus(FocusState.Programmatic);
                close = true;
            }
            catch (Exception)
            {
                showError();
            }
        }

        private void resetOpacity()
        {
            re1.Opacity = 0;
            re2.Opacity = 0;
            re3.Opacity = 0;
            re4.Opacity = 0;
            re5.Opacity = 0;
        }

        #region edit
        private void showEditFlyout(FrameworkElement element)
        {
            close = true;
            selection = element;
            if (editZensuren.Text.Trim() == "keine,")
            {
                editZensuren.Text = "";
            }

            editFlyout.ShowAt(element);
            editZensuren.SelectionStart = editZensuren.Text.Length;
        }

        private void editZensuren_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                speichern();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            speichern();
        }

        private async void showError()
        {
            close = false;
            await new MessageDialog("Mindestens eine der eingegebenen Zensuren ist ungültig" + Environment.NewLine + "(Nur Zahlen von 0 bis 15 sind möglich)", "Hinweis").ShowAsync();
            editFlyout.ShowAt(selection);
            close = true;
        }
        #endregion


        private async void santaHat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MessageDialog("Eynorey wünscht frohe Festtage!" + Environment.NewLine + Environment.NewLine + "(und bedankt sich weiterhin für die Benutzung dieser App :)", "Hey!").ShowAsync();
        }
    }
}
