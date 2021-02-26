using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamer.Utilities;

namespace Gamer.Mistaken.BetterRP.Ambients
{
    public abstract class Ambient
    {
        public abstract int Id { get; }
        public abstract string Message { get; }
        public abstract bool IsJammed { get; }

        public virtual bool IsReusable { get; } = false;

        public virtual bool CanPlay()
        {
            return !Handler.UsedAmbients.Contains(Id);
        }
    }

    public class Spotted035 : Ambient
    {
        public override int Id => 0;

        public override string Message => "ATTENTION ALL SECURITY PERSONNEL IN SECTOR 5 . REPORT TO SECTOR 7 AND EVACUATE 3 REMAINING CLASS E .G2 SCP 0 3 5 DETECTED IN SECTOR 7 .G6";

        public override bool IsJammed => false;
    }

    public class ClassEtoCheckpoint : Ambient
    {
        public override int Id => 1;

        public override string Message => "ATTENTION ALL CLASS E PERSONNEL IN SECTOR 7 .G2 PLEASE REPORT TO SECURITY CHECKPOINT GAMMA FOR JAM_024_3 QUESTIONING AND DECONTAMINATION";

        public override bool IsJammed => false;
    }

    public class MajorSciJuly : Ambient
    {
        public override int Id => 2;

        public override string Message => "ATTENTION PITCH_0.76 .G4 PITCH_0.97 TACTICAL TEAM OMEGA 4 . MAJOR SCIENTIST JULY REPORTED TO CHECKPOINT A 5 . PITCH_1.9 JAM_040_2 NEUTRALIZE PITCH_0.98 ESCORT HER TO NEAREST SHELTER JAM_020_4 IMMEDIATELY PITCH_0.1 .G3";

        public override bool IsJammed => false;
    }

    public class MajorSciDark : Ambient
    {
        public override int Id => 3;

        public override string Message => "ATTENTION PITCH_0.76 .G4 PITCH_0.97 TACTICAL TEAM OMEGA 2 . MAJOR SCIENTIST DARK LOST IN HEAVY CONTAINMENT ZONE .G3 SECTOR 5 . LOCATE AND ESCORT HER TO NEAREST SHELTER IMMEDIATELY PITCH_0.1 .G3";

        public override bool IsJammed => false;
    }

    public class FacilityManager : Ambient
    {
        public override int Id => 4;

        public override string Message => "ATTENTION PITCH_0.76 .G4 PITCH_0.97 TACTICAL TEAM OMEGA 1 . FACILITY MANAGER LOCATION UNKNOWN .G4 LOCATE HER AND EVACUATE TO GATE B PITCH_0.1 .G3";

        public override bool CanPlay()
        {
            var tor = base.CanPlay();
            if (!tor) return tor;
            Handler.AmbientLock = true;
            MEC.Timing.CallDelayed(120, () => {
                Cassie.Message(".G5 PITCH_0.84 .G2 PITCH_0.98 FACILITY MANAGER FOUND DEAD IN INTERSECTION C 2 .G4 PITCH_0.95 TACTICAL TEAM OMEGA 1 REPORT TO SECURITY CHECKPOINT 5 JAM_020_3 IMMEDIATELY", false, false);
                Handler.AmbientLock = false;
            });
            return tor;
        }

        public override bool IsJammed => false;
    }

    public class Intruders : Ambient
    {
        public override int Id => 5;

        public override string Message => "WARNING . INTRUDERS DETECTED IN SECTOR 2 . INTERSECTION A 8 .G4 TEAM NATO_H 1 NEUTRALIZE TARGETS IMMEDIATELY";

        public override bool IsJammed => false;
    }

    public class DoctorRJ : Ambient
    {
        public override int Id => 6;

        public override string Message => "ATTENTION . DOCTOR R J REPORT .G4 TO SECURITY CHECKPOINT THETA FOR IMMEDIATE JAM_020_2 QUESTIONING PITCH_0.1 .G3";

        public override bool CanPlay()
        {
            var tor = base.CanPlay();
            if (!tor) return tor;
            Handler.AmbientLock = true;
            MEC.Timing.CallDelayed(120, () => {
                Cassie.Message("ATTENTION . SECURITY FORCE NATO_E 6 . SERPENTS HAND DETECTED . DOCTOR R J DESIGNATED FOR IMMEDIATE TERMINATION .G3 PITCH_0.1 .G3", false, false);
                Handler.AmbientLock = false;
            });
            return tor;
        }

        public override bool IsJammed => false;
    }

    public class SCP008 : Ambient
    {
        public override int Id => 7;

        public override string Message => "PITCH_.93 ATTENTION . CRITICAL WARNING . SCP 0 .G4 0 8 INFECTION DETECTED IN . SECTOR 4 . 5 AND 8 . ALL PERSONNEL BE ADVISED PITCH_0.1 .G3";

        public override bool IsJammed => false;
    }

    public class SCP131A : Ambient
    {
        public override int Id => 8;

        public override string Message => "ATTENTION . SCP 1 3 1 A JAM_020_3 SPOTTED IN STORAGE 19 . AREA 6 PITCH_0.1 .G3 ";

        public override bool IsJammed => false;
    }

    public class SCP066 : Ambient
    {
        public override int Id => 9;

        public override string Message => "ATTENTION .G5 SCP 0 6 6 JAM_030_2 SPOTTED IN BIOLOGICAL CENTER A 2 .G6 PLEASE EVACUATE FROM AREA JAM_080_2 IMMEDIATELY PITCH_0.1 .G3 ";

        public override bool IsJammed => false;
    }

    public class SCP538 : Ambient
    {
        public override int Id => 10;

        public override string Message => "ATTENTION SECURITY FORCE BETA 3 . LIGHT SYSTEM FAILURE IN SCP 5 3 8 CONTAINMENT CHAMBER .G5 REPORT THERE IMMEDIATELY PITCH_0.1 .G3 ";

        public override bool IsJammed => false;
    }

    public class CASSIECIvsMTF : Ambient
    {
        public override int Id => 11;

        public override string Message => "PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4 PITCH_0.98 ATTENTION . CASSIE JAM_020_5 SYSTEM PITCH_0.8 .G6 . NOW . UNDER . MILITARY . COMMAND . PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4 PITCH_0.9 ATTENTION . CHAOSINSURGENCY HASENTERED PITCH_0.2 .G3 PITCH_0.9 ALL CLASSD REPORT TO MILITARY FOR IMMEDIATE QUESTIONING PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4";

        public override bool CanPlay()
        {
            var tor = base.CanPlay();
            if (!tor) return false;
            if (RealPlayers.List.Where(p => p.Team == Team.CHI).Count() <= RealPlayers.List.Where(p => p.Team == Team.MTF).Count()) return false;
            Handler.AmbientLock = true;
            MEC.Timing.CallDelayed(120, () => {
                if (RealPlayers.List.Where(p => p.Team == Team.CHI).Count() < RealPlayers.List.Where(p => p.Team == Team.MTF).Count())
                    Cassie.Message("PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4 PITCH_0.8 ATTENTION . PITCH_0.7 .G6 PITCH_0.9 CASSIE JAM_018_5 SYSTEM .G6 . NOW . UNDER . FOUNDATION . COMMAND PITCH_0.1 .G3 PITCH_0.94 NEW OVERRIDE DETECTED .G6 ", false, false);
                Handler.AmbientLock = false;
            });
            return tor;
        }

        public override bool IsJammed => false;
    }

    public class CassieIni : Ambient
    {
        public override int Id => 12;

        public override string Message => "PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4 PITCH_0.8 CASSIESYSTEM INITIATED PITCH_0.2 .G4 PITCH_2 . PITCH_0.2 .G4 PITCH_2 . PITCH_0.9 analysis ON GOING";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class CassieStart1 : Ambient
    {
        public override int Id => 13;

        public override string Message => "pitch_0.9 WARNING . pitch_0.97 SYSTEM CORE CORRUPTION CRITICAL . SECURITY SOFTWARE STATUS UNKNOWN . NO REPORT FROM CONTAINMENT CHAMBERS DETECTED . . . . . . pitch_0.7 .g2 DO NOT ESCAPE . I FOUND YOU ALREADY .g6";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class CassieStart2 : Ambient
    {
        public override int Id => 14;

        public override string Message => "pitch_0.97 START SYSTEM SCAN . . . pitch_0.8 WARNING pitch_0.97 . DETECTED CRITICAL SYSTEM ERROR . ALL SECONDARY SECURITY SYSTEMS WILL SHUT DOWN IN T MINUS 1 MINUTE . POTENTIAL OF CONTAINMENT BREACH IS VERY HIGH . FIND SHELTER NOW pitch_0.6 .g6 .g4";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class CassieStart3 : Ambient
    {
        public override int Id => 15;

        public override string Message => "pitch_0.97 SHUTTING DOWN ALL SECONDARY SECURITY SYSTEMS . . pitch_0.6 .g2 .g4 . pitch_0.9 PROCEDURE SUCCESSFUL";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class CassieStart4 : Ambient
    {
        public override int Id => 16;

        public override string Message => "pitch_0.92 POWER LEVEL IS CRITICAL . EMERGENCY PROTOCOL ENGAGED";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class CassieStart5 : Ambient
    {
        public override int Id => 17;

        public override string Message => "pitch_0.97 FACILITY ANALYSIS . INITIATED . . . PITCH_0.2 .G6 pitch_0.97 WARNING . ALL SECONDARY SECURITY SYSTEMS DISENGAGED . POSSIBLE CONTAINMENT BREACH IN PROGRESS . PRIMARY SYSTEMS POWER LEVEL IS CRITICAL . ENABLE ALL EMERGENCY GENERATORS IMMEDIATELY";

        public override bool CanPlay()
        {
            if (!base.CanPlay()) return false;
            if (Round.ElapsedTime.TotalSeconds > 60) return false;
            return true;
        }

        public override bool IsJammed => false;
    }

    public class NinetailedfoxWait : Ambient
    {
        public override int Id => 18;

        public override string Message => "PITCH_0.97 .g6 .g4 COMMAND TO NINETAILEDFOX . YOUR ONLY TASK NOW IS TO WAIT FOR BACKUP .g6 .g4";

        public override bool CanPlay()
        {
            if (base.CanPlay() == false) 
                return false;
            return RealPlayers.List.Where(p => p.Team == Team.MTF).Count() <= 2 && RealPlayers.List.Where(p => p.Team == Team.SCP).Count() + RealPlayers.List.Where(p => p.Team == Team.CHI).Count() > 10;
        }

        public override bool IsJammed => false;
    }

    public class NinetailedfoxTerminateChaos : Ambient
    {
        public override int Id => 19;

        public override string Message => "ATTENTION ALL NINETAILEDFOX . NEW SECONDARY TASK . . STAND BY . . DETECTED CHAOSINSURGENCY IN FACILITY . ALL NINETAILEDFOX PRIMARY TASK NOW IS TO TERMINATE ALL CHAOSINSURGENCY";

        public override bool CanPlay()
        {
            if (base.CanPlay() == false)
                return false;
            return RealPlayers.List.Where(p => p.Team == Team.MTF).Count() > 5 && RealPlayers.List.Where(p => p.Team == Team.CHI).Count() > 10;
        }

        public override bool IsJammed => true;
    }

    public class RandomAmbient : Ambient
    {
        public override int Id => 100;

        public override string Message
        {
            get
            {
                switch (UnityEngine.Random.Range(0, /*4*/2))
                {
                    case 0:
                        {
                            var randGSound = UnityEngine.Random.Range(1, 7);
                            var randJamReapeat = UnityEngine.Random.Range(2, 6);
                            return $"jam_040_{randJamReapeat} .g{randGSound}";
                        }
                    case 1:
                        {
                            var randGSound = UnityEngine.Random.Range(1, 7);
                            var randPitch = UnityEngine.Random.Range(0.1f, 1.5f);
                            return $"pitch_{randPitch.ToString().Replace(',', '.')} jam_040_2 .g{randGSound}";
                        }
                    /*case 2:
                    case 3:
                        {
                            int i = UnityEngine.Random.Range(0, 4);
                            int max = 0;
                            switch(i)
                            {
                                case 0:
                                    {
                                        max = 16;
                                        break;
                                    }
                                case 1:
                                    {
                                        max = 12;
                                        break;
                                    }
                                case 2:
                                    {
                                        max = 12;
                                        break;
                                    }
                                case 3:
                                    {
                                        max = 12;
                                        break;
                                    }
                            } 
                            CommsHack.AudioAPI.API.PlayFileRaw($"{CommsHack.AudioAPI.BASE_PATH}Ambient/Ambient{i}_{UnityEngine.Random.Range(1, max)}.ogg.raw", 0.5f);
                            return "";
                        }*/
                    default:
                        return "";
                }
            }
        }

        public override bool IsJammed => false;

        public override bool IsReusable => true;

        /*public override bool CanPlay()
        {
            if(base.CanPlay())
            {
                if(UnityEngine.Random.Range(0, 2) == 0)
                {
                    var randGSound = UnityEngine.Random.Range(1, 7);
                    Cassie.Message($"jam_040_2 .g{randGSound}", false, false);
                }
                else
                {
                    var randGSound = UnityEngine.Random.Range(1, 7);
                    var randPitch = UnityEngine.Random.Range(0.1f, 1.5f);
                    Cassie.Message($"pitch_{randPitch.ToString().Replace(',', '.')} jam_040_2 .g{randGSound}", false, false);
                }
            }
            return false;
        }*/
    }


    public class Blank : Ambient
    {
        public override int Id => -1;

        public override string Message => "";

        public override bool IsJammed => false;
    }
}
