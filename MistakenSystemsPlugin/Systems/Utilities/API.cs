using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.Systems.Utilities.API
{
    public static class Map
    {
        public static TeslaMode TeslaMode = TeslaMode.ENABLED;
        public static bool FriendlyFire = true;
        public static bool RespawnLock = false;
        private static int _rl = 0;
        private static MEC.CoroutineHandle? _roundLimit;
        public static int RoundLimit
        {
            get
            {
                return _rl;
            }
            set
            {
                _rl = value;
                if (_roundLimit.HasValue) Timing.KillCoroutines(_roundLimit.Value);
                if (_rl != 0) _roundLimit = Timing.RunCoroutine(ExecuteRoundLimit());
            }
        }

        public static class Blackout
        {
            private static MEC.CoroutineHandle? Handle;

            private static bool _enabled = false;
            public static bool Enabled
            {
                get
                {
                    return _enabled;
                }
                set
                {
                    _enabled = value;
                    if (value)
                    {
                        if (Handle.HasValue)
                            Timing.KillCoroutines(Handle.Value);
                        Handle = Timing.RunCoroutine(ExecuteBlackout());
                    }
                }
            }
            public static float Length = 10;
            public static float Delay = 10;
            public static bool OnlyHCZ = false;

            internal static void Restart()
            {
                Enabled = false;
                Length = 10;
                Delay = 10;
                OnlyHCZ = false;
            }

            private static IEnumerator<float> ExecuteBlackout()
            {
                while (Map.Blackout.Enabled)
                {
                    try
                    {
                        Generator079.mainGenerator.ServerOvercharge(Map.Blackout.Length, Map.Blackout.OnlyHCZ);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                    yield return MEC.Timing.WaitForSeconds(Map.Blackout.Delay);
                }
            }
        }

        public static class Overheat
        {
            private static int _ohl = -1;
            public static int OverheatLevel
            {
                get
                {
                    return _ohl;
                }
                set
                {
                    _ohl = value;
                    if (Handle.HasValue)
                        Timing.KillCoroutines(Handle.Value);
                    if (_ohl != -1)
                        Handle = Timing.RunCoroutine(HandleOvercheat(RoundPlus.RoundId, _ohl, _ohl));
                }
            }

            private static MEC.CoroutineHandle? Handle;

            private static IEnumerator<float> HandleOvercheat(int roundId, int proggressLevel, int startLevel)
            {
                if (RoundPlus.RoundId != roundId)
                    yield break;
                if (!Round.IsStarted)
                    yield break;
                _ohl = proggressLevel;
                switch (proggressLevel)
                {
                    case -1:
                        yield break;
                    case 0:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 30 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                0.15f,
                                0.10f
                            );
                            yield return MEC.Timing.WaitForSeconds(300);
                            break;
                        }
                    case 1:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 25 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.15f,
                                    0.10f
                                );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 25 MINUTES",
                                    0.20f,
                                    0.15f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(300);
                            break;
                        }
                    case 2:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                        "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 20 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                        0.15f,
                                        0.10f
                                    );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 20 MINUTES",
                                    0.20f,
                                    0.15f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(300);
                            break;
                        }
                    case 3:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 15 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.20f,
                                    0.15f
                                );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 15 MINUTES",
                                    0.20f,
                                    0.15f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(300);
                            break;
                        }
                    case 4:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 10 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.25f,
                                    0.20f
                                );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 10 MINUTES",
                                    0.25f,
                                    0.20f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(300);
                            break;
                        }
                    case 5:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 5 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.30f,
                                    0.25f
                                );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 5 MINUTES",
                                    0.30f,
                                    0.25f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(120);
                            break;
                        }
                    case 6:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 3 MINUTES . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.35f,
                                    0.30f
                                );
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 3 MINUTES",
                                    0.35f,
                                    0.30f
                                );
                            }
                            RespawnLock = true;
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                "FACILITY LIGHT SYSTEM CRITICAL DAMAGE . LIGHTS OUT",
                                0.35f,
                                0.30f
                            );
                            Generator079.mainGenerator.ServerOvercharge(300, false);
                            yield return MEC.Timing.WaitForSeconds(90);
                            break;
                        }
                    case 7:
                        {
                            while (Cassie.IsSpeaking)
                                yield return Timing.WaitForOneFrame;
                            if (startLevel == proggressLevel)
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "ALERT  ALERT .  DETECTED FACILITY REACTOR CORE OVERHEAT . REACTOR WILL OVERHEAT IN T MINUS 90 SECONDS . ALL PERSONNEL HAVE TO EVACUATE UNTIL OVERHEAT . IT WILL CAUSE THERMAL EXPLOSION OF FACILITY",
                                    0.40f,
                                    0.35f
                                );
                                while (Cassie.IsSpeaking)
                                    yield return Timing.WaitForOneFrame;
                                Cassie.Message(
                                    "FACILITY LIGHT SYSTEM CRITICAL DAMAGE . LIGHTS OUT",
                                    false,
                                    false
                                );
                                Generator079.mainGenerator.ServerOvercharge(300, false);
                            }
                            else
                            {
                                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(
                                    "DANGER . REACTOR OVERHEAT STATUS .  REACTOR WILL OVERHEAT IN T MINUS 90 SECONDS . STARTING COUNTDOWN",
                                    0.40f,
                                    0.35f
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(30);
                            break;
                        }
                    case 8:
                        {
                            if (startLevel == proggressLevel)
                                yield break;
                            else
                            {
                                while (Cassie.IsSpeaking)
                                    yield return Timing.WaitForOneFrame;
                                Cassie.Message(
                                    "T MINUS 60 SECONDS",
                                    false,
                                    false
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(30);
                            break;
                        }
                    case 9:
                        {
                            if (startLevel == proggressLevel)
                                yield break;
                            else
                            {
                                while (Cassie.IsSpeaking)
                                    yield return Timing.WaitForOneFrame;
                                Cassie.Message(
                                    "T MINUS 30 SECONDS",
                                    false,
                                    false
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(20);
                            break;
                        }
                    case 10:
                        {
                            if (startLevel == proggressLevel)
                                yield break;
                            else
                            {
                                while (Cassie.IsSpeaking)
                                    yield return Timing.WaitForOneFrame;
                                Cassie.Message(
                                    "10 SECONDS 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1",
                                    false,
                                    false
                                );
                            }
                            yield return MEC.Timing.WaitForSeconds(5);
                            break;
                        }
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        {
                            if (startLevel == proggressLevel)
                                yield break;
                            else
                                Warhead.Shake();
                            yield return MEC.Timing.WaitForSeconds(1);
                            break;
                        }
                    case 16:
                        {
                            AlphaWarheadController.Host.InstantPrepare();
                            AlphaWarheadController.Host.StartDetonation();
                            AlphaWarheadController.Host.NetworktimeToDetonation = 0.1f;
                            RespawnLock = false;
                            foreach (var player in RealPlayers.List)
                            {
                                player.ReferenceHub.characterClassManager.TargetDeathScreen(player.Connection, new PlayerStats.HitInfo(-1f, "*Facility Reactor", new DamageTypes.DamageType("Facility Reactor"), 0));
                                player.Role = RoleType.Spectator;
                            }
                            Round.IsLocked = false;
                            //RoundSummary.singleton.ForceEnd();
                            break;
                        }
                    default:
                        {
                            Handle = null;
                            yield break;
                        }
                }
                Handle = Timing.RunCoroutine(HandleOvercheat(roundId, proggressLevel + 1, startLevel));
            }
        }

        internal static void Restart()
        {
            TeslaMode = TeslaMode.ENABLED;
            FriendlyFire = true;
            RoundLimit = 0;
            RespawnLock = false;

            Blackout.Restart();
            Overheat.OverheatLevel = -1;
        }

        private static IEnumerator<float> ExecuteRoundLimit()
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                if (Round.ElapsedTime.TotalSeconds > _rl && _rl != 0)
                {
                    StartWarheadLock(WarheadLockType.SEC30);
                    break;
                }
                yield return Timing.WaitForSeconds(2);
            }
        }

        public static void RestartTeslaGates(bool loud)
        {
            Timing.RunCoroutine(IRestartTeslaGates(loud));
        }

        private static IEnumerator<float> IRestartTeslaGates(bool loud)
        {
            if (loud)
            {
                while (Cassie.IsSpeaking)
                    yield return Timing.WaitForOneFrame;
                Cassie.Message("FACILITY TESLA GATES REACTIVATION IN 3 . 2 . 1 . . ");
                yield return Timing.WaitForSeconds(8);
            }

            for (int i = 0; i < 5; i++)
            {
                Exiled.API.Features.Map.TeslaGates.ToList().ForEach(tesla => tesla.RpcInstantBurst());
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        public static void OpenAllDoors()
        {
            foreach (var d in Exiled.API.Features.Map.Doors)
                d.NetworkTargetState = true;
        }

        public static void CloseAllDoors()
        {
            foreach (var d in Exiled.API.Features.Map.Doors)
                d.NetworkTargetState = false;
        }

        public static void RestartDoors()
        {
            Timing.RunCoroutine(IRestartDoors());
        }

        private static IEnumerator<float> IRestartDoors()
        {
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            Exiled.API.Features.Cassie.Message("FACILITY DOOR SYSTEM REACTIVATION IN 3 . 2 . 1 . . . . . PROCEDURE SUCCESSFUL", false, true);
            yield return Timing.WaitForSeconds(8);
            CloseAllDoors();
        }

        private static IEnumerator<float> ExecuteWarheadLock(float time)
        {
            yield return Timing.WaitForSeconds(time);
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            Cassie.Message("Danger . Alpha Warhead emergency sequence initiated", false, true);
            yield return Timing.WaitForSeconds(8);
            Warhead.Start();
            Warhead.IsLocked = true;
        }
        private static MEC.CoroutineHandle? _handle;
        public static void StartWarheadLock(WarheadLockType type)
        {
            switch (type)
            {
                case WarheadLockType.SEC30:
                    {
                        _handle = Timing.RunCoroutine(ExecuteWarheadLock(38));
                        break;
                    }
                case WarheadLockType.MIN1:
                    {
                        _handle = Timing.RunCoroutine(ExecuteWarheadLock(60));
                        break;
                    }
                case WarheadLockType.MIN5:
                    {
                        _handle = Timing.RunCoroutine(ExecuteWarheadLock(300));
                        break;
                    }
                case WarheadLockType.MIN10:
                    {
                        _handle = Timing.RunCoroutine(ExecuteWarheadLock(600));
                        break;
                    }
            }
        }
        public static void CancleWarheadLock()
        {
            if (_handle.HasValue)
            {
                Timing.KillCoroutines(_handle.Value);
            }
        }
    }

    public enum OverheatLevel
    {
        NONE = -1,
        T_MINUS_30_MINUTES = 0,
        T_MINUS_25_MINUTES = 1,
        T_MINUS_20_MINUTES = 2,
        T_MINUS_15_MINUTES = 3,
        T_MINUS_10_MINUTES = 4,
        T_MINUS_5_MINUTES = 5,
        T_MINUS_3_MINUTES = 6,
        T_MINUS_90_SECONDS = 7,
        T_MINUS_60_SECONDS = 8,
        T_MINUS_30_SECONDS = 9,
        T_MINUS_10_SECONDS = 10,
        T_MINUS_5_SECONDS = 11,
        T_MINUS_4_SECONDS = 12,
        T_MINUS_3_SECONDS = 13,
        T_MINUS_2_SECONDS = 14,
        T_MINUS_1_SECONDS = 15,
        T_MINUS_0_SECONDS = 16,
    }

    public enum TeslaMode
    {
        ENABLED,
        DISABLED,
        DISABLED_FOR_079,
        DISABLED_FOR_ALL,
    }

    public enum WarheadLockType
    {
        MIN10 = 0,
        MIN5 = 1,
        MIN1 = 2,
        SEC30 = 3,
    }
}

