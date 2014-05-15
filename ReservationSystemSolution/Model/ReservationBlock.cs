using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReservationSystemSolution.Model
{
    /// <summary>
    /// Keeps track of reservations during a block of time
    /// </summary>
    public class ReservationBlock : INotifyPropertyChanged
    {
        #region Fields
        private String _Time;
        private ObservableCollection<CustomerReservation> _CustomerReservations;
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
        public ReservationBlock() {}
        public ReservationBlock(DateTime time, ICollection<CustomerReservation> customerReservations)
        {
            // Store time as the hour with am/pm
            _Time = time.ToString("t");
            _CustomerReservations = new ObservableCollection<CustomerReservation>(customerReservations);
        }
        #endregion

        #region Properties
        public String Time
        {
            get
            {
                return _Time;
            }
            set
            {
                if (value != this._Time)
                {
                    _Time = value;
                    NotifyPropertyChanged("Time");
                }
            }
        }

        public ObservableCollection<CustomerReservation> CustomerReservations
        {
            get
            {
                if (_CustomerReservations == null)
                    _CustomerReservations = new ObservableCollection<CustomerReservation>();
                return _CustomerReservations;
            }
            set
            {
                if (value != this._CustomerReservations)
                {
                    _CustomerReservations = value;
                    NotifyPropertyChanged("CustomerReservations");
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets the customer that reserved the table specified. 
        /// Assumes there is just one 
        /// </summary>
        /// <param name="tableID"></param>
        /// <returns></returns>
        public CustomerReservation GetCustomerReservationAt(String tableID)
        {
            foreach (CustomerReservation reservation in CustomerReservations)
            {
                if (reservation.TableId.Equals(tableID))
                {
                    return reservation;
                }
            }
            return null;
        }

        public bool DeleteCustomerReservationAt(String tableID)
        {
            bool deleted = false;
            foreach (CustomerReservation reservation in CustomerReservations)
            {
                if (reservation.TableId.Equals(tableID))
                {
                    CustomerReservations.Remove(reservation);
                    deleted = true;
                    break;
                }
            }
            return deleted;
        }
        #endregion
    }
}
