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
        public int personaCoreCost = 1500;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref tradeCaravanCost, "tradeCaravanCost", 15);
            Scribe_Values.Look(ref militaryAidCost, "militaryAidCost", 25);
            Scribe_Values.Look(ref personaCoreCost, "personaCoreCost", 1500);
            base.ExposeData();
        }
    }

    public class AdjustableTradeCosts : Mod
    {
        TradeCostsSettings settings;

        string tradeCaravanCost;
        string militaryAidCost;
        string personaCoreCost;

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
            listingStandard.Label("Persona core".Translate());
            listingStandard.TextFieldNumeric(ref settings.personaCoreCost, ref personaCoreCost);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Adjustable Trader Costs".Translate();
        }
    }

    [HarmonyPatch]
    public static class AdjustTradeCostsPatch
    {
        [HarmonyTargetMethod]
        static MethodBase CalculateMethod()
        {
            // get the class FactionDialogMaker
            Type typeFactionDialogMaker = typeof(FactionDialogMaker);

            // get innter type <>c__DisplayClass4_1
            var innerTypes = typeFactionDialogMaker.GetNestedTypes(AccessTools.all)
                .Where(t => t.Name.Equals("<>c__DisplayClass4_1"));
            ;
            if (innerTypes.Count() > 1)
            {
                FileLog.Log("found more then one innertype");
                return null;
            }
            Type innerType = innerTypes.First();

            // get method <RequestTraderOption>b__1
            var methods = innerType.GetMethods(AccessTools.all)
                .Where(m => m.Name.Equals("<RequestTraderOption>b__1"))
                ;
            if (methods.Count() > 1)
            {
                FileLog.Log("found more then one methods");
                return null;
            }

            MethodInfo mInfo = methods.First();
            return mInfo;
        }

        // replaces the hardcoded -15 goodwill trade costs
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TradeCostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCost = -LoadedModManager.GetMod<AdjustableTradeCosts>().GetSettings<TradeCostsSettings>().tradeCaravanCost;

            // create a copy of the instructions to be able to modify them
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                //FileLog.Log("opcode " +i +": " +codes[i]);
                // find opcode where -15 is pushed to the stack and modify it
                if (codes[i].opcode == OpCodes.Ldc_I4_S)
                {
                    var operand = codes[i].operand;
                    FileLog.Log("Found opcode Ldc i4 s ");

                    if (operand.Equals(Convert.ToSByte(-15)))
                    {
                        codes[i].operand = Convert.ToSByte(newCost);
                        FileLog.Log("changed goodwill costs to " + newCost);

                        // we are done
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }

    [StaticConstructorOnStartup]
    internal class PatchLoader
    {
        static PatchLoader()
        {
            var instance = new HarmonyLib.Harmony("trader_costs");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            FileLog.Log("Harmony Loaded");
        }
    }

    [HarmonyPatch(typeof(ModsConfig), "TrySortMods")]
    internal static class Example_MainMenu_Patch
    {

        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        try
        //        {
        //            FileLog.Log("Methods ");
        //            FileLog.Log("================== ");
        //            foreach (methodinfo m in typeof(factiondialogmaker).getmethods())
        //            {
        //                FileLog.Log(m.Name + ": " + m);
        //            }

        //            FileLog.Log("");
        //            FileLog.Log("Runtime Methods ");
        //            FileLog.Log("================== ");
        //            foreach (MethodInfo m in typeof(FactionDialogMaker).GetRuntimeMethods())
        //            {
        //                FileLog.Log(m.Name +": " +m);
        //            }

        //            FileLog.Log("");
        //            FileLog.Log("Nested types ");
        //            FileLog.Log("================== ");
        //            foreach (Type m in typeof(FactionDialogMaker).GetNestedTypes())
        //            {
        //                FileLog.Log(m.Name + ": " + m);
        //            }

        //            FileLog.Log("");
        //            FileLog.Log("Assembly Types");
        //            FileLog.Log("================== ");
        //            Assembly assem = typeof(FactionDialogMaker).Assembly;
        //            Type inner = AccessTools.Inner(typeof(FactionDialogMaker), "<RequestTraderOption>b__1");
        //            FileLog.Log("inner:" +inner);
        //            //foreach (Type m in assem.GetTypes())
        //            //{
        //            //  FileLog.Log(m.Name + ": " + m);
        //            //}
        //        }
        //        catch (Exception e)
        //        {
        //            FileLog.Log("Exception" +e);
        //        }
        //    }
    }

}
