using System;
using System.Globalization;
using System.Windows.Data;

namespace LolAnimationChanger.Resources
{
    public class StringToUriConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Convertit une valeur.
        /// </summary>
        /// <returns>
        /// Une valeur convertie. Si la méthode retourne null, la valeur Null valide est utilisée.
        /// </returns>
        /// <param name="value">Valeur produite par la source de liaison.</param><param name="targetType">Type de la propriété de cible de liaison.</param><param name="parameter">Paramètre de convertisseur à utiliser.</param><param name="culture">Culture à utiliser dans le convertisseur.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as string;
            if (val != null)
            {
                return new Uri(val);
            }
            return null;
        }

        /// <summary>
        /// Convertit une valeur.
        /// </summary>
        /// <returns>
        /// Une valeur convertie. Si la méthode retourne null, la valeur Null valide est utilisée.
        /// </returns>
        /// <param name="value">Valeur produite par la cible de liaison.</param><param name="targetType">Type dans lequel convertir.</param><param name="parameter">Paramètre de convertisseur à utiliser.</param><param name="culture">Culture à utiliser dans le convertisseur.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as Uri;
            if (val != null)
            {
                return val.AbsoluteUri;
            }
            return null;
        }

        #endregion
    }
}