using ModSettings;

namespace DisableDoors
{
    internal class DisableDoorsSettings : JsonModSettings
    {
        internal static DisableDoorsSettings Instance;
        [Name("Disable Doors")]
        [Description("Enables the mod")]
        public bool DisableDoors = true;
    }
}