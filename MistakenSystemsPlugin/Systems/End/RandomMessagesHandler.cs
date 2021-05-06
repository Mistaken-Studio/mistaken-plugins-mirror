using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gamer.Mistaken.Systems.End
{
    internal class RandomMessagesHandler : Module
    {
        public RandomMessagesHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "RandomMessage";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }

        public static readonly ReadOnlyCollection<string> Messages = new ReadOnlyCollection<string>(new List<string>()
        {
            "<color=#CCC>Zapraszamy na naszego discorda <color=#6B9ADF>discord.mistaken.pl</color></color>",
            //"<color=#CCC>Otwarto podania na <b>White Listę</b> do serwera HardRP. Więcej informacji dotyczących tego znajdziecie na naszym discordzie (<color=#6B9ADF>discord.mistaken.pl</color>)</color>",
            "<color=#CCC><size=35>Podmioty <color=red>SCP</color> <b>nie</b> mogą współpracować z \n kimkolwiek poza <color=red>SCP</color> <size=30>(nawet <color=#008F00>Rebelią Chaosu</color>)</size> wyjątek stanowi <color=red>SCP 079</color> który może współpracować ze wszystkimi ale <b>nie może</b> przeszkadzać <color=red>SCP</color></size></color>",
            "<color=#CCC>Samobójstwo jako <color=red>SCP</color> = <color=red><b>ban</b></color></color>",
            "<color=#CCC>Zamykanie drzwi swoim (wyjątkiem jest <color=orange>Klasa D</color>) = <color=red><b>ban</b></color></color>",
            "<color=#CCC><b>Zabijanie</b> członków swojej drużyny grozi banem (wyjątkiem jest <color=orange>Klasa D</color>)</color>",
            "<color=#CCC>Jeżeli znajdziesz błąd z działaniem pluginu, zgłoś go na discordzie lub dla Gamer#5833</color>",
            //"<color=#CCC>Została otwarta rekrutacja na Moderatora SCP:SL, link do formularza znajduje się na ogłoszeniach na naszym discordzie (<color=#6B9ADF>discord.mistaken.pl</color>)</color>",
            "<color=#CCC>Regulamin serwera można znaleźć na naszej stronie (<color=#6B9ADF>mistaken.pl</color>) oraz na naszym discordzie (<color=#6B9ADF>discord.mistaken.pl</color>)</color>",
            "<color=#CCC>Zapraszamy do sprawdzenia oferty VIP na naszej stronie (<color=#6B9ADF>mistaken.pl/#scp-vip</color>)</color>",
            //"<color=#CCC>Administracja serwera Mistaken życzy wam wszystkim Wesołych Świąt i Szczęśliwego Nowego Roku!</color>",
            //"<color=#CCC>Serwer #4 od teraz jest serwerem RP 2 otwartym dla wszystkich! Obowiązują te same zasady co na serwerze #2</color>",
            "<color=#CCC><size=35>Na naszym serwerze zdobywasz <color=#6B9ADF>rangi EVO</color> podczas grania, możesz sprawdzić swoje rangi komendą <b>.evo get</b>, ustawić <b>.evo set</b>, a żeby zobaczyc wszystkie rangi do zdobycia <b>.evo info</b></size></color>",
            //"<color=#CCC><size=35>Wprowadzono pewne zmiany do regulaminów serwerów Mistaken - zapoznaj się z nimi na naszym discordzie (<color=#6B9ADF>discord.mistaken.pl</color>) lub stronie (<color=#6B9ADF>mistaken.pl</color>).</size></color>",
        });

        private static readonly List<string> LastUsed = new List<string>();

        private void Server_WaitingForPlayers()
        {
            if (Mistaken.PluginHandler.Config.IsHardRP())
                return;
            if (Gamer.Utilities.TranslationManagerSystem.TranslationManager.Language != "PL")
                return;
            LastUsed.Clear();
            CurrentId++;
            this.RunCoroutine(AutoMessage(CurrentId), "AutoMessage");
        }

        public int CurrentId = 0;

        private IEnumerator<float> AutoMessage(int id)
        {
            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(60, 300));
            while (CurrentId == id)
            {
                var tmpMessages = Messages.Where(item => !LastUsed.Contains(item)).ToArray();
                if (tmpMessages.Length == 0)
                    break;
                string message = tmpMessages[UnityEngine.Random.Range(0, tmpMessages.Length)];
                Map.Broadcast(20, message);
                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(120, 600));
            }
        }
    }
}
