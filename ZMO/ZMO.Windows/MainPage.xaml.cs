using ZMO.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml;
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
using Windows.UI;
using Windows.ApplicationModel.Store;

namespace ZMO
{
    public sealed partial class MainPage : Page
    {
        #region NavHelper
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                navigationHelper.OnNavigatedTo(e);
            }
            catch (Exception)
            { }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        public MainPage()
        {
            try
            {
                loadSettings();
                startUp(false);
                App.Current.Resources["Foreground"] = GetColorFromHex(ApplicationData.Current.RoamingSettings.Values["Farbe"].ToString());
                this.InitializeComponent();
                this.navigationHelper = new NavigationHelper(this);
                this.navigationHelper.LoadState += navigationHelper_LoadState;
                this.navigationHelper.SaveState += navigationHelper_SaveState;
                //Weihnachten
                if (DateTime.Now.Month == 12)
                {
                    santaHat.Visibility = Visibility.Visible;
                }

                zensurNeuArt.ItemsSource = artArray;

                if (ApplicationData.Current.RoamingSettings.Values["TopBar"].ToString() == "1")
                {
                    TopAppBar.IsOpen = true;
                    ApplicationData.Current.RoamingSettings.Values["TopBar"] = "0";
                }
            }
            catch (Exception)
            {
                startUp(true);
            }
        }

        public static SolidColorBrush GetColorFromHex(string myColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                    Convert.ToByte(myColor.Substring(1, 2), 16),
                    Convert.ToByte(myColor.Substring(3, 2), 16),
                    Convert.ToByte(myColor.Substring(5, 2), 16),
                    Convert.ToByte(myColor.Substring(7, 2), 16)));
        }

        #region welcome Dialog / Update Message
        private async void showDialogue()
        {
            if (faecherIList.Count == 0)
            {
                MessageDialog welcome = new MessageDialog(
                               "Es sind noch keine Fächer vorhanden."
                               + Environment.NewLine +
                               "Um zu beginnen befindet sich in der AppBar ein Button zum Hinzufügen neuer Fächer."
                               + Environment.NewLine + Environment.NewLine +
                               "Viel Spaß mit der App! :)",
                               "Willkommen!"
                               );
                welcome.Commands.Add(new UICommand("los geht's!", new UICommandInvokedHandler(this.welcomeHandler)));
                await welcome.ShowAsync();
                ApplicationData.Current.RoamingSettings.Values["UpdateWin"] = "2";
            }

            if (ApplicationData.Current.RoamingSettings.Values["UpdateWin"].ToString() != "3" && faecherIList.Count() != 0)
            {
                try
                {
                    await new MessageDialog(
                        "- App startet wieder auf Windows PCs (peinlich...) - Bitte Semester und Farbe neu einstellen."
                        + Environment.NewLine +
                        "- Codeoptimierungen & Geschwindigkeitsverbesserungen"
                        , "Release Notes: Version 2.9.0.2").ShowAsync();
                }
                catch (Exception)
                { }
                ApplicationData.Current.RoamingSettings.Values["UpdateWin"] = "3";
            }
        }

        private async void welcomeHandler(IUICommand error)
        {
            BottomAppBar.IsOpen = true;
            await Task.Delay(20);
            fachNeuButton_Click(fachNeuButton, new RoutedEventArgs());
        }
        #endregion

        #region Auswahl
        private void checkEintragen()
        {
            #region zurücksetzen
            ren.IsEnabled = false;
            del.IsEnabled = false;
            zen.IsEnabled = true;
            moveDown.IsEnabled = false;
            moveUp.IsEnabled = false;
            zensurNeuEintragen.IsEnabled = false;
            #endregion

            if (faecherUebersicht.SelectedIndex != -1)
            {
                del.IsEnabled = true;
                if (faecherUebersicht.SelectedItems.Count() == 1)
                {
                    string fach = faecherIList.ElementAt(faecherUebersicht.SelectedIndex);
                    zensurNeuFach.Text = fach;
                    fachRenameName.Text = fach;
                    fachRenameName.SelectionStart = fach.Length;
                    ren.IsEnabled = true;
                    if (faecherUebersicht.SelectedIndex > 0) { moveUp.IsEnabled = true; }
                    if (faecherUebersicht.SelectedIndex < faecherIList.Count() - 1) { moveDown.IsEnabled = true; }
                    if (zensurNeuArt.SelectedIndex != -1)
                    {
                        zensurNeuEintragen.IsEnabled = true;
                    }
                }
                else
                {
                    zensurNeuFach.Text = "nur ein Fach auswählen";
                    zen.IsEnabled = false;
                }
            }
            else
            {
                zensurNeuFach.Text = "Fach rechts auswählen";
            }
        }
        
        private void faecherUebersicht_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (faecherUebersicht.SelectedIndex != -1)
            {
                this.BottomAppBar.IsOpen = true;
                this.BottomAppBar.IsSticky = true;
            }
            else
            {
                this.BottomAppBar.IsOpen = false;
                this.BottomAppBar.IsSticky = false;
            }
            checkEintragen();
        }
        
        private void zensurNeuArt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkEintragen();
        }
        #endregion
        
        #region AppBar
        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }
        private void CommandBar_Opened(object sender, object e)
        {
            placeholder.MinHeight = 90;
        }
        private void CommandBar_Closed(object sender, object e)
        {
            placeholder.MaxHeight = 0;
            placeholder.MinHeight = 0;
        }

        private void semesterCaption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.IsOpen = true;
        }
        
        private void fachNeuButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fachAdd = sender as FrameworkElement;
            if (fachAdd != null)
            {
                FlyoutBase.ShowAttachedFlyout(fachAdd);
            }
        }
        private void zen_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.RoamingSettings.Values["Temp"] = faecherUebersicht.SelectedIndex.ToString();
            this.Frame.Navigate(typeof(zensuren));
        }
        private void ren_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }
        private void del_Click(object sender, RoutedEventArgs e)
        {
            if (faecherUebersicht.SelectedItems.Count() == 1)
            {
                question.Text = "Fach löschen?";
            }
            else
            {
                question.Text = "Die " + faecherUebersicht.SelectedItems.Count() + " Fächer löschen?";
            }

            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }

        private async void moveUp_Click(object sender, RoutedEventArgs e)
        {
            int index = faecherUebersicht.SelectedIndex;

            for (int i = 1; i < 5; i++)
            {
                StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                string tempZensuren = tempZensurenIList.ElementAt(index);

                tempZensurenIList.RemoveAt(index);
                tempZensurenIList.Insert(index - 1, tempZensuren);

                await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
            }

            string tempFach = faecherIList.ElementAt(index);

            faecherIList.RemoveAt(index);
            faecherIList.Insert(index - 1, tempFach);

            await FileIO.WriteLinesAsync(faecherFile, faecherIList);
            loadData();
            BottomAppBar.IsOpen = true;
            await Task.Delay(500);
            faecherUebersicht.SelectedIndex = (index - 1);
        }
        private async void moveDown_Click(object sender, RoutedEventArgs e)
        {
            int index = faecherUebersicht.SelectedIndex;

            for (int i = 1; i < 5; i++)
            {
                StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                string tempZensuren = tempZensurenIList.ElementAt(index);

                tempZensurenIList.RemoveAt(index);
                tempZensurenIList.Insert(index + 1, tempZensuren);

                await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
            }

            string tempFach = faecherIList.ElementAt(index);

            faecherIList.RemoveAt(index);
            faecherIList.Insert(index + 1, tempFach);

            await FileIO.WriteLinesAsync(faecherFile, faecherIList);
            loadData();
            BottomAppBar.IsOpen = true;
            await Task.Delay(500);
            faecherUebersicht.SelectedIndex = (index + 1);
        }

        private async void rate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:Review?PFN=ee9276ff-7366-4799-82f9-b3ee047d530a"));
            }
            catch { }
        }
        private async void suggestions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:?to=dev@eynorey.com&subject=Zensuren Manager (Windows) Vorschlag&body=Folgendes ist mir aufgefallen: "));
            }
            catch { }
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            Settings SettingsFlyout = new Settings();
            SettingsFlyout.Show();
        }
        #endregion

        #region zensur eintragen
        private void zensurNeuEintragen_Click(object sender, RoutedEventArgs e)
        {
            zensurNeu(faecherUebersicht.SelectedIndex);
        }
        private async void Rectangle_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            if (zensurNeuEintragen.IsEnabled == false)
            {
                SolidColorBrush redColor = new SolidColorBrush { Color = Windows.UI.Color.FromArgb(255, 255, 20, 20) };
                SolidColorBrush whiteColor = new SolidColorBrush { Color = Windows.UI.Color.FromArgb(255, 255, 255, 255) };
                if (faecherUebersicht.SelectedItems.Count() != 1 && zensurNeuArt.SelectedIndex == -1)
                {
                    zensurNeuFach.Foreground = redColor;
                    zensurNeuArt.BorderBrush = redColor;
                    await Task.Delay(2000);
                    zensurNeuFach.Foreground = whiteColor;
                    zensurNeuArt.BorderBrush = whiteColor;
                }
                else if (zensurNeuArt.SelectedIndex == -1)
                {
                    zensurNeuArt.BorderBrush = redColor;
                    await Task.Delay(2000);
                    zensurNeuArt.BorderBrush = whiteColor;
                }
                else if (faecherUebersicht.SelectedItems.Count() != 1)
                {
                    zensurNeuFach.Foreground = redColor;
                    await Task.Delay(2000);
                    zensurNeuFach.Foreground = whiteColor;
                }
            }
            else
            {
                zensurNeu(faecherUebersicht.SelectedIndex);
            }
        }
        private void zensurNeuEintragen_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (zensurNeuEintragen.IsEnabled)
            {
                eintragenRectangle.Visibility = Visibility.Collapsed;
            }
            else
            {
                eintragenRectangle.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region löschen || umbenennen
        private void delConfirm_Click(object sender, RoutedEventArgs e)
        {
            fachLoeschen();
        }
        private void delAbort_Click(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;
        }
        private async void fachLoeschen()
        {
            if (faecherUebersicht.SelectedItems.Count == 1)
            {
                faecherIList.RemoveAt(faecherUebersicht.SelectedIndex);
                for (int i = 1; i < 5; i++)
                {
                    StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                    IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                    tempZensurenIList.RemoveAt(faecherUebersicht.SelectedIndex);
                    await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
                }
            }
            else
            {
                IList<string> newFaecherIList = faecherIList;
                for (int i = faecherIList.Count() - 1; i >= 0; i--)
                {
                    if ((faecherUebersicht.ContainerFromIndex(i) as ListViewItem).IsSelected == true)
                    {
                        newFaecherIList.RemoveAt(i);
                    }
                }
                faecherIList = newFaecherIList;

                for (int i = 1; i < 5; i++)
                {
                    StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                    IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                    for (int k = faecherIList.Count() - 1; k >= 0; k--)
                    {
                        if ((faecherUebersicht.ContainerFromIndex(k) as ListViewItem).IsSelected == true)
                        {
                            tempZensurenIList.RemoveAt(k);
                        }
                    }

                    await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
                }

            }

            await FileIO.WriteLinesAsync(faecherFile, faecherIList);
            loadData();
        }

        private async void fachRenameButton_Click(object sender, RoutedEventArgs e)
        {
            int index = faecherUebersicht.SelectedIndex;
            if (fachRenameName.Text.Length != 0)
            {
                faecherIList.RemoveAt(faecherUebersicht.SelectedIndex);
                faecherIList.Insert(faecherUebersicht.SelectedIndex, fachRenameName.Text);
            }
            else
            {
                await new MessageDialog("Bitte neuen Namen des Fachs eingeben", "Hinweis").ShowAsync();
            }

            fachRenameName.Text = "";

            await FileIO.WriteLinesAsync(faecherFile, faecherIList);
            loadData();
            await Task.Delay(50);
            faecherUebersicht.SelectedIndex = index;
        }
        #endregion

        private void faecherUebersicht_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            listener(e);
        }
        private void listener(KeyRoutedEventArgs taste)
        {
            if (taste.Key == Windows.System.VirtualKey.Up && moveUp.IsEnabled)
            {
                moveUp_Click(moveUp, new RoutedEventArgs());
            }
            if (taste.Key == Windows.System.VirtualKey.Down && moveDown.IsEnabled)
            {
                moveDown_Click(moveDown, new RoutedEventArgs());
            }
            if (taste.Key == Windows.System.VirtualKey.Delete && del.IsEnabled)
            {
                del_Click(del, new RoutedEventArgs());
            }
            if (taste.Key == Windows.System.VirtualKey.U && ren.IsEnabled)
            {
                ren_Click(ren, new RoutedEventArgs());
            }
            if (taste.Key == Windows.System.VirtualKey.D)
            {
                zen_Click(zen, new RoutedEventArgs());
            }
        }

        #region Tile
        public void SetLiveTile(string text1, string text2)
        {
            if ((ApplicationData.Current.RoamingSettings.Values["LiveTile"].ToString() == "true") && gesamtAnzahlNoten != 0)
            {
                var tileXml1 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndText02);
                var tileXml2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150ImageAndText02);
                var tileXml3 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);

                var notification1 = new ScheduledTileNotification(tileXml1, DateTime.Now.AddSeconds(2));
                var notification2 = new ScheduledTileNotification(tileXml2, DateTime.Now.AddSeconds(4));
                var notification3 = new ScheduledTileNotification(tileXml3, DateTime.Now.AddSeconds(6));

                notification1.ExpirationTime = DateTime.Now + TimeSpan.FromDays(14);
                notification2.ExpirationTime = DateTime.Now + TimeSpan.FromDays(14);
                notification3.ExpirationTime = DateTime.Now + TimeSpan.FromDays(14);

                //groß
                XmlNodeList nodes1 = tileXml1.GetElementsByTagName("image");
                nodes1[0].Attributes[1].NodeValue = "ms-appx:///Assets/" + "large-live.png";
                nodes1 = tileXml1.GetElementsByTagName("text");
                nodes1[0].InnerText = text1;
                nodes1[1].InnerText = text2;

                //weit
                XmlNodeList nodes2 = tileXml2.GetElementsByTagName("image");
                nodes2[0].Attributes[1].NodeValue = "ms-appx:///Assets/" + "wide-live.png";
                nodes2 = tileXml2.GetElementsByTagName("text");
                nodes2[0].InnerText = text1;
                nodes2[1].InnerText = text2;

                //normal
                XmlNodeList nodes3 = tileXml3.GetElementsByTagName("image");
                nodes3[0].Attributes[1].NodeValue = "ms-appx:///Assets/" + "small-live.png";
                nodes3 = tileXml3.GetElementsByTagName("text");
                nodes3[0].InnerText = "Stand";
                nodes3[1].InnerText = text1;
                nodes3[2].InnerText = text2;

                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(notification1);
                TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(notification2);
                TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(notification3);
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }  
        }
        #endregion

        private void abiButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(abischnitt));
        }
    }
    
}
