using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System.Linq;
using UnidecodeSharpFork;

namespace Gamer.Mistaken.Systems.NicknameFixer
{
    public class Handler : Module
    {
        public override bool IsBasic => true;
        public Handler(PluginHandler p) : base(p)
        {
            p.RegisterTranslation("nickfix_change_msg", "<color=red>You have unacceptable nickname.</color> You're nickanme have been changed. New nick is $newnick");
            p.RegisterTranslation("nickfix_change_msg_critical", "<color=red>You have unacceptable nickname.</color> System tried to change your nickaname and newnickname was empty. |_n<b><color=red>Change your nickname</color></b>");
        }

        public override string Name => "NicknameFixer";
        public override void OnEnable()
        {

            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
        }

        private static readonly string[] ToRemove = new string[]
        {
            "key-drop.pl",
            "key-drop.com",
            "casedrop.pl",
            "casedrop.eu",
            "casedrop.com",
            "hell-case.pl",
            "hellcase.com",
            "g4skins.com",
            "j4p.pl",
            "gamehag.com",
            "key-drom.com",
            "key-drom.pl",
            "csgocases.com",
            "tradeit.gg",
            "rust chance.com",
            "itemdrop.pl",
            "#grubychuj i katujemy.eu",
            "cs-creativ.pl",
            "fade.pro",
            "force-drop.net",
            "www.twitch.tv",
            "keydrop.com",
            "key.drop.pl",
            "casefortune.com",
            "keydrop.pl",
            "gamdom.com",
            "csgoatse.com",
            "hurtfun.com",
            "cs.money",
            "goboosting.pl",
            "katujemy.eu",
            "rustchance.com",
            "skindrop.PL",
            "cs-zone.pl",
            "pvpro.com",
            "feelthegame.eu",
            "magicdrop.ru",
            "magicskill.pl",
            "go-dream.pl",
            "empire.com",
            "keycase.pl",
            "twitch.tv",
            "key drop com",
            "skindrop.pl",
            "polski-survival.pl",
            "trade.tf",
            "eskill.pl",
            "hellcase.org",
            "casedrop.gg",
            "casedrop",
            "keydrop",
            "key drop",

            "~~",
            "```",
        };
        private static readonly string[] Links = new string[]
        {
            "discord.gg/",
            "twitch.tv/"
        };
        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            if (ev.Player.IsActiveDev())
                return;
            string oldnick = ev.Player.Nickname.Trim();
            string newNick = ev.Player.Nickname.ToLower().Trim();

            foreach (var item in Links)
            {
                if (!newNick.Contains(item))
                    continue;
                var tmp = newNick.Split(' ');
                newNick = string.Join(" ", tmp.Where(i => !i.Contains(item)));
            }

            foreach (var item in ToRemove)
            {
                if (!newNick.Contains(item))
                    continue;
                newNick = newNick.Replace(item, "");
            }

            newNick = newNick.Unidecode().Replace("[?]", "").Trim();
            if (newNick.Trim() == oldnick.ToLower().Replace("ł", "l").Replace("ć", "c").Replace("ś", "s").Replace("ó", "o").Replace("ą", "a").Replace("ż", "z").Replace("ź", "z").Replace("ń", "n").Replace("ę", "e").Trim())
                newNick = oldnick;

            if (newNick == "")
            {
                Log.Debug("Player's(" + ev.Player.UserId + ") nick was " + oldnick + " and it couldn't be converted");
                RoundLogger.Log("NICKNAME FIXER", "FAILED", $"Failed to change nickname for {ev.Player.PlayerToString()}");
                if (PluginHandler.Config.IsRP())
                    ev.Player.Disconnect(plugin.ReadTranslation("nickfix_change_msg_critical"));
                else
                {
                    ev.Player.Broadcast(10, plugin.ReadTranslation("nickfix_change_msg_critical"));
                    ev.Player.DisplayNickname = "Change your nickname";
                }
            }
            else
            {
                if (newNick.Length < 3)
                {
                    while (newNick.Length < 3)
                        newNick += "x";
                }
                if (oldnick.ToLower() != newNick.ToLower())
                {
                    Log.Debug("Player's(" + ev.Player.UserId + ") nick changed from " + oldnick + " to " + newNick);
                    ev.Player.Broadcast(10, plugin.ReadTranslation("nickfix_change_msg").Replace("$newnick", newNick));
                    ev.Player.DisplayNickname = newNick;
                    RoundLogger.Log("NICKNAME FIXER", "CHANGED", $"Changed nickname for {ev.Player.PlayerToString()} from \"{oldnick}\" to \"{newNick}\"");
                }
            }
        }
    }
}
