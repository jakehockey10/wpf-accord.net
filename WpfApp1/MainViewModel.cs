using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApp1.Annotations;

namespace WpfApp1
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private string _supportsSnapshots;
        private string _resolution;

        public string SupportsSnapshots
        {
            get => _supportsSnapshots;
            set
            {
                _supportsSnapshots = value;
                OnPropertyChanged(nameof(SupportsSnapshots));
            }
        }

        public string Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}