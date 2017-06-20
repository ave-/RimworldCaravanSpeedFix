using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using HugsLib;
using Harmony;

namespace CaravanSpeedFix
{
    public class CaravanSpeedFixMod: ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "CaravanSpeedFix";
            }
        }
    }

    [HarmonyPatch(typeof(Caravan_PathFollower), nameof(Caravan_PathFollower.CostToMove), new Type[]{ typeof(int), typeof(int), typeof(int), typeof(float)})]
    public static class CostToMovePatchClass
    {
        [HarmonyPrefix]
        public static bool CostToMove_Prefix(ref int caravanTicksPerMove, ref int __state)
        {
            Log.Message("Current caravanTicksPerMove = " + caravanTicksPerMove);
            __state = caravanTicksPerMove;

            caravanTicksPerMove = CaravanTicksPerMoveUtility.DefaultTicksPerMove; //override with default value. Yeah, this is hacking, I know.

            return true;
        }

        [HarmonyPostfix]
        public static void CostToMove_Postfix(int caravanTicksPerMove, ref int __result, int __state)
        {
            Log.Message("_state = " + __state);
            Log.Message("before _result = " + __result);
            __result = Mathf.RoundToInt((float)__result * ((float)__state / CaravanTicksPerMoveUtility.DefaultTicksPerMove)); //multiply by the acceleration coefficient
            Log.Message("after _result = " + __result);
        }
    }

    [HarmonyPatch(typeof(Caravan_PathFollower), nameof(Caravan_PathFollower.CostToDisplay))]
    public static class CostToDisplayPatchClass
    {
        [HarmonyPostfix]
        public static void CostToDisplayPostfix(Caravan caravan, int start, int end, ref int __result)
        {
            if ((start == end) || (end == -1))
            {
                Tile tile = Find.WorldGrid[start];
                float roadMultiplier = 1f;
                if (tile.roads != null)
                {
                    for (int i = 0; i < tile.roads.Count; i++)
                    {
                        roadMultiplier = Mathf.Min(roadMultiplier, tile.roads[i].road.movementCostMultiplier);
                    }
                }
                Log.Message("before _result = " + __result);
                __result += Mathf.RoundToInt(roadMultiplier * (float)(CaravanTicksPerMoveUtility.DefaultTicksPerMove - caravan.TicksPerMove)); //add the missing part of the default TicksPerMove

                __result = Mathf.RoundToInt((float)__result * ((float)caravan.TicksPerMove / CaravanTicksPerMoveUtility.DefaultTicksPerMove)); //multiply by the acceleration coefficient
                Log.Message("after _result = " + __result);
            }
        }
    }
}
