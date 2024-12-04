﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace taranchuk_apparelgraphics
{
    public class taranchuk_apparelgraphicsMod : Mod
    {
        public taranchuk_apparelgraphicsMod(ModContentPack pack) : base(pack)
        {
            new Harmony("taranchuk_apparelgraphicsMod").PatchAll();
        }
    }

    public class ApparelExtension : DefModExtension
    {
        public bool hideBody;
        public BodyTypeDef femaleBody;
        public BodyTypeDef maleBody;
    }

    [StaticConstructorOnStartup]
    public static class Utils
    {
        private static Dictionary<ThingDef, ApparelExtension> cachedExtensions = new Dictionary<ThingDef, ApparelExtension>();
        public static bool ShouldHideBody(this ThingDef def)
        {
            if (!cachedExtensions.TryGetValue(def, out var extension))
            {
                cachedExtensions[def] = extension = def.GetModExtension<ApparelExtension>();
            }
            if (extension != null && extension.hideBody)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker), "AppendDrawRequests")]
    public static class PawnRenderNodeWorker_AppendDrawRequests_Patch
    {
        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            if ((node is PawnRenderNode_Body || node.parent is PawnRenderNode_Body) && parms.pawn.apparel.AnyApparel)
            {
                foreach (var apparel in parms.pawn.apparel.WornApparel)
                {
                    if (apparel.def.ShouldHideBody())
                    {
                        requests.Add(new PawnGraphicDrawRequest(node)); // adds an empty draw request to not draw body
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    public static class PawnRenderNode_Body_GraphicFor_Patch
    {
        public static void Prefix(PawnRenderNode_Body __instance, Pawn pawn, out BodyTypeDef __state)
        {
            __state = pawn.story.bodyType;
            pawn.TryOverrideBody(ref pawn.story.bodyType);
        }

        public static void Postfix(PawnRenderNode_Body __instance, Pawn pawn, BodyTypeDef __state)
        {
            pawn.story.bodyType = __state;
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class ApparelGraphicRecordGetter_TryGetGraphicApparel_Patch
    {
        public static void Prefix(Apparel apparel, ref BodyTypeDef bodyType)
        {
            var pawn = apparel.Wearer;
            if (pawn != null)
            {
                pawn.TryOverrideBody(ref bodyType);
            }
        }

        public static void TryOverrideBody(this Pawn pawn, ref BodyTypeDef bodyType)
        {
            foreach (var apparel2 in pawn.apparel.WornApparel)
            {
                var extension = apparel2.def.GetModExtension<ApparelExtension>();
                if (extension != null)
                {
                    if (pawn.gender == Gender.Male && extension.maleBody != null)
                    {
                        bodyType = extension.maleBody;
                        break;
                    }
                    else if (pawn.gender == Gender.Female && extension.femaleBody != null)
                    {
                        bodyType = extension.femaleBody;
                        break;
                    }
                }
            }
        }
    }
}