
using CommandSystem;
using Gamer.Utilities;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class TutorialMeCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "tutorial";

        public override string Description =>
            "Tutorial";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "tutorial";

        public override string[] Aliases => new string[] { "tut" };

        public string GetUsage()
        {
            return "Tut [args]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            if (player.Role != RoleType.Tutorial)
            {
                player.IsOverwatchEnabled = false;
                player.Role = RoleType.Tutorial;
                player.IsGodModeEnabled = true;
                player.IsBypassModeEnabled = true;

                foreach (var item in args.Select(i => i.ToLower()))
                {
                    switch (item)
                    {
                        case "-vi":
                        case "--vision":
                            {
                                MEC.Timing.CallDelayed(1, () =>
                                {
                                    player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Visuals939>();
                                    player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<CustomPlayerEffects.Visuals939>(1);
                                });
                                break;
                            }
                        case "-n":
                        case "-noclip":
                            {
                                if (player.CheckPermission("Global.Noclip"))
                                    player.NoClipEnabled = true;
                                break;
                            }
                        case "-v":
                        case "--vanish":
                            {
                                if (player.CheckPermission(PluginHandler.PluginName + ".vanish"))
                                    Systems.End.VanishHandler.SetGhost(player, true);
                                break;
                            }
                    }
                }
            }
            else
            {
                player.Role = RoleType.Spectator;
                Systems.End.VanishHandler.SetGhost(player, false);
                player.IsGodModeEnabled = false;
                player.IsBypassModeEnabled = false;
                player.NoClipEnabled = false;
                MEC.Timing.CallDelayed(1, () => player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Visuals939>());
            }

            success = true;
            return new string[] { "Done" };
        }
    }
}
