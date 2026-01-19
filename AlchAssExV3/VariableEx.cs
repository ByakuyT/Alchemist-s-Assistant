using BepInEx.Configuration;

namespace AlchAssExV3
{
    public static class VariableEx
    {
        #region 配置数据
        public static ConfigEntry<KeyboardShortcut>[] KeyLevels = new ConfigEntry<KeyboardShortcut>[3];
        public static ConfigEntry<KeyboardShortcut> KeyRelease;

        public static ConfigEntry<float> ControlThreshold;
        public static ConfigEntry<float> ControlStrength;
        public static ConfigEntry<float> ControlAsymptote;
        public static ConfigEntry<float> ControlAdsorption;
        public static ConfigEntry<float> ControlEnterSpeed;
        public static ConfigEntry<float> ControlMinSpeed;

        public static ConfigEntry<float>[] ConfigGrindSet = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigHeatSet = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigGrindSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigStirSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigLadleSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<float>[] ConfigHeatSpeed = new ConfigEntry<float>[3];
        public static ConfigEntry<int>[] ConfigBrewMassive = new ConfigEntry<int>[3];
        #endregion

        #region 功能开关
        public static bool EnableGrindSet = false;
        public static bool EnableHeatSet = false;
        public static bool EnableGrindSpeed = false;
        public static bool EnableStirSpeed = false;
        public static bool EnableLadleSpeed = false;
        public static bool EnableHeatSpeed = false;
        public static bool EnableBrewMassive = false;

        public static bool EnableEdgeControl = false;
        public static bool EnableClosestControl = false;
        public static bool EnableProximityControl = false;

        public static bool EnableExpand = true;
        public static bool L1Expand = true;
        public static bool L2Expand = false;
        public static bool L3Expand = false;

        public static bool LoadReset = false;
        #endregion

        #region 状态数据
        public static float GrindSet;
        public static float HeatSet;
        public static float GrindSpeed;
        public static float StirSpeed;
        public static float LadleSpeed;
        public static float HeatSpeed;
        public static int BrewMassive;

        public static float[] ClosestSpeed = new float[2];
        public static float[] ProximitySpeed = new float[2];
        public static float EdgeSpeed;
        // 0 - 搅拌; 1 - 加水

        public static (string, bool)[] InputGrindSet = new (string, bool)[3];
        public static (string, bool)[] InputHeatSet = new (string, bool)[3];
        public static (string, bool)[] InputGrindSpeed = new (string, bool)[3];
        public static (string, bool)[] InputStirSpeed = new (string, bool)[3];
        public static (string, bool)[] InputLadleSpeed = new (string, bool)[3];
        public static (string, bool)[] InputHeatSpeed = new (string, bool)[3];
        public static (string, bool)[] InputBrewMassive = new (string, bool)[3];
        #endregion
    }
}
