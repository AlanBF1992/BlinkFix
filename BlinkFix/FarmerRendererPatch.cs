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

        // Eye state offsets
        internal readonly static Vector2 currentOffset1 = new(0, 8);
        internal readonly static Vector2 currentOffset4 = new(0, 4);

        // Eyes offsets
        internal readonly static Vector2 eyelidOffset = new(0, 4);
        internal readonly static Vector2 eyebrowSideLeftOffset = new(-4, 0);
        internal readonly static Vector2 eyebrowSideRightOffset = new(8, 0);
        internal readonly static Vector2 eyebrowFullRightOffset = new(24, 0);

        // Sex changes
        internal static readonly Rectangle eyebrowSideRect = new(4, 9, 1, 1);
        internal static Rectangle eyelashSingleRect { get; set; }
        internal static Rectangle eyelashFullRect { get; set; }
        internal static Rectangle skinShadowSingleRect { get; set; }
        internal static Rectangle skinShadowFullRect { get; set; }
        internal static Rectangle skinBaseSingleRect { get; set; }
        internal static Rectangle skinBaseFullRect { get; set; }

        // Mod compats
        internal static bool IsSoftFarmerLoaded { get; set; } = false;
        internal static readonly Vector2 softFarmerOffset = new(0, 4);

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
            var currentEyes = who.currentEyes;
            //If this is used someday
            if (currentEyes != 1 && currentEyes != 4)
            {
                b.Draw(baseTexture, eyePosition, new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, eyePosition, new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), who.currentEyes * 2, (who.FacingDirection == 2) ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }

            bool lookingDown = who.FacingDirection == 2;

            if (who.IsMale)
            {
                if (!lookingDown)
                {
                    if (FarmerRendererPatch.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    b.Draw(baseTexture, eyePosition, skinBaseSingleRect, color, rotation, origin, scale, effects, layerDepth1);

                    if (who.currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition - eyelidOffset, skinShadowSingleRect, color, rotation, origin, scale, effects, layerDepth2);
                    }
                }
                else
                {
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseFullRect, color, rotation, origin, scale, effects, layerDepth1);

                    if (who.currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition, skinShadowFullRect, color, rotation, origin, scale, effects, layerDepth2);
                    }
                }
            }
            else
            {
                var currentOffset = currentEyes == 1 ? currentOffset1 : currentOffset4;

                if (!lookingDown)
                {
                    if (FarmerRendererPatch.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    //Eyelashes
                    b.Draw(baseTexture, eyePosition - eyelidOffset + currentOffset, eyelashSingleRect, color, rotation, origin, scale, effects, layerDepth2);
                    //Eyebrow
                    b.Draw(baseTexture, eyePosition - eyelidOffset, skinShadowSingleRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelid
                    if (who.currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition, skinBaseSingleRect, color, rotation, origin, scale, effects, layerDepth1);
                    }
                }
                else
                {
                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashFullRect, color, rotation, origin, scale, effects, layerDepth2);
                    //Eyebrow
                    b.Draw(baseTexture, eyePosition, skinShadowFullRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelid
                    if (who.currentEyes == 1)
                    {
                        b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseFullRect, color, rotation, origin, scale, effects, layerDepth1);
                    }
                }
            }
        }

        internal static void drawGeneral(SpriteBatch b, Texture2D baseTexture, Vector2 eyePosition, int facingDirection, Farmer who, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth1, float layerDepth2)
        {
            bool lookingDown = facingDirection == 2;
            bool lookingLeft = facingDirection == 3;

            int currentEyes = who.currentEyes;

            //If this is used someday
            if (currentEyes != 1 && currentEyes != 4)
            {
                var positionDif = new Vector2(0, ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44) - ((who.IsMale && who.FacingDirection != 2) ? 36 : 40));

                b.Draw(baseTexture, eyePosition, new Rectangle(5, 16, lookingDown ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth1);
                b.Draw(baseTexture, eyePosition + positionDif, new Rectangle(264 + (lookingLeft ? 4 : 0), 2 + (currentEyes - 1) * 2, lookingDown ? 6 : 2, 2), color, rotation, origin, scale, effects, layerDepth2);
                return;
            }

            var currentOffset = currentEyes == 1 ? currentOffset1 : currentOffset4;

            if (who.IsMale) {

                if (!lookingDown)
                {
                    eyePosition.Y -= 4;

                    if (FarmerRendererPatch.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    Vector2 lookOffset = lookingLeft ? eyebrowSideRightOffset : eyebrowSideLeftOffset;

                    //Eyebrow
                    b.Draw(baseTexture, eyePosition + lookOffset, eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth1);
                    b.Draw(baseTexture, eyePosition, skinShadowSingleRect, color, rotation, origin, scale, effects, layerDepth1);

                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseSingleRect, color, rotation, origin, scale, effects, layerDepth1);

                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashSingleRect, color, rotation, origin, scale, effects, layerDepth2);
                }
                else
                {
                    if (who.FacingDirection == 2)
                    {
                        eyePosition.Y -= 4;
                    }

                    if (!who.UsingTool)
                    {
                        //Extra Eyebrow Pixel
                        b.Draw(baseTexture, eyePosition + eyebrowSideLeftOffset, eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth2);
                        b.Draw(baseTexture, eyePosition + eyebrowFullRightOffset, eyebrowSideRect, color, rotation, origin, scale, effects, layerDepth2);

                        //Eyebrow
                        b.Draw(baseTexture, eyePosition, skinShadowFullRect, color, rotation, origin, scale, effects, layerDepth2);
                    }

                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseFullRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashFullRect, color, rotation, origin, scale, effects, layerDepth2);
                }
            }
            else
            {
                if (!lookingDown)
                {
                    eyePosition.Y -= 4;

                    if (FarmerRendererPatch.IsSoftFarmerLoaded)
                    {
                        eyePosition += softFarmerOffset;
                    }

                    //Eyebrow
                    b.Draw(baseTexture, eyePosition, skinShadowSingleRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseSingleRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashSingleRect, color, rotation, origin, scale, effects, layerDepth2);
                }
                else
                {
                    //Eyebrow
                    b.Draw(baseTexture, eyePosition, skinShadowFullRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelid
                    b.Draw(baseTexture, eyePosition + eyelidOffset, skinBaseFullRect, color, rotation, origin, scale, effects, layerDepth1);
                    //Eyelashes
                    b.Draw(baseTexture, eyePosition + currentOffset, eyelashFullRect, color, rotation, origin, scale, effects, layerDepth2);
                }
            }
        }
    }
}