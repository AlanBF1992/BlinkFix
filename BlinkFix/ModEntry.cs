using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

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

            VanillaPatches(new Harmony(ModManifest.UniqueID));

            helper.Events.GameLoop.GameLaunched += checkSoftFarmer;
            helper.Events.GameLoop.SaveLoaded += assignFarmerSex;
            helper.Events.Display.MenuChanged += reassignFarmerSex;
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        internal static void VanillaPatches(Harmony harmony)
        {
            // Changes the way the eyes are drawn in Vanilla
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), [typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer)]),
                transpiler: new HarmonyMethod(typeof(FarmerRendererPatch), nameof(FarmerRendererPatch.drawTranspiler))
            );
        }

        /**********
         * EVENTS *
         **********/
        private static void checkSoftFarmer(object? sender, GameLaunchedEventArgs e)
        {
            FarmerRendererPatch.IsSoftFarmerLoaded = ModHelper.ModRegistry.IsLoaded("Crisaius.SoftFarmer");
        }

        private static void assignFarmerSex(object? sender, SaveLoadedEventArgs e)
        {
            SetSex(Game1.player.IsMale);
        }

        private static void reassignFarmerSex(object? sender, MenuChangedEventArgs e)
        {
            if (e.OldMenu is CharacterCustomization)
            {
                SetSex(Game1.player.IsMale);
            }
        }

        /***********
         * HELPERS *
         ***********/
        /// <summary>Applies the farmer sex-specific sprite-sheet rects.</summary>
        /// <param name="IsMale">Whether the farmer is male.</param>
        private static void SetSex(bool IsMale)
        {
            if (IsMale)
            {
                FarmerRendererPatch.eyelashSingleRect = new(5, 10, 2, 1);
                FarmerRendererPatch.eyelashFullRect = new(5, 10, 6, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(264, 2, 2, 1);
                FarmerRendererPatch.skinShadowFullRect = new(264, 2, 6, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(264, 3, 2, 1);
                FarmerRendererPatch.skinBaseFullRect = new(264, 3, 6, 1);
            }
            else
            {
                FarmerRendererPatch.eyelashSingleRect = new(5, 11, 2, 1);
                FarmerRendererPatch.eyelashFullRect = new(5, 11, 6, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(264, 3, 2, 1);
                FarmerRendererPatch.skinShadowFullRect = new(264, 3, 6, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(264, 2, 2, 1);
                FarmerRendererPatch.skinBaseFullRect = new(264, 2, 6, 1);
            }
        }
    }
}
