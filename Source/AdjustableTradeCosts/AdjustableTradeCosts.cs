using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace AdjustableTradeCosts
{
    public class TradeCostsSettings : ModSettings
    {
        public int tradeCaravanCost = 15;
        public int militaryAidCost = 25;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref tradeCaravanCost, "tradeCaravanCost", 15);
            Scribe_Values.Look(ref militaryAidCost, "militaryAidCost", 25);
            base.ExposeData();
        }
    }

    public class AdjustableTradeCosts : Mod
    {
        TradeCostsSettings settings;

        string tradeCaravanCost;
        string militaryAidCost;

        public AdjustableTradeCosts(ModContentPack pack) : base(pack)
        {
            this.settings = GetSettings<TradeCostsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Trade caravan".Translate());
            listingStandard.TextFieldNumeric(ref settings.tradeCaravanCost, ref tradeCaravanCost);
            listingStandard.Gap();
            listingStandard.Label("Military aid".Translate());
            listingStandard.TextFieldNumeric(ref settings.militaryAidCost, ref militaryAidCost);
            listingStandard.Gap();
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Adjustable Trade Costs".Translate();
        }
    }

    // ====================================================================
    // patches the RequestTraderOption dialog option presented to the user
    [HarmonyPatch(typeof(FactionDialogMaker), "RequestTraderOption")]
    public static class RequestTraderOption_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TradeCostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCost = LoadedModManager.GetMod<AdjustableTradeCosts>().GetSettings<TradeCostsSettings>().tradeCaravanCost;
            var codes = new List<CodeInstruction>(instructions); // modifiable copy

            // search for correct argument and change it
            for (int i = 0; i < codes.Count; i++)
            {
                //FileLog.Log("opcode " + i + ": " + codes[i]);
                if (codes[i].opcode == OpCodes.Ldstr && "RequestTrader".Equals(Convert.ToString(codes[i].operand)))
                {
                    //FileLog.Log("Found RequestTrader reference, adapt argument");
                    if (codes.Count > (i + 1) && codes[i + 1].opcode == OpCodes.Ldc_I4_S)
                    {
                        codes[i + 1].operand = Convert.ToSByte(newCost);
                        //FileLog.Log("changed RequestTrader goodwill costs to " + newCost);
                        // we are done
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }

    // ========================================================================
    // patches the RequestMilitaryAidOption dialog option presented to the user
    [HarmonyPatch(typeof(FactionDialogMaker), "RequestMilitaryAidOption")]
    public static class RequestMilitaryAidOption_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TradeCostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCost = LoadedModManager.GetMod<AdjustableTradeCosts>().GetSettings<TradeCostsSettings>().militaryAidCost;
            var codes = new List<CodeInstruction>(instructions); // modifiable copy

            // search for correct argument and change it
            for (int i = 0; i < codes.Count; i++)
            {
                //FileLog.Log("opcode " + i + ": " + codes[i]);
                if (codes[i].opcode == OpCodes.Ldstr && "RequestMilitaryAid".Equals(Convert.ToString(codes[i].operand)))
                {
                    //FileLog.Log("Found RequestMilitaryAid reference, adapt argument");
                    if (codes.Count > (i + 1) && codes[i + 1].opcode == OpCodes.Ldc_I4_S)
                    {
                        codes[i + 1].operand = Convert.ToSByte(newCost);
                        //FileLog.Log("changed RequestMilitaryAid goodwill costs to " + newCost);
                        // we are done
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }

    // ========================================
    // patches the TryAffectGoodwillWith Method 
    [HarmonyPatch(typeof(Faction), "TryAffectGoodwillWith")]
    public static class TryAffectGoodwillWith_Patch
    {
        static void Prefix(RimWorld.Faction other, ref int goodwillChange, bool canSendMessage, bool canSendHostilityLetter, ref string reason, RimWorld.Planet.GlobalTargetInfo? lookTarget)
        {
            var tradeCaravanCost = -LoadedModManager.GetMod<AdjustableTradeCosts>().GetSettings<TradeCostsSettings>().tradeCaravanCost;
            var militaryAidCost = -LoadedModManager.GetMod<AdjustableTradeCosts>().GetSettings<TradeCostsSettings>().militaryAidCost;

            String translatedReqTrader = "GoodwillChangedReason_RequestedTrader".Translate();
            if (translatedReqTrader.Equals(reason))
            {
                goodwillChange = tradeCaravanCost;
            }

            String translatedMilitaAid = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
            if (translatedMilitaAid.Equals(reason))
            {
                goodwillChange = militaryAidCost;
            }
        }
    }

    [StaticConstructorOnStartup]
    internal class PatchLoader
    {
        static PatchLoader()
        {
            var instance = new HarmonyLib.Harmony("trader_costs");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            //FileLog.Log("Harmony Loaded");
        }
    }
}
