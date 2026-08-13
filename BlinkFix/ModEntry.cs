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
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;
        public static bool IsSoftFarmerLoaded { get; set; } = false;
        public static Func<bool> IsHorseThin { get; set; } = () => false;

        /******************
        ** Public methods *
        *******************/
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = helper;

            VanillaPatches(new Harmony(ModManifest.UniqueID));

            helper.Events.GameLoop.GameLaunched += checkSoftFarmer;
            helper.Events.GameLoop.GameLaunched += setHorseWeightChecker;
            helper.Events.GameLoop.SaveLoaded += checkHorseWeight;
            helper.Events.GameLoop.SaveLoaded += assignFarmerSex;
            helper.Events.Display.MenuChanged += reassignFarmerSex;
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

        /**********
         * EVENTS *
         **********/

        private static void checkSoftFarmer(object? sender, GameLaunchedEventArgs e)
        {
            IsSoftFarmerLoaded = ModHelper.ModRegistry.IsLoaded("Crisaius.SoftFarmer");
        }

        private static void setHorseWeightChecker(object? sender, GameLaunchedEventArgs e)
        {
            if (ModHelper.ModRegistry.IsLoaded("Goldenrevolver.HorseOverhaul"))
            {
                // Hacer
                IsHorseThin = () => (bool)((dynamic)AccessTools.Field("HorseOverhaul.Patches.ThinHorseDrawPatches:mod").GetValue(null)!).Config.ThinHorse;
            }
        }

        private static void checkHorseWeight(object? sender, SaveLoadedEventArgs e)
        {
            if (IsHorseThin())
            {
                FarmerRendererPatch.horseOffsetLeft = new(-8, 0);
                FarmerRendererPatch.horseOffsetRight = new(28, 0);
                FarmerRendererPatch.horseOffsetDown = new(16, 0);
            }
            else
            {
                FarmerRendererPatch.horseOffsetLeft = new(-48, 0);
                FarmerRendererPatch.horseOffsetRight = new(-16, 0);
                FarmerRendererPatch.horseOffsetDown = new(-24, 0);
            }
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
        private static void SetSex(bool IsMale)
        {
            if (IsMale)
            {
                FarmerRendererPatch.eyelashRect = new(5, 10, 1, 1);
                FarmerRendererPatch.skinShadowRect = new(264, 2, 1, 1);
                FarmerRendererPatch.skinBaseRect = new(264, 3, 1, 1);
            }
            else
            {
                FarmerRendererPatch.eyelashRect = new(5, 11, 1, 1);
                FarmerRendererPatch.skinShadowRect = new(264, 3, 1, 1);
                FarmerRendererPatch.skinBaseRect = new(264, 2, 1, 1);
            }
        }
    }
}
