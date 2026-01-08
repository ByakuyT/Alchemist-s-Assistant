using BepInEx;
using BepInEx.Configuration;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.PotionEffectMapItem;
using PotionCraft.LocalizationSystem;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mono.CSharp;
using JetBrains.Annotations;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace AlchAssV3
{
    public static class Variable
    {
        #region Constants
        public const int MaxDebugWindows = 12;
        public const int MaxAuxiliaryLines = 10;

        public enum SwitchDictionaryKey
        {
            PathLine = 0,
            LadleLine = 1,
            TargetLine = 2,
            VortexLine = 3,
            PathCurve = 4,
            VortexCurve = 5,
            TargetRange = 6,
            VortexRange = 7,
            AreaTracking = 8,
            SwampScaling = 9,
            Transparency = 10,
            PolarMode = 11,
            SaltDegreeMode = 12
        }
        // TODO: WindowDictionaryKeys
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
                new() { tag = "AlchAssV3_path_information", x = -12.5f, y = -2.3f },
                new() { tag = "AlchAssV3_pouring_information", x = -12.5f, y = -4.6f },
                new() { tag = "AlchAssV3_move_information", x = -9.9f, y = -5.2f },
                new() { tag = "AlchAssV3_target_information", x = -4.9f, y = -4.6f },
                new() { tag = "AlchAssV3_position_information", x = -2.4f, y = -5.2f },
                new() { tag = "AlchAssV3_deviation_information", x = -7.5f, y = -5.2f },
                new() { tag = "AlchAssV3_vortex_information", x = 7.6f, y = -4.9f },
                new() { tag = "AlchAssV3_health_information", x = 10.0f, y = -4.9f },
                new() { tag = "AlchAssV3_grinding_information", x = 10.0f, y = -5.9f },
                new() { tag = "AlchAssV3_debug_information", x = 0.0f, y = 0.0f },
                new() { tag = "AlchAssV3_I/O_information", x = 0.0f, y = 0.0f },
                new() { tag = "AlchAssV3_Hotkey_Information", x = -12.7f, y = 7.9f }
            ]
        };
        #endregion

        #region 配置数据
        public static ConfigEntry<double> LineWidth;
        public static ConfigEntry<double> NodeSize;
        public static ConfigEntry<double> WindowScale;
        public static ConfigEntry<KeyboardShortcut> SelectEffectKeyShortcut;
        public static ConfigEntry<KeyboardShortcut> NextVortexKeyShortcut;
        public static ConfigEntry<KeyboardShortcut> PrevVortexKeyShortcut;
        public static ConfigEntry<KeyboardShortcut> NearestVortexKeyShortcut;
        public static ConfigEntry<KeyboardShortcut> UnselectVortexKeyShortcut;

        // 0 - Path; 1 - Ladle; 2 - Move; 3 - Target; 4 - Position; 5 - Deviation; 6 - Vortex; 7 - Health; 8 - Grind
        public static ConfigEntry<bool>[] EnableWindows = new ConfigEntry<bool>[MaxDebugWindows];
        // 0 - PathLine; 1 - LadleLine; 2 - TargetLine; 3 - VortexLine; 4 - PathCurveOdd; 5 - PathCurveEven; 6 - VortexCurve
        // 7 - Range; 8 - ClosestPoint; 9 - DangerPoint; 10 - IntersectionPoint; 11 - DefeatPoint
        public static ConfigEntry<Color>[] Colors = new ConfigEntry<Color>[12];
        public static Dictionary<SwitchDictionaryKey, ConfigEntry<KeyboardShortcut>> SwitchKeyShortcuts = new();
        #endregion

        #region 信息窗口
        // 0 - Path; 1 - Ladle; 2 - Move; 3 - Target; 4 - Position; 5 - Deviation; 6 - Vortex; 7 - Health; 8 - Grind; 9 - Debug 10 - I/O
        public static Vector3[] DebugWindowPos = new Vector3[MaxDebugWindows];
        public static DebugWindow[] DebugWindows = new DebugWindow[MaxDebugWindows];
        public static List<DebugWindow> ActiveDebugWindows = [];
        #endregion

        #region 渲染材质
        public static Material SolidMaterial;
        public static Material DashedMaterial;
        public static Sprite SquareSprite;
        public static Sprite RoundSprite;

        public static LineRenderer VortexCurveRenderer;
        public static LineRenderer VortexRangeRenderer;
        public static LineRenderer[] LineRenderers = new LineRenderer[5];
        public static LineRenderer[] TargetRangeRenderers = new LineRenderer[2];
        public static SpriteRenderer[] TargetDiskRenderers = new SpriteRenderer[2];
        public static SpriteRenderer[] ClosestPointRenderers = new SpriteRenderer[2];
        public static SpriteRenderer[] DefeatPointRenderers = new SpriteRenderer[3];
        public static List<LineRenderer> PathCurveRenderers = [];
        public static List<SpriteRenderer> SwampPointRenderers = [];
        public static List<SpriteRenderer>[] IntersectionPointRenderers = [[], [], [], []];
        public static List<SpriteRenderer>[] DangerPointRenderers = [[], [], []];
        #endregion

        #region 功能开关
        public static string KeyMode = "Normal";

        public static BaseSwitch SwitchPathLine = new BaseSwitch();
        public static BaseSwitch SwitchLadleLine = new BaseSwitch();
        public static BaseSwitch SwitchTargetLine = new BaseSwitch();
        public static BaseSwitch SwitchVortexLine = new BaseSwitch();
        public static BaseSwitch SwitchPathCurve = new BaseSwitch();
        public static BaseSwitch SwitchVortexCurve = new BaseSwitch();
        public static BaseSwitch SwitchTargetRange = new BaseSwitch();
        public static BaseSwitch SwitchVortexRange = new BaseSwitch();
        public static BaseSwitch SwitchAreaTracking = new BaseSwitch();
        public static BaseSwitch SwitchSwampScaling = new BaseSwitch();
        public static BaseSwitch SwitchTransparency = new BaseSwitch();
        public static BaseSwitch SwitchPolarMode = new BaseSwitch();
        public static BaseSwitch SwitchSaltDegreeMode = new BaseSwitch();

        public static DerivedSwitch SwitchPathDirection = new DerivedSwitch([SwitchPathLine, SwitchPathCurve]);
        public static DerivedSwitch SwitchPathTargetPoint = new DerivedSwitch([SwitchPathCurve, SwitchTargetRange]);
        public static DerivedSwitch SwitchLadleTargetPoint = new DerivedSwitch([SwitchLadleLine, SwitchTargetRange]);
        public static DerivedSwitch SwitchPathVortexPoint = new DerivedSwitch([SwitchPathCurve, SwitchVortexRange]);
        public static DerivedSwitch SwitchLadleVortexPoint = new DerivedSwitch([SwitchLadleLine, SwitchVortexRange]);
        public static DerivedSwitch SwitchPathDangerPoint = new DerivedSwitch([SwitchPathCurve, SwitchAreaTracking]);
        public static DerivedSwitch SwitchLadleDangerPoint = new DerivedSwitch([SwitchLadleLine, SwitchAreaTracking]);
        public static DerivedSwitch SwitchVortexDangerPoint = new DerivedSwitch([SwitchVortexCurve, SwitchAreaTracking]);
        public static DerivedSwitch SwitchSwampPoint = new DerivedSwitch([SwitchPathCurve, SwitchSwampScaling]);
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
        public static double[] LineDirections = Enumerable.Repeat(double.NaN, 5).ToArray();
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

        public static Queue<double> FloatInput = [];
        public static int FloatInputStage = 0;
        public static string FloatInputStream = "";

        public static LineRenderer[] AuxiliaryLines = new LineRenderer[10];
        public static double[] AuxiliaryLineDirections = Enumerable.Repeat(double.NaN, 10).ToArray();
        #endregion

        #region 辅助结构
        public enum Derives
        {
            And = 0,
            Or = 1
        }

        /// <summary>
        /// Represents an abstract base class for a switch that can report its current state.
        /// </summary>
        /// <remarks>Derive from this class to implement custom switch types that provide their own logic
        /// for determining whether the switch is on or off.</remarks>
        public abstract class Switch
        {
            public abstract bool getState();
        }

        /// <summary>
        /// Represents a basic switch that maintains an on or off state.
        /// </summary>
        public class BaseSwitch : Switch
        {
            public bool state { get; set; }
            public BaseSwitch(bool state = false)
            {
                this.state = state;
            }
            public override bool getState()
            {
                return state;
            }
        }

        /// <summary>
        /// Represents a switch whose state is derived from one or more parent switches using a specified logical
        /// operation.
        /// </summary>
        /// <remarks>The derived state is determined by applying the logical operation specified by the
        /// <see cref="Derives"/> value to the states of the parent switches. This class is useful for modeling
        /// composite switches that depend on the state of multiple other switches.</remarks>
        public class DerivedSwitch : Switch
        {
            public Switch[] parents;
            public Derives derive = Derives.And;
            public DerivedSwitch(Switch[] parents, Derives derive = Derives.And)
            {
                this.parents = parents;
                this.derive = derive;
            }
            public override bool getState() => derive switch
            {
                Derives.And => parents.All(p => p.getState()),
                Derives.Or => parents.Any(p => p.getState()),
                _ => throw new NotImplementedException(),
            };
        }

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
