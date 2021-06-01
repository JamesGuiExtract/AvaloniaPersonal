using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExtractLicenseUI.Utility
{
    public class OrganizationContainer
    {
        public Database.Organization Organization { get; set; }

        public void NavigateToOrganization(Database.Organization organization)
        {
            this.Organization = organization;
            string url = "/PagesContent/AddOrEditOrganization.xaml";
            BBCodeBlock bbBlock = new BBCodeBlock();
            bbBlock.LinkNavigator.Navigate(new Uri(url, UriKind.Relative), ((MainWindow)Application.Current.MainWindow).Organization, NavigationHelper.FrameSelf);
        }
    }
}
