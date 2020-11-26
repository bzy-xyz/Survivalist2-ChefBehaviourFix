using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;

namespace ChefBehaviourFix
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            Harmony.DEBUG = true;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            mod = modEntry;

            return true;
        }      
    }

    [HarmonyPatch]
    static class CraftGoal__ReversePatch {

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CraftGoal), "HasUsedIngredientsWaitingForProduct")]
        public static bool HasUsedIngredientsWaitingForProduct(CraftGoal instance, Character character) {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CraftGoal), "GetRetrieveFinishedItemGoal")]
        public static bool GetRetrieveFinishedItemGoal(CraftGoal instance, Character character, CraftingProp craftingProp, out Goal goal) {
            throw new NotImplementedException("stub");
        }
    }
    
    [HarmonyPatch(typeof(CraftGoal), "GetNextSubGoal")]
    static class CraftGoal__GetNextSubGoal__Patch {
        static Goal Postfix(Goal __result, ref CraftGoal __instance, Character character, Goal parent) {
            // System.Text.StringBuilder goal_sb = new System.Text.StringBuilder();
            // System.Text.StringBuilder result_goal_sb = new System.Text.StringBuilder();
            // __instance.BuildDebugString(goal_sb);
            if (__result != null) {
                // __result.BuildDebugString(result_goal_sb);
            } else {
                // result_goal_sb.Append("null");
            }
            // Main.mod.Logger.Log($"{character.FirstName} {character.Surname} {goal_sb.ToString()} => {result_goal_sb.ToString()}");
            if (__instance.FollowingRecipe == null) {
                // Main.mod.Logger.Warning("no recipe!");
            }
            else {
                // Main.mod.Logger.Log($"recipe {__instance.FollowingRecipe.UniqueID}");

                // try to create an order to clear a reserved campfire with food
                if (__result == null && !CraftGoal__ReversePatch.HasUsedIngredientsWaitingForProduct(__instance, character) && __instance.FollowingRecipe.RequiredPropToWorkOn() != 0)
                {
                    GameTerrain terrain = GameTerrain.Instance;
                    CraftingProp craftingProp = terrain.GetFixedObjectOnTile(__instance.DestTile.x, __instance.DestTile.y) as CraftingProp;
                    if (craftingProp != null && (craftingProp.CraftingFinished || craftingProp.CraftingRecipe == null)) {
                        if (craftingProp.Inventory.GetTotalNutrition() > 0f) {
                            // Main.mod.Logger.Log("target prop has stuck food");
                            if (__instance.DesiredAmount == int.MaxValue && character.GetRole() == Role.Cook) {
                                // Main.mod.Logger.Log("is cook");
                                if (CraftGoal__ReversePatch.GetRetrieveFinishedItemGoal(__instance, character, craftingProp, out Goal goal2)) {
                                    // result_goal_sb.Clear();
                                    // goal2.BuildDebugString(result_goal_sb);
                                    // Main.mod.Logger.Log($"found a new goal: {result_goal_sb.ToString()}");
                                    return goal2;
                                } else {
                                    // Main.mod.Logger.Warning("couldn't find a new goal");
                                }
                            }
                        }
                    }
                }

                // stop current recipe if crafting limit was reached
                if (__result == null && character.Community.HasReachedCraftingLimitForProduct(__instance.FollowingRecipe)) {
                    // Main.mod.Logger.Log("crafting limit reached for recipe");
                    __instance.StopRecipe(character);
                }
            }

            return __result;
        }
    }

    [HarmonyPatch(typeof(CraftAnim), "Update")]
    static class CraftAnim__Update__Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            
            var StopRecipe_MethodInfo = typeof(CraftGoal).GetMethod("StopRecipe");

            for (int i = codes.Count - 1; i >= 2; i--) {
                CodeInstruction ci = codes[i];
                if (ci.Calls(StopRecipe_MethodInfo)) {
                    codes.RemoveRange(i - 2, 3);
                    break;
                }
            }
            
            return codes;
        }
    }

    [HarmonyPatch(typeof(CraftGoal), "OnCraftingFinished")]
    static class CraftGoal__OnCraftingFinished__Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            
            var StopRecipe_MethodInfo = typeof(CraftGoal).GetMethod("StopRecipe");

            for (int i = 2; i < codes.Count; i++) {
                CodeInstruction ci = codes[i];
                if (ci.Calls(StopRecipe_MethodInfo)) {
                    codes.RemoveRange(i - 2, 3);
                    break;
                }
            }
            
            return codes;
        }
    }

    [HarmonyPatch(typeof(CraftGoal), "OnActivate")]
    static class CraftGoal__OnActivate__Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);

            var GetTotalNutrition_MethodInfo = typeof(EquipmentContainer).GetMethod("GetTotalNutrition");

            for (int i = 4; i < codes.Count - 16; i++) {
                CodeInstruction ci = codes[i];
                if (ci.Calls(GetTotalNutrition_MethodInfo)) {
                    codes.RemoveRange(i - 4, 21);
                    break;
                }
            }

            return codes;
        }
    }

    // [HarmonyPatch(typeof(CraftGoal), "OnActivate")]
    // static class CraftGoal__OnActivate__Patch2 {
    //     static void Postfix(ref CraftGoal __instance, Character character) {
    //         System.Text.StringBuilder goal_sb = new System.Text.StringBuilder();
    //         __instance.BuildDebugString(goal_sb);
    //         Main.mod.Logger.Log($"OnActivate_Postfix {character.FirstName} {character.Surname} {goal_sb.ToString()}");
    //         if (character.Role == Role.Cook) {
    //             Main.mod.Logger.Log("is cook");
    //         }
    //         if (__instance.FollowingRecipe == null) {
    //             Main.mod.Logger.Warning("no recipe!");
    //         }
    //         else {
    //             Main.mod.Logger.Log($"recipe {__instance.FollowingRecipe.UniqueID}");
    //         }
    //     }
    // }
}
