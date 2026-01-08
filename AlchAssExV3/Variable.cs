using BepInEx.Configuration;

namespace AlchAssExV3
{
    public static class Variable
    {
        #region 配置选项
        public static ConfigEntry<bool> QuantiHeating;
        public static ConfigEntry<bool> QuantiGrinding;
        public static ConfigEntry<bool> enableVortexEdgeControl;
        public static ConfigEntry<bool> enableClosestPointControl;
        public static ConfigEntry<bool> enableTargetProximityControl;
        public static ConfigEntry<bool> enableGrindSpeed;
        public static ConfigEntry<bool> enableStirSpeed;
        public static ConfigEntry<bool> enableLadleSpeed;
        public static ConfigEntry<bool> enableHeatSpeed;
        public static ConfigEntry<bool> enableMassBrewing;

        public static ConfigEntry<KeyboardShortcut> KeyVortexEdgeControl;
        public static ConfigEntry<KeyboardShortcut> KeyClosestPointControl;
        public static ConfigEntry<KeyboardShortcut> KeyTargetProximityControl;
        public static ConfigEntry<KeyboardShortcut> KeyMinorSlowDown;
        public static ConfigEntry<KeyboardShortcut> KeyMajorSlowDown;

        //public static ConfigEntry<float> HeatValue;
        //public static ConfigEntry<float> GrindValue;
        public static ConfigEntry<float> ControlAreaThreshold;
        public static ConfigEntry<float> ControlSlowdownStrength;
        public static ConfigEntry<float> ControlAsymptoteFactor;
        public static ConfigEntry<float> MajorSlowdownFactor;
        public static ConfigEntry<float> MinorSlowdownFactor;
        public static ConfigEntry<int> MajorBrewingMultiplier;
        public static ConfigEntry<int> MinorBrewingMultiplier;
        #endregion

        #region 状态变量
        public static bool resetWhenLoading = false;
        public static bool vortexEdgeControl = false;
        public static bool closestPointControl = false;
        public static bool targetProximityControl = false;

        public static float vortexEdgeSpeed = float.MaxValue;
        public static float[] closestPointspeed = [float.MaxValue, float.MaxValue];
        public static float[] targetProximitySpeed = [float.MaxValue, float.MaxValue, float.MaxValue];
        #endregion

        #region 只读数据
        public static readonly string functionPath = "AlchAssExFunctionsConfig.txt";
        #endregion

        #region 缓存数据
        public static float HeatValue = 100f;
        public static float GrindValue = 100f;
        public static bool _functionCacheValid = false;
        #endregion
    }
}
