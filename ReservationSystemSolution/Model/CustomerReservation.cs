using System;
using System.ComponentModel;

namespace ReservationSystemSolution.Model
{
    /// <summary>
    /// Reservation details for a customer
    /// </summary>
    public class CustomerReservation : INotifyPropertyChanged, IComparable
    {
        #region Fields
        private String _TableId;
        private String _Name;
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

        #region IComparable Implementation
        /// <summary>
        /// Compares CustomerReservations based on TableID.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            CustomerReservation otherRes = obj as CustomerReservation;
            if (otherRes != null)
                return this.TableId.CompareTo(otherRes.TableId);
            else
                throw new ArgumentException("Object is not a CustomerReservation!");
        }
        #endregion

        #region Constructor
        public CustomerReservation() { }
        public CustomerReservation(String TableId, String Name, String PhoneNumber, String NumberInParty)
        {
            this.TableId = TableId;
            this.Name = Name;
            this.PhoneNumber = PhoneNumber;
            this.NumberInParty = NumberInParty;
        }
        #endregion

        #region Properties
        public String TableId
        {
            get {
                return _TableId;
            }
            set {
                if (value != this._TableId)
                {
                    _TableId = value;
                    NotifyPropertyChanged("TableId");
                }
            }
        }

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != this._Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public String PhoneNumber
        {
            get
            {
                return _PhoneNumber;
            }
            set
            {
                if (value  != this._PhoneNumber)
                {
                    _PhoneNumber = value;
                    NotifyPropertyChanged("PhoneNumber");
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
                if (value != this._NumberInParty)
                {
                    _NumberInParty = value;
                    NotifyPropertyChanged("NumberInParty");
                }
            }
        }
        #endregion
    }
}
