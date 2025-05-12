using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;
using static StardewValley.FarmerRenderer;

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
            //If this is used someday
            if (who.currentEyes != 1 && who.currentEyes != 4)
            {
                b.Draw(baseTexture, eyePosition, new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, eyePosition, new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }
            if (who.IsMale)
            {
                b.Draw(baseTexture, eyePosition + new Vector2(0, who.FacingDirection == 2? 4: 0), new Rectangle(5, 16, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                if(who.currentEyes == 1)
                {
                    b.Draw(baseTexture, eyePosition - new Vector2(0, who.FacingDirection != 2 ? 4 : 0), new Rectangle(264 + (who.FacingDirection == 3 ? 4 : 0), 2, who.FacingDirection == 2 ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                }
            }
            else
            {
                var sideOffset = new Vector2(0, who.FacingDirection != 2 ? 4 : 0);
                var currentOffset = new Vector2(0, (-4 * who.currentEyes + 28) / 3); //f(4) = 8, f(1) = 4

                //Eyelashes
                b.Draw(baseTexture, eyePosition + currentOffset - sideOffset, new Rectangle(5, 11, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth2);
                //Eyebrow
                b.Draw(baseTexture, eyePosition - sideOffset, new Rectangle(5, 16, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                //Eyelid
                if (who.currentEyes == 1)
                {
                    b.Draw(baseTexture, eyePosition - sideOffset + new Vector2(0,4), new Rectangle(5, 17, who.FacingDirection == 2 ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                }
            }
        }

        internal static void drawGeneral(SpriteBatch b, Texture2D baseTexture, Vector2 position, int facingDirection, Farmer who, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth1, float layerDepth2)
        {
            bool lookingDown = facingDirection == 2;
            bool lookingLeft = facingDirection == 3;

            int currentEyes = who.currentEyes;

            //If this is used someday
            if (currentEyes != 1 && currentEyes != 4)
            {
                var positionDif = new Vector2(0, ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44) - ((who.IsMale && who.FacingDirection != 2) ? 36 : 40));

                b.Draw(baseTexture, position, new Rectangle(5, 16, lookingDown ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, position + positionDif, new Rectangle(264 + (lookingLeft ? 4 : 0), 2 + (currentEyes - 1) * 2, lookingDown ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }
            if (who.IsMale) {
                var portraitOffset = position - new Vector2(0, (who.FacingDirection != 2 && lookingDown) ? 0 : 4);
                var currentOffset = new Vector2(0, (-4 * currentEyes + 28) / 3); //f(4) = 8, f(1) = 4
                var leftLookOffset = new Vector2(lookingLeft ? 0 : 4, 0);

                if(!who.UsingTool || (who.UsingTool && !lookingDown))
                {
                    //Eyebrow
                    b.Draw(baseTexture, portraitOffset - leftLookOffset, new Rectangle(4, 9, lookingDown ? 8 : 3, 1), color, rotation, origin, scale, effects, layerDepth1);
                    b.Draw(baseTexture, portraitOffset, new Rectangle(5, 15, lookingDown ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth2);
                }
                //Eyelid
                b.Draw(baseTexture, portraitOffset - leftLookOffset + new Vector2(0, 4), new Rectangle(4 + (lookingLeft ? 5 : 0), 16, lookingDown ? 8 : 3, 1), color, rotation, origin, scale, effects, layerDepth1);
                //Eyelashes
                if (currentEyes == 4)
                {
                    if (!who.UsingTool)
                    {
                        b.Draw(baseTexture, portraitOffset + currentOffset - leftLookOffset, new Rectangle(4, 10, (lookingDown ? 8 : 3), 1), color, rotation, origin, scale, effects, layerDepth2);
                    } else
                    {
                        b.Draw(baseTexture, portraitOffset + currentOffset - leftLookOffset + new Vector2(lookingDown ? 4 : 0, 0), new Rectangle(lookingDown ? 5 : 4, 10, (lookingDown ? 6 : 3), 1), color, rotation, origin, scale, effects, layerDepth2);
                    }
                }
                else if (currentEyes == 1)
                {
                    b.Draw(baseTexture, portraitOffset + currentOffset, new Rectangle(5, 10, (lookingDown ? 6 : 2), 1), color, rotation, origin, scale, effects, layerDepth2);
                }
            }
            else
            {
                var portraitOffset = position - new Vector2(0, !lookingDown ? 4 : 0);
                var currentOffset = new Vector2(0, (-4 * currentEyes + 28) / 3) ;

                //Eyelashes
                b.Draw(baseTexture, portraitOffset + currentOffset, new Rectangle(5, 11, lookingDown ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth2);
                //Eyebrow
                b.Draw(baseTexture, portraitOffset, new Rectangle(5, 16, lookingDown ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                //Eyelid
                if (currentEyes == 1)
                {
                    b.Draw(baseTexture, portraitOffset + new Vector2(0,4), new Rectangle(5, 17, lookingDown ? 6 : 2, 1), color, rotation, origin, scale, effects, layerDepth1);
                }
            }
        }
    }
}