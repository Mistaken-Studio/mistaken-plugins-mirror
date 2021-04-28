using CommandSystem;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.SLToCentral;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class PrefCommand : IBetterCommand
    {
        public override string Command => "pref";
        public override string[] Aliases => new string[] { "prefs", "preferences" };
        public override string Description => "Player Preferences";

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
                    ".pref [name] to toggle",
                    "Preferences List:",
                    $"Colorful Entrance Zone (cez): {(prefs.HasFlag(API.PlayerPreferences.DISABLE_COLORFUL_EZ) ? "<color=red>Disabled</color>" : "<color=green>Enabled</color>")}",
                    $"Transkcrypt (transkcrypt): {(prefs.HasFlag(API.PlayerPreferences.DISABLE_TRANSCRYPT) ? "<color=red>Disabled</color>" : "<color=green>Enabled</color>")}"
                };
            }
            switch (args[0].ToLower())
            {
                case "trans":
                case "transkcrypt":
                    if (prefs.HasFlag(API.PlayerPreferences.DISABLE_TRANSCRYPT))
                        prefs &= ~API.PlayerPreferences.DISABLE_TRANSCRYPT;
                    else
                        prefs |= API.PlayerPreferences.DISABLE_TRANSCRYPT;
                    break;
                case "cez":
                case "colorfulez":
                case "colorfulentrancezone":
                case "colorful":
                case "colorfull":
                    if (prefs.HasFlag(API.PlayerPreferences.DISABLE_COLORFUL_EZ))
                        prefs &= ~API.PlayerPreferences.DISABLE_COLORFUL_EZ;
                    else
                        prefs |= API.PlayerPreferences.DISABLE_COLORFUL_EZ;
                    break;
                default:
                    return new string[]
                    {
                        "Unknown arg, options: ",
                        "cez -> Colorful Entrance Zone",
                        "transkcrypt -> Transkcrypt"
                    };
            }
            SSL.Client.Send(MessageType.SL_SET_PLAYER_PREFERENCES, new SL_Player_Set_Preferences
            {
                Player = player.UserId,
                Prefs = (SL_Player_Prefs)prefs
            });
            Systems.Handler.PlayerPreferencesDict[player.UserId] = prefs;
            return new string[] { "Zmiany zostaną zaplikowane przy następnym wejściu na serwer, polecamy wpisać \"dc\" i potem \"rc\"" };
        }
    }
}
