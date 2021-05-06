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
        /// <summary>
        /// Calls <see cref="MEC.Timing.CallDelayed(float, Action)"/> and adds try catch to action
        /// </summary>
        /// <param name="module">Module calling function</param>
        /// <param name="delay">Delay passed to called function</param>
        /// <param name="action">Action passed to called function</param>
        /// <param name="name">Function name</param>
        /// <returns>Courotine handle returned by called function</returns>
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
        /// <summary>
        /// Calls <see cref="MEC.Timing.CallDelayed(float, Action)"/> and adds try catch to action
        /// </summary>
        /// <param name="delay">Delay passed to called function</param>
        /// <param name="action">Action passed to called function</param>
        /// <param name="name">Function name</param>
        /// <returns>Courotine handle returned by called function</returns>
        public static MEC.CoroutineHandle CallDelayed(float delay, Action action, string name)
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
        /// <summary>
        /// Calls <see cref="MEC.Timing.RunCoroutine(IEnumerator{float})"/> and reroutes exceptions
        /// </summary>
        /// <param name="module">Module calling function</param>
        /// <param name="courotine">Delay passed to called function</param>
        /// <param name="name">Courotine name</param>
        /// <returns>Courotine handle returned by called function</returns>
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
        /// <summary>
        /// Calls <see cref="MEC.Timing.RunCoroutine(IEnumerator{float})"/> and reroutes exceptions
        /// </summary>
        /// <param name="courotine">Delay passed to called function</param>
        /// <param name="name">Courotine name</param>
        /// <returns>Courotine handle returned by called function</returns>
        public static MEC.CoroutineHandle RunCoroutine(IEnumerator<float> courotine, string name)
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
