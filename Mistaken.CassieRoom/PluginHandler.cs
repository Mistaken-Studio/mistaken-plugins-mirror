using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistaken.CassieRoom
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Mistaken-Studio";
        public override string Name => "CassieRoom";
        public override void OnDisabled()
        {
            Gamer.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }

        public override void OnEnabled()
        {
            new CassieRoomHandler(this);
            new Elevator(this);

            Gamer.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }
    }
}
