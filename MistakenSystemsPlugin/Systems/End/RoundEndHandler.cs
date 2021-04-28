using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.Systems.End
{
    internal class RoundEndHandler : Module
    {
        public RoundEndHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "RoundEnd";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.EndingRound += this.Handle<Exiled.Events.EventArgs.EndingRoundEventArgs>((ev) => Server_EndingRound(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.EndingRound -= this.Handle<Exiled.Events.EventArgs.EndingRoundEventArgs>((ev) => Server_EndingRound(ev));
        }

        private void Server_EndingRound(Exiled.Events.EventArgs.EndingRoundEventArgs ev)
        {
            if (!ev.IsRoundEnded)
                return;
            bool CanClassCIWin = true;
            bool CanClassMTFWin = true;
            bool CanClassSCPWin = true;
            var stats = RoundSummary.singleton.classlistStart;
            float classDStart = stats.class_ds < 1 ? 1 : stats.class_ds;
            float sciStart = stats.scientists < 1 ? 1 : stats.scientists;

            if (Warhead.IsDetonated)
            {
                //CanClassSCPWin = false;
            }
            if (RealPlayers.List.Where(p => p.Team == Team.SCP && p.Role != RoleType.Scp0492).Count() == 0)
            {
                CanClassSCPWin = false;
            }
            if (!(((RoundSummary.escaped_scientists + RealPlayers.List.Where(p => p.Team == Team.RSC).Count() / sciStart) >= 0.5f && RealPlayers.List.Where(p => p.Team == Team.MTF).Count() > 0)))
            {
                CanClassMTFWin = false;
            }
            if (!(((RoundSummary.escaped_ds + RealPlayers.List.Where(p => p.Team == Team.CDP).Count()) / classDStart) >= 0.25f))
            {
                CanClassCIWin = false;
            }
            if (CanClassCIWin && CanClassSCPWin)
                ev.LeadingTeam = LeadingTeam.ChaosInsurgency;
            else if ((CanClassCIWin || CanClassSCPWin) && CanClassMTFWin)
                ev.LeadingTeam = LeadingTeam.Draw;
            else if ((CanClassCIWin || CanClassMTFWin) && CanClassSCPWin)
                ev.LeadingTeam = LeadingTeam.Draw;
            else if ((CanClassSCPWin || CanClassMTFWin) && CanClassCIWin)
                ev.LeadingTeam = LeadingTeam.Draw;
            else if (CanClassCIWin)
                ev.LeadingTeam = LeadingTeam.ChaosInsurgency;
            else if (CanClassMTFWin)
                ev.LeadingTeam = LeadingTeam.FacilityForces;
            else if (CanClassSCPWin)
                ev.LeadingTeam = LeadingTeam.Anomalies;
            else
                ev.LeadingTeam = LeadingTeam.Draw;
        }
    }
}
