using BlinkFix.Compatibility.FashionSense.Patches;
using HarmonyLib;
using StardewModdingAPI;

namespace BlinkFix.Compatibility.FashionSense
{
    internal static class FashionSenseLoader
    {
        internal static void Loader(IModHelper _, Harmony harmony)
        {
            FashionSensePatches(harmony);
        }

        private static void FashionSensePatches(Harmony harmony)
        {
            // Changes the way the eyes are drawn in FS for Vanilla Player Layers
            harmony.Patch(
                original: AccessTools.Method("FashionSense.Framework.Managers.DrawManager:DrawPlayerVanilla"),
                transpiler: new HarmonyMethod(typeof(DrawManagerPatch), nameof(DrawManagerPatch.DrawPlayerVanillaTranspiler))
            );

            // Changes the way the eyes are drawn in FS for Custom Player Layers
            harmony.Patch(
                original: AccessTools.Method("FashionSense.Framework.Managers.DrawManager:DrawPlayerCustom"),
                transpiler: new HarmonyMethod(typeof(DrawManagerPatch), nameof(DrawManagerPatch.DrawPlayerCustomTranspiler))
            );
        }
    }
}
