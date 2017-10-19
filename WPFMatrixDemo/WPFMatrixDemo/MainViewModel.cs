using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFMatrixDemo
{
    public class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /**********************************************************************/
        #region Constructors
        #endregion Constructors

        /**********************************************************************/
        #region Properties

        public int MatrixWidth
        {
            get => _matrixWidth;
            set
            {
                if (_matrixWidth == value)
                    return;
                _matrixWidth = value;

                RaisePropertyChanged();

                if (_matrixWidth < 0)
                    SetError("Cannot be negative");
                else if (MatrixHeight >= 0)
                    ResizeMatrix();
            }
        }
        private int _matrixWidth = 0;

        public int MatrixHeight
        {
            get => _matrixHeight;
            set
            {
                if (_matrixHeight == value)
                    return;
                _matrixHeight = value;

                RaisePropertyChanged();

                if (_matrixHeight < 0)
                    SetError("Cannot be negative");
                else if (MatrixWidth >= 0)
                    ResizeMatrix();
            }
        }
        private int _matrixHeight = 0;

        public ObservableCollection<ObservableCollection<MatrixItemViewModel>> Matrix { get; }
            = new ObservableCollection<ObservableCollection<MatrixItemViewModel>>();

        #endregion Properties

        /**********************************************************************/
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        internal protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion INotifyPropertyChanged

        /**********************************************************************/
        #region INotifyDataErrorInfo

        public bool HasErrors
            => _errorsByPropertyName.Values.Any(x => !string.IsNullOrEmpty(x));

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errorsByPropertyName.TryGetValue(propertyName ?? string.Empty, out var error))
                return Enumerable.Repeat(error, 1);

            return Enumerable.Empty<string>();
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        internal protected void SetError(string error = null, [CallerMemberName]string propertyName = null)
        {
            propertyName = propertyName ?? string.Empty;
            error = error ?? string.Empty;

            _errorsByPropertyName.TryGetValue(propertyName, out var previousError);
            _errorsByPropertyName[propertyName] = error;

            if((previousError ?? string.Empty) != error)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private Dictionary<string, string> _errorsByPropertyName
            = new Dictionary<string, string>();

        #endregion INotifyDataErrorInfo

        /**********************************************************************/
        #region Private Methods

        private void ResizeMatrix()
        {
            while (Matrix.Count < MatrixHeight)
                Matrix.Add(new ObservableCollection<MatrixItemViewModel>());

            while (Matrix.Count > MatrixHeight)
                Matrix.RemoveAt(Matrix.Count - 1);

            foreach (var item in Matrix.Select((x, i) => new { Row = x, Index = i }))
            {
                while (item.Row.Count < MatrixWidth)
                    item.Row.Add(new MatrixItemViewModel() { Value = $"({item.Index}, {item.Row.Count})" });

                while (item.Row.Count > MatrixWidth)
                    item.Row.RemoveAt(Matrix.Count - 1);
            }
        }

        #endregion Private Methods
    }
}
