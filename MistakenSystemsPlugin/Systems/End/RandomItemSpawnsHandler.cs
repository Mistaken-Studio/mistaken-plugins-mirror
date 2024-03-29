﻿using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.End
{
    internal class RandomItemSpawnsHandler : Module
    {
        public override bool Enabled => false;
        public RandomItemSpawnsHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "RandomItems";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }

        private void Server_RoundStarted()
        {
            int janitor = 0;
            int sciencist = 0;
            foreach (var item in Pickup.Instances)
            {
                if (item.ItemId == ItemType.KeycardJanitor)
                    janitor++;
                else if (item.ItemId == ItemType.KeycardScientist)
                    sciencist++;
            }

            Log.Debug($"Janitor: {janitor}");
            Log.Debug($"Sciencist: {sciencist}");
        }
    }
}
