using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Reflection;

namespace BlinkFix.Compatibility.SoftFarmer
{
    internal static class SoftFarmerLoader
    {
        private static IContentPack softFarmerPack { get; set; } = null!;
        private static IContentPack softFarmerMythicPack { get; set; } = null!;
        private static Func<bool> isSFEnabled { get; set; } = null!;
        private static Func<bool> isSFMTEnabled { get; set; } = () => false;

        internal static void Loader(IModHelper helper, Harmony _)
        {
            helper.Events.GameLoop.SaveLoaded -= VanillaLoader.assignFarmerSex;
            helper.Events.Display.MenuChanged -= VanillaLoader.reassignFarmerSex;

            helper.Events.GameLoop.GameLaunched += getPackInfo;
            helper.Events.GameLoop.SaveLoaded += setOffsetsAndSex;
            helper.Events.Display.MenuChanged += reSetOffsetsAndSex;
        }

        /**********
         * EVENTS *
         **********/
        private static void getPackInfo(object? sender, GameLaunchedEventArgs e)
        {
            object? SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!
                .GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            object ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;

            object softFarmerMod = AccessTools.Method(ModRegistry.GetType(), "Get")?.Invoke(ModRegistry, ["Crisaius.SoftFarmer"])!;
            softFarmerPack = (IContentPack)AccessTools.Property(softFarmerMod.GetType(), "ContentPack")?.GetValue(softFarmerMod)!;

            isSFEnabled = () => (bool)softFarmerPack.ReadJsonFile<JObject>("config.json")!.GetValue("Enable Soft Farmer Mod")!;

            if (ModEntry.ModHelper.ModRegistry.IsLoaded("Crisaius.SoftFarmer.MythicTraits"))
            {
                object softFarmerMythicMod = AccessTools.Method(ModRegistry.GetType(), "Get")?.Invoke(ModRegistry, ["Crisaius.SoftFarmer.MythicTraits"])!;
                softFarmerMythicPack = (IContentPack)AccessTools.Property(softFarmerMythicMod.GetType(), "ContentPack")?.GetValue(softFarmerMythicMod)!;

                isSFMTEnabled = () => (bool)softFarmerMythicPack.ReadJsonFile<JObject>("config.json")!.GetValue("Enable Mythic Traits")!;
            }
        }

        private static void setOffsetsAndSex(object? sender, SaveLoadedEventArgs e)
        {
            if (!isSFEnabled())
            {
                applyVanillaOffsets();
                VanillaLoader.SetSex(Game1.player.IsMale);
            }
            else if (!isSFMTEnabled())
            {
                applySFOffsets();
                VanillaLoader.SetSex(Game1.player.IsMale);
            }
            else
            {
                applySFMTOffsets();
                setMTSex(Game1.player.IsMale);
            }
        }

        private static void reSetOffsetsAndSex(object? sender, MenuChangedEventArgs e)
        {
            // Check how to do it when GMCM is closed. And CharacterCustomization for sex changes
            if (e.NewMenu is not null) return;

            if (!isSFEnabled())
            {
                applyVanillaOffsets();
                VanillaLoader.SetSex(Game1.player.IsMale);
            }
            else if (!isSFMTEnabled())
            {
                applySFOffsets();
                VanillaLoader.SetSex(Game1.player.IsMale);
            }
            else
            {
                applySFMTOffsets();
                setMTSex(Game1.player.IsMale);
            }
        }

        /***********
         * HELPERS *
         ***********/
        /// <summary>Applies the farmer eyePosition offsets changes for Vanilla.</summary>
        private static void applyVanillaOffsets()
        {
            FarmerRendererPatch.VerticalOffset = Vector2.Zero;
            FarmerRendererPatch.LookLeftOffset = Vector2.Zero;
            FarmerRendererPatch.LookDownOffset = Vector2.Zero;
        }

        /// <summary>Applies the farmer eyePosition offsets changes for Soft Farmer.</summary>
        private static void applySFOffsets()
        {
            FarmerRendererPatch.VerticalOffset = new Vector2(0, 4);
            FarmerRendererPatch.LookLeftOffset = Vector2.Zero;
            FarmerRendererPatch.LookDownOffset = Vector2.Zero;
        }

        /// <summary>Applies the farmer eyePosition offsets changes for Soft Farmer - Mythic Traits.</summary>
        private static void applySFMTOffsets()
        {
            FarmerRendererPatch.VerticalOffset = new Vector2(0, 4);
            FarmerRendererPatch.LookLeftOffset = new Vector2(-4, 0);
            FarmerRendererPatch.LookDownOffset = new Vector2(4, 0);
        }

        /// <summary>Applies the farmer sex-specific sprite-sheet rects.</summary>
        /// <param name="IsMale">Whether the farmer is male.</param>
        private static void setMTSex(bool IsMale)
        {
            if (IsMale)
            {
                FarmerRendererPatch.eyebrowSideRect = new(4, 9, 1, 1);

                FarmerRendererPatch.eyelashFullRect = new(6, 10, 4, 1);
                FarmerRendererPatch.skinShadowFullRect = new(264, 2, 6, 1);
                FarmerRendererPatch.skinBaseFullRect = new(264, 3, 6, 1);

                FarmerRendererPatch.eyelashSingleRect = new(6, 10, 3, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(265, 2, 3, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(265, 3, 3, 1);
            }
            else
            {
                FarmerRendererPatch.eyebrowSideRect = new(4, 9, 1, 1);

                FarmerRendererPatch.eyelashFullRect = new(6, 11, 4, 1);
                FarmerRendererPatch.skinShadowFullRect = new(264, 3, 6, 1);
                FarmerRendererPatch.skinBaseFullRect = new(264, 2, 6, 1);

                FarmerRendererPatch.eyelashSingleRect = new(6, 11, 3, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(265, 3, 3, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(265, 2, 3, 1);
            }
        }
    }
}
