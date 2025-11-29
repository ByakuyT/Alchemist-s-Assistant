using BepInEx;
using BepInEx.Configuration;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace AlchAssV3
{
    internal class Variable
    {
        #region 配置数据
        public static ConfigEntry<double> LineWidth;
        public static ConfigEntry<double> NodeSize;
        public static ConfigEntry<double> WindowScale;
        public static ConfigEntry<KeyboardShortcut> KeyPolarMode;
        public static ConfigEntry<KeyboardShortcut> KeySelectEffect;
        public static ConfigEntry<KeyboardShortcut> KeyNextVortex;
        public static ConfigEntry<KeyboardShortcut> KeyPrevVortex;
        public static ConfigEntry<KeyboardShortcut> KeyNearVortex;
        public static ConfigEntry<KeyboardShortcut> KeyNoneVortex;

        // 0 - Path; 1 - Ladle; 2 - Move; 3 - Target; 4 - Position; 5 - Deviation; 6 - Vortex; 7 - Health; 8 - Grind
        public static ConfigEntry<bool>[] EnableWindows = new ConfigEntry<bool>[9];
        // 0 - PathLine; 1 - LadleLine; 2 - TargetLine; 3 - VortexLine; 4 - PathCurveOdd; 5 - PathCurveEven; 6 - VortexCurve
        // 7 - Range; 8 - ClosestPoint; 9 - DangerPoint; 10 - IntersectionPoint; 11 - DefeatPoint
        public static ConfigEntry<Color>[] Colors = new ConfigEntry<Color>[12];
        // 0 - PathLine; 1 - LadleLine; 2 - TargetLine; 3 - VortexLine; 4 - PathCurve; 5 - VortexCurve
        // 6 - TargetRange; 7 - VortexRange; 8 - DangerRange; 9 - SwampScale; 10 - Transparency
        public static ConfigEntry<KeyboardShortcut>[] Keys = new ConfigEntry<KeyboardShortcut>[11];
        #endregion

        #region 只读数据
        public static readonly int BaseLayer = 8;
        public static readonly int BaseSortingOrder = 1;
        public static readonly double IndicatorRadius = 0.74;
        public static readonly double VortexCurveA = 1 / (2 * Math.PI);
        public static readonly double MaxVortexDanger = 1.55;
        public static readonly string BaseSortingLayerName = "RecipeMapIndicator";
        public static readonly string ConfigPath = Path.Combine(Path.GetDirectoryName(Paths.PluginPath), "config/AlchAssV3");
        public static readonly string WindowConfigPath = Path.Combine(Path.GetDirectoryName(Paths.PluginPath), "config/AlchAssV3/WindowConfig.json");
        public static readonly WindowConfigData DefaultWindowConfig = new()
        {
            positions =
            [
                new() { tag = "路径信息", x = -12.5f, y = -2.3f },
                new() { tag = "加水信息", x = -12.5f, y = -4.6f },
                new() { tag = "移动信息", x = -9.9f, y = -5.2f },
                new() { tag = "目标信息", x = -4.9f, y = -4.6f },
                new() { tag = "位置信息", x = -2.4f, y = -5.2f },
                new() { tag = "偏离信息", x = -7.5f, y = -5.2f },
                new() { tag = "漩涡信息", x = 7.6f, y = -4.9f },
                new() { tag = "血量信息", x = 10.0f, y = -4.9f },
                new() { tag = "研磨信息", x = 10.0f, y = -5.9f },
            ]
        };

        public static readonly string[] MessageText =
        [
            "路径方向线",
            "加水方向线",
            "目标方向线",
            "漩涡方向线",
            "路径曲线",
            "漩涡曲线",
            "目标范围",
            "漩涡范围",
            "区域追踪",
            "沼泽收缩",
            "透明瓶身",
        ];
        #endregion

        #region 信息窗口
        // 0 - Path; 1 - Ladle; 2 - Move; 3 - Target; 4 - Position; 5 - Deviation; 6 - Vortex; 7 - Health; 8 - Grind
        public static Vector3[] DebugWindowPos = new Vector3[9];
        public static DebugWindow[] DebugWindows = new DebugWindow[9];
        public static List<DebugWindow> ActiveDebugWindows = [];
        #endregion

        #region 渲染材质
        public static Material SolidMaterial;
        public static Material DashedMaterial;
        public static Sprite SquareSprite;
        public static Sprite RoundSprite;

        public static LineRenderer VortexCurve;
        public static LineRenderer VortexRange;
        public static LineRenderer[] Lines = new LineRenderer[4];
        public static LineRenderer[] TargetRanges = new LineRenderer[2];
        public static SpriteRenderer[] TargetDisks = new SpriteRenderer[2];
        public static SpriteRenderer[] ClosestPoints = new SpriteRenderer[2];
        public static SpriteRenderer[] DefeatPoints = new SpriteRenderer[3];
        public static List<LineRenderer> PathCurves = [];
        public static List<SpriteRenderer> SwampPoints = [];
        public static List<SpriteRenderer>[] IntersectionPoints = [[], [], [], []];
        public static List<SpriteRenderer>[] DangerPoints = [[], [], []];
        #endregion

        #region 功能开关
        public static bool PolarMode = false;

        // 0 - PathLine; 1 - LadleLine; 2 - TargetLine; 3 - VortexLine; 4 - PathCurve; 5 - VortexCurve
        // 6 - TargetRange; 7 - VortexRange; 8 - DangerRange; 9 - SwampScale; 10 - Transparency
        public static bool[] Enables = [false, false, false, false, false, false, false, false, false, false, false];
        // 0 - PathLine; 1 - LadleLine; 2 - TargetLine; 3 - VortexLine; 4 - PathCurve; 5 - VortexCurve
        // 6 - TargetRange; 7 - VortexRange; 8 - Transparency; 9 - PathTargetPoint; 10 - LadleTargetPoint
        // 11 - PathVortexPoint; 12 - LadleVortexPoint; 13 - PathDangerPoint; 14 - LadleDangerPoint; 15 - VortexDangerPoint; 16 - SwampPoint
        public static bool[] DerivedEnables = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];
        #endregion

        #region 缓存数据
        public static List<Vortex> Vortex_Water = [];
        public static List<Vortex> Vortex_Oil = [];
        public static List<Shape> Strong_Water = [];
        public static List<Shape> Strong_Oil = [];
        public static List<Shape> Strong_Wine = [];
        public static List<Shape> Weak_Wine = [];
        public static List<Shape> Heal_Wine = [];
        public static List<Shape> Swamp_Oil = [];
        public static List<Node> Strong_Water_BVH = [];
        public static List<Node> Strong_Oil_BVH = [];
        public static List<Node> Strong_Wine_BVH = [];
        public static List<Node> Weak_Wine_BVH = [];
        public static List<Node> Heal_Wine_BVH = [];
        public static List<Node> Swamp_Oil_BVH = [];

        public static Vector3[] VortexGraphical;
        public static List<(Vector3, bool)> PathPhysical = [];
        public static List<(Vector3[], bool)> PathGraphical = [];
        #endregion

        #region 状态数据
        public static float IndicatorRotation;
        public static string CurrentMapID;
        public static Vector2 IndicatorPosition;
        public static Vector2 BaseRenderPosition;
        public static SpriteRenderer BaseLadleRenderer;
        public static PotionEffectMapItem TargetEffect;

        // 0 - Water; 1 - Oil
        public static int[] VortexIndex = [-1, -1];
        // 0 - Water; 1 - Oil; 2 - Wine
        public static double[] DangerDistance = [double.NaN, double.NaN, double.NaN];
        // 0 - PositionX; 1 - PositionY; 2 - Rotation; 3 - MaxAngle; 4 - MinAngle
        public static double[] VortexParameters = [double.NaN, double.NaN, double.NaN, double.NaN, double.NaN];
        // 0 - Path; 1 - Ladle; 2 - Target; 3 - Vortex
        public static double[] LineDirections = [double.NaN, double.NaN, double.NaN, double.NaN];
        // 0 - Path; 1 - Ladle
        public static Vector2[] ClosestPositions = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
        // 0 - Path; 1 - Ladle; 2 - Vortex
        public static Vector2[] DefeatPositions = [new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN)];
        // 0 - Path
        public static List<Vector2> SwampPositions = [];
        // 0 - Path; 1 - Ladle; 2 - Vortex
        public static List<Vector2>[] DangerPositions = [[], [], []];
        // 0 - PathTarget; 1 - LadleTarget; 2 - PathVortex; 3 - LadleVortex
        public static List<Vector2>[] IntersectionPositions = [[], [], [], []];
        #endregion

        #region 辅助结构
        public struct Vortex
        {
            public double x, y, r;
        }

        public abstract record Shape
        {
            public sealed record Arc(
                double X, double Y, double R,
                double StartAngle, double EndAngle
            ) : Shape;

            public sealed record Line(
                double X1, double Y1, double X2, double Y2
            ) : Shape;
        }

        public abstract record Node(double MinX, double MinY, double MaxX, double MaxY)
        {
            public sealed record InternalNode(
                double MinX, double MinY, double MaxX, double MaxY,
                int Left, int Right
            ) : Node(MinX, MinY, MaxX, MaxY);

            public sealed record LeafNode(
                double MinX, double MinY, double MaxX, double MaxY,
                int[] Items
            ) : Node(MinX, MinY, MaxX, MaxY);
        }

        [Serializable]
        public struct WindowConfigData
        {
            [Serializable]
            public struct PositionData
            {
                public string tag;
                public float x, y;
            }
            public PositionData[] positions;
        }
        #endregion
    }
}
