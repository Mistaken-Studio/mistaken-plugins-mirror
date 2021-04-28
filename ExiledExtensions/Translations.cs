using Exiled.API.Interfaces;
using Gamer.Utilities.TranslationManagerSystem;

namespace Gamer.Utilities
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Translations
    {
        /// <summary>
        /// Registers translation
        /// </summary>
        /// <param name="me">Plugin</param>
        /// <param name="name">Translation name</param>
        /// <param name="value">Translation value</param>
        public static void RegisterTranslation(this IPlugin<IConfig> me, string name, string value)
        {
            TranslationManager.RegisterTranslation(name, me.Name, value);
        }
        /// <summary>
        /// Reads translation
        /// </summary>
        /// <param name="me">Plugin</param>
        /// <param name="name">Translation name</param>
        /// <returns>Translation value</returns>
        public static string ReadTranslation(this IPlugin<IConfig> me, string name)
        {
            return TranslationManager.ReadTranslation(name, me.Name);
        }
        /// <summary>
        /// Reads and formats translation
        /// </summary>
        /// <param name="me">Plugin</param>
        /// <param name="name">Translation name</param>
        /// <param name="format">Format args</param>
        /// <returns>Formated translation value</returns>
        public static string ReadTranslation(this IPlugin<IConfig> me, string name, params object[] format)
        {
            return string.Format(TranslationManager.ReadTranslation(name, me.Name), format);
        }
    }
}
