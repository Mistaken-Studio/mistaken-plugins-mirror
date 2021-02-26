using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Utilities.TranslationManagerSystem;

namespace Gamer.Utilities
{
    public static class Translations
    {
        public static void RegisterTranslation(this IPlugin<IConfig> me, string name, string value)
        {
            TranslationManager.RegisterTranslation(name, me.Name, value);
        }

        public static string ReadTranslation(this IPlugin<IConfig> me, string name)
        {
            return TranslationManager.ReadTranslation(name, me.Name);
        }
        
        public static string ReadTranslation(this IPlugin<IConfig> me, string name, params object[] format)
        {
            return string.Format(TranslationManager.ReadTranslation(name, me.Name), format);
        }
    }
}
