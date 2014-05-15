/* Reservation System
 * 4/23/2014
 * Written by William Chang
 * --------------------------------------
 * 
 * Description:
 * Design a WPF application for handling restaurant reservations. 
 * The input for the application is an XML file containing information 
 * about the tables in the restaurant and the maximum occupancy per table. 
 * (please see the attached tables.xml file) 
 * The application should handle the following functionality for a single day. 
 * Assume that the restaurant operates from 10am to 10pm.
        a.     Add reservation
        b.     Delete reservation
        c.     Edit reservation.

 * While designing your application make the following considerations-
        a.     Recovery if the app crashes. (Reservations should not be lost)
        b.     Please make sure that the UI is always responsive.
 *             Please design your application such that it's easy to add 
 *             functionality which syncs the changes a client makes to a web service.
 * 
 * --------------------------------------
 * Assumptions
 *  -Restaurant is open from 10am-10pm
 *  -Reservations last for 1 hour
 *  -Reservations should hold a name, number in party, and phone number,
 *      although only the name is required.
 *  -Reservation edit means that you choose a reservation, and change the details, 
 *      e.g. Name, Number in Party, and/or Phone Number
 *  -There are no duplicate tableIDs, which is used for customer reservation logic
 *  -The user does not expect data to save unless the save button is pressed
 * 
 * --------------------------------------
 * Future Modifications
 *  -Background thread could sync the XML stored in the file 
 *      "[mm-dd-yyyy]Reservations.xml" to a web service
 *  -Display more rich content in the datagrid for easier browsing of multiple 
 *      reservations and potentially modifying/deleting reservations also.
 *  -Enhance indexing of times/tables/reservations for faster access
 *  -Expand the application to accept multiple days with a date picker
 *  
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using ReservationSystemSolution.Model;
using ReservationSystemSolution.Utilities;

namespace ReservationSystemSolution
{
    /// <summary>
    /// Interaction logic for ReservationWindow.xaml
    /// </summary>
    public partial class ReservationWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private Reservations _Reservations;
        private String _TablesFileName;
        private String _ReservationsFileName;
        private Tables _Tables;
        private ObservableCollection<String> _ReservationBlockTimes;
        private String _FirstReservationTime;
        private String _LastReservationTime;
        private String _ReservationBlockSelection;
        private TablesTable _TableSelection;
        private String _CustomerName;
        private String _PhoneNumber;
        private String _NumberInParty;
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region Constructor
        public ReservationWindow()
        {
            InitializeComponent();
            LoadModel();
            LoadViewModel();
        }
        #endregion

        #region Initialization
        private void LoadModel()
        {
            // Load Tables
            if (File.Exists(TablesFileName))
            {
                try { 
                    XmlSerializer serializer = new XmlSerializer(typeof(Tables));
                    using (FileStream fs = new FileStream(TablesFileName, FileMode.Open))
                    {
                        Tables = serializer.Deserialize(fs) as Tables;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Tables.xml!: " + e.Message);
                    Application.Current.Shutdown();
                }
            }
            else
                throw new Exception("Failed to load Tables xml file! Ensure that Tables.xml is included in the working directory!");

            // Load Reservations
            if (File.Exists(ReservationsFileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Reservations));
                    using (StreamReader reader = new StreamReader(ReservationsFileName))
                    {
                        Reservations = serializer.Deserialize(reader) as Reservations;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Failed to load {0}: {1}", ReservationsFileName, e.Message));
                    Application.Current.Shutdown();
                }
            }
            else
            {
                // Initialize the Reservations if there's no file found
                LoadReservationBlockList();
            }
        }

        /// <summary>
        /// Loads late binding objects
        /// </summary>
        private void LoadViewModel()
        {
            // Update Date
            DateTextBlock.Text = "Reservation Date: " + Reservations.Date;

            // Load Reservation Block List Box
            ReservationBlocksListBox.ItemsSource = Reservations.ReservationBlocks.Select(x => x.Time);            

            // Initially hide the reservation details grid since 
            // no times or table are selected at first.
            ShowReservationDetailFields(false);
            TablesGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Loads the Reservation Block List object with 1-hour intervals from
        /// FirstReservationTime to LastReservationTime
        /// </summary>
        private void LoadReservationBlockList()
        {
            DateTime startTime = DateTime.Parse(FirstReservationTime);
            DateTime lastTime = DateTime.Parse(LastReservationTime);

            DateTime currentTime = startTime;
            do
            {
                Reservations.ReservationBlocks.Add(
                    new ReservationBlock(currentTime, new List<CustomerReservation>()));
                currentTime = currentTime.AddHours(1.0);
            }
            while (currentTime <= lastTime);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Default behavior is to return a Reservations object for today.
        /// </summary>
        public Reservations Reservations { 
            get {
                if (_Reservations == null)
                    _Reservations = new Reservations(DateTime.Now, new List<ReservationBlock>());
                return _Reservations;
            }
            set
            {
                if (value != _Reservations)
                {
                    _Reservations = value;
                    NotifyPropertyChanged("Reservations");
                }
            }
        }

        /// <summary>
        /// Stores all the table information.
        /// </summary>
        public Tables Tables
        {
            get
            {
                if (_Tables == null)
                    _Tables = new Tables();
                return _Tables;
            }
            set
            {
                if (value != _Tables)
                {
                    _Tables = value;
                    NotifyPropertyChanged("Tables");
                }
            }
        }

        private String ReservationsFileName
        {
            get
            {
                if (String.IsNullOrEmpty(_ReservationsFileName))
                {
                    // First get the file format (Should be of format "[Text]<Date>[Text].xml" 
                    // where <Date> is to be replaced by the short date requested. Default is 
                    // today for now.
                    String xmlFileFormat = Properties.Settings.Default.ReservationFileNameFormat;
                    String xmlFileFormatDateTag = Properties.Settings.Default.ReservationFileNameFormatDateTag;
                    String xmlFile = xmlFileFormat.Replace(xmlFileFormatDateTag, DateTime.Now.ToString(Constants.ShortDateFormat));

                    _ReservationsFileName = xmlFile;
                }
                return _ReservationsFileName;
            }
        }

        private String TablesFileName
        {
            get
            {
                if (String.IsNullOrEmpty(_TablesFileName))
                    _TablesFileName = Properties.Settings.Default.TablesFileName;
                return _TablesFileName;
            }
        }

        private String FirstReservationTime
        {
            get
            {
                if (String.IsNullOrEmpty(_FirstReservationTime))
                    _FirstReservationTime = Properties.Settings.Default.FirstReservationTime;
                return _FirstReservationTime;
            }
        }

        private String LastReservationTime
        {
            get
            {
                if (String.IsNullOrEmpty(_LastReservationTime))
                    _LastReservationTime = Properties.Settings.Default.LastReservationTime;
                return _LastReservationTime;
            }
        }

        public String ReservationBlockSelection
        {
            get { return _ReservationBlockSelection; }
            set
            {
                if (value != _ReservationBlockSelection)
                {    
                    _ReservationBlockSelection = value;
                    NotifyPropertyChanged("ReservationBlockSelection");
                }
            }
        }

        public TablesTable TableSelection
        {
            get { return _TableSelection; }
            set
            {
                if (value != _TableSelection)
                {
                    _TableSelection = value;
                    NotifyPropertyChanged("TableSelection");
                }
            }
        }

        public String CustomerName
        {
            get { return _CustomerName; }
            set
            {
                if (value != _CustomerName)
                {
                    _CustomerName = value;
                    NotifyPropertyChanged("CustomerName");
                }
            }
        }

        public String PhoneNumber
        {
            get { return _PhoneNumber; }
            set
            {
                if (value != _PhoneNumber)
                {
                    _PhoneNumber = value;
                    NotifyPropertyChanged("PhoneNumber");
                }
            }
        }

        private ObservableCollection<String> ReservationBlockTimes
        {
            get {
                if (_ReservationBlockTimes == null)
                    _ReservationBlockTimes = new ObservableCollection<String>();
                return _ReservationBlockTimes;
            }
            set
            {
                if (value != _ReservationBlockTimes)
                {
                    _ReservationBlockTimes = value;
                    NotifyPropertyChanged("ReservationBlockTimes");
                }
            }
        }

        public String NumberInParty
        {
            get
            {
                return _NumberInParty;
            }
            set
            {
                if (value != _NumberInParty)
                {
                    _NumberInParty = value;
                    NotifyPropertyChanged("NumberInParty");
                }
            }
        }
        #endregion

        #region Event Handlers
        private void OnReservationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load Reservation based on Reservation Block and Table Selected
            if (ReservationBlocksListBox != null && TablesDataGrid != null && ReservationBlockSelection != null)
            {
                TablesGrid.Visibility = Visibility.Visible;
                //ReservationBlockSelection
                //TableSelection
                // Look up reservation at the time and table to see if it exists
                //IEnumerable<ReservationBlock> blocks = Reservations.ReservationBlocks.Where(x => x.Time.Equals(ReservationBlockSelection));

                ReservationBlock block = Reservations.GetReservationBlockAt(ReservationBlockSelection);
                if (block != null && TableSelection != null)
                {
                    ShowReservationDetailFields();
                    CustomerReservation reservation = block.GetCustomerReservationAt(TableSelection.Id);
                    if (reservation != null)
                    {
                        CustomerName = reservation.Name;
                        PhoneNumber = reservation.PhoneNumber;
                        NumberInParty = reservation.NumberInParty;
                    }
                    // If no reservation found, make sure to reset CustomerName, PhoneNumber, and NumberInParty
                    else
                    {
                        ClearReservationDetailFields();
                    }
                }
                else
                    ShowReservationDetailFields(false);
            }
            else
            {
                TablesGrid.Visibility = Visibility.Hidden;
                ShowReservationDetailFields(false);
            }
        }
        
        /// <summary>
        /// On Save, Add or Update Customer Reservation and Serialize Reservations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //ReservationBlockSelection
            //TableSelection.Id
            // Check if Name is filled out before proceeding
            if (!AreReservationDetailFieldsValid())
            {
                return;
            }

            // For this CustomerReservation either add to the current ReservationsBlock 
            // if it does not exist, or update the existing entry!
            ReservationBlock block = Reservations.GetReservationBlockAt(ReservationBlockSelection);
            if (block != null)
            {
                CustomerReservation reservation = block.GetCustomerReservationAt(TableSelection.Id);
                if (reservation != null)
                {
                    reservation.Name = CustomerName;
                    reservation.PhoneNumber = PhoneNumber;
                    reservation.NumberInParty = NumberInParty;
                }
                else
                {
                    CustomerReservation currentReservation = 
                        new CustomerReservation(TableSelection.Id, CustomerName, PhoneNumber, NumberInParty);
                    block.CustomerReservations.Add(currentReservation);
                }
                SaveReservations();
            }
        }

        /// <summary>
        /// On Delete, Delete Reservation and Serialize Reservations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ReservationBlock block = Reservations.GetReservationBlockAt(ReservationBlockSelection);
            if (block != null)
            {                
                // If the reservation was actually deleted, update the reservations file.
                if (block.DeleteCustomerReservationAt(TableSelection.Id))
                {
                    SaveReservations();
                    ClearReservationDetailFields();
                }
                else
                    MessageBox.Show("No reservation found to be deleted!");
            }
        }

        // TODO : On Name Lose Focus, save to temp file? On proper exit, delete temp file.
        //        On next load, need to look for temp file

        #endregion

        #region Helper Functions
        /// <summary>
        /// Saves the reservations to the file specified by the configuration 
        /// values ReservationsFileNameFormat and ReservationFileNameFormatDateTag
        /// </summary>
        private void SaveReservations()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Reservations));
                using (FileStream fs = new FileStream(ReservationsFileName, FileMode.Create))
                {
                    serializer.Serialize(fs, Reservations);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Failed to write to {0}: {1}", ReservationsFileName, e.Message));
            }
        }

        private void ClearReservationDetailFields()
        {
            CustomerName = String.Empty;
            PhoneNumber = String.Empty;
            NumberInParty = String.Empty;
        }

        private void ShowReservationDetailFields(bool show=true)
        {
            if (show)
            {
                ReservationDetailInstructions.Visibility = System.Windows.Visibility.Hidden;
                ReservationDetailsGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ReservationDetailInstructions.Visibility = System.Windows.Visibility.Visible;
                ReservationDetailsGrid.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        

        /// <summary>
        /// Validates the reservation detail fields for valid input
        /// </summary>
        /// <returns></returns>
        private bool AreReservationDetailFieldsValid()
        {
            // Check Name
            if (String.IsNullOrEmpty(CustomerName))
            {
                MessageBox.Show("A valid name must be entered!");
                return false;
            }
            // Check Number in Party
            int numInPartyTemp;
            int maxOccupancyTemp;
            bool ableToParseNumInParty = int.TryParse(NumberInParty, out numInPartyTemp);
            int.TryParse(TableSelection.MaxOccupancy, out maxOccupancyTemp);
            if (!String.IsNullOrEmpty(NumberInParty) && !ableToParseNumInParty)                
            {
                MessageBox.Show("Number in Party must be an int!");
                return false;
            }
            if (ableToParseNumInParty && numInPartyTemp > maxOccupancyTemp && numInPartyTemp > 0)
            { 
                MessageBox.Show("Number in Party must be less than or equal to the max occupancy!");
                return false;
            }
            return true;
        }
        #endregion
    }
}
