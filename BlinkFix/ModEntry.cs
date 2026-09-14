using BlinkFix.Compatibility.SoftFarmer;
using HarmonyLib;
using StardewModdingAPI;

namespace BlinkFix
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IMonitor LogMonitor { get; internal set; } = null!;
        /// <summary>API access for mod events, assets, and reflection.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;

        /*****************
        * Public methods *
        ******************/
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = helper;

            Harmony harmony = new(ModManifest.UniqueID);

            VanillaLoader.Loader(helper, harmony);

            if (ModHelper.ModRegistry.IsLoaded("Crisaius.SoftFarmer"))
            {
                SoftFarmerLoader.Loader(helper, harmony);
            }
        }
    }
}
