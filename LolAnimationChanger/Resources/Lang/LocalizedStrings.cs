
namespace LolAnimationChanger.Resources.Lang
{
    class LocalizedStrings
    {
        public LocalizedStrings()
        {

        }

        private static readonly Strings LocaleInstance = new Strings();

        public Strings Locale
        {
            get { return LocaleInstance; }
        }

    }
}
