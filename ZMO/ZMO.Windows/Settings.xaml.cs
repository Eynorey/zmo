using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.Storage;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;

// Die Elementvorlage "Einstellungs-Flyout" ist unter http://go.microsoft.com/fwlink/?LinkId=273769 dokumentiert.

namespace ZMO
{   
    public sealed partial class Settings : SettingsFlyout
    {
        public Settings()
        {
            this.InitializeComponent();
            loadData();
        }

        private void loadData()
        {
            if (ApplicationData.Current.RoamingSettings.Values["LiveTile"].ToString() == "true")
            {
                LiveTile_switch.IsOn = true;
            }
            else
            {
                LiveTile_switch.IsOn = false;
            }
        }
        
        private void LiveTile_switch_Toggled(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch).IsOn)
            {
                ApplicationData.Current.RoamingSettings.Values["LiveTile"] = "true";
            }
            else
            {
                ApplicationData.Current.RoamingSettings.Values["LiveTile"] = "false";
            }
        }

        private void colorTap(object sender, TappedRoutedEventArgs e)
        {
            SolidColorBrush farbe = (sender as Rectangle).Fill as SolidColorBrush;
            ApplicationData.Current.RoamingSettings.Values["Farbe"] = farbe.Color.ToString();
            (Window.Current.Content as Frame).Navigate(typeof(MainPage));
        }
    }
}
