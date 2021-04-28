using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class SwapSCPCommand : IBetterCommand, IPermissionLocked
    {       
        public string Permission => "swapscp";

        public override string Description => "Pozwala zmienić SCP";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "swapscp";

        public override string[] Aliases => new string[] { "swap" };

        public string GetUsage()
        {
            return "swapscp [SCP]";
        }

        public readonly List<KeyValuePair<int, KeyValuePair<int, RoleType>>> RoleRequests = new List<KeyValuePair<int, KeyValuePair<int, RoleType>>>();

        public static readonly List<int> AlreadyChanged = new List<int>();

        public static readonly Dictionary<string, uint> SwapCooldown = new Dictionary<string, uint>();
        private const int RoundsCooldown = 3;

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) 
                return new string[] { GetUsage() };
            var player = sender.GetPlayer();
            if (player.Team != Team.SCP) 
                return new string[] { "Nie możesz zmienić SCP nie będąc SCP" };
            if (player.Role == RoleType.Scp0492) 
                return new string[] { "Nie możesz zmienić SCP jako SCP 049-2" };
            if(RoleRequests.Any(i => i.Value.Key == player.Id))
            {
                var data = RoleRequests.First(i => i.Value.Key == player.Id);
                var requester = RealPlayers.Get(data.Key);
                if (args[0].ToLower() == "yes")
                {
                    player.Role = requester.Role;
                    requester.Role = data.Value.Value;
                    AlreadyChanged.Add(requester.Id);
                    if(!Ranks.RanksHandler.Vips.Contains(requester.UserId)) 
                        SwapCooldown.Add(requester.UserId, RoundsCooldown);
                    RoleRequests.Remove(data);
                    return new string[] { "Ok" };
                }
                else if (args[0].ToLower() == "no")
                {
                    requester.Broadcast("Swap SCP", 5, $"{player.Nickname} nie chce zamienić się SCP");
                    RoleRequests.Remove(data);
                    return new string[] { "Ok" };
                }
                else
                    return new string[] { ".scpswap yes/no", $"'yes' aby zmienić SCP na {requester.Role}", $"'no' aby zostać jako {player.Role}" };
            }
            else if(Round.ElapsedTime.TotalSeconds > 30)
                return new string[] { "Za późno, możesz zmienić SCP tylko przez pierwsze 30 sekund rundy" };
            if (RoleRequests.Any(i => i.Key == player.Id)) return new string[] { "Już wysłałeś prośbę aby zamienić SCP" };
            if (AlreadyChanged.Contains(player.Id)) return new string[] { "Możesz zmienić SCP tylko raz na rundę" };
            if (SwapCooldown.ContainsKey(player.UserId)) return new string[] { $"Możesz użyć tej komendy tylko raz na {RoundsCooldown} rundy rozegrane jako SCP" };
            var scp = args[0];
            scp = scp.ToLower().Replace("scp", "").Replace("-", "");
            var role = RoleType.Scp0492;
            switch(scp)
            {
                case "173":
                    role = RoleType.Scp173;
                    break;
                case "106":
                    role = RoleType.Scp106;
                    break;
                case "93953":
                case "939":
                    role = RoleType.Scp93953;
                    break;
                case "93989":
                    role = RoleType.Scp93989;
                    break;
                case "049":
                    role = RoleType.Scp049;
                    break;
                case "079":
                    if (RealPlayers.List.Any(p => p.Team == Team.SCP && p.Id != player.Id))
                        role = RoleType.Scp079;
                    else
                        return new string[] { "Jesteś jedynym SCP, nie możesz się zamienić w SCP 079" };
                    break;
                case "096":
                    role = RoleType.Scp096;
                    break;
                default:
                    return new string[] { "Nieznany SCP", GetUsage() };
            }
            
            if(RealPlayers.List.Any(p => p.Role == role))
            {
                var target = RealPlayers.List.First(p => p.Role == role);
                var data = new KeyValuePair<int, KeyValuePair<int, RoleType>>(player.Id, new KeyValuePair<int, RoleType>(target.Id, role));
                RoleRequests.Add(data);
                target.Broadcast("Swap SCP", 15, $"{player.Nickname} chcię się z tobą zamienić SCP, jeżeli się zgodzisz to zostaniesz <b>{player.Role}</b>\nWpisz \".swapscp yes\" lub \".swapscp no\" w konsoli(~) aby się zamienić lub aby tego nie robić");
                MEC.Timing.CallDelayed(15, () => {
                    if (RoleRequests.Contains(data))
                    {
                        player.Broadcast("Swap SCP", 5, "Czas minął");
                        RoleRequests.Remove(data);
                    }
                });
                return new string[] { "Prośba zamiany wysłana" };
            }
            else
            {
                AlreadyChanged.Add(player.Id);
                if (!Ranks.RanksHandler.Vips.Contains(player.UserId)) 
                    SwapCooldown.Add(player.UserId, RoundsCooldown);
                player.Role = role;
                return new string[] { "Done" };
            }
        }
    }
}
