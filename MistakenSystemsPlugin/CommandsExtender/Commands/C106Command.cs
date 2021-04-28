using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class C106Command : IBetterCommand, IPermissionLocked
    {
        public string Permission => "c106";

        public override string Description =>
        "Manipulate SCP 106 conntainment";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "c106";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "C106 True/False/Force";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            if (args[0].ToLower() == "force")
            {
                var rh = ReferenceHub.GetHub(PlayerManager.localPlayer);
                foreach (var player in Player.List)
                {
                    if (player.Role == RoleType.Scp106)
                    {
                        player.ReferenceHub.scp106PlayerScript.Contain(rh);
                    }
                }
                rh.playerInteract.RpcContain106(rh.gameObject);
                OneOhSixContainer.used = true;
                success = true;
                return new string[] { "Done" };
            }
            else if (bool.TryParse(args[0], out bool value))
            {
                GameObject.FindObjectOfType<LureSubjectContainer>().SetState(value);
                success = true;
                return new string[] { "Changed" };
            }
            else return new string[] { GetUsage() };
        }
    }
}
