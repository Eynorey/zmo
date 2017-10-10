using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ZMO.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// Die Elementvorlage für die Seite "Elementdetails" ist unter http://go.microsoft.com/fwlink/?LinkId=234232 dokumentiert.

namespace ZMO
{

    public class fachSchnitte
    {
        public IList<string> Faecher { get; set; }
        public IList<string> Art { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public int Wertigkeitsmultiplikator { get; set; }
        public int Semester1 { get; set; }
        public int Semester2 { get; set; }
        public int Semester3 { get; set; }
        public int Semester4 { get; set; }
        public bool Streichen { get; set; }
    }

    public sealed partial class abischnitt : Page
    {
        #region NavHelper
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// Dies kann in ein stark typisiertes Anzeigemodell geändert werden.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper wird auf jeder Seite zur Unterstützung bei der Navigation verwendet und 
        /// Verwaltung der Prozesslebensdauer
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        

        /// <summary>
        /// Füllt die Seite mit Inhalt auf, der bei der Navigation übergeben wird.  Gespeicherte Zustände werden ebenfalls
        /// bereitgestellt, wenn eine Seite aus einer vorherigen Sitzung neu erstellt wird.
        /// </summary>
        /// <param name="sender">
        /// Die Quelle des Ereignisses, normalerweise <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Ereignisdaten, die die Navigationsparameter bereitstellen, die an
        /// <see cref="Frame.Navigate(Type, Object)"/> als diese Seite ursprünglich angefordert wurde und
        /// ein Wörterbuch des Zustands, der von dieser Seite während einer früheren
        /// beibehalten wurde.  Der Zustand ist beim ersten Aufrufen einer Seite NULL.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            object navigationParameter;
            if (e.PageState != null && e.PageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = e.PageState["SelectedItem"];
            }

            // TODO: this.DefaultViewModel["Group"] eine bindbare Gruppe zuweisen
            // TODO: this.DefaultViewModel["Items"] eine Auflistung von bindbaren Elementen zuweisen
            // TODO: this.flipView.SelectedItem das ausgewählte Element zuweisen
        }

        #region NavigationHelper-Registrierung

        /// Die in diesem Abschnitt bereitgestellten Methoden lassen sich einfach für Folgendes verwenden:
        /// Reaktion von NavigationHelper auf die Navigationsmethoden der Seite.
        /// 
        /// Platzieren von seitenspezifischer Logik in Ereignishandlern für  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// und <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// Der Navigationsparameter ist in der LoadState-Methode verfügbar 
        /// zusätzlich zum Seitenzustand, der während einer früheren Sitzung beibehalten wurde.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        #endregion

        #region Deklarationen
        public StorageFolder data = ApplicationData.Current.RoamingFolder;
        public StorageFile zensurenFile;
        public StorageFile faecherFile;

        public int gesamtZahl;
        public double gesamtSumme;
        public int gesamtAnzahlNoten;
        public double gesamtSummeNoten;
        public double gesamtRundSumme;

        public IList<string> alleNoten;
        public IList<string> faecherIList;
        public IList<string> zensurenIList;

        public IList<string> alleFaecherAktuellesSemester;

        public IList<fachSchnitte> gesamtUebersicht;

        public int fachNummer = 0;
        #endregion

        public abischnitt()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

            allesLaden();
        }

        private async void allesLaden()
        {
            #region laden
            faecherFile = await data.GetFileAsync("faecher.rey");
            faecherIList = await FileIO.ReadLinesAsync(faecherFile);
            #endregion

            #region rechnen
            for (int sem = 1; sem < 5; sem++)
            {
                alleFaecherAktuellesSemester = new List<string>();

                data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + sem);
                zensurenFile = await data.GetFileAsync("zensuren.rey");
                zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);
                for (int fach = 0; fach < faecherIList.Count(); fach++)
                {
                    durchschnittRechner(fach);
                }

                await FileIO.WriteLinesAsync(await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("abiturschnitt")).GetFileAsync("semester" + sem + ".rey"), alleFaecherAktuellesSemester);
            }
            #endregion

            #region zuweisen

            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("abiturschnitt");
            var sems = await data.GetFilesAsync();
           
            gesamtUebersicht = new List<fachSchnitte>();
            var counter = 0;
            foreach (var fach in faecherIList)
            {
                gesamtUebersicht.Add(new fachSchnitte
                {
                    Index = counter,
                    Name = fach,
                    Art = new List<string>() { "erhöhtes Niveau", "erhöhtes Niveau (Abiturfach)", "grundlegendes Niveau", "grundlegendes Niveau (Abiturfach)" },
                    Wertigkeitsmultiplikator = 1,
                    Semester1 = Convert.ToInt32((await FileIO.ReadLinesAsync(sems.ElementAt(0))).ElementAt(counter)),
                    Semester2 = Convert.ToInt32((await FileIO.ReadLinesAsync(sems.ElementAt(1))).ElementAt(counter)),
                    Semester3 = Convert.ToInt32((await FileIO.ReadLinesAsync(sems.ElementAt(2))).ElementAt(counter)),
                    Semester4 = Convert.ToInt32((await FileIO.ReadLinesAsync(sems.ElementAt(3))).ElementAt(counter)),
                    Streichen = false
                });
                counter++;
            }

            listViewFaecher.ItemsSource = gesamtUebersicht;
            #endregion
        }

        private void streichenChecked()
        {

        }

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
                }
            }
            foreach (string i in stunde)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                }
            }
            foreach (string i in mitarbeit)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                }
            }
            foreach (string i in sonstige)
            {
                if (i != "keine")
                {
                    summeOhneKlausur += (Convert.ToInt16(i));
                    anzahlOhneKlausur++;
                }
            }
            foreach (string i in klausur)
            {
                if (i != "keine")
                {
                    summeKlausur += (Convert.ToInt16(i));
                    anzahlKlausur++;
                }
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

            alleFaecherAktuellesSemester.Add(Math.Round(durchschnittGesamt, 0).ToString().Replace("NaN", "/"));
        }
        #endregion

        private void streichenChecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
