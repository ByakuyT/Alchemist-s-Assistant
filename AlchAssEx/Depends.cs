using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion.Entities;
using PotionCraft.ObjectBased.UIElements.Books.RecipeBook;
using PotionCraft.ScriptableObjects;
using PotionCraft.LocalizationSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AlchAssEx
{
    public static class Depends
    {
        #region 本地适配
        public static void SetModLocalization()
        {
            AlchAss.Depends.RegisterLoc("avortex", "Vortex control ", "漩涡制动");
            AlchAss.Depends.RegisterLoc("aclosest", "Closest control ", "最近点制动");
            AlchAss.Depends.RegisterLoc("aproximity", "Proximity control ", "接近点制动");
        }
        #endregion

        #region 加载配置
        public static void InitializeConfigFile(string configFilePath)
        {
            var defaultConfig = @"# 加热数值 (0-100) | Heat value (0-100)
HeatValue=100

# 研磨数值 (0-100) | Grinding value (0-100)
GrindValue=100

# Z键减速倍数 (数值越大越慢) | Z key slowdown factor (higher = slower)
SlowdownFactorZ=10

# X键减速倍数 (数值越大越慢) | X key slowdown factor (higher = slower)
SlowdownFactorX=100

# Z键批量制作倍数 | Z key batch brewing multiplier
BrewingMultiplierZ=10

# X键批量制作倍数 | X key batch brewing multiplier
BrewingMultiplierX=100

# 控制区域半径 (数值越大控制区域越大) | Control area threshold (higher = larger control area)
ControlAreaThreshold=0.05

# 控制减速强度 (数值越大减速越剧烈) | Control slowdown strength (higher = more aggressive slowdown)
ControlSlowdownStrength=1.35

# 控制减速渐近因子 (数值越大越平滑) | Control slowdown asymptote factor (higher = smoother)
ControlAsymptoteFactor=0.002";
            File.WriteAllText(configFilePath, defaultConfig);
            Variables._functionCacheValid = true;
        }
        public static void UpdateFunctionsConfigCache()
        {
            var configFilePath = Path.Combine(AlchAss.Variables.ConfigDirectory, Variables.functionPath);
            if (!File.Exists(configFilePath))
            {
                InitializeConfigFile(configFilePath);
                return;
            }
            var configActions = new Dictionary<string, Action<float>>
            {
                { "SlowdownFactorX", value => Variables._cachedSlowdownFactorX = value },
                { "SlowdownFactorZ", value => Variables._cachedSlowdownFactorZ = value },
                { "BrewingMultiplierX", value => Variables._cachedBrewingMultiplierX = (int)value },
                { "BrewingMultiplierZ", value => Variables._cachedBrewingMultiplierZ = (int)value },
                { "HeatValue", value => Variables._cachedHeatValue = value },
                { "GrindValue", value => Variables._cachedGrindValue = value },
                { "ControlAreaThreshold", value => Variables._cachedControlAreaThreshold = value },
                { "ControlSlowdownStrength", value => Variables._cachedControlSlowdownStrength = value },
                { "ControlAsymptoteFactor", value => Variables._cachedControlAsymptoteFactor = value }
            };
            foreach (var line in File.ReadAllLines(configFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    if (float.TryParse(parts[1].Trim(), out var value) && configActions.TryGetValue(key, out var action))
                        action(value);
                }
            }
            Variables._functionCacheValid = true;
        }
        #endregion

        #region 工具方法
        public static bool CanBrewTimes(IRecipeBookPageContent recipePageContent, int ingredientCount, int brewingTimes)
        {
            var requiredComponents = RecipeBookRecipeBrewController.GetUsedDuringBrewingIngredientsAmount(
                recipePageContent.GetComponentsToUseInBrewWithPreparedIngredients(),
                recipePageContent.GetComponentsToUseInBrewWithoutPreparedIngredients(),
                ingredientCount * brewingTimes,
                true);
            foreach (var requiredComponent in requiredComponents)
            {
                if (requiredComponent.Type == AlchemySubstanceComponentType.InventoryItem)
                {
                    var inventoryItem = requiredComponent.Component as InventoryItem;
                    if (inventoryItem == null)
                        continue;
                    int availableAmount = Managers.Player.Inventory.GetItemCount(inventoryItem);
                    if (availableAmount < requiredComponent.Amount)
                        return false;
                }
            }
            return true;
        }
        public static float CalculateControlSpeedFactor(float distance)
        {
            var normalizedDistance = distance / Variables._cachedControlAreaThreshold;
            var baseSpeedFactor = Mathf.Pow(normalizedDistance, Variables._cachedControlSlowdownStrength);
            var asymptoteCalc = normalizedDistance / (normalizedDistance + Variables._cachedControlAsymptoteFactor);
            return baseSpeedFactor * asymptoteCalc;
        }
        #endregion
    }
}
