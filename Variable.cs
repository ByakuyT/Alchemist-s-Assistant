using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using UnityEngine;

namespace AlchAssV4;

public static class Variable
{
    #region 配置数据

    public static ConfigEntry<KeyboardShortcut> KeyMenu;
    public static ConfigEntry<KeyboardShortcut> KeyEffect;
    public static ConfigEntry<KeyboardShortcut> KeyVortex;
    public static ConfigEntry<KeyboardShortcut> KeyCustomLine;
    public static ConfigEntry<KeyboardShortcut> KeyCustomMarker;
    public static ConfigEntry<float> LineWidth;
    public static ConfigEntry<float> NodeSize;
    public static ConfigEntry<float> WindowScale;
    public static ConfigEntry<Color> ColorRange;
    public static ConfigEntry<Color> ColorSpiral;
    public static ConfigEntry<Color> ColorIntersection;
    public static ConfigEntry<Color> ColorClosest;
    public static ConfigEntry<Color> ColorDefeat;
    public static ConfigEntry<Color> ColorHandoff;
    public static ConfigEntry<Color> ColorCustomLineNormal;
    public static ConfigEntry<Color> ColorCustomLineHover;
    public static ConfigEntry<Color> ColorCustomMarkerNormal;
    public static ConfigEntry<Color> ColorCustomMarkerHover;

    public static ConfigEntry<Color>[] ColorPaths = new ConfigEntry<Color>[2];

    // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线
    public static ConfigEntry<Color>[] ColorLines = new ConfigEntry<Color>[5];

    // 0 - 路径信息; 1 - 加水信息; 2 - 移动信息; 3 - 目标效果; 4 - 酿造信息
    // 5 - 效果偏离; 6 - 活跃漩涡; 7 - 目标漩涡; 8 - 研磨信息
    public static ConfigEntry<Vector3>[] WindowPositions = new ConfigEntry<Vector3>[9];
    public static ConfigEntry<Rect> MenuRectConfig;

    #endregion

    #region 只读数据

    public const double EffectRad = 1.53;
    public const double IndicatorRad = 0.74;
    public const double OccCell = 0.5;
    public const double BoxSkin = 0.01;
    public const double DangerStrongDamage = 0.4;
    public static readonly Font Font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);

    public static readonly string[] WindowTags =
        ["路径信息", "加水信息", "移动信息", "目标效果", "酿造信息", "效果偏离", "活跃漩涡", "目标漩涡", "研磨信息"];

    public static readonly string[] VortexTags = ["Vortex_Water", "Vortex_Oil"];

    public static readonly string[] ZoneTags =
        ["Strong_Water", "Strong_Oil", "Strong_Wine", "Weak_Wine", "Heal_Wine", "Swamp_Oil"];

    public static readonly Dictionary<string, int> MapId = new() { { "Water", 0 }, { "Oil", 1 }, { "Wine", 2 } };

    #endregion

    #region 文件数据

    // 0 - 水; 1 - 油
    public static readonly List<Vortex>[] Vortices = [[], []];

    // 0 - 水强危险区; 1 - 油强危险区; 2 - 酒强危险区; 3 - 弱危险区; 4 - 治疗区; 5 - 沼泽区
    public static readonly List<ZoneCircle>[] Circles = [[], [], [], [], [], []];

    // 0 - 水强危险区; 1 - 油强危险区; 2 - 酒强危险区; 3 - 弱危险区; 4 - 治疗区; 5 - 沼泽区
    public static readonly List<ZoneBox>[] Boxes = [[], [], [], [], [], []];

    // 0 - 水强危险区; 1 - 油强危险区; 2 - 酒强危险区; 3 - 弱危险区; 4 - 治疗区; 5 - 沼泽区
    public static readonly List<ZoneBvh>[] BVH = [[], [], [], [], [], []];

    // 0 - 水强危险区; 1 - 油强危险区; 2 - 酒强危险区; 3 - 弱危险区; 4 - 治疗区; 5 - 沼泽区
    public static readonly int[][,] OccSum = new int[6][,];
    public static readonly Vec2D[] OccOrigin = new Vec2D[6];

    #endregion

    #region 渲染材质

    public static Texture2D MenuTexture;
    public static Material SolidMaterial;
    public static Material DashedMaterial;
    public static Sprite RoundSprite;
    public static Sprite SquareSprite;
    public static Rect MenuRect;
    public static Vector2 ScrollPosition;
    public static float CustomLineLabelWidth;
    public static float CustomMarkerLabelWidth;
    public static GUIStyle MenuStyle;
    public static GUIStyle CategoryStyle;
    public static GUIStyle ToggleStyle;
    public static GUIStyle ButtonStyle;
    public static GUIStyle DeleteButtonStyle;
    public static GUIStyle TextFieldStyle;
    public static GUIStyle TextFieldErrorStyle;
    public static GUIStyle LabelStyle;
    public static GUIStyle AxisStyle;
    public static GUIStyle SliderStyle;
    public static LineRenderer VortexRange;
    public static LineRenderer IndicatorRange;
    public static LineRenderer IndicatorDirection;
    public static LineRenderer EffectRangeOuter;
    public static LineRenderer EffectRangeMiddle;
    public static LineRenderer VortexCurve;
    public static LineRenderer[] Lines = new LineRenderer[5];
    public static List<LineRenderer> PathCurves = [];
    public static List<LineRenderer> CustomLines = [];
    public static List<(LineRenderer, LineRenderer)> CustomMarkers = [];
    public static SpriteRenderer EffectDiskMiddle;
    public static SpriteRenderer EffectDiskInner;

    public static SpriteRenderer BaseLadleRenderer;

    // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
    public static SpriteRenderer[] ClosestPoints = new SpriteRenderer[4];

    // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
    public static List<SpriteRenderer>[] IntersectionPoints = [[], [], [], []];

    // 0 - 路径; 1 - 加水; 2 - 漩涡
    public static List<SpriteRenderer>[] DangerPoints = [[], [], []];

    // 0 - 路径; 1 - 加水; 2 - 漩涡
    public static SpriteRenderer[] DefeatPoints = new SpriteRenderer[3];
    public static List<SpriteRenderer> SwampPoints = [];
    public static SpriteRenderer[] HandoffPoints = new SpriteRenderer[2];

    #endregion

    #region 功能开关

    public static bool ShowMenu = false;
    public static bool EnableExpand = true;
    public static bool DisplayExpand = true;
    public static bool ActionExpand = true;
    public static bool HandoffExpand = true;
    public static bool CustomLineListExpand = true;
    public static bool CustomMarkerListExpand = true;
    public static bool EnablePathLine = false;
    public static bool EnableLadleLine = false;
    public static bool EnableEffectLine = false;
    public static bool EnableVortexLine = false;
    public static bool EnableTangentLine = false;
    public static bool EnableCustomLine = false;
    public static bool EnableCustomMarker = false;
    public static bool EnablePathCurve = false;
    public static bool EnableVortexCurve = false;
    public static bool EnableEffectRange = false;
    public static bool EnableVortexRange = false;
    public static bool EnableDangerSimulation = false;
    public static bool EnableSwampSimulation = false;
    public static bool EnableTransparency = false;
    public static bool DoPathCurve = false;
    public static bool DoVortexCurve = false;
    public static bool DoCustomLine = false;
    public static bool DoCustomMarker = false;
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

    // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线
    public static bool[] DoLines = [false, false, false, false, false];
    public static bool DisplaySalt = false;
    public static bool DisplayStage = false;
    public static bool DisplayOffset = false;
    public static bool DisplayPolar = false;

    #endregion

    #region 状态数据

    public static bool IsResizing = false;
    public static int HandoffRunning = -1;
    public static Vector2 LastMousePos;
    public static Vec2D Offset;
    public static PotionEffectMapItem TargetEffect;
    public static int TargetLineIndex;
    public static int TargetMarkerIndex;
    public static double? SwampDistance;
    public static List<double> CustomLineDirections = [];
    public static List<bool> CustomLineHovers = [];
    public static List<(string, bool)> CustomLineInputs = [];
    public static List<Vec2D> CustomMarkerPositions = [];
    public static List<bool> CustomMarkerHovers = [];

    public static List<(string, string, bool, bool)> CustomMarkerInputs = [];

    // 0 - 路径; 1 - 加水;
    public static bool[] ClosestUseOffset = new bool[2];

    // 0 - 水; 1 - 油
    public static int[] VortexIndex = [-1, -1];

    // 0 - 效果; 1 - 漩涡
    public static Vec2D?[] HandoffPositions = [null, null];

    // 0 - 路径; 1 - 加水; 2 - 漩涡
    public static Vec2D?[] DefeatPositions = [null, null, null];

    // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
    public static List<Vec2D>[] IntersectionPositions = [[], [], [], []];

    // 0 - 路径; 1 - 加水; 2 - 漩涡
    public static List<Vec2D>[] DangerPositions = [[], [], []];

    public static List<Vec2D> SwampPositions = [];

    // 0 - 路径和效果; 1 - 路径和漩涡; 2 - 加水和效果; 3 - 加水和漩涡
    public static Vec2D?[] ClosestPositions = [null, null, null, null];
    public static List<(Vec2D, bool)> PathPhysical = [];
    public static List<(Vector3[], bool)> PathGraphical = [];
    public static Spiral? SpiralPhysical;

    public static Vector3[] SpiralGraphical;

    // 0 - 路径切向线; 1 - 加水方向线; 2 - 效果径向线; 3 - 漩涡径向线; 4 - 漩涡切向线
    public static double?[] LineDirections = [null, null, null, null, null];

    //0 - 路径; 1 - 加水; 2 - 漩涡
    public static double?[] DangerDistance = [null, null, null];

    // 0 - 路径信息; 1 - 加水信息; 2 - 移动信息; 3 - 目标效果; 4 - 酿造信息
    // 5 - 效果偏离; 6 - 活跃漩涡; 7 - 目标漩涡; 8 - 研磨信息
    public static DebugWindow[] DebugWindows = new DebugWindow[9];

    #endregion

    #region 数据结构

    /// <summary>
    /// 本地化文本
    /// </summary>
    public struct Localization
    {
        public string key;
        public Dictionary<string, string> values;
    }

    /// <summary>
    /// 漩涡
    /// </summary>
    public struct Vortex
    {
        public Vec2D Pos;
        public double Rad;
    }

    /// <summary>
    /// 圆形区域零件
    /// </summary>
    public struct ZoneCircle
    {
        public Vec2D Pos;
        public double Rad;
    }

    /// <summary>
    /// 矩形区域零件
    /// </summary>
    public struct ZoneBox
    {
        public Vec2D Pos;
        public double XHalf, YHalf, Rot;
    }

    /// <summary>
    /// 区域零件 BVH 节点
    /// </summary>
    public struct ZoneBvh
    {
        public double XMin, YMin, XMax, YMax;
        public int Left, Right;
        public List<int> Circles;
        public List<int> Boxes;
    }

    /// <summary>
    /// 漩涡螺线
    /// </summary>
    public struct Spiral
    {
        public Vec2D Pos;
        public double Rot, MaxAngle, MinAngle;
        public static double A => 1.0 / (2.0 * Math.PI);
    }

    /// <summary>
    /// double向量
    /// </summary>
    public readonly struct Vec2D(double x, double y)
    {
        public readonly double X = x, Y = y;
        public static Vec2D From(Vector3 v) => new(v.x, v.y);
        public static Vec2D From(Vector2 v) => new(v.x, v.y);
        public Vector3 ToVec3() => new((float)X, (float)Y, 0f);
        public Vector2 ToVec2() => new((float)X, (float)Y);
        public static readonly Vec2D Zero = new(0.0, 0.0);
        public static readonly Vec2D One = new(1.0, 1.0);
        public static readonly Vec2D Up = new(0.0, 1.0);
        public static readonly Vec2D Down = new(0.0, -1.0);
        public static readonly Vec2D Right = new(1.0, 0.0);
        public static readonly Vec2D Left = new(-1.0, 0.0);
        public double SqrMagnitude => X * X + Y * Y;
        public double Magnitude => Math.Sqrt(SqrMagnitude);
        public double Dot(Vec2D v) => X * v.X + Y * v.Y;
        public double Cross(Vec2D v) => X * v.Y - Y * v.X;
        public double Distance(Vec2D v) => (this - v).Magnitude;
        public double SignedAngle(Vec2D v) => Math.Atan2(Cross(v), Dot(v)) * (180.0 / Math.PI);
        public static Vec2D operator +(Vec2D a, Vec2D b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec2D operator -(Vec2D a, Vec2D b) => new(a.X - b.X, a.Y - b.Y);
        public static Vec2D operator *(Vec2D v, double k) => new(v.X * k, v.Y * k);
        public static Vec2D operator -(Vec2D v) => new(-v.X, -v.Y);
        public static Vec2D operator *(double k, Vec2D v) => v * k;
    }

    /// <summary>
    /// 多项式
    /// </summary>
    public readonly struct Poly(params double[] cofs)
    {
        private readonly double[] _cofs = cofs == null ? null : Trim(cofs);

        private static double[] Trim(double[] cofs)
        {
            var n = cofs.Length;
            while (n > 0 && cofs[n - 1] == 0.0) n--;
            if (n == 0) return null;
            var t = new double[n];
            Array.Copy(cofs, t, n);
            return t;
        }

        public int Degree => _cofs is null ? -1 : _cofs.Length - 1;
        public double Coef(int k) => k > Degree ? 0.0 : _cofs[k];

        public double Evaluate(double t)
        {
            if (Degree < 0) return 0.0;
            var r = 0.0;
            for (var i = Degree; i >= 0; i--)
                r = r * t + _cofs[i];
            return r;
        }

        public Poly NormalizedInf()
        {
            if (Degree < 0) return default;
            var r = _cofs.Select(Math.Abs).Prepend(0.0).Max();
            return this * (1 / r);
        }

        public Poly Derivative()
        {
            if (Degree < 1) return default;
            var t = new double[Degree];
            for (var i = 1; i <= Degree; i++)
                t[i - 1] = _cofs[i] * i;
            return new Poly(t);
        }

        public static Poly operator *(Poly a, double k)
        {
            if (a.Degree < 0 || k == 0.0) return default;
            var t = new double[a.Degree + 1];
            for (var i = 0; i <= a.Degree; i++)
                t[i] = a._cofs[i] * k;
            return new Poly(t);
        }

        public static Poly operator *(double k, Poly a) => a * k;
        public static Poly operator -(Poly a) => a * -1.0;

        public static Poly operator +(Poly a, Poly b)
        {
            if (a.Degree < 0) return b;
            if (b.Degree < 0) return a;
            var t = new double[Math.Max(a.Degree, b.Degree) + 1];
            for (var i = 0; i < t.Length; i++)
                t[i] = (i <= a.Degree ? a._cofs[i] : 0.0) + (i <= b.Degree ? b._cofs[i] : 0.0);
            return new Poly(t);
        }

        public static Poly operator -(Poly a, Poly b)
        {
            if (a.Degree < 0) return -b;
            if (b.Degree < 0) return a;
            var t = new double[Math.Max(a.Degree, b.Degree) + 1];
            for (var i = 0; i < t.Length; i++)
                t[i] = (i <= a.Degree ? a._cofs[i] : 0.0) - (i <= b.Degree ? b._cofs[i] : 0.0);
            return new Poly(t);
        }

        public static Poly operator *(Poly a, Poly b)
        {
            if (a.Degree < 0 || b.Degree < 0) return default;
            var t = new double[a.Degree + b.Degree + 1];
            for (var i = 0; i <= a.Degree; i++)
            for (var j = 0; j <= b.Degree; j++)
                t[i + j] += a._cofs[i] * b._cofs[j];
            return new Poly(t);
        }
    }

    public readonly struct Interval
    {
        public readonly double L, H;

        public Interval(double l, double h)
        {
            if (double.IsNaN(l) || double.IsNaN(h) || l > h)
            {
                L = double.NaN;
                H = double.NaN;
            }
            else
            {
                L = l;
                H = h;
            }
        }

        public bool IsEmpty => double.IsNaN(L) || double.IsNaN(H);
        public static Interval Empty => new(double.NaN, double.NaN);
        public static Interval Point(double x) => new(x, x);

        public static Interval operator -(Interval a) =>
            a.IsEmpty ? Empty : new(Algebra.Down(-a.H), Algebra.Up(-a.L));

        public static Interval operator *(Interval a, double k)
        {
            if (a.IsEmpty || double.IsNaN(k)) return Empty;
            if (k == 0.0) return new(0.0, 0.0);
            return k > 0.0
                ? new(Algebra.Down(a.L * k), Algebra.Up(a.H * k))
                : new(Algebra.Down(a.H * k), Algebra.Up(a.L * k));
        }

        public static Interval operator *(double k, Interval a) => a * k;

        public static Interval operator +(Interval a, Interval b)
        {
            if (a.IsEmpty || b.IsEmpty) return Empty;
            return new(Algebra.Down(a.L + b.L), Algebra.Up(a.H + b.H));
        }

        public static Interval operator -(Interval a, Interval b)
        {
            if (a.IsEmpty || b.IsEmpty) return Empty;
            return new(Algebra.Down(a.L - b.H), Algebra.Up(a.H - b.L));
        }

        public static Interval operator *(Interval a, Interval b)
        {
            if (a.IsEmpty || b.IsEmpty) return Empty;
            var l = Math.Min(Math.Min(Algebra.Down(a.L * b.L), Algebra.Down(a.L * b.H)),
                Math.Min(Algebra.Down(a.H * b.L), Algebra.Down(a.H * b.H)));
            var h = Math.Max(Math.Max(Algebra.Up(a.L * b.L), Algebra.Up(a.L * b.H)),
                Math.Max(Algebra.Up(a.H * b.L), Algebra.Up(a.H * b.H)));
            return new(l, h);
        }

        public Interval Sqrt()
        {
            if (IsEmpty || H <= 0.0) return Empty;
            var l = L <= 0.0 ? 0.0 : Algebra.Down(Math.Sqrt(L));
            var h = Algebra.Up(Math.Sqrt(H));
            return new(l, h);
        }

        public Interval Sin()
        {
            if (IsEmpty) return Empty;
            var l = Math.Min(Algebra.Down(Math.Sin(L)), Algebra.Down(Math.Sin(H)));
            var h = Math.Max(Algebra.Up(Math.Sin(L)), Algebra.Up(Math.Sin(H)));
            if (Contains(Math.PI / 2, 2 * Math.PI)) h = 1.0;
            if (Contains(-Math.PI / 2, 2 * Math.PI)) l = -1.0;
            return new(l, h);
        }

        public Interval Cos()
        {
            if (IsEmpty) return Empty;
            var l = Math.Min(Algebra.Down(Math.Cos(L)), Algebra.Down(Math.Cos(H)));
            var h = Math.Max(Algebra.Up(Math.Cos(L)), Algebra.Up(Math.Cos(H)));
            if (Contains(0.0, 2 * Math.PI)) h = 1.0;
            if (Contains(Math.PI, 2 * Math.PI)) l = -1.0;
            return new(l, h);
        }

        private bool Contains(double baseVal, double period)
        {
            var k = Math.Ceiling((L - baseVal) / period);
            return baseVal + k * period <= H;
        }
    }

    #endregion
}