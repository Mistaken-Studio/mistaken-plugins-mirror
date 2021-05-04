using CommandSystem;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.SLToCentral;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class PrefCommand : IBetterCommand
    {
        public override string Command => "pref";
        public override string[] Aliases => new string[] { "prefs", "preferences" };
        public override string Description => "Preferencje gracza";

        public string GetUsage()
        {
            return "pref (name)";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = true;
            var player = sender.GetPlayer();
            var prefs = Systems.Handler.PlayerPreferencesDict[player.UserId];
            if (args.Length == 0)
            {
                return new string[]
                {
                    "wpisz \".pref [name]\" aby przełączyć opcję o podanej nazwie",
                    "Nazwy:",
                    $"Colorful Entrance Zone (cez) (Wyłączenie działą tylko dla SCP-079 i Spectatorów): {(prefs.HasFlag(API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079) ? "<color=red>Wyłączony dla SCP-079 i Spectatorów</color>" : "<color=green>Włączony</color>")}",
                    $"Transkrypt (transkrypt): {(prefs.HasFlag(API.PlayerPreferences.DISABLE_TRANSCRYPT) ? "<color=red>Wyłączony</color>" : "<color=green>Włączony</color>")}"
                };
            }
            switch (args[0].ToLower())
            {
                case "trans":
                case "transkrypt":
                case "transcript":
                    if (prefs.HasFlag(API.PlayerPreferences.DISABLE_TRANSCRYPT))
                        prefs &= ~API.PlayerPreferences.DISABLE_TRANSCRYPT;
                    else
                        prefs |= API.PlayerPreferences.DISABLE_TRANSCRYPT;
                    break;
                case "kolorowy":
                case "cez":
                case "colorfulez":
                case "colorfulentrancezone":
                case "colorful":
                case "colorfull":
                    if (prefs.HasFlag(API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079))
                        prefs &= ~API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079;
                    else
                        prefs |= API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079;
                    break;
                default:
                    return new string[]
                    {
                        "Nieznana nazwa, nazwy: ",
                        "cez -> Colorful Entrance Zone (Kolorowy Entrance Zone)",
                        "transkrypt -> Transkrypt"
                    };
            }
            SSL.Client.Send(MessageType.SL_SET_PLAYER_PREFERENCES, new SL_Player_Set_Preferences
            {
                Player = player.UserId,
                Prefs = (SL_Player_Prefs)prefs
            });
            Systems.Handler.PlayerPreferencesDict[player.UserId] = prefs;
            return new string[] { "Zapisano" };
        }
    }
}
