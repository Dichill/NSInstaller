using NSInstaller.Core;
using System.Threading;
using System.Threading.Tasks;

namespace NSInstaller.Model
{
    class ProgressLabelModel : ObservableObject
    {
        private string _text;

        public string ProgressText
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }
}
