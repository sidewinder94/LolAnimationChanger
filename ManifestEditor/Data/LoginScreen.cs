using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ManifestEditor.Annotations;

namespace ManifestEditor.Data
{
    public class LoginScreen : INotifyPropertyChanged
    {
        private String _name;
        private String _nameFr;
        private String _sha1;
        private String _filename;

        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public String NameFr
        {
            get
            {
                return _nameFr;
            }
            set
            {
                _nameFr = value;
                OnPropertyChanged();
            }
        }
        public String Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                OnPropertyChanged();
            }
        }
        public String SHA1
        {
            get
            {
                return _sha1;
            }
            set
            {
                _sha1 = value;
                OnPropertyChanged();
            }
        }

        #region Overrides of Object

        /// <summary>
        /// Retourne une chaîne qui représente l'objet actuel.
        /// </summary>
        /// <returns>
        /// Chaîne qui représente l'objet en cours.
        /// </returns>
        public override string ToString()
        {
            return (NameFr ?? Name);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}