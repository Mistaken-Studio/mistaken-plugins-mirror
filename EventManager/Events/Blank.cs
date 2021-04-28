using System.Collections.Generic;

namespace Gamer.EventManager.Events
{
    internal class Blank :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "blank";

        public override string Description { get; set; } = "Blank event";

        public override string Name { get; set; } = "Blank";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
        }

        public override void OnDeIni()
        {
        }

        public override void Register()
        {
        }
    }
}
