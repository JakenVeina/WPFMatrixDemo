using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFMatrixDemo
{
    public class MatrixItemViewModel : INotifyPropertyChanged
    {
        /**********************************************************************/
        #region Properties

        public string Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                _value = value;

                RaisePropertyChanged();
            }
        }
        private string _value = "TextBox";

        #endregion Properties

        /**********************************************************************/
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        internal protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion INotifyPropertyChanged
    }
}
