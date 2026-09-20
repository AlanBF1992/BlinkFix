using HarmonyLib;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace BlinkFix.Compatibility.FashionSense.Patches
{
    internal static class DrawManagerPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> DrawPlayerVanillaTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo drawSwimmingInfo = AccessTools.Method(typeof(FarmerRendererPatch), nameof(FarmerRendererPatch.drawSwimming));
                MethodInfo drawGeneralInfo = AccessTools.Method(typeof(FarmerRendererPatch), nameof(FarmerRendererPatch.drawGeneral));

                CodeMatcher matcher = new(instructions);

                //From:  DrawTool.SpriteBatch.Draw(DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(AppearanceHelpers.GetFarmerRendererXFeatureOffset(DrawTool.CurrentFrame) * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + 40), new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                //       DrawTool.SpriteBatch.Draw(DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(AppearanceHelpers.GetFarmerRendererXFeatureOffset(DrawTool.CurrentFrame) * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + 40), new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                // To:   drawSwimming(DrawTool.SpriteBatch, DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(AppearanceHelpers.GetFarmerRendererXFeatureOffset(DrawTool.CurrentFrame) * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + 40)  who, DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth(), IncrementAndGetLayerDepth());

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_5),
                        new CodeMatch(OpCodes.Ldc_I4_S)
                    )
                    .ThrowIfNotMatch("DrawManagerPatch.DrawPlayerVanillaTranspiler: IL code 1 not found")
                    .RemoveInstructions(2)
                    .Advance(1)
                    .RemoveInstructions(9)
                    .Advance(15)
                    .RemoveInstructions(93)
                    .Advance(2)
                    .SetInstruction(new CodeInstruction(OpCodes.Call, drawSwimmingInfo))
                ;

                // From: DrawTool.SpriteBatch.Draw(DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(x_adjustment, AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40)), new Rectangle(5, 16, (DrawTool.FacingDirection == 2) ? 6 : 2, 2), DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                //       DrawTool.SpriteBatch.Draw(DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(x_adjustment, AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44)), new Rectangle(264 + ((DrawTool.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (DrawTool.FacingDirection == 2) ? 6 : 2, 2), DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                // To:   drawGeneral(DrawTool.SpriteBatch, DrawTool.BaseTexture, DrawTool.Position + DrawTool.Origin + DrawTool.PositionOffset + new Vector2(x_adjustment, AppearanceHelpers.GetFarmerRendererYFeatureOffset(DrawTool.CurrentFrame) * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40)), DrawTool.FacingDirection, who, DrawTool.OverrideColor, 0f, DrawTool.Origin, 4f * DrawTool.Scale, SpriteEffects.None, IncrementAndGetLayerDepth(), IncrementAndGetLayerDepth());
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_5),
                        new CodeMatch(OpCodes.Ldc_I4_S)
                    )
                    .ThrowIfNotMatch("DrawManagerPatch.DrawPlayerVanillaTranspiler: IL code 2 not found")
                    .RemoveInstructions(2)
                    .Advance(3)
                    .RemoveInstructions(8)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1)
                    )
                    .Advance(15)
                    .RemoveInstructions(84)
                    .Advance(2)
                    .SetInstruction(new CodeInstruction(OpCodes.Call, drawGeneralInfo))
                    ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(DrawPlayerVanillaTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> DrawPlayerCustomTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }
    }
}
