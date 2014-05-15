using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReservationSystemSolution.Utilities;

namespace ReservationSystemSolution.Model
{
    /// <summary>
    /// Reservations factory for a specific date.
    /// </summary>
    public class Reservations : INotifyPropertyChanged
    {
        #region Fields
        private String _Date;
        private ObservableCollection<ReservationBlock> _ReservationBlocks;
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
        public Reservations() { }
        public Reservations(DateTime date, ICollection<ReservationBlock> reservationBlocks)
        {
            Date = date.ToString(Constants.ShortDateFormat);
            ReservationBlocks = new ObservableCollection<ReservationBlock>(reservationBlocks);
        }
        #endregion 

        #region Properties
        public String Date
        {
            get
            {
                return _Date;
            }
            set
            {
                if (value != this._Date)
                {
                    _Date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }

        public ObservableCollection<ReservationBlock> ReservationBlocks
        {
            get
            {
                if (_ReservationBlocks == null)
                    _ReservationBlocks = new ObservableCollection<ReservationBlock>();
                return _ReservationBlocks;
            }
            set
            {
                if (value != _ReservationBlocks)
                {
                    _ReservationBlocks = value;
                    NotifyPropertyChanged("ReservationBlocks");
                }
            }
        }
        #endregion

        #region Helper Methods
        public ReservationBlock GetReservationBlockAt(String time)
        {
            foreach (ReservationBlock block in ReservationBlocks)
            {
                if (block.Time.Equals(time))
                {
                    return block;
                }
            }
            return null;
        }
        #endregion
    }
}
