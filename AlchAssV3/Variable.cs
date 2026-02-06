using BepInEx.Configuration;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace AlchAssV3
{
    public static class Variable
    {
        #region 配置数据
        public static ConfigEntry<KeyboardShortcut> KeyWindow;
        public static ConfigEntry<KeyboardShortcut> KeyEffect;
        public static ConfigEntry<KeyboardShortcut> KeyVortex;
        public static ConfigEntry<KeyboardShortcut> KeyCustom;

        public static ConfigEntry<float> LineWidth;
        public static ConfigEntry<float> NodeSize;
        public static ConfigEntry<float> WindowScale;

        public static ConfigEntry<Color> ColorVortex;
        public static ConfigEntry<Color> ColorRange;
        public static ConfigEntry<Color> ColorClosest;
        public static ConfigEntry<Color> ColorDanger;
        public static ConfigEntry<Color> ColorIntersection;
        public static ConfigEntry<Color> ColorDefeat;
        public static ConfigEntry<Color> ColorCustomNormal;
        public static ConfigEntry<Color> ColorCustomHover;
        public static ConfigEntry<Color>[] ColorPaths = new ConfigEntry<Color>[2];
        public static ConfigEntry<Color>[] ColorLines = new ConfigEntry<Color>[5];
        // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线

        public static ConfigEntry<Rect> WindowRectConfig;
        public static ConfigEntry<Vector3>[] WindowPositions = new ConfigEntry<Vector3>[9];
        // 0 - 路径信息; 1 - 加水信息; 2 - 移动信息; 3 - 目标效果; 4 - 酿造信息
        // 5 - 效果偏离; 6 - 活跃漩涡; 7 - 目标漩涡; 8 - 研磨信息
        #endregion

        #region 只读数据
        public static readonly double VortexA = 1 / (2 * Math.PI);
        public static readonly Font Font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
        public static readonly Dictionary<string, int> MapId = new() {
            { "Water", 0 }, { "Oil", 1 }, { "Wine", 2 } };
        public static readonly string[] WindowTags = [
            "路径信息", "加水信息", "移动信息", "目标效果", "酿造信息",
            "效果偏离", "活跃漩涡", "目标漩涡", "研磨信息"];
        #endregion

        #region 渲染材质
        public static Texture2D WindowTexture;
        public static Material SolidMaterial;
        public static Material DashedMaterial;
        public static Sprite RoundSprite;
        public static Sprite SquareSprite;

        public static LineRenderer VortexCurve;
        public static LineRenderer VortexRange;
        public static LineRenderer IndicatorRange;
        public static LineRenderer IndicatorDirection;
        public static LineRenderer EffectRangeOuter;
        public static LineRenderer EffectRangeMiddle;
        public static LineRenderer[] Lines = new LineRenderer[5];
        // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线
        public static List<LineRenderer> PathCurves = [];
        public static List<LineRenderer> CustomLines = [];

        public static SpriteRenderer BaseLadleRenderer;
        public static SpriteRenderer EffectDiskMiddle;
        public static SpriteRenderer EffectDiskInner;
        public static SpriteRenderer[] ClosestPoints = new SpriteRenderer[4];
        // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
        public static SpriteRenderer[] DefeatPoints = new SpriteRenderer[3];
        // 0 - 路径; 1 - 加水; 2 - 漩涡
        public static List<SpriteRenderer> SwampPoints = [];
        public static List<SpriteRenderer>[] DangerPoints = [[], [], []];
        // 0 - 路径; 1 - 加水; 2 - 漩涡
        public static List<SpriteRenderer>[] IntersectionPoints = [[], [], [], []];
        // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
        #endregion

        #region 窗口样式
        public static GUIStyle WindowStyle;
        public static GUIStyle CategoryStyle;
        public static GUIStyle ToggleStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle TextFieldStyle;
        public static GUIStyle TextFieldErrorStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle DeleteButtonStyle;
        public static GUIStyle SliderStyle;
        #endregion

        #region 功能开关
        public static bool ShowWindow = false;
        public static bool IsResizing = false;
        public static bool EnableExpand = true;
        public static bool DisplayExpand = true;
        public static bool ActionExpand = true;
        public static bool CustomListExpand = true;

        public static bool EnablePathLine = false;
        public static bool EnableLadleLine = false;
        public static bool EnableEffectLine = false;
        public static bool EnableVortexLine = false;
        public static bool EnableTangentLine = false;
        public static bool EnableCustomLine = false;
        public static bool EnablePathCurve = false;
        public static bool EnableVortexCurve = false;
        public static bool EnableEffectRange = false;
        public static bool EnableVortexRange = false;
        public static bool EnableDangerSimulation = false;
        public static bool EnableSwampSimulation = false;
        public static bool EnableTransparency = false;

        public static bool DoCustomLine = false;
        public static bool DoPathCurve = false;
        public static bool DoVortexCurve = false;
        public static bool DoEffectRange = false;
        public static bool DoVortexRange = false;
        public static bool DoTransparency = false;
        public static bool DoPathEffectPoint = false;
        public static bool DoLadleEffectPoint = false;
        public static bool DoPathVortexPoint = false;
        public static bool DoLadleVortexPoint = false;
        public static bool DoPathDangerPoint = false;
        public static bool DoLadleDangerPoint = false;
        public static bool DoVortexDangerPoint = false;
        public static bool DoSwampPoint = false;
        public static bool[] DoLines = [false, false, false, false, false];
        // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线

        public static bool DisplaySalt = false;
        public static bool DisplayStage = false;
        public static bool DisplayOffset = false;
        public static bool DisplayPolar = false;
        #endregion

        #region 文件数据
        public static List<Shape> WeakWine = [];
        public static List<Shape> HealWine = [];
        public static List<Shape> SwampOil = [];
        public static List<Shape>[] Strongs = [[], [], []];
        // 0 - 水; 1 - 油; 2 - 酒

        public static List<Node> WeakWineBVH = [];
        public static List<Node> HealWineBVH = [];
        public static List<Node> SwampOilBVH = [];
        public static List<Node>[] StrongBVHs = [[], [], []];
        // 0 - 水; 1 - 油; 2 - 酒

        public static List<Vortex>[] Vortexs = [[], []];
        // 0 - 水; 1 - 油
        #endregion

        #region 状态数据
        public static int TargetLineIndex = -1;
        public static int[] VortexIndex = [-1, -1];
        // 0 - 水; 1 - 油
        public static float LabelWidth;
        public static double VortexX;
        public static double VortexY;
        public static double VortexRotation;
        public static double VortexMaxAngle;
        public static double VortexMinAngle;
        public static double DangerDistancePath;
        public static double DangerDistanceLadle;
        public static double DangerDistanceVortex;
        public static double DistanceSwamp;
        public static double[] LineDirections = new double[5];
        // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线

        public static List<float> CustomLineDirections = [];
        public static List<bool> CustomLineHovers = [];
        public static List<(string, bool)> Inputs = [];

        public static Vector2 LastMousePosition;
        public static Vector2 ScrollPosition;
        public static Vector3 Offset;
        public static Vector2[] ClosestPositions = new Vector2[4];
        // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
        public static Vector2[] DefeatPositions = new Vector2[3];
        // 0 - 路径; 1 - 加水; 2 - 漩涡
        public static Vector3[] VortexGraphical;
        public static List<Vector2> SwampPositions = [];
        public static List<Vector2>[] DangerPositions = [[], [], []];
        // 0 - 路径; 1 - 加水; 2 - 漩涡
        public static List<Vector2>[] IntersectionPositions = [[], [], [], []];
        // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
        public static List<(Vector3, bool)> PathPhysical = [];
        public static List<(Vector3[], bool)> PathGraphical = [];

        public static Rect WindowRect = new(200, 200, 400, 400);
        public static PotionEffectMapItem TargetEffect;
        public static DebugWindow[] DebugWindows = new DebugWindow[9];
        // 0 - 路径信息; 1 - 加水信息; 2 - 移动信息; 3 - 目标效果; 4 - 酿造信息
        // 5 - 效果偏离; 6 - 活跃漩涡; 7 - 目标漩涡; 8 - 研磨信息
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

        public struct Localization
        {
            public string key;
            public Dictionary<string, string> values;
        }
        #endregion
    }
}
