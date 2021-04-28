using CommandSystem;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.Suicide
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class SuicideCommandHandler : IBetterCommand
    {
        public override string Command => "suicide";

        public override string[] Aliases => new string[] { };

        public override string Description => "DO IT";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (player.IsHuman)
            {
                if (args.Contains("-force") && false)
                {
                    if (
                        player.Inventory.availableItems.Any(i => i.id == ItemType.GunMP7) ||
                        player.Inventory.availableItems.Any(i => i.id == ItemType.GunCOM15) ||
                        player.Inventory.availableItems.Any(i => i.id == ItemType.GunUSP)
                        )
                    {
                        Handler.KillPlayer(player);
                        success = true;
                        return new string[] { PluginHandler.Instance.ReadTranslation("suicide_force") };
                    }
                }
                success = true;
                if (!Handler.InSuicidalState.Contains(player.Id))
                {
                    Handler.InSuicidalState.Add(player.Id);
                    return new string[] { PluginHandler.Instance.ReadTranslation("suicide_enter") };
                }
                else
                {
                    Handler.InSuicidalState.Remove(player.Id);
                    return new string[] { PluginHandler.Instance.ReadTranslation("suicide_exit") };
                }
            }
            return new string[] { "Only Human Suicide" };
        }
    }
}
