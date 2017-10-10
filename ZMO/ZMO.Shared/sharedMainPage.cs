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
    public class avg
    {
        public string Notenpunkte { get; set; }
        public string Zensur { get; set; }
    }
    public class fachDetail
    {
        public string Name { get; set; }
        public string Zensuren { get; set; }
        public string Klausur { get; set; }
        public string Leistungsnachweis { get; set; }
        public string StandNP { get; set; }
        public string StandZ { get; set; }
    }

    public partial class MainPage : Page
    {
      
        #region Deklarationen
        public string[] artArray = { "Test", "Stunden- + Hausaufgabe", "Mitarbeitsnote", "sonstige Note", "Klausur + Leistungsnachweis" };
        
        public StorageFolder data;
        public StorageFile zensurenFile;
        public StorageFile faecherFile;
        public StorageFile lehrerFile;

        public int gesamtZahl;
        public double gesamtSumme;
        public int gesamtAnzahlNoten;
        public double gesamtSummeNoten;
        public double gesamtRundSumme;

        public IList<string> alleNoten;
        public IList<string> faecherIList;
        public IList<string> zensurenIList;
        public IList<string> lehrerIList;

        public IList<string> zUebersicht;
        public IList<string> kUebersicht;
        public IList<string> gUebersicht;

        public IList<fachDetail> faecher;

        public int fachNummer = 0;
        #endregion

        private void loadSettings()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["TopBar"] == null)
            {
                settings.Values["TopBar"] = "0";
            }
            if (settings.Values["LiveTile"] == null)
            {
                settings.Values["LiveTile"] = "true";
            }
            if (settings.Values["Semester"] == null)
            {
                settings.Values["Semester"] = "1";
            }
            if (settings.Values["Farbe"] == null)
            {
                settings.Values["Farbe"] = "#3D6E89";
            }
            if (settings.Values["UpdateWP"] == null)
            {
                settings.Values["UpdateWP"] = "0";
            }
            if (settings.Values["UpdateWin"] == null)
            {
                settings.Values["UpdateWin"] = "0";
            }
        }

        private async void startUp(bool reparieren = false)
        {
            //Reparatur nötig?
            if (reparieren)
            {
                #region Reparatur
                var localData = ApplicationData.Current.LocalFolder;
                var roamingData = ApplicationData.Current.RoamingFolder;

                try
                {
                    //nach Local
                    await (await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey")).CopyAsync(localData, "faecher.rey", NameCollisionOption.ReplaceExisting);
                    for (int i = 1; i < 5; i++)
                    {
                        var repZensuren = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                        await repZensuren.CopyAsync(await localData.CreateFolderAsync("Semester" + i, CreationCollisionOption.ReplaceExisting));
                    }
                    await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Roaming);

                }
                catch (Exception)
                { }
                try
                {
                    //nach Roaming
                    await (await ApplicationData.Current.LocalFolder.GetFileAsync("faecher.rey")).CopyAsync(roamingData);
                    for (int i = 1; i < 5; i++)
                    {
                        var repZensuren = await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                        await repZensuren.CopyAsync(await roamingData.CreateFolderAsync("Semester" + i));
                    }
                    await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Local);
                }
                catch (Exception)
                { }
                
                #endregion

                this.InitializeComponent();
                repairMessage.Visibility = Visibility.Visible;

            }
            else
            {
                //Fächer vorhanden? --> anlegen
                #region Fächer
                if ((await ApplicationData.Current.RoamingFolder.GetFilesAsync()).Count() != 0)
                {
                    faecherFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey");
                }
                else
                {
                    faecherFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync("faecher.rey", CreationCollisionOption.ReplaceExisting);
                }
                #endregion

                //Ordner vorhanden? --> Anlegen
                #region Semester
                if ((await ApplicationData.Current.RoamingFolder.GetFoldersAsync()).Count() == 0)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        StorageFolder sem = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Semester" + i, CreationCollisionOption.OpenIfExists);
                        await sem.CreateFileAsync("zensuren.rey", CreationCollisionOption.ReplaceExisting);
                    }
                }
                if ((await ApplicationData.Current.RoamingFolder.GetFoldersAsync()).Count() == 4)
                {
                    StorageFolder abi = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("abiturschnitt", CreationCollisionOption.OpenIfExists);
                    for (int i = 1; i < 5; i++)
                    {
                        await abi.CreateFileAsync("semester" + i + ".rey", CreationCollisionOption.ReplaceExisting);
                    }
                }
                #endregion

                chooseSemester(Convert.ToInt16(ApplicationData.Current.RoamingSettings.Values["Semester"]), true);
            }
        }

        #region Semester
        private void sem_Click(object sender, RoutedEventArgs e)
        {
            int sem = Convert.ToInt16((sender as FrameworkElement).Name.Substring(3, 1));
            chooseSemester(sem);
        }
        private async void chooseSemester(int semester, bool load = true)
        {
            await Task.Delay(100);
            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + semester);
            zensurenFile = await data.GetFileAsync("zensuren.rey");
            zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);

            if (load)
            {
                ApplicationData.Current.RoamingSettings.Values["Semester"] = semester.ToString();
                loadData();

                #region farbe/Überschrift
                sem1.IsChecked = false;
                sem2.IsChecked = false;
                sem3.IsChecked = false;
                sem4.IsChecked = false;

                switch (semester)
                {
                    case 1:
                        {
                            sem1.IsChecked = true;
                            semesterCaption.Text = "erstes Semester";
                            break;
                        }
                    case 2:
                        {
                            sem2.IsChecked = true;
                            semesterCaption.Text = "zweites Semester";
                            break;
                        }
                    case 3:
                        {
                            sem3.IsChecked = true;
                            semesterCaption.Text = "drittes Semester";
                            break;
                        }
                    case 4:
                        {
                            sem4.IsChecked = true;
                            semesterCaption.Text = "viertes Semester";
                            break;
                        }
                }
                #endregion
            }
        }
        #endregion

        #region Rechner
        private void durchschnittRechner(int fachNr)
        {
            alleNoten = zensurenIList.ElementAt(fachNr).Split(',');

            string[] tests = alleNoten.ElementAt(0).Split('|');
            string[] stunde = alleNoten.ElementAt(1).Split('|');
            string[] mitarbeit = alleNoten.ElementAt(2).Split('|');
            string[] sonstige = alleNoten.ElementAt(3).Split('|');
            string[] klausur = alleNoten.ElementAt(4).Split('|');

            float durchschnittGesamt = 0;

            string linie = "";
            string KLLN = "";
            int summeOhneKlausur = 0;
            int anzahlOhneKlausur = 0;

            int summeKlausur = 0;
            int anzahlKlausur = 0;

            #region extrahieren
            foreach (string i in tests)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                    linie += i + "   ";
                }
            }
            foreach (string i in stunde)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                    linie += i + "   ";
                }
            }
            foreach (string i in mitarbeit)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                    linie += i + "   ";
                }
            }
            foreach (string i in sonstige)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                    linie += i + "   ";
                }
            }
            foreach (string i in klausur)
            {
                if (i != "keine")
                {
                    summeKlausur += (Convert.ToInt16(i));
                    anzahlKlausur++;
                    KLLN += i;
                }
                KLLN += "|";
            }
            #endregion

            if (anzahlKlausur == 1)
            {
                durchschnittGesamt = (((float)summeOhneKlausur / (float)anzahlOhneKlausur) / 3.0f * 2.0f) + (((float)summeKlausur / (float)klausur.Count()) / 3.0f);
            }
            else if (anzahlKlausur > 1)
            {
                durchschnittGesamt = (((float)summeOhneKlausur / (float)anzahlOhneKlausur) / 3.0f) + (((float)summeKlausur / (float)klausur.Count()) / 3.0f * 2.0f);
            }
            else
            {
                durchschnittGesamt = ((float)summeOhneKlausur / (float)anzahlOhneKlausur);
            }
            if (anzahlOhneKlausur == 0)
            {
                durchschnittGesamt = ((float)summeKlausur / (float)anzahlKlausur);
            }

            if (durchschnittGesamt.ToString() != "NaN")
            {
                gesamtSumme += (float)durchschnittGesamt;
                gesamtRundSumme += Math.Round(durchschnittGesamt, 0, MidpointRounding.AwayFromZero);
                gesamtZahl++;
            }

            gesamtAnzahlNoten += (anzahlOhneKlausur + anzahlKlausur);
            gesamtSummeNoten += (summeOhneKlausur + summeKlausur);

            string LN = "";
            string KL = "";
            if (KLLN.Split('|').ElementAt(1) != "")
            {
                LN = "LN: " + KLLN.Split('|').ElementAt(1);
            }
            if (KLLN.Split('|').ElementAt(0) != "")
            {
                KL = "Klausur: " + KLLN.Split('|').ElementAt(0);
            }

            faecher.Add(new fachDetail
            {
                Name = faecherIList.ElementAt(fachNr).Trim(),
                Zensuren = linie,
                Klausur = KL,
                Leistungsnachweis = LN,
                StandNP = (Math.Round(durchschnittGesamt, 2).ToString().Replace("NaN", "/")),
                StandZ = (Math.Round((17 - durchschnittGesamt) / 3, 2).ToString().Replace("NaN", "/"))
            });
        }
        #endregion

        #region LoadData
        public async void loadData()
        {
            zensurenFile = await data.GetFileAsync("zensuren.rey");

            faecherIList = await FileIO.ReadLinesAsync(faecherFile);
            zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);

            #region keine Fächer / Update Message
            showDialogue();
            #endregion

            #region reset
            gesamtAnzahlNoten = 0;
            gesamtSummeNoten = 0;
            gesamtZahl = 0;
            gesamtSumme = 0;
            gesamtRundSumme = 0;
            #endregion

            await Task.Delay(30);

            faecher = new List<fachDetail>();

            try
            {
                for (int y = 0; y < faecherIList.Count(); y++)
                {
                    fachNummer = y;
                    durchschnittRechner(y);
                }
            }
            catch (Exception)
            {
                showError();
            }

            faecherUebersicht.ItemsSource = faecher;

            #region Gesamtübersicht
            gesamtAnzahl.Text = gesamtAnzahlNoten.ToString();

            //exakte Endnoten
            gesamtPunkte.Text = Math.Round(gesamtSumme / gesamtZahl, 2).ToString().Replace("NaN", "/");
            gesamtNote.Text = Math.Round((17 - (gesamtSumme / gesamtZahl)) / 3, 2).ToString("0.00").Replace("NaN", "/");

            //gerundete Endnoten
            gesamtRundPunkte.Text = Math.Round(gesamtRundSumme / gesamtZahl, 2).ToString().Replace("NaN", "/");
            gesamtRundNote.Text = Math.Round((17 - (gesamtRundSumme / gesamtZahl)) / 3, 2).ToString("0.00").Replace("NaN", "/");

            //Alle Zensuren
            gesamtZensurenPunkte.Text = Math.Round(gesamtSummeNoten / gesamtAnzahlNoten, 2).ToString().Replace("NaN", "/");
            gesamtZensurenNote.Text = Math.Round((17 - (gesamtSummeNoten / gesamtAnzahlNoten)) / 3, 2).ToString("0.00").Replace("NaN", "/");

            #endregion

            SetLiveTile("Punkte: " + gesamtRundPunkte.Text, "Note: " + gesamtRundNote.Text);
        }
        #endregion

        #region file corrupt error
        private async void showError()
        {
            MessageDialog error = new MessageDialog("Eine der Dateien ist korrupt und konnte nicht verarbeitet werden."
                + Environment.NewLine + Environment.NewLine
                + "Semester: " + ApplicationData.Current.RoamingSettings.Values["Semester"].ToString()
                + Environment.NewLine
                + "Fach: " + faecherIList.ElementAt(fachNummer).Trim()
                , "Fehler");
            error.Commands.Add(new UICommand("betroffene Daten löschen", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            await error.ShowAsync();
        }
        private async void CommandInvokedHandler(IUICommand error)
        {
            switch (error.Label)
            {
                case "betroffene Daten löschen":
                    {
                        zensurenIList.RemoveAt(fachNummer);
                        zensurenIList.Insert(fachNummer, "keine,keine,keine,keine,keine");
                        await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);
                        loadData();
                        break;
                    }
            }
        }
        #endregion

        #region Semester löschen
        private void deleteThisSem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }
        private void deleteAllSem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }

        private async void deleteThisSemConfirm_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < zensurenIList.Count(); i++)
            {
                zensurenIList.RemoveAt(i);
                zensurenIList.Insert(i, "keine,keine,keine,keine,keine");
            }
            await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);
            loadData();
            await new MessageDialog("Alle Zensuren des" + ApplicationData.Current.RoamingSettings.Values["Semester"].ToString() + ". Semesters wurden gelöscht.", "Zensuren gelöscht").ShowAsync();
        }
        private async void deleteAllSemConfirm_Click(object sender, RoutedEventArgs e)
        {
            for (int k = 1; k < 5; k++)
            {
                chooseSemester(k, false);
                await Task.Delay(30);

                for (int i = 0; i < zensurenIList.Count(); i++)
                {
                    zensurenIList.RemoveAt(i);
                    zensurenIList.Insert(i, "keine,keine,keine,keine,keine");
                }
                await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);
            }
            loadData();
            await new MessageDialog("Alle in der App gespeicherten Zensuren wurden gelöscht.", "Zensuren gelöscht").ShowAsync();
        }
        #endregion

        #region neues eintragen
        private async void zensurNeu(int index)
        {
            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + Convert.ToInt16(ApplicationData.Current.RoamingSettings.Values["Semester"]));
            zensurenFile = await data.GetFileAsync("zensuren.rey");

            zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);
            string[] alleNotenArray = zensurenIList.ElementAt(index).Split(',');
            alleNoten = alleNotenArray.ToList();

            string temp = alleNoten.ElementAt(zensurNeuArt.SelectedIndex);
            if (temp == "keine")
            {
                alleNoten.RemoveAt(zensurNeuArt.SelectedIndex);
                alleNoten.Insert(zensurNeuArt.SelectedIndex, zensurNeuZensur.Value.ToString());
            }
            else
            {
                alleNoten.RemoveAt(zensurNeuArt.SelectedIndex);
                alleNoten.Insert(zensurNeuArt.SelectedIndex, temp + "|" + zensurNeuZensur.Value.ToString());
            }

            zensurenIList.RemoveAt(index);
            zensurenIList.Insert(index, alleNoten.ElementAt(0) + "," + alleNoten.ElementAt(1) + "," + alleNoten.ElementAt(2) + "," + alleNoten.ElementAt(3) + "," + alleNoten.ElementAt(4));
            await FileIO.WriteLinesAsync(zensurenFile, zensurenIList);

            loadData();
            await Task.Delay(100);
            faecherUebersicht.SelectedIndex = index;

            string message = "Zensur eingetragen.";
            string klausurLn = "";
            if (zensurNeuArt.SelectedIndex == 4)
            {
                klausurLn = Environment.NewLine + Environment.NewLine + "Hinweis: Eine Klausurnote wird als 1/3 der Gesamtnote berechnet." + Environment.NewLine + "Wird eine weitere Zensur (Anderer Leistungsnachweis) eingetragen, zählt dieser ebenfalls zu 1/3.";
            }
            if (zensurNeuZensur.Value == 0)
            {
                message = "Ungenügend!";
            }
            else if (zensurNeuZensur.Value > 0 && zensurNeuZensur.Value < 4)
            {
                message = "Mangelhaft!";
            }
            else if (zensurNeuZensur.Value > 3 && zensurNeuZensur.Value < 7)
            {
                message = "Ausreichend!";
            }
            else if (zensurNeuZensur.Value > 6 && zensurNeuZensur.Value < 10)
            {
                message = "Befriedigend!";
            }
            else if (zensurNeuZensur.Value > 9 && zensurNeuZensur.Value < 13)
            {
                message = "Gut!";
            }
            else if (zensurNeuZensur.Value > 12)
            {
                message = "Sehr gut!";
            }
            await new MessageDialog(
                zensurNeuZensur.Value +
                " Notenpunkte wurden dem Fach " +
                faecher.ElementAt(faecherUebersicht.SelectedIndex).Name.Trim() +
                " als "
                + zensurNeuArt.SelectedValue +
                " hinzugefügt!" +
                klausurLn
                , message).ShowAsync();
        }

        private void fachNeuEintragen_Click(object sender, RoutedEventArgs e)
        {
            fachEintragen();
        }
        private async void fachEintragen()
        {
            if (fachNeuName.Text.Length != 0)
            {
                faecherIList.Add(fachNeuName.Text);
                await FileIO.WriteLinesAsync(faecherFile, faecherIList);

                for (int i = 1; i < 5; i++)
                {
                    StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                    IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                    tempZensurenIList.Add("keine,keine,keine,keine,keine");
                    await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
                }
                loadData();
            }
            else
            {
                MessageDialog fachNeuError = new MessageDialog("Kein Fach eingegeben", "Fehler");
                await fachNeuError.ShowAsync();
            }

            fachNeuName.Text = "";
            await Task.Delay(500);
            faecherUebersicht.SelectedIndex = faecherIList.Count() - 1;
            fachNeuButton_Click(fachNeuButton, new RoutedEventArgs());
        }
        #endregion

        #region umbenennen/bearbeiten
        private void fachRenameName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                fachRenameButton_Click(new object(), new RoutedEventArgs());
            }
        }
        private void fachNeuName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                fachNeuEintragen_Click(new object(), new RoutedEventArgs());
            }
        }
        #endregion

        private async void santaHat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MessageDialog("Eynorey wünscht frohe Festtage!" + Environment.NewLine + Environment.NewLine + "(und bedankt sich weiterhin für die Benutzung dieser App :)", "Hey!").ShowAsync();
        }
    }
}
