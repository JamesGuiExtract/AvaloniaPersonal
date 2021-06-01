using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExtractLicenseUI.Database;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using FirstFloor.ModernUI.Windows.Navigation;
using System.Threading;
using FirstFloor.ModernUI.Windows;

namespace ExtractLicenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AddOrEditOrganization : UserControl, IContent
    {
        public MainWindow MainWindow { get; }

        public AddOrEditOrganization()
        {
            MainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
            InitializeComponent();
            StatesCombobox.ItemsSource = StateArray.States;
        }


        public void OnNavigatedTo(NavigationEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(LoadOrganization));
            thread.Start();
        }

        /// <summary>
        /// This is needed because on first load the page has not loaded. Therefore
        /// any modifications to controls does not take. This appears to be a common modernUI bug.
        /// </summary>
        public void LoadOrganization()
        {
            Thread.Sleep(200);
            Dispatcher.Invoke(() =>
            {
                MainWindow.OrganizationContainer.Organization.PropertyChanged += Organization_PropertyChanged;
                OrganizationStackPannel.DataContext = MainWindow.OrganizationContainer.Organization;
                StatesCombobox.SelectedItem = StateArray.States.FirstOrDefault(m => m.Abbreviation.Equals(MainWindow.OrganizationContainer.Organization.State, StringComparison.OrdinalIgnoreCase));
                Status.Fill = Brushes.Green;
            });
        }

        private void Organization_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Status.Fill = Brushes.Red;
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsValid(OrganizationStackPannel))
                {
                    MessageBox.Show("Please correct all data errors before saving!", "Add New Customer");
                }
                else
                {
                    using DatabaseWriter databaseWriter = new DatabaseWriter();
                    databaseWriter.WriteOrganization(MainWindow.OrganizationContainer.Organization);
                    MainWindow.Organization.RefreshOrganizations();
                    MainWindow.Organization.SelectedOrganization = MainWindow.OrganizationContainer.Organization;
                    Status.Fill = Brushes.Green;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Determines if every object is valid in the object tree.
        /// </summary>
        /// <param name="obj">The dependency object to check</param>
        /// <returns></returns>
        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsValid);
        }

        private void StatesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((US_State)StatesCombobox.SelectedItem);
            if(selection != null)
            {
                MainWindow.OrganizationContainer.Organization.State = string.IsNullOrEmpty(selection.Abbreviation) ? null : selection.Abbreviation;
            }
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }
    }


    static class StateArray
    {
        internal static List<US_State> States { get; } = new List<US_State>
        {
            new US_State("", ""),
            new US_State("AL", "Alabama"),
            new US_State("AK", "Alaska"),
            new US_State("AZ", "Arizona"),
            new US_State("AR", "Arkansas"),
            new US_State("CA", "California"),
            new US_State("CO", "Colorado"),
            new US_State("CT", "Connecticut"),
            new US_State("DE", "Delaware"),
            new US_State("DC", "District Of Columbia"),
            new US_State("FL", "Florida"),
            new US_State("GA", "Georgia"),
            new US_State("HI", "Hawaii"),
            new US_State("ID", "Idaho"),
            new US_State("IL", "Illinois"),
            new US_State("IN", "Indiana"),
            new US_State("IA", "Iowa"),
            new US_State("KS", "Kansas"),
            new US_State("KY", "Kentucky"),
            new US_State("LA", "Louisiana"),
            new US_State("ME", "Maine"),
            new US_State("MD", "Maryland"),
            new US_State("MA", "Massachusetts"),
            new US_State("MI", "Michigan"),
            new US_State("MN", "Minnesota"),
            new US_State("MS", "Mississippi"),
            new US_State("MO", "Missouri"),
            new US_State("MT", "Montana"),
            new US_State("NE", "Nebraska"),
            new US_State("NV", "Nevada"),
            new US_State("NH", "New Hampshire"),
            new US_State("NJ", "New Jersey"),
            new US_State("NM", "New Mexico"),
            new US_State("NY", "New York"),
            new US_State("NC", "North Carolina"),
            new US_State("ND", "North Dakota"),
            new US_State("OH", "Ohio"),
            new US_State("OK", "Oklahoma"),
            new US_State("OR", "Oregon"),
            new US_State("PA", "Pennsylvania"),
            new US_State("RI", "Rhode Island"),
            new US_State("SC", "South Carolina"),
            new US_State("SD", "South Dakota"),
            new US_State("TN", "Tennessee"),
            new US_State("TX", "Texas"),
            new US_State("UT", "Utah"),
            new US_State("VT", "Vermont"),
            new US_State("VA", "Virginia"),
            new US_State("WA", "Washington"),
            new US_State("WV", "West Virginia"),
            new US_State("WI", "Wisconsin"),
            new US_State("WY", "Wyoming")
        };
    }

    class US_State
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }

        public US_State(string abbreviations, string name)
        {
            Abbreviation = abbreviations;
            Name = name;
        }
    }
}
