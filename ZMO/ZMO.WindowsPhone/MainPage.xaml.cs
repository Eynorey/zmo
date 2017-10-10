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
using Windows.ApplicationModel.Activation;
using ZMO.Common;
using Windows.ApplicationModel.Store;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace ZMO
{
    
    public sealed partial class MainPage : Page
    {
               
        public FrameworkElement rectangle = null;

        public MainPage()
        {
            this.InitializeComponent();
            if (DateTime.Now.Month == 12)
            {
                santaHat.Visibility = Visibility.Visible;
            }
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            loadSettings();
            startUp(false);
            startUp2();
        }    
        
        private async void startUp2()
        {
            zensurNeuArt.ItemsSource = artArray;

            zensurNeuFach.ItemsSource = await FileIO.ReadLinesAsync(await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey"));

            await Task.Delay(200);
            if (ApplicationData.Current.RoamingSettings.Values["CortanaText"] != null)
            {
                zensurNeuCortana();
            }

            var cortanaBefehle = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Cortana.xml"));
            await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(cortanaBefehle);

            try
            {
                Windows.Media.SpeechRecognition.VoiceCommandSet commandSetDeDe;
                if (Windows.Media.SpeechRecognition.VoiceCommandManager.InstalledCommandSets.TryGetValue("commandSet_de-de", out commandSetDeDe))
                {
                    string[] faecherArray = faecherIList.ToArray();
                    await commandSetDeDe.SetPhraseListAsync("fach", faecherArray);
                }
            }
            catch (Exception)
            { }

            #region Weihnachten + Neujahr
            //if (ApplicationData.Current.RoamingSettings.Values["notificationsSet"] != "true")
            //{
            //    ToastNotificationManager.History.Clear();
            //    try
            //    {
            //        toastNotification("Frohe Weihnachten! :)", new DateTime(DateTime.Now.Year, 12, 24, 12, 24, 00, DateTimeKind.Local), "weihnachten");
            //    }
            //    catch (Exception)
            //    { }
            //    try
            //    {
            //        toastNotification("Frohes Neues Jahr! :D", new DateTime(DateTime.Now.Year + 1, 01, 01, 00, 00, 00), "silvester");
            //    }
            //    catch (Exception)
            //    { }
            //    ApplicationData.Current.RoamingSettings.Values["notificationsSet"] = "true";
            //}
            #endregion
        }

        private void zensurNeuEintragen_Click(object sender, RoutedEventArgs e)
        {
            zensurNeu(zensurNeuFach.SelectedIndex);
        }
        
        #region welcome Dialog / Update Message
        private async void showDialogue()
        {
            if (faecherIList.Count == 0)
            {
                MessageDialog welcome = new MessageDialog(
                               "Es sind noch keine Fächer vorhanden."
                               + Environment.NewLine +
                               "In der Detailansicht können neue Fächer hinzugefügt werden."
                               + Environment.NewLine + Environment.NewLine +
                               "Viel Spaß mit der App! :)",
                               "Willkommen!"
                               );
                welcome.Commands.Add(new UICommand("los geht's!", new UICommandInvokedHandler(this.welcomeHandler)));
                await welcome.ShowAsync();
            }
            
            if (ApplicationData.Current.RoamingSettings.Values["UpdateWP"].ToString() != "4" && faecherIList.Count() != 0)
            {
                try
                {
                    ApplicationData.Current.RoamingSettings.Values["UpdateWP"] = "4";
                    await new MessageDialog(
                        "- überarbeitetes Design der Windows & Windows Phone App"
                        + Environment.NewLine +
                        "- neue Berechnung für Durchschnitt aller Zensuren des Semesters aus allen Fächern"
                        + Environment.NewLine +
                        "- neue tabellarische Übersicht mit diversen Umrechnungen (Brandenburg)"
                        + Environment.NewLine +
                        "- allgemeine Leistungsverbesserungen & Bug Fixes"
                        + Environment.NewLine + Environment.NewLine +
                        "- Garantiert NSA-frei! -"
                    , "Release Notes: Version 2.9.0.0").ShowAsync();
                }
                catch (Exception)
                { }
            }
        }

        private void welcomeHandler(IUICommand error)
        {
            mainPivot.SelectedIndex = 1;
            bottomBar.IsOpen = true;
            fachNeuButton_Click(new object(), new RoutedEventArgs());
        }
        #endregion

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

        private void semesterCaption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (bottomBar.IsOpen)
            {
                bottomBar.IsOpen = false;
            }
            else
            {
                bottomBar.IsOpen = true;
            }
        }

        #region Tile & Toast     
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

        private void toastNotification(string text1, DateTime dueTime, string toastId)
        { 
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(text1));

            ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, dueTime);
            scheduledToast.Id = toastId;
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);
        }
        #endregion

        private void faecherUebersicht_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (faecherUebersicht.SelectedIndex != -1)
            {
                del.IsEnabled = true;
                ren.IsEnabled = true;
                zen.IsEnabled = true;
            }
            else
            {
                del.IsEnabled = false;
                ren.IsEnabled = false;
                zen.IsEnabled = false;
            }
        }
        private void ListViewRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (rectangle == sender as FrameworkElement)
            {
                zen_Click(new object(), new RoutedEventArgs());
            }
            if (rectangle == null)
            {
                rectangle = sender as FrameworkElement;
            }
            else
            {
                rectangle.Opacity = 0;
                rectangle = sender as FrameworkElement;
            }
            rectangle.Opacity = 0.3;
        }        

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainPivot.SelectedIndex == 0)
            {
                bottomBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                appBarButtonsReset();
                sem1.Visibility = Visibility.Visible;
                sem2.Visibility = Visibility.Visible;
                sem3.Visibility = Visibility.Visible;
                sem4.Visibility = Visibility.Visible;

                zensurNeuFach.ItemsSource = faecherIList;
            }
            if (mainPivot.SelectedIndex == 1)
            {
                bottomBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                appBarButtonsReset();
                fachNeuButton.Visibility = Visibility.Visible;
                ren.Visibility = Visibility.Visible;
                del.Visibility = Visibility.Visible;
                zen.Visibility = Visibility.Visible;
            }
            if (mainPivot.SelectedIndex == 2)
            {
                stundenplan();
                bottomBar.Visibility = Visibility.Collapsed;
            }
        }

        #region löschen || umbenennen
        private void delConfirm_Click(object sender, RoutedEventArgs e)
        {
            fachLoeschen();
        }
        private void delAbort_Click(object sender, RoutedEventArgs e)
        {
            faecherUebersicht.Focus(FocusState.Programmatic);
        }
        private async void fachLoeschen()
        {
            faecherIList.RemoveAt(faecherUebersicht.SelectedIndex);
            for (int i = 1; i < 5; i++)
            {
                StorageFile tempZensurenFile = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Semester" + i)).GetFileAsync("zensuren.rey");
                IList<string> tempZensurenIList = await FileIO.ReadLinesAsync(tempZensurenFile);
                tempZensurenIList.RemoveAt(faecherUebersicht.SelectedIndex);
                await FileIO.WriteLinesAsync(tempZensurenFile, tempZensurenIList);
            }

            await FileIO.WriteLinesAsync(faecherFile, faecherIList);
            loadData();
            zensurNeuFach.ItemsSource = await FileIO.ReadLinesAsync(await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey"));
            deleteFlyout.Hide();
        }
        private async void fachRenameButton_Click(object sender, RoutedEventArgs e)
        {
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
            zensurNeuFach.ItemsSource = await FileIO.ReadLinesAsync(await ApplicationData.Current.RoamingFolder.GetFileAsync("faecher.rey"));

            renameFlyout.Hide();
            faecherUebersicht.Focus(FocusState.Programmatic);
        }
        #endregion

        #region AppBar
        private void del_Click(object sender, RoutedEventArgs e)
        {
            question.Text = "Fach " + faecher.ElementAt(faecherUebersicht.SelectedIndex).Name.Trim() + " löschen?";
            FrameworkElement del = sender as FrameworkElement;
            if (del != null)
            {
                FlyoutBase.ShowAttachedFlyout(del);
            }
        }
        private void fachNeuButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fachAdd = sender as FrameworkElement;
            if (fachAdd != null)
            {
                FlyoutBase.ShowAttachedFlyout(fachAdd);
            }
        }
        private void ren_Click(object sender, RoutedEventArgs e)
        {
            fachRenameName.Text = faecher.ElementAt(faecherUebersicht.SelectedIndex).Name.Trim();
            FrameworkElement ren = sender as FrameworkElement;
            if (ren != null)
            {
                FlyoutBase.ShowAttachedFlyout(ren);
            }
            fachRenameName.SelectionStart = fachRenameName.Text.Length;
        }
        private async void zen_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.RoamingSettings.Values["Temp"] = faecherUebersicht.SelectedIndex.ToString();
            await Task.Delay(100);

            this.Frame.Navigate(typeof(zensuren));
        } 

        private void appBarButtonsReset()
        {
            sem1.Visibility = Visibility.Collapsed;
            sem2.Visibility = Visibility.Collapsed;
            sem3.Visibility = Visibility.Collapsed;
            sem4.Visibility = Visibility.Collapsed;
            fachNeuButton.Visibility = Visibility.Collapsed;
            ren.Visibility = Visibility.Collapsed;
            del.Visibility = Visibility.Collapsed;
            zen.Visibility = Visibility.Collapsed;
        }

        private async void rate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + Uri.EscapeDataString("6d7b6663-d912-47ae-aa9b-8da310813dfc")));
            }
            catch { }
        }
        private async void suggestions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:?to=dev@eynorey.com&subject=Zensuren Manager WP Vorschlag&body=Folgendes ist mir aufgefallen: "));
            }
            catch { }
        }
        #endregion

        private void stundenplan()
        {
            //await data.CreateFileAsync("lehrer.rey");
            //lehrerFile = await data.GetFileAsync("lehrer.rey");
            //await FileIO.WriteTextAsync(lehrerFile, "blaaaah");
            //lehrerIList = await FileIO.ReadLinesAsync(lehrerFile);
        }

        #region Cortana
        private void zensurNeuCortana()
        {
            string input = ApplicationData.Current.RoamingSettings.Values["CortanaText"].ToString().ToLower();

            //Fach ermitteln
            foreach (string cortanaFach in faecherIList)
            {
                if (input.Contains(cortanaFach.ToLower()))
                {
                    zensurNeuFach.SelectedValue = cortanaFach;
                    break;
                }
            }

            //Zensur ermitteln
            string inputZahl = "  " + input;
            int startIndex = inputZahl.IndexOf("punkt");
            try
            {
                zensurNeuZensur.Value = Convert.ToInt16(inputZahl.Substring(startIndex - 3, 2).Trim());
            }
            catch (Exception)
            {
                if (inputZahl.Substring(startIndex - 6, 5).Contains("ein"))
                {
                    zensurNeuZensur.Value = 1;
                }
            }

            //Art ermitteln
            if (new string[]{"test", "kontrolle", "leistungskontrolle"}.Any(input.Contains))
            {
                zensurNeuArt.SelectedIndex = 0;
            }
            else if (new string[] { "stundenaufgabe", "unterrichtsaufgabe", "hausaufgabe" }.Any(input.Contains))
            {
                zensurNeuArt.SelectedIndex = 1;
            }
            else if (new string[] { "mitarbeit", "mitarbeitsnote", "mitarbeitszensur", "mündlich", "mündliche note" }.Any(input.Contains))
            {
                zensurNeuArt.SelectedIndex = 2;
            }
            else if (new string[] { "sonstige", "sonstige zensur", "sonstige note", "andere", "andere note", "andere zensur" }.Any(input.Contains))
            {
                zensurNeuArt.SelectedIndex = 3;
            }
            else if (new string[] { "klausur", "klausurnote", "leistungsnachweis", "leistungsfeststellung", "drittel"}.Any(input.Contains))
            {
                zensurNeuArt.SelectedIndex = 4;
            }

            if (zensurNeuEintragen.IsEnabled)
            {
                zensurNeuEintragen_Click(new object(), new RoutedEventArgs());
            }
            else
            {
                showCortanaError();
            }

            ApplicationData.Current.RoamingSettings.Values["CortanaText"] = null;
        }

        private async void showCortanaError()
        {
            string bereich;
            if (zensurNeuFach.SelectedIndex == -1)
            {
                bereich = "das Fach ";
            }
            else if (zensurNeuArt.SelectedIndex == -1)
            {
                bereich = "die Art der Zensur ";
            }
            else
            {
                bereich = "die Zensur ";
            }
            await new MessageDialog("Ich habe " + bereich + "leider nicht verstanden. Bitte manuell einstellen und dann eintragen.", "Entschuldigung...").ShowAsync();
        }
        #endregion

    }
}
