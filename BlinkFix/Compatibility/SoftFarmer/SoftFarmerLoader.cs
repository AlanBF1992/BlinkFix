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
            helper.Events.GameLoop.GameLaunched += getPackInfo;
            helper.Events.GameLoop.SaveLoaded += setOffsets;
            helper.Events.Display.MenuChanged += checkConfigOnMenuClosed;
        }

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

                isSFMTEnabled = () =>
                {
                    return (bool)softFarmerPack.ReadJsonFile<JObject>("config.json")!.GetValue("Enable Soft Farmer Mod")!
                        && (bool)softFarmerMythicPack.ReadJsonFile<JObject>("config.json")!.GetValue("Enable Mythic Traits")!;
                };
            }
        }

        private static void setOffsets(object? sender, SaveLoadedEventArgs e)
        {
            FarmerRendererPatch.VerticalOffset = (isSFEnabled()) ? new Vector2(0, 4) : Vector2.Zero;
            FarmerRendererPatch.LookLeftOffset = (isSFMTEnabled()) ? new Vector2(-4, 0) : Vector2.Zero;
            FarmerRendererPatch.LookDownOffset = (isSFMTEnabled()) ? new Vector2(4, 0) : Vector2.Zero;
        }

        private static void checkConfigOnMenuClosed(object? sender, MenuChangedEventArgs e)
        {
            // Check how to do it only when GMCM is closed. Or a config is modified
            if (e.NewMenu is not null) return;

            if (!isSFEnabled())
            {
                FarmerRendererPatch.VerticalOffset = Vector2.Zero;
                FarmerRendererPatch.LookLeftOffset = Vector2.Zero;
                FarmerRendererPatch.LookDownOffset = Vector2.Zero;

                VanillaLoader.SetSex(Game1.player.IsMale);
                return;
            }

            FarmerRendererPatch.VerticalOffset = new Vector2(0, 4);

            if (!isSFMTEnabled())
            {
                FarmerRendererPatch.LookLeftOffset = Vector2.Zero;
                FarmerRendererPatch.LookDownOffset = Vector2.Zero;

                VanillaLoader.SetSex(Game1.player.IsMale);
                return;
            }

            FarmerRendererPatch.LookLeftOffset = new Vector2(-4, 0);
            FarmerRendererPatch.LookDownOffset = new Vector2(4, 0);
            setMTSex(Game1.player.IsMale);
        }

        private static void setMTSex(bool isMale)
        {
            if (isMale)
            {
                FarmerRendererPatch.eyelashFullRect = new(6, 10, 4, 1);
                FarmerRendererPatch.eyelashSingleRect = new(6, 10, 3, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(265, 2, 3, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(265, 3, 3, 1);
            }
            else
            {
                FarmerRendererPatch.eyelashFullRect = new(6, 11, 4, 1);
                FarmerRendererPatch.eyelashSingleRect = new(6, 11, 3, 1);
                FarmerRendererPatch.skinShadowSingleRect = new(265, 3, 3, 1);
                FarmerRendererPatch.skinBaseSingleRect = new(265, 2, 3, 1);
            }
        }
    }
}
