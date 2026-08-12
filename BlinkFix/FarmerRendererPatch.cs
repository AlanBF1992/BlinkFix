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

        internal static readonly Vector2 doublePixelScale = new(8, 4);
        internal static readonly Vector2 rightEyeOffset = new(16, 0);
        internal static readonly Vector2 rightEyeExtraOffset = new(20, 0);
        internal static readonly Vector2 eyelidOffset = new(0, 4);
        internal static readonly Vector2 currentOffset1 = new(0, 8);
        internal static readonly Vector2 currentOffset4 = new(0, 4);
        internal static readonly Vector2 softFarmerOffset = new(0, 4);
        internal static readonly Vector2 lookOffsetLeft = new(-8, 0);
        internal static readonly Vector2 lookOffsetRight = new(4, 0);
        internal static readonly Vector2 pixelOffset = new(4, 0);
        internal static readonly Vector2 sittingOffsetLeft = new(16, 0);
        internal static readonly Vector2 sittingOffsetRight = new(-16, 0);
        internal static readonly Vector2 horseOffsetLeft = new(-8, 28);
        internal static readonly Vector2 horseOffsetRight = new(28, 0);
        internal static readonly Vector2 horseOffsetDown = new(16, 0);

        // Sex change
        internal static readonly Rectangle eyebrowSideRect = new(4, 9, 1, 1);
        internal static Rectangle eyelashRect { get; set; }
        internal static Rectangle skinShadowRect { get; set; }
        internal static Rectangle skinBaseRect { get; set; }

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
            int currentEyes = who.currentEyes;

            //If this is used someday
            if (currentEyes != 1 && currentEyes != 4)
            {
                b.Draw(baseTexture, eyePosition, new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, eyePosition, new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }

            bool lookingDown = who.FacingDirection == 2;

            if (who.IsMale)
            {
                if (!lookingDown)
                {
                    if (ModEntry.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    b.Draw(baseTexture, eyePosition, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition - eyelidOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    }
                }
                else
                {
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    b.Draw(baseTexture, eyePosition + eyelidOffset + rightEyeOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);

                        b.Draw(baseTexture, eyePosition + rightEyeOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    }
                }
            }
            else
            {
                Vector2 currentOffset = currentEyes == 1? currentOffset1: currentOffset4;

                if (!lookingDown)
                {
                    if (ModEntry.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    b.Draw(baseTexture, eyePosition - eyelidOffset + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition - eyelidOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    }
                }
                else
                {
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    b.Draw(baseTexture, eyePosition + currentOffset + rightEyeOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition + rightEyeOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                        b.Draw(baseTexture, eyePosition + eyelidOffset + rightEyeOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    }
                }
            }
        }

        internal static void drawGeneral(SpriteBatch b, Texture2D baseTexture, Vector2 eyePosition, int facingDirection, Farmer who, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth1, float layerDepth2)
        {
            int currentEyes = who.currentEyes;
            //If this is used someday
            if (currentEyes != 1 && currentEyes != 4)
            {
                var positionDif = new Vector2(0, ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44) - ((who.IsMale && who.FacingDirection != 2) ? 36 : 40));

                b.Draw(baseTexture, eyePosition, new Rectangle(5, 16, 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, eyePosition + positionDif, new Rectangle(264 + (facingDirection == 3 ? 4 : 0), 2 + (currentEyes - 1) * 2, 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }

            bool lookingDown = facingDirection == 2;

            Vector2 currentOffset = currentEyes == 1 ? currentOffset1 : currentOffset4;

            if (who.IsMale) {
                if (!lookingDown)
                {
                    eyePosition.Y -= 4; // Fixes for portrait

                    bool lookingLeft = facingDirection == 3;

                    if (ModEntry.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    if (who.IsSitting())
                    {
                        eyePosition += lookingLeft ? sittingOffsetLeft : sittingOffsetRight;
                    }

                    //Eyebrow
                    b.Draw(baseTexture, eyePosition - (lookingLeft ? lookOffsetLeft : lookOffsetRight), eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth1); // Changes when bald

                    if (who.isRidingHorse())
                    {
                        eyePosition += lookingLeft ? horseOffsetLeft : horseOffsetRight;
                    }

                    b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                }
                else
                {
                    if (who.FacingDirection == 2)
                    {
                        eyePosition.Y -= 4; // Fixes for portrait
                    }

                    if (!who.UsingTool)
                    {
                        //Eyebrow
                        b.Draw(baseTexture, eyePosition - pixelOffset, eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth1); // Changes when bald
                        b.Draw(baseTexture, eyePosition + pixelOffset + rightEyeExtraOffset, eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth1); // Changes when bald

                        if (who.isRidingHorse())
                        {
                            eyePosition += horseOffsetDown;
                        }

                        b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                        b.Draw(baseTexture, eyePosition + rightEyeOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    }

                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    b.Draw(baseTexture, eyePosition + eyelidOffset + rightEyeOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);

                    b.Draw(baseTexture, eyePosition + currentOffset + rightEyeOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                }
            }
            else
            {
                if (!lookingDown)
                {
                    eyePosition.Y -= 4; // Fixes for portrait

                    bool lookingLeft = facingDirection == 3;

                    if (ModEntry.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    if (who.IsSitting())
                    {
                        eyePosition += lookingLeft ? sittingOffsetLeft : sittingOffsetRight; // Maybe only when lookin not down
                    }
                    else if (who.isRidingHorse())
                    {
                        eyePosition += lookingLeft ? horseOffsetLeft : horseOffsetRight;
                    }

                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    }
                }
                else
                {
                    if (who.isRidingHorse())
                    {
                        eyePosition += horseOffsetDown;
                    }

                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    b.Draw(baseTexture, eyePosition + currentOffset + rightEyeOffset, eyelashRect, color, rotation, origin, doublePixelScale, effects, layerDepth2);
                    b.Draw(baseTexture, eyePosition + rightEyeOffset, skinShadowRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);

                    if (currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                        b.Draw(baseTexture, eyePosition + eyelidOffset + rightEyeOffset, skinBaseRect, color, rotation, origin, doublePixelScale, effects, layerDepth1);
                    }
                }
            }
        }
    }
}