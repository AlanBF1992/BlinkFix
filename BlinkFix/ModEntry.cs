using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace BlinkFix
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IMonitor LogMonitor { get; internal set; } = null!;
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;

        /******************
        ** Public methods *
        *******************/
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = helper;

            VanillaPatches(new Harmony(ModManifest.UniqueID));
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        internal static void VanillaPatches(Harmony harmony)
        {
            // Cambia la forma en la que se calcula el nivel en Vanilla
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), [typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer)]),
                transpiler: new HarmonyMethod(typeof(FarmerRendererPatch), nameof(FarmerRendererPatch.drawTranspiler))
            );
        }
    }
}
