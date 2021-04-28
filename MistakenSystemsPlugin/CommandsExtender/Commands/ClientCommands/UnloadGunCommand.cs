using CommandSystem;
using Exiled.API.Extensions;
using Gamer.Mistaken.Base.CustomItems;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class UnloadGunCommand : IBetterCommand
    {
        public override string Description => "Unload all ammo";
        public override string Command => "unload";
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.CurrentItem.id.IsWeapon(false))
                return new string[] { "Nie masz broni w ręku" };
            if (CustomItemsHandler.GetCustomItem(player.CurrentItem) != null)
                return new string[] { "Nie możesz wyciągnać amunicji z tej broni" };
            uint ammo = (uint)player.CurrentItem.GetWeaponAmmo();
            if (ammo != 0)
            {
                player.SetWeaponAmmo(player.CurrentItem, 0);
                switch (player.CurrentItem.id)
                {
                    case ItemType.GunProject90:
                    case ItemType.GunUSP:
                    case ItemType.GunCOM15:
                        ItemType.Ammo9mm.Spawn(ammo, player.Position);
                        break;
                    case ItemType.GunLogicer:
                    case ItemType.GunMP7:
                        ItemType.Ammo762.Spawn(ammo, player.Position);
                        break;
                    case ItemType.GunE11SR:
                        ItemType.Ammo556.Spawn(ammo, player.Position);
                        break;
                }
            }
            success = true;
            return new string[] { "Zrobione" };
        }
    }
}
