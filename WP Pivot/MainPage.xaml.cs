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

// Die Vorlage "Pivotanwendung" ist unter http://go.microsoft.com/fwlink/?LinkID=391641 dokumentiert.

namespace ZMO
{
    public sealed partial class MainPage : Page
    {
        #region pivot
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";
        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            startUp();
            startUp2();
        }

        private async void startUp2()
        {
            zensurNeuArt.ItemsSource = artArray;
            await Task.Delay(30);
            chooseSemester(Convert.ToInt16(await FileIO.ReadTextAsync(semesterFile)), true);
        }

        public async void loadData(bool delay)
        {
            if (delay)
            {
                await Task.Delay(50);
            }
            zensurenFile = await data.GetFileAsync("zensuren.rey");
            tempFile = await data.GetFileAsync("temp.rey");

            faecherIList = await FileIO.ReadLinesAsync(faecherFile);
            zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);
            zensurNeuFach.ItemsSource = faecherIList;

            #region keine Fächer Meldung
            if (faecherIList.Count() == 0)
            {
                showWelcome();
            }
            else
            {
                zensurNeuFach.ItemsSource = faecherIList;
            }
            #endregion

            #region reset
            gesamtAnzahlNoten = 0;
            gesamtZahl = 0;
            gesamtSumme = 0;
            gesamtRundSumme = 0;
            zUebersicht = new List<string>();
            kUebersicht = new List<string>();
            gUebersicht = new List<string>();
            #endregion

            await Task.Delay(30);

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

            #region Gesamtübersicht
            gesamtAnzahl.Text = gesamtAnzahlNoten.ToString();
            gesamtPunkte.Text = Math.Round(gesamtSumme / gesamtZahl, 2).ToString().Replace("NaN", "/");
            gesamtNote.Text = Math.Round((17 - (gesamtSumme / gesamtZahl)) / 3, 2).ToString().PadRight(2, ',').PadRight(3, '0').Replace("NaN", "/");

            gesamtRundPunkte.Text = Math.Round(gesamtRundSumme / gesamtZahl, 2).ToString().Replace("NaN", "/");
            gesamtRundNote.Text = Math.Round((17 - (gesamtRundSumme / gesamtZahl)) / 3, 2).ToString().PadRight(2, ',').PadRight(3, '0').Replace("NaN", "/");
            #endregion

            SetLiveTile("Punkte: " + gesamtRundPunkte.Text, "Note: " + gesamtRundNote.Text);
        }

        #region welcome Dialog
        private async void showWelcome()
        {
            MessageDialog welcome = new MessageDialog(
                "Es sind noch keine Fächer vorhanden."
                + Environment.NewLine +
                "Unter Detailansicht in der AppBar können neue Fächer hinzugefügt werden."
                + Environment.NewLine + Environment.NewLine +
                "Viel Spaß mit der App! :)", "Willkommen!"
                );
            welcome.Commands.Add(new UICommand("los geht's!", new UICommandInvokedHandler(this.welcomeHandler)));
            await welcome.ShowAsync();
        }
        private void welcomeHandler(IUICommand error)
        {
            this.Frame.Navigate(typeof(detailPage));
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(detailPage));
        }

        private void zensurNeuFach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkEintragen();
        }
        private void zensurNeuArt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkEintragen();
        }
        private void checkEintragen()
        {
            if (zensurNeuArt.SelectedIndex != -1 && zensurNeuFach.SelectedIndex != -1)
            {
                zensurNeuEintragen.IsEnabled = true;
            }
            else
            {
                zensurNeuEintragen.IsEnabled = false;
            }
        }

        private async void zensurNeuEintragen_Click(object sender, RoutedEventArgs e)
        {
            int index = zensurNeuFach.SelectedIndex;

            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + Convert.ToInt16(await FileIO.ReadTextAsync(semesterFile)));
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

            string message = "Zensur eingetragen.";
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
            await new MessageDialog(zensurNeuZensur.Value + " Notenpunkte wurden dem Fach " + zensurNeuFach.SelectedValue.ToString().Trim() + " als " + zensurNeuArt.SelectedValue + " hinzugefügt!", message).ShowAsync();

            loadData(false);

            await Task.Delay(30);

            zensurNeuFach.SelectedIndex = index;
            zensurNeuZensur.Value = 0;
        }

        private void semesterCaption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(detailPage));
        }

        #region Tile
        public void SetLiveTile(string text1, string text2)
        {
            var tileXml2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImage02);
            var tileXml3 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);

            var notification2 = new ScheduledTileNotification(tileXml2, DateTime.Now + TimeSpan.FromSeconds(2));
            var notification3 = new ScheduledTileNotification(tileXml3, DateTime.Now + TimeSpan.FromSeconds(4));

            notification2.ExpirationTime = DateTime.Now + TimeSpan.FromDays(14);
            notification3.ExpirationTime = DateTime.Now + TimeSpan.FromDays(14);

            //weit
            XmlNodeList nodes2 = tileXml2.GetElementsByTagName("image");
            nodes2[0].Attributes[1].NodeValue = "ms-appx:///Assets/" + "wide-live.png";
            nodes2 = tileXml2.GetElementsByTagName("text");
            nodes2[0].InnerText = "Stand";
            nodes2[1].InnerText = text1;
            nodes2[2].InnerText = text2;

            //normal
            XmlNodeList nodes3 = tileXml3.GetElementsByTagName("image");
            nodes3[0].Attributes[1].NodeValue = "ms-appx:///Assets/" + "small-live.png";
            nodes3 = tileXml3.GetElementsByTagName("text");
            nodes3[0].InnerText = "Stand";
            nodes3[1].InnerText = text1;
            nodes3[2].InnerText = text2;

            if (gesamtAnzahlNoten != 0)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(notification2);
                TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(notification3);
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        #endregion
    }
}
