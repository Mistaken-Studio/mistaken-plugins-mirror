using Exiled.API.Features;
using Gamer.API;

namespace Xname.CE
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<CEConfig>
    {
        /// <inheritdoc/>
        public static new CEConfig Config;
        /// <inheritdoc/>
        public override string Author => "Xname";
        /// <inheritdoc/>
        public override string Name => "CombatExtended";
        /// <inheritdoc/>
        public override string Prefix => "ce";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Config = base.Config;
            new CEHandler(this);

            Gamer.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }
        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Gamer.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
    /// <inheritdoc/>
    public class CEConfig : Config
    {
        /// <inheritdoc/>
        [System.ComponentModel.Description("Value on which player wakes up")]
        public int normalBpm { get; set; } = 60;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Max value to be picked for unconscious player")]
        public int maxBpmOnShot { get; set; } = 50;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Min value to be picked for unconscious player")]
        public int minBpmOnShot { get; set; } = 30;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Chance of getting unconscious on taking damage. Value from 0 to 99")]
        public int unconsciousChance { get; set; } = 10;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Bpm change frequency value for unconscious player. Value in seconds")]
        public int wakeUpRate { get; set; } = 20;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Max bpm value change for unconscious player")]
        public int maxBpmOnUpdate { get; set; } = 12;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Min bpm value change for unconscious player")]
        public int minBpmOnUpdate { get; set; } = 8;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Max hp value when the player wakes up")]
        public int maxHpOnWakeUp { get; set; } = 35;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Min hp value when the player wakes up")]
        public int minHpOnWakeUp { get; set; } = 20;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Durration of the effect Blinded in seconds. Value 0 means no effect")]
        public int blindedTime { get; set; } = 10;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Durration of the effect Concussed in seconds. Value 0 means no effect")]
        public int concussedTime { get; set; } = 20;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Durration of the effect Deafened in seconds. Value 0 means no effect")]
        public int deafenedTime { get; set; } = 20;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Durration of the effect Exhausted in seconds. Value 0 means no effect")]
        public int exhaustedTime { get; set; } = 30;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Getting unconscious by only headshots. Works only if unconsciousWeaponOnly is set to true")]
        public bool unconsciousHeadShotOnly { get; set; } = true;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Getting unconscious by only weapons")]
        public bool unconsciousWeaponOnly { get; set; } = true;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Not being able to move when unconscious")]
        public bool unconsciousEnsnare { get; set; } = true;
        /// <inheritdoc/>
        [System.ComponentModel.Description("Reason of death on ragdolls of unconscious player")]
        public string reasonOnUnconsciousRagdoll { get; set; } = "Osoba wydaje się oddychać normalnie, ale jej tętno jest za niskie by była przytomna";
        /// <inheritdoc/>
        [System.ComponentModel.Description("Reason of death on ragdolls of unconscious player")]
        public string reasonOnDeadRagdoll { get; set; } = "Brak oznak funkcji życiowych";
    }
}
