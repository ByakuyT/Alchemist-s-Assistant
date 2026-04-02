using BepInEx.Configuration;

namespace AlchAssExV3
{
    public static class VariableEx
    {
        #region 配置数据
        public static ConfigEntry<KeyboardShortcut>[] KeyManuals = new ConfigEntry<KeyboardShortcut>[3];
        public static ConfigEntry<KeyboardShortcut> KeyAutoPause;

        public static ConfigEntry<float> AutoThreshold;
        public static ConfigEntry<float> AutoStrength;
        public static ConfigEntry<float> AutoCrossSpeed;
        public static ConfigEntry<float> AutoMinSpeed;

        public static ConfigEntry<float>[] ConfigGrindSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigStirSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigLadleSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigHeatSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<int>[] ConfigBrewBulk = new ConfigEntry<int>[3];
        #endregion

        #region 只读数据
        public static readonly float[] EffectLevels = [0f, 100f, 600f, 2754f];
        public static readonly float[] Speed = [10f, 1f, 0.1f];
        public static readonly int[] Bulk = [10, 20, 50];
        #endregion

        #region 窗口样式
        public static float LabelWidthAutos = float.NaN;
        public static float LabelWidthManuals = float.NaN;
        #endregion

        #region 功能开关
        public static bool EnableQuantitativeHeating = false;
        public static bool EnableQuantitativeGrinding = false;
        public static bool EnableSlowGrinding = false;
        public static bool EnableSlowHeating = false;
        public static bool EnableSlowLadling = false;
        public static bool EnableSlowStirring = false;
        public static bool EnableBulkBrewing = false;

        public static bool EnableQuantitativeStirring = false;
        public static bool EnableQuantitativeLadling = false;
        public static bool EnableQuantitativeRestoring = false;
        public static bool EnableEffectIntersection = false;
        public static bool EnableEffectAlignment = false;
        public static bool EnableVortexIntersection = false;
        public static bool EnableVortexAlignment = false;
        public static bool EnableDangerIntersection = false;
        public static bool EnableDangerAlignment = false;
        public static bool EnableSwampIntersection = false;
        public static bool EnableSwampAlignment = false;

        public static bool AutoEnableExpand = true;
        public static bool ManualEnableExpand = true;
        public static bool AutoSettingExpand = true;
        public static bool[] ManualSettingExpand = [true, false, false];
        #endregion

        #region 控制选项
        public static (string, bool)[] InputGrindSpeed = new (string, bool)[3];
        public static (string, bool)[] InputStirSpeed = new (string, bool)[3];
        public static (string, bool)[] InputLadleSpeed = new (string, bool)[3];
        public static (string, bool)[] InputHeatSpeed = new (string, bool)[3];
        public static (string, bool)[] InputBrewBulk = new (string, bool)[3];

        public static (string, bool) InputGrindingTarget;
        public static (string, bool) InputHeatingTarget;
        public static (string, bool) InputEffectDeviation;
        public static (string, bool) InputHealthThreshold;
        public static (string, bool) InputStirringLength;
        public static (string, bool) InputLadlingLength;
        public static (string, bool) InputRestoringAngle;

        public static float AutoStirSpeed;
        public static float AutoLadleSpeed;
        public static float AutoHeatSpeed;
        public static float GrindingTarget;
        public static float HeatingTarget;
        public static float GrindSpeed;
        public static float HeatSpeed;
        public static float LadleSpeed;
        public static float StirSpeed;
        public static int BrewBulk;

        public static float StirringLength;
        public static float LadlingLength;
        public static float RestoringAngle;
        public static float StirringPrevious;
        public static float LadlingPrevious;
        public static float RestoringPrevious;
        public static float DeviationRotation;
        public static float HealthThreshold;
        public static float EffectDeviation;

        public static float SpeedEffect = float.MaxValue;
        public static float SpeedVortex = float.MaxValue;
        public static float SpeedDanger = float.MaxValue;
        public static float SpeedSwamp = float.MaxValue;
        public static float StirSpeedQuantitative = float.MaxValue;
        public static float StirSpeedEffect = float.MaxValue;
        public static float StirSpeedVortex = float.MaxValue;
        public static float StirSpeedDanger = float.MaxValue;
        public static float LadleSpeedQuantitative = float.MaxValue;
        public static float LadleSpeedRestoring = float.MaxValue;
        public static float LadleSpeedEffect = float.MaxValue;
        public static float LadleSpeedVortex = float.MaxValue;
        public static float LadleSpeedDanger = float.MaxValue;
        public static float LadleSpeedSwamp = float.MaxValue;
        public static float HeatSpeedEffect = float.MaxValue;
        public static float HeatSpeedVortex = float.MaxValue;
        public static float HeatSpeedDanger = float.MaxValue;
        public static float HeatSpeedSwamp = float.MaxValue;
        #endregion

        #region 边界记录
        public static bool EffectIntersectionPrevious;
        public static bool VortexIntersectionPrevious;
        public static bool DangerIntersectionPrevious;
        public static bool SwampIntersectionPrevious;
        public static bool EffectAlignmentStirPrevious;
        public static bool VortexAlignmentStirPrevious;
        public static bool DangerAlignmentStirPrevious;
        public static bool SwampAlignmentStirPrevious;
        public static bool EffectAlignmentLadlePrevious;
        public static bool VortexAlignmentLadlePrevious;
        public static bool DangerAlignmentLadlePrevious;
        public static bool DangerAlignmentHeatPrevious;
        public static bool DangerOutStirPrevious;
        public static bool DangerOutLadlePrevious;
        public static bool DangerOutHeatPrevious;
        public static bool SwampOutStirPrevious;
        #endregion
    }
}
