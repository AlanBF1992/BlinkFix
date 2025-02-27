using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace BlinkFix
{
    internal static class FarmerRendererPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> drawTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo drawSwimmingInfo = AccessTools.Method(typeof(FarmerRendererPatch), nameof(drawSwimming));
                MethodInfo drawGeneralInfo = AccessTools.Method(typeof(FarmerRendererPatch), nameof(drawGeneral));

                CodeMatcher matcher = new(instructions);

                // from: The swimming one
                // to:   Call a function
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_1),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_5)
                    )
                    .ThrowIfNotMatch("FarmerRendererPatch.drawTranspiler: IL code 1 not found")
                    .Advance(4)
                    .RemoveInstructions(2)
                    .Advance(1)
                    .RemoveInstructions(9)
                    .Advance(9)
                    .RemoveInstructions(37)
                    .Advance(4)
                    .SetInstruction(new CodeInstruction(OpCodes.Call, drawSwimmingInfo))
                ;

                // from: The normal one (and portrait)
                // to:   Call another function
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_1),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Ldarg_S)
                    )
                    .ThrowIfNotMatch("FarmerRendererPatch.drawTranspiler: IL code 2 not found")
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_5)
                    )
                    .RemoveInstructions(11)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 8),
                        new CodeInstruction(OpCodes.Ldarg_S, 12)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt)
                    )
                    .RemoveInstructions(60)
                    .Advance(4)
                    .SetInstruction(new CodeInstruction(OpCodes.Call, drawGeneralInfo))
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(drawTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static void drawSwimming(SpriteBatch b, Texture2D baseTexture, Vector2 eyePosition, Farmer who, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth1, float layerDepth2)
        {
            if(who.IsMale)
            {
                b.Draw(baseTexture, eyePosition + new Vector2(0, who.FacingDirection == 2? 4: 0), new Rectangle(5, 16, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                if(who.currentEyes == 1)
                {
                    b.Draw(baseTexture, eyePosition - new Vector2(0, who.FacingDirection != 2 ? 4 : 0), new Rectangle(264 + (who.FacingDirection == 3 ? 4 : 0), 2, who.FacingDirection == 2 ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                }
            }
            else
            {
                var sideOffset = new Vector2(0, (-4 * who.currentEyes + 28) / 3) - new Vector2(0, who.FacingDirection != 2 ? 4 : 0);
                //Eyelashes
                b.Draw(baseTexture, eyePosition + sideOffset, new Rectangle(5, 11, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth2);
                //Eyelids
                if(who.currentEyes == 4)
                {
                    b.Draw(baseTexture, eyePosition + sideOffset - new Vector2(0,4), new Rectangle(264, 3, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                }
                if (who.currentEyes == 1)
                {
                    b.Draw(baseTexture, eyePosition + sideOffset - new Vector2(0, 8), new Rectangle(264, 2, who.FacingDirection == 2 ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                }
            }
        }

        internal static void drawGeneral(SpriteBatch b, Texture2D baseTexture, Vector2 position, int facingDirection, Farmer who, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth1, float layerDepth2)
        {
            if (who.IsMale) {
                var sideOffset = new Vector2(0, (who.FacingDirection != 2 && facingDirection == 2) ? 0 : 4) + new Vector2(facingDirection == 3 ? 0 : 4, 0);
                var currentOffset = new Vector2(0, (-4 * who.currentEyes + 28) / 3); //f(4)=8, f(1)=4

                //Eyelashes
                b.Draw(baseTexture, position + currentOffset - sideOffset, new Rectangle(4, 10, facingDirection == 2 ? 8 : 3, 1), color, rotation, origin, scale, effects, layerDepth2);
                //EyelidTop
                b.Draw(baseTexture, position - sideOffset, new Rectangle(4, 17, facingDirection == 2 ? 8 : 3, 1), color, rotation, origin, scale, effects, layerDepth1);
                //EyelidBot
                if (who.currentEyes == 1)
                {
                    b.Draw(baseTexture, position - sideOffset + new Vector2(0,4), new Rectangle(4 + (facingDirection == 3 ? 5 : 0), 15, facingDirection == 2 ? 8 : 3, 1), color, rotation, origin, scale, effects, layerDepth1);
                }
            }
            else
            {
                var sideOffset = new Vector2(0, facingDirection != 2 ? 4 : 0);
                var currentOffset = new Vector2(0, (-4 * who.currentEyes + 28) / 3) ;
                //Eyelashes
                b.Draw(baseTexture, position + currentOffset - sideOffset, new Rectangle(5, 11, facingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth2);
                //Eyelids
                if (who.currentEyes == 4)
                {
                    b.Draw(baseTexture, position - sideOffset, new Rectangle(264, 3, facingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                }
                if (who.currentEyes == 1)
                {
                    b.Draw(baseTexture, position - sideOffset, new Rectangle(264, 2, facingDirection == 2 ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                }
            }
        }
    }
}