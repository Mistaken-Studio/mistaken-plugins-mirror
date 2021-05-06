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
        public static MEC.CoroutineHandle CallDelayed(this Module module, float delay, Action action, string name = "CallDelayed")
        {
            return MEC.Timing.CallDelayed(delay, () =>
            {
                var start = DateTime.Now;
                try
                {
                    action();
                    MasterHandler.LogTime(module.Name, name, start, DateTime.Now);
                }
                catch(System.Exception ex)
                {
                    MasterHandler.LogError(ex, module, name);
                    Log.Error($"[{module.Name}: {name}] {ex.Message}");
                    Log.Error($"[{module.Name}: {name}] {ex.StackTrace}");
                }
            });
        }

        public static MEC.CoroutineHandle CallDelayed(float delay, Action action, string name = "CallDelayed")
        {
            return MEC.Timing.CallDelayed(delay, () =>
            {
                var start = DateTime.Now;
                try
                {
                    action();
                    MasterHandler.LogTime("Rouge", name, start, DateTime.Now);
                }
                catch (System.Exception ex)
                {
                    MasterHandler.LogError(ex, null, name);
                    Log.Error($"[Rouge: {name}] {ex.Message}");
                    Log.Error($"[Rouge: {name}] {ex.StackTrace}");
                }
            });
        }

        public static MEC.CoroutineHandle RunCoroutine(this Module module, IEnumerator<float> courotine, string name = "RunCoroutine")
        {
            DateTime start = DateTime.Now;
            courotine.Prepend(() => start = DateTime.Now);
            courotine.Append(() => MasterHandler.LogTime(module.Name, name, start, DateTime.Now));
            courotine.RerouteExceptions((ex) =>
            {
                MasterHandler.LogError(ex, module, name);
                Log.Error($"[{module.Name}: {name}] {ex.Message}");
                Log.Error($"[{module.Name}: {name}] {ex.StackTrace}");
            });
            return MEC.Timing.RunCoroutine(courotine);
        }

        public static MEC.CoroutineHandle RunCoroutine(IEnumerator<float> courotine, string name = "RunCoroutine")
        {
            DateTime start = DateTime.Now;
            courotine.Prepend(() => start = DateTime.Now);
            courotine.Append(() => MasterHandler.LogTime("Rouge", name, start, DateTime.Now));
            courotine.RerouteExceptions((ex) =>
            {
                MasterHandler.LogError(ex, null, name);
                Log.Error($"[Rouge: {name}] {ex.Message}");
                Log.Error($"[Rouge: {name}] {ex.StackTrace}");
            });
            return MEC.Timing.RunCoroutine(courotine);
        }
    }
}
