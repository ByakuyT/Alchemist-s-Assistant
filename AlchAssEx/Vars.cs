using BepInEx.Configuration;

namespace AlchAssEx
{
    public static class Vars
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
        #endregion

        #region 状态变量
        public static bool vortexEdgeControl = false;
        public static bool closestPointControl = false;
        public static bool targetProximityControl = false;

        public static float vortexEdgeSpeed = float.MaxValue;
        public static float[] closestPointspeed = [float.MaxValue, float.MaxValue];
        public static float[] targetProximitySpeed = [float.MaxValue, float.MaxValue, float.MaxValue];
        #endregion

        #region 只读数据
        public static readonly string functionPath = "AlchAssFunctionsConfig.txt";
        #endregion

        #region 缓存数据
        public static float _cachedHeatValue = 100f;
        public static float _cachedGrindValue = 100f;
        public static float _cachedControlAreaThreshold = 0.05f;
        public static float _cachedControlSlowdownStrength = 1.35f;
        public static float _cachedControlAsymptoteFactor = 1.5e-3f;
        public static float _cachedSlowdownFactorX = 100f;
        public static float _cachedSlowdownFactorZ = 10f;
        public static int _cachedBrewingMultiplierX = 100;
        public static int _cachedBrewingMultiplierZ = 10;
        public static bool _functionCacheValid = false;
        #endregion
    }
}
