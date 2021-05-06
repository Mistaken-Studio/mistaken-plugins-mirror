using Exiled.API.Features;
using Gamer.Diagnostics;
using System;
using System.Collections.Generic;

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
                try
                {
                    action();
                }
                catch (System.Exception ex)
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
                try
                {
                    action();
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
