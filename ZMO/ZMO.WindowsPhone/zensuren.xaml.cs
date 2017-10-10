using ZMO.Common;
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

// Die Elementvorlage "Standardseite" ist unter "http://go.microsoft.com/fwlink/?LinkID=390556" dokumentiert.

namespace ZMO
{

    public sealed partial class zensuren : Page
    {
        #region NavigationHelper
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        public int kategorie;

        public zensuren()
        {
            this.InitializeComponent();
            //Weihnachten
            if (DateTime.Now.Month == 12)
            {
                santaHat.Visibility = Visibility.Visible;
            }

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            startUp();
            loadTitle();
        }

        private async void loadTitle()
        {
            await Task.Delay(100);
            loadData();
        }
        private async void chooseSemester(int semester, bool load = true)
        {
            data = await ApplicationData.Current.RoamingFolder.GetFolderAsync("semester" + semester);
            zensurenFile = await data.GetFileAsync("zensuren.rey");
            zensurenIList = await FileIO.ReadLinesAsync(zensurenFile);
            
            if (load)
            {
                loadData();
            }
        } 

        #region AppBar
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            fachNummer = fachSelector.SelectedIndex;
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
                await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:?to=dev-eynorey@outlook.com&subject=Zensuren Manager WP Vorschlag&body=Folgendes ist mir aufgefallen: "));
            }
            catch { }
        }
        #endregion

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        #region select
        private void Tests_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re1.Opacity = 0.3;
            editZensuren.Text = testNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Tests | Notenpunkte:";
            kategorie = 0;
            showEditFlyout(sender as FrameworkElement);
            scrollViewerMain.Margin = new Thickness(10, 110, 10, 0);
        }
        private void Stunden_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re2.Opacity = 0.3;
            editZensuren.Text = stundenNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Stunden- & Hausaufgaben | Notenpunkte:";
            kategorie = 1;
            showEditFlyout(sender as FrameworkElement);
            scrollViewerMain.Margin = new Thickness(10, 55, 10, 0);
        }
        private void Mitarbeit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re3.Opacity = 0.3;
            editZensuren.Text = mitarbeitNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Mitarbeit | Notenpunkte:";
            kategorie = 2;
            showEditFlyout(sender as FrameworkElement);
            scrollViewerMain.Margin = new Thickness(10, -10, 10, 0);
        }
        private void Sonstige_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re4.Opacity = 0.3;
            editZensuren.Text = sonstigeNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Sonstige | Notenpunkte:";
            kategorie = 3;
            showEditFlyout(sender as FrameworkElement);
            scrollViewerMain.Margin = new Thickness(10, -80, 10, 0);
        }
        private void Klausur_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re5.Opacity = 0.3;
            editZensuren.Text = klausurNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Klausur + Leistungsnachweis | Notenpunkte:";
            kategorie = 4;
            showEditFlyout(sender as FrameworkElement);
            scrollViewerMain.Margin = new Thickness(10, -150, 10, 0);
        }

        private void editFlyout_Closed(object sender, object e)
        {
            if (close)
            {
                resetOpacity();
            }
            scrollViewerMain.Margin = new Thickness(10, 0, 10, 0);
        }
        #endregion
    }
    
}
