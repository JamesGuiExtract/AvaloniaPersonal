using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Windows;

namespace ExtractLicenseUI.Utility
{
    public class LicenseContainer
    {
        public Database.ExtractLicense License { get; set; }
        public LicenseNavigationOptions LicenseNavigationOption { get; set; }

        public void NavigateToLicense(Database.ExtractLicense license, LicenseNavigationOptions navigationOption)
        {
            this.License = license;
            this.LicenseNavigationOption = navigationOption;
            string url = "/PagesContent/License.xaml";
            BBCodeBlock bbBlock = new BBCodeBlock();
            bbBlock.LinkNavigator.Navigate(new Uri(url, UriKind.Relative), ((MainWindow)Application.Current.MainWindow).Organization, NavigationHelper.FrameSelf);
        }
    }
}
