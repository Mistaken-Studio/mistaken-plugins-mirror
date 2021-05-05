using Exiled.API.Features;
using Gamer.Diagnostics;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Utilities
{
    /// <summary>
    /// Better Courotines
    /// </summary>
    public static class BetterCourotines
    {
        public static IEnumerator<float> CallDelayed(this Module module, float delay, Action action)
        {
            MEC.Timing.CallDelayed(delay, () =>
            {
                var start = DateTime.Now;
                try
                {
                    action();
                    MasterHandler.LogTime(module.Name, "CALL DELAYED", start, DateTime.Now)
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            });
        }
    }
}
