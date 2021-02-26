using System.Collections.Generic;
using Gamer.Utilities.TranslationManagerSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CustomAchievements
{
    public static partial class CustomAchievements
    {
        public class Achievement
        {
            public uint Id;
            public string Name;
            public Dictionary<Level, uint> ProggresLevel;

            public Achievement(uint id, string name, Dictionary<Level, uint> proggresLevel)
            {
                Id = id;
                Name = name;
                ProggresLevel = proggresLevel;
            }

            public void Achive(Player player, Level level)
            {
                Log.Info($"{player} unlocked {Name} | level: {level}");
                AchiveBcSave(player, level);
            }

            private void AchiveBcSave(Player player, Level level)
            {
                var msg = TranslationManager.ReadTranslation("achiv_get", PluginHandler.PluginName);
                msg = msg.Replace("{name}", /*TranslationManager.ReadTranslation(this.Name, PluginHandler.PluginName)*/Name);
                msg = msg.Replace("{lvl}", level.ToString());
                msg = msg.Replace("{lvlcolor}", GetColor(level));
                player.Broadcast(10, msg);
            }

            private static string GetColor(Level level)
            {
                switch (level)
                {
                    case Level.NONE:
                        return "#aeaeae";
                    case Level.BRONZE:
                        return "#a6452c";
                    case Level.SILVER:
                        return "#7c8493";
                    case Level.GOLD:
                        return "#fec219";
                    case Level.DIAMOND:
                        return "#6cbdc6";
                    case Level.EXPERT:
                        return "#4e3075";
                    default:
                        return "ERROR";
                }
            }
        }
    }
}
