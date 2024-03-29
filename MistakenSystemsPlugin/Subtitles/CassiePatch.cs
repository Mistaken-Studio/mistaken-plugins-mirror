﻿using MEC;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gamer.Mistaken.Subtitles
{
    //[HarmonyPatch(typeof(RespawnEffectsController), "RpcCassieAnnouncement")]
    internal static class CassiePatch
    {
        public static readonly Queue<string> Messages = new Queue<string>();
        public static void Postfix(string words)
        {
            string tmp = words;
            if (tmp.StartsWith("sl"))
                tmp = tmp.Substring(3);
            tmp = Regex.Replace(tmp, "\\.G\\d|PITCH_\\d*[,.]?(\\d*)?|YIELD_\\d*[,.]?(\\d*)?|JAM_\\d*_\\d*|BELL_START|BELL_END|\\.", "", RegexOptions.IgnoreCase);
            while (tmp.Contains("  "))
                tmp = tmp.Replace("  ", " ");
            tmp = tmp.Trim();
            Messages.Enqueue(tmp);
            SubtitlesHandler.UpdateAll();
        }
    }

    //[HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.Start))]
    internal static class CassiePatch2
    {
        public static int Index = -1;
        public static bool Prefix(NineTailedFoxAnnouncer __instance)
        {
            Gamer.Utilities.BetterCourotines.RunCoroutine(Start(__instance), "CassiePatch2.Start");
            return false;
        }

        private static IEnumerator<float> Start(NineTailedFoxAnnouncer __instance)
        {
            Index = -1;
            CassiePatch.Messages.Clear();
            global::NineTailedFoxAnnouncer.scpDeaths.Clear();
            global::NineTailedFoxAnnouncer.singleton = __instance;
            float speed = 1f;
            int jammed = 0;
            int jamSize = 0;
            while (__instance != null)
            {
                if (__instance.queue.Count == 0)
                {
                    speed = 1f;
                    yield return Timing.WaitForOneFrame;
                    __instance.Free = true;
                    CassiePatch.Messages.Clear();
                    Subtitles.SubtitlesHandler.UpdateAll();
                }
                else
                {
                    __instance.Free = false;
                    global::NineTailedFoxAnnouncer.VoiceLine line = __instance.queue[0];
                    __instance.queue.RemoveAt(0);
                    if (line.apiName == "END_OF_MESSAGE")
                    {
                        Index = -1;
                        __instance.speakerSource.pitch = 1f;
                        yield return Timing.WaitForSeconds(4f);
                        CassiePatch.Messages.Dequeue();
                        Subtitles.SubtitlesHandler.UpdateAll();
                    }
                    else
                    {
                        bool flag = line.apiName.StartsWith("BG_") || line.apiName.StartsWith("BELL_");
                        bool flag2 = line.apiName.StartsWith("SUFFIX_");
                        float absoluteTimeAddition = 0f;
                        float relativeTimeAddition = 0f;
                        if (line.clip != null)
                        {
                            if (flag)
                            {
                                __instance.backgroundSource.PlayOneShot(line.clip);
                            }
                            else if (flag2)
                            {
                                __instance.speakerSource.Stop();
                                __instance.speakerSource.PlayOneShot(line.clip);
                            }
                            else if (jammed > 0)
                            {
                                if (!line.apiName.StartsWith(".g", StringComparison.OrdinalIgnoreCase))
                                {
                                    Index++;
                                    Subtitles.SubtitlesHandler.UpdateAll();
                                }
                                __instance.speakerSource.Stop();
                                float timeToJam = line.length * ((float)jammed / 100f);
                                __instance.speakerSource.clip = line.clip;
                                __instance.speakerSource.time = 0f;
                                __instance.speakerSource.Play();
                                yield return Timing.WaitForSeconds(timeToJam);
                                float stepSize = 0.13f;
                                int num;
                                for (int i = 0; i < jamSize; i = num + 1)
                                {
                                    absoluteTimeAddition -= stepSize * 3f;
                                    __instance.speakerSource.time = timeToJam;
                                    yield return Timing.WaitForSeconds(stepSize);
                                    num = i;
                                }
                                jammed = 0;
                            }
                            else
                            {
                                if (!line.apiName.StartsWith(".g", StringComparison.OrdinalIgnoreCase))
                                {
                                    Index++;
                                    Subtitles.SubtitlesHandler.UpdateAll();
                                }
                                __instance.speakerSource.PlayOneShot(line.clip);
                            }
                        }
                        else if (global::NineTailedFoxAnnouncer.VoiceLine.IsPitch(line.apiName, out float num2))
                        {
                            speed = num2;
                            __instance.speakerSource.pitch = speed;
                        }
                        else if (global::NineTailedFoxAnnouncer.VoiceLine.IsJam(line.apiName, out int num3, out int num4))
                        {
                            jammed = num3;
                            jamSize = num4;
                        }
                        if (global::NineTailedFoxAnnouncer.VoiceLine.IsRegular(line.apiName))
                        {
                            float num5 = 0f;
                            int num6 = 0;
                            while (num6 < __instance.queue.Count && !global::NineTailedFoxAnnouncer.VoiceLine.IsRegular(__instance.queue[num6].apiName))
                            {
                                if (global::NineTailedFoxAnnouncer.VoiceLine.IsYield(__instance.queue[num6].apiName, out float num7))
                                {
                                    num5 = num7;
                                    break;
                                }
                                num6++;
                            }
                            if (num5 > 0f)
                            {
                                yield return Timing.WaitForSeconds(num5);
                            }
                            else
                            {
                                float num8 = (line.length + relativeTimeAddition) / speed + absoluteTimeAddition;
                                if (num8 > 0f)
                                {
                                    yield return Timing.WaitForSeconds(num8);
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }
}
