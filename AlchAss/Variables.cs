using BepInEx;
using BepInEx.Configuration;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.SolventDirectionHint;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AlchAss
{
    public static class Variables
    {
        #region 配置选项
        public static ConfigEntry<bool> enableGrindStatus;
        public static ConfigEntry<bool> enableHealthStatus;
        public static ConfigEntry<bool> enableVortexStatus;
        public static ConfigEntry<bool> enableStirStatus;
        public static ConfigEntry<bool> enableZoneStatus;
        public static ConfigEntry<bool> enablePositionStatus;
        public static ConfigEntry<bool> enableTargetStatus;
        public static ConfigEntry<bool> enableDeviationStatus;
        public static ConfigEntry<bool> enablePathStatus;
        public static ConfigEntry<bool> enableLadleStatus;
        public static ConfigEntry<bool> enableTooltip;
        public static ConfigEntry<bool> enableDirectionLine;
        public static ConfigEntry<float> windowScale;
        #endregion

        #region 信息窗口
        public static DebugWindow grindDebugWindow;
        public static DebugWindow healthDebugWindow;
        public static DebugWindow vortexDebugWindow;
        public static DebugWindow stirDebugWindow;
        public static DebugWindow zoneDebugWindow;
        public static DebugWindow positionDebugWindow;
        public static DebugWindow targetDebugWindow;
        public static DebugWindow deviationDebugWindow;
        public static DebugWindow pathDebugWindow;
        public static DebugWindow ladleDebugWindow;
        public static DebugWindow tooltipDebugWindow;

        public static Vector2 positionGrindDebugWindow = Vector2.zero;
        public static Vector2 positionHealthDebugWindow = Vector2.zero;
        public static Vector2 positionVortexDebugWindow = Vector2.zero;
        public static Vector2 positionStirDebugWindow = Vector2.zero;
        public static Vector2 positionZoneDebugWindow = Vector2.zero;
        public static Vector2 positionPositionDebugWindow = Vector2.zero;
        public static Vector2 positionDeviationDebugWindow = Vector2.zero;
        public static Vector2 positionPathDebugWindow = Vector2.zero;
        public static Vector2 positionLadleDebugWindow = Vector2.zero;
        public static Vector2 positionTargetDebugWindow = Vector2.zero;
        public static Vector2 positionTooltipDebugWindow = Vector2.zero;
        #endregion

        #region 状态变量
        public static bool resetWhenLoading = false;
        public static bool directionLine = false;
        public static bool endMode = false;
        public static bool xOy = false;
        public static int zoneMode = 0;

        public static string hoveredItemName = null;
        public static Room lab = null;
        public static Texture2D texture = null;
        public static SolventDirectionHint solventDirectionHint = null;
        public static float[] lineDirection = new float[3];
        public static float[] closestPointDis = [float.MaxValue, float.MaxValue];
        public static object[,] zonePoints = new object[4, 4];
        public static Vector2?[] closestPoints = new Vector2?[2];
        public static SpriteRenderer[] lineRenderer = new SpriteRenderer[9];
        #endregion

        #region 只读数据
        public static readonly float PotionBottleRadius = 0.74f;
        public static readonly string ConfigDirectory = Path.Combine(Path.GetDirectoryName(Paths.PluginPath), "config", "AlchAss");
        public static readonly string windowPath = "AlchAssWindowConfig.txt";
        public static readonly string[] zoneModeName = ["swamp", "sdanger", "wdanger", "heal"];
        public static readonly List<DebugWindow> foreground_queue = [];

        public static readonly string chineseTooltip = @"减速操作 & 批量制作: Z / X
定量操作 & 目标选择: 右键
搅拌 / 加水方向示线: /
最近 / 搅拌末端距离: \
直角 / 极坐标: 空格
漩涡制动: '
最近点制动: ]
接近点制动: [
区域切换: .";
        public static readonly string englishTooltip = @"Slowdown/Batch: Z/X
Set Values/Target: Right Click
Stir/Ladle Lines: /
Proximity/Boundary: \
Cartesian/Polar: Space
Vortex Control: '
Closest Control: ]
Proximity Control: [
Zone Switch: .";
        public static readonly Color[] lineColor = [
            new(0.8f, 0.1f, 0.1f, 0.8f),      // 红色 - 搅拌方向线
            new(0.1f, 0.8f, 0.1f, 0.8f),      // 绿色 - 加水方向线
            new(0.1f, 0.1f, 0.8f, 0.8f),      // 蓝色 - 目标方向线
            new(0.3f, 0.3f, 0.3f, 0.9f),      // 深灰 - 沼泽区域点
            new(0.6f, 0.0f, 0.0f, 0.9f),      // 深红 - 骷髅区域点  
            new(0.8f, 0.2f, 1.0f, 0.9f),      // 紫色 - 碎骨区域点
            new(0.2f, 1.0f, 0.2f, 0.9f),      // 亮绿 - 治疗区域点
            new(1.0f, 0.2f, 0.8f, 1.0f),      // 品红 - 路径最近点
            new(0.0f, 1.0f, 1.0f, 1.0f)       // 青色 - 加水最近点  
        ];
        #endregion

        #region 缓存数据
        public static class SharedCache
        {
            public static PotionEffectMapItem targetEffect;
            public static Vector2 targetPosition;
            public static float targetRotation;
            public static bool isValid;
            public static string cachedItemName;
            public static void UpdateCache()
            {
                if (cachedItemName == hoveredItemName)
                    return;
                cachedItemName = hoveredItemName;
                targetEffect = Managers.RecipeMap.currentMap.referencesContainer.potionEffectsOnMap.FirstOrDefault(effect => effect.name == hoveredItemName);
                isValid = targetEffect != null;
                if (isValid)
                {
                    targetPosition = targetEffect.transform.localPosition;
                    targetRotation = Mathf.DeltaAngle(targetEffect.transform.localEulerAngles.z, 0f) / 9f * 25f;
                }
            }
        }
        #endregion
    }
}
