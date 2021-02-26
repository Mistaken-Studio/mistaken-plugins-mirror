using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Exiled.API.Features;

namespace Gamer.Utilities.TranslationManagerSystem
{
    public static class TranslationManager
    {
        public static bool Initiated
        {
            get
            {
                return BasePath != null;
            }
        }
        private static string BasePath = null;
        public static string Language = "EN";
        public const bool SHOW_DEBUG = false;

        private static void Initiate()
        {
            if (Initiated) 
                return;
            BasePath = $"{Paths.Configs}/Translations";
            string configPath = $"{Paths.Configs}/lang.txt";
            if (!File.Exists(configPath))
            {
                File.Create(configPath).Close();
                File.AppendAllText(configPath, "EN");
            }
            Language = File.ReadAllText(configPath);
            if (Language == "")
            {
                Language = "EN";
                Log.Error("Language was empty!");
            }
            if (!Directory.Exists(BasePath))
            {
                Log.Debug($"Creating direcory|{BasePath}", SHOW_DEBUG);
                Directory.CreateDirectory(BasePath);
                Log.Debug($"Created direcory|{BasePath}", SHOW_DEBUG);
            }
            BasePath += $"/{Language}";
            if (!Directory.Exists(BasePath))
            {
                Log.Debug($"Creating direcory|{BasePath}", SHOW_DEBUG);
                Directory.CreateDirectory(BasePath);
                Log.Debug($"Created direcory|{BasePath}", SHOW_DEBUG);
            }
            RefreshTranslations();
            Exiled.Events.Handlers.Server.RestartingRound += () =>
            {
                RefreshTranslations();
            };
        }

        private static readonly Dictionary<string, string> CachedTranslations = new Dictionary<string, string>();
        public static void RefreshTranslations()
        {
            foreach (var item in Translations.ToArray())
                ReadTranslationToCache(item.Plugin, item.ID);
        }

        public static void RegisterTranslation(string key, string PluginName, string defaultValue)
        {
            Initiate();
            Log.Debug($"Registering {PluginName}.{key}", SHOW_DEBUG);
            if (Translations.Any(i => i.ID == key && i.Plugin == PluginName))
            {
                Log.Debug($"Not registered {PluginName}.{key} because it was already registered");
                return;
            }
            string path = $"{BasePath}/{PluginName}.txt";
            if (!File.Exists(path))
            {
                Log.Debug($"Creating file|{path}", SHOW_DEBUG);
                File.Create(path).Close();
                Log.Debug($"Created file|{path}", SHOW_DEBUG);
            }
            bool found = false;
            foreach (string item in File.ReadAllLines(path))
            {
                if (item.Split(':')[0] == key)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                File.AppendAllLines(path, new string[] { key + ":" + defaultValue.Replace("\n", "|_n") });
            Translations.Add(new Translation(key, PluginName));
            Log.Debug($"Registered {PluginName}.{key}", SHOW_DEBUG);
        }

        public static string ReadTranslation(string key, string PluginName)
        {
            Initiate();
            if(CachedTranslations.TryGetValue($"{PluginName}.{key}", out string content))
                return content.Replace("|_n", "\n");
            ReadTranslationToCache(PluginName, key);
            if (CachedTranslations.TryGetValue($"{PluginName}.{key}", out content))
                return content.Replace("|_n", "\n");
            Log.Error($"{PluginName}.{key} was not registered before read!");
            return "REGISTER FIRST";
        }

        private static void ReadTranslationToCache(string PluginName, string key)
        {
            Initiate();
            foreach (Translation item in Translations.ToArray())
            {
                if (item.ID == key && PluginName == item.Plugin)
                {
                    CachedTranslations[$"{item.Plugin}.{item.ID}"] = item.Content;
                    return;
                }
            }
            Log.Error($"{PluginName}.{key} was not registered before read!");
            CachedTranslations[$"{PluginName}.{key}"] = "REGISTER FIRST";
        }

        public static List<Translation> Translations = new List<Translation>();
        public class Translation
        {
            public string ID;
            public string Content;
            public string Plugin;

            private string Path
            {
                get
                {
                    return $"{BasePath}/{this.Plugin}.txt";
                }
            }

            public Translation(string ID, string PluginName)
            {
                if (ID == "") 
                    throw new ArgumentNullException("ID", "ID can't be empty string");
                if (PluginName == "") 
                    throw new ArgumentNullException("PluginName", "PluginName can't be empty string");
                if (Language == "") 
                    throw new NullReferenceException("Language can't be empty string. Initiate first");
                this.ID = ID;
                this.Plugin = PluginName;
                if (!File.Exists(Path))
                    File.Create(Path).Close();
                Update();
            }
            
            public void Update()
            {
                string[] Content = File.ReadAllLines(Path);
                foreach (string item in Content)
                {
                    string key = item.Split(':')[0];
                    string value = "";
                    string[] array = item.Split(':');
                    for (int i = 1; i < array.Length; i++)
                        value += array[i] + ":";
                    if (value.Length > 1) 
                        value = value.Substring(0, value.Length - 1);
                    if (key == ID)
                        this.Content = value;
                }
            }
        }
    }
}
