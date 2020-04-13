using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            test2();

        }
        static private void test2()
        {

            object op = (object)((sbyte)-15);

            Debug.Print(op.ToString());
            Debug.Print("" + Convert.ToSByte(op));

            Debug.Print("" + op.Equals(Convert.ToSByte(-15)));
        }
        static private void test1()
        {
            Debug.Print("Methods ");
            Debug.Print("================== ");
            Type typeFactionDialogMaker = typeof(FactionDialogMaker);
            var class41 = typeFactionDialogMaker.GetNestedType("FactionDialogMaker+<>c__DisplayClass4_1");
            Debug.Print("class41: {0}", class41);

            // get innter type <>c__DisplayClass4_1
            var innerTypes = typeFactionDialogMaker.GetNestedTypes(AccessTools.all)
                .Where(t => t.Name.Equals("<>c__DisplayClass4_1"));
            //.Where(innerType => innerType.IsDefined(typeof(CompilerGeneratedAttribute)))
            ;
            if (innerTypes.Count() > 1)
            {
                Debug.Print("found more then one innertype");
                return;
            }
            Type innerType = innerTypes.First();

            Debug.Print("Name: '{0}'", innerType.Name);
            Debug.Print("type:" + innerType);

            // get method
            var methods = innerType.GetMethods(AccessTools.all)
                .Where(m => m.Name.Equals("<RequestTraderOption>b__1"))
                ;
            if (methods.Count() > 1)
            {
                Debug.Print("found more then one methods");
                return;
            }

            MethodInfo mInfo = methods.First();
            Debug.Print("Method name: {0}", mInfo.Name);

            var h = new HarmonyLib.Harmony("trader_costs");
        }
    }
}
