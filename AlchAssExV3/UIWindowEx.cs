using AlchAssV3;
using PotionCraft.LocalizationSystem;
using UnityEngine;

namespace AlchAssExV3
{
    public static class UIWindowEx
    {
        #region 界面渲染
        /// <summary>
        /// 绘制手动控制开关
        /// </summary>
        public static void DrawManualEnables()
        {
            GUILayout.Space(10);
            var icon = $"{(VariableEx.ManualEnableExpand ? "▼" : "▲")} {LocalizationManager.GetText("基础控制选项")}";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.ManualEnableExpand = !VariableEx.ManualEnableExpand;

            if (VariableEx.ManualEnableExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                VariableEx.EnableSlowStirring = GUILayout.Toggle(VariableEx.EnableSlowStirring, LocalizationManager.GetText("慢速搅拌"), Variable.ToggleStyle);
                VariableEx.EnableSlowLadling = GUILayout.Toggle(VariableEx.EnableSlowLadling, LocalizationManager.GetText("慢速加水"), Variable.ToggleStyle);
                VariableEx.EnableSlowGrinding = GUILayout.Toggle(VariableEx.EnableSlowGrinding, LocalizationManager.GetText("慢速研磨"), Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                VariableEx.EnableSlowHeating = GUILayout.Toggle(VariableEx.EnableSlowHeating, LocalizationManager.GetText("慢速加热"), Variable.ToggleStyle);
                VariableEx.EnableBulkBrewing = GUILayout.Toggle(VariableEx.EnableBulkBrewing, LocalizationManager.GetText("大批量酿造"), Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制手动控制开关
        /// </summary>
        public static void DrawAutoEnables()
        {
            GUILayout.Space(10);
            var icon = $"{(VariableEx.AutoEnableExpand ? "▼" : "▲")} {LocalizationManager.GetText("自动控制选项")}";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.AutoEnableExpand = !VariableEx.AutoEnableExpand;

            if (VariableEx.AutoEnableExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                VariableEx.EnableQuantitativeStirring = GUILayout.Toggle(VariableEx.EnableQuantitativeStirring, LocalizationManager.GetText("定量搅拌"), Variable.ToggleStyle);
                VariableEx.EnableQuantitativeLadling = GUILayout.Toggle(VariableEx.EnableQuantitativeLadling, LocalizationManager.GetText("定量加水"), Variable.ToggleStyle);
                VariableEx.EnableQuantitativeRestoring = GUILayout.Toggle(VariableEx.EnableQuantitativeRestoring, LocalizationManager.GetText("定量回正"), Variable.ToggleStyle);
                VariableEx.EnableQuantitativeGrinding = GUILayout.Toggle(VariableEx.EnableQuantitativeGrinding, LocalizationManager.GetText("定量研磨"), Variable.ToggleStyle);
                VariableEx.EnableQuantitativeHeating = GUILayout.Toggle(VariableEx.EnableQuantitativeHeating, LocalizationManager.GetText("定量加热"), Variable.ToggleStyle);
                VariableEx.EnableEffectIntersection = GUILayout.Toggle(VariableEx.EnableEffectIntersection, LocalizationManager.GetText("效果交会点"), Variable.ToggleStyle);
                VariableEx.EnableEffectAlignment = GUILayout.Toggle(VariableEx.EnableEffectAlignment, LocalizationManager.GetText("效果临界点"), Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                VariableEx.EnableVortexIntersection = GUILayout.Toggle(VariableEx.EnableVortexIntersection, LocalizationManager.GetText("漩涡交会点"), Variable.ToggleStyle);
                VariableEx.EnableVortexAlignment = GUILayout.Toggle(VariableEx.EnableVortexAlignment, LocalizationManager.GetText("漩涡临界点"), Variable.ToggleStyle);
                VariableEx.EnableDangerIntersection = GUILayout.Toggle(VariableEx.EnableDangerIntersection, LocalizationManager.GetText("骷髅交会点"), Variable.ToggleStyle);
                VariableEx.EnableDangerAlignment = GUILayout.Toggle(VariableEx.EnableDangerAlignment, LocalizationManager.GetText("骷髅临界点"), Variable.ToggleStyle);
                VariableEx.EnableSwampIntersection = GUILayout.Toggle(VariableEx.EnableSwampIntersection, LocalizationManager.GetText("沼泽交会点"), Variable.ToggleStyle);
                VariableEx.EnableSwampAlignment = GUILayout.Toggle(VariableEx.EnableSwampAlignment, LocalizationManager.GetText("沼泽临界点"), Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制手动控制选项
        /// </summary>
        public static void DrawManualSettings()
        {
            for (var i = 0; i < 3; i++)
            {
                GUILayout.Space(10);
                var icon = $"{(VariableEx.ManualSettingExpand[i] ? "▼" : "▲")} {LocalizationManager.GetText($"手动控制配置 {i + 1}")}";
                if (GUILayout.Button(icon, Variable.CategoryStyle))
                    VariableEx.ManualSettingExpand[i] = !VariableEx.ManualSettingExpand[i];

                if (VariableEx.ManualSettingExpand[i])
                {
                    VariableEx.ConfigStirSpeed[i].Value = DrawFloatSlider("搅拌速度", VariableEx.ConfigStirSpeed[i].Value, VariableEx.LabelWidthManuals, 2f, -2f, 0f, 100f, true, ref VariableEx.InputStirSpeed[i]);
                    VariableEx.ConfigLadleSpeed[i].Value = DrawFloatSlider("加水速度", VariableEx.ConfigLadleSpeed[i].Value, VariableEx.LabelWidthManuals, 2f, -2f, 0f, 100f, true, ref VariableEx.InputLadleSpeed[i]);
                    VariableEx.ConfigGrindSpeed[i].Value = DrawFloatSlider("研磨速度", VariableEx.ConfigGrindSpeed[i].Value, VariableEx.LabelWidthManuals, 2f, -2f, 0f, 100f, true, ref VariableEx.InputGrindSpeed[i]);
                    VariableEx.ConfigHeatSpeed[i].Value = DrawFloatSlider("加热速度", VariableEx.ConfigHeatSpeed[i].Value, VariableEx.LabelWidthManuals, 2f, -2f, 0f, 100f, true, ref VariableEx.InputHeatSpeed[i]);
                    VariableEx.ConfigBrewBulk[i].Value = DrawIntSlider("酿造倍率", VariableEx.ConfigBrewBulk[i].Value, VariableEx.LabelWidthManuals, 0f, 3f, 1, int.MaxValue, ref VariableEx.InputBrewBulk[i]);
                }
            }
        }

        /// <summary>
        /// 绘制自动控制选项
        /// </summary>
        public static void DrawAutoSettings()
        {
            GUILayout.Space(10);
            var icon = $"{(VariableEx.AutoSettingExpand ? "▼" : "▲")} {LocalizationManager.GetText("自动控制配置")}";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.AutoSettingExpand = !VariableEx.AutoSettingExpand;

            if (VariableEx.AutoSettingExpand)
            {
                VariableEx.StirringLength = DrawFloatSlider("搅拌长度", VariableEx.StirringLength, VariableEx.LabelWidthAutos, 0f, 30f, 0f, float.MaxValue, false, ref VariableEx.InputStirringLength);
                VariableEx.LadlingLength = DrawFloatSlider("加水长度", VariableEx.LadlingLength, VariableEx.LabelWidthAutos, 0f, 30f, 0f, float.MaxValue, false, ref VariableEx.InputLadlingLength);
                VariableEx.RestoringAngle = DrawFloatSlider("回正角度", VariableEx.RestoringAngle, VariableEx.LabelWidthAutos, 0f, 180f, 0f, 180f, false, ref VariableEx.InputRestoringAngle);
                VariableEx.GrindingTarget = DrawFloatSlider("研磨目标", VariableEx.GrindingTarget, VariableEx.LabelWidthManuals, 0f, 100f, 0f, 100f, false, ref VariableEx.InputGrindingTarget);
                VariableEx.HeatingTarget = DrawFloatSlider("加热目标", VariableEx.HeatingTarget, VariableEx.LabelWidthManuals, 0f, 100f, 0f, 100f, false, ref VariableEx.InputHeatingTarget);
                VariableEx.HealthThreshold = DrawFloatSlider("血量阈值", VariableEx.HealthThreshold, VariableEx.LabelWidthAutos, 0f, 100f, float.MinValue, 100f, false, ref VariableEx.InputHealthThreshold);
                VariableEx.EffectDeviation = DrawPiecewiseSlider("效果偏离", VariableEx.EffectDeviation, VariableEx.LabelWidthAutos, VariableEx.EffectLevels, VariableEx.DeviationRotation, 0f, float.MaxValue, ref VariableEx.InputEffectDeviation);
            }
        }
        #endregion

        #region 滑条渲染
        /// <summary>
        /// 绘制整数滑条
        /// </summary>
        public static int DrawIntSlider(string label, int value, float width, float min, float max, int cmin, int cmax, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{LocalizationManager.GetText(label)}:", Variable.LabelStyle, GUILayout.Width(width));

            var logVal = Mathf.Log10(value);
            var newVal = GUILayout.HorizontalSlider(logVal, min, max, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (newVal != logVal)
            {
                value = (int)Mathf.Pow(10, newVal);
                input = ($"{value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var iptVal = GUILayout.TextField(input.Item1, style);
            if (iptVal != input.Item1)
            {
                if (int.TryParse(iptVal, out var parVal))
                {
                    value = Mathf.Clamp(parVal, cmin, cmax);
                    if (parVal < cmin || parVal > cmax)
                        input = ($"{value}", false);
                    else
                        input = (iptVal, false);
                }
                else
                    input = (iptVal, true);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// 绘制浮点滑条
        /// </summary>
        public static float DrawFloatSlider(string label, float value, float width, float min, float max, float cmin, float cmax, bool pow, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{LocalizationManager.GetText(label)}:", Variable.LabelStyle, GUILayout.Width(width));

            var logVal = pow ? Mathf.Log10(value) : value;
            var newVal = GUILayout.HorizontalSlider(logVal, min, max, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (newVal != logVal)
            {
                value = pow ? Mathf.Pow(10, newVal) : newVal;
                input = ($"{value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var iptVal = GUILayout.TextField(input.Item1, style);
            if (iptVal != input.Item1)
            {
                if (float.TryParse(iptVal, out var parVal))
                {
                    value = Mathf.Clamp(parVal, cmin, cmax);
                    if (parVal < cmin || parVal > cmax)
                        input = ($"{value}", false);
                    else
                        input = (iptVal, false);
                }
                else
                    input = (iptVal, true);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// 绘制分段滑条
        /// </summary>
        public static float DrawPiecewiseSlider(string label, float value, float width, float[] pars, float dev, float cmin, float cmax, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{LocalizationManager.GetText(label)}:", Variable.LabelStyle, GUILayout.Width(width));

            var nomVal = FunctionEx.ValueToNormalized(value, pars);
            var newVal = GUILayout.HorizontalSlider(nomVal, 0f, 1f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (newVal != nomVal)
            {
                value = FunctionEx.NormalizedToValue(newVal, pars);
                input = ($"{value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var iptVal = GUILayout.TextField(input.Item1, style);
            if (iptVal != input.Item1)
            {
                if (float.TryParse(iptVal, out var parVal))
                {
                    value = Mathf.Clamp(parVal, cmin, cmax);
                    if (parVal < cmin || parVal > cmax)
                        input = ($"{value}", false);
                    else
                        input = (iptVal, false);
                }
                else
                    input = (iptVal, true);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            for (var i = 1; i < pars.Length - 1; i++)
                if (GUILayout.Button($"{LocalizationManager.GetText(label)} L{pars.Length - i}", Variable.ButtonStyle))
                {
                    value = Mathf.Max(0f, pars[i] - dev);
                    input = ($"{value}", false);
                }
            GUILayout.EndHorizontal();
            return value;
        }
        #endregion
    }
}
