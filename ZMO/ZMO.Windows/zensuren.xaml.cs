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
using Windows.ApplicationModel.Store;



namespace ZMO
{
    
    public sealed partial class zensuren : Page
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
            navigationHelper.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
#endregion

        public int kategorie;

        public zensuren()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            //Weihnachten
            if (DateTime.Now.Month == 12)
            {
                santaHat.Visibility = Visibility.Visible;
            }

            startUp();
            chooseSemester(Convert.ToInt16(ApplicationData.Current.RoamingSettings.Values["Semester"]));
        }

        //AppBar

        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bottomBar.IsOpen = true;
        }

        #region Semester
        private void sem_Click(object sender, RoutedEventArgs e)
        {
            int sem = Convert.ToInt16((sender as FrameworkElement).Name.Substring(3, 1));
            chooseSemester(sem);
        }
        private async void chooseSemester(int semester, bool load = true)
        {
            await Task.Delay(30);
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
        private void semesterCaption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.IsOpen = true;
        }
        #endregion

        #region select
        private void Tests_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re1.Opacity = 0.3;
            editZensuren.Text = testNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Tests | Notenpunkte:";
            kategorie = 0;
            showEditFlyout(sender as FrameworkElement);
        }
        private void Stunden_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re2.Opacity = 0.3;
            editZensuren.Text = stundenNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Stunden- & Hausaufgaben | Notenpunkte:";
            kategorie = 1;
            showEditFlyout(sender as FrameworkElement);
        }
        private void Mitarbeit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re3.Opacity = 0.3;
            editZensuren.Text = mitarbeitNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Mitarbeit | Notenpunkte:";
            kategorie = 2;
            showEditFlyout(sender as FrameworkElement);
        }
        private void Sonstige_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re4.Opacity = 0.3;
            editZensuren.Text = sonstigeNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Sonstige | Notenpunkte:";
            kategorie = 3;
            showEditFlyout(sender as FrameworkElement);
        }
        private void Klausur_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOpacity();
            re5.Opacity = 0.3;
            editZensuren.Text = klausurNoten.Text.Replace("   ", ",");
            editZensuren.Header = "Klausur + Leistungsnachweis | Notenpunkte:";
            kategorie = 4;
            showEditFlyout(sender as FrameworkElement);
        }
        #endregion
        private void editFlyout_Closed(object sender, object e)
        {
            if (close)
            {
                resetOpacity();
            }
        }

    }
}
