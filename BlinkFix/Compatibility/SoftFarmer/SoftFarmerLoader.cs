using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using System.Reflection;

namespace BlinkFix.Compatibility.SoftFarmer
{
    internal static class SoftFarmerLoader
    {
        private static IContentPack softFarmerPack { get; set; } = null!;
        private static Func<bool> isEnabled { get; set; } = null!;

        internal static void Loader(IModHelper helper, Harmony _)
        {
            helper.Events.GameLoop.GameLaunched += getPackInfo;
            helper.Events.GameLoop.SaveLoaded += setOffsets;
            helper.Events.Display.MenuChanged += checkConfigOnMenuClosed;
        }

        private static void setOffsets(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            FarmerRendererPatch.LateralOffsets = (isEnabled()) ? new Vector2(0, 4) : new Vector2(0, 0);
        }

        private static void getPackInfo(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            object? SCore = typeof(Mod).Assembly.GetType("StardewModdingAPI.Framework.SCore")!.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
            object ModRegistry = AccessTools.Field(SCore!.GetType(), "ModRegistry")?.GetValue(SCore)!;
            object mod = AccessTools.Method(ModRegistry.GetType(), "Get")?.Invoke(ModRegistry, ["Crisaius.SoftFarmer"])!;

            softFarmerPack = (IContentPack)AccessTools.Property(mod.GetType(), "ContentPack")?.GetValue(mod)!;

            isEnabled = () => (bool)softFarmerPack.ReadJsonFile<JObject>("config.json")!.GetValue("Enable Soft Farmer Mod")!;
        }

        private static void checkConfigOnMenuClosed(object? sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.NewMenu is not null) return;
            FarmerRendererPatch.LateralOffsets = (isEnabled()) ? new Vector2(0, 4) : Vector2.Zero;
        }
    }
}
