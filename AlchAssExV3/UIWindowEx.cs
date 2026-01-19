using AlchAssV3;
using BepInEx.Configuration;
using UnityEngine;

namespace AlchAssExV3
{
    public static class UIWindowEx
    {
        #region 元素渲染
        /// <summary>
        /// 绘制功能选项
        /// </summary>
        public static void DrawEnables()
        {
            GUILayout.Space(10);
            var icon = VariableEx.EnableExpand ? "▼ 扩展功能选项" : "▲ 扩展功能选项";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.EnableExpand = !VariableEx.EnableExpand;

            if (VariableEx.EnableExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                VariableEx.EnableGrindSet = GUILayout.Toggle(VariableEx.EnableGrindSet, "定量研磨", Variable.ToggleStyle);
                VariableEx.EnableHeatSet = GUILayout.Toggle(VariableEx.EnableHeatSet, "定量加热", Variable.ToggleStyle);
                VariableEx.EnableGrindSpeed = GUILayout.Toggle(VariableEx.EnableGrindSpeed, "减速研磨", Variable.ToggleStyle);
                VariableEx.EnableStirSpeed = GUILayout.Toggle(VariableEx.EnableStirSpeed, "减速搅拌", Variable.ToggleStyle);
                VariableEx.EnableLadleSpeed = GUILayout.Toggle(VariableEx.EnableLadleSpeed, "减速加水", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                VariableEx.EnableHeatSpeed = GUILayout.Toggle(VariableEx.EnableHeatSpeed, "减速加热", Variable.ToggleStyle); ;
                VariableEx.EnableBrewMassive = GUILayout.Toggle(VariableEx.EnableBrewMassive, "批量炼药", Variable.ToggleStyle);
                VariableEx.EnableEdgeControl = GUILayout.Toggle(VariableEx.EnableEdgeControl, "漩涡制动", Variable.ToggleStyle);
                VariableEx.EnableClosestControl = GUILayout.Toggle(VariableEx.EnableClosestControl, "最近点制动", Variable.ToggleStyle);
                VariableEx.EnableProximityControl = GUILayout.Toggle(VariableEx.EnableProximityControl, "接近点制动", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制变速配置
        /// </summary>
        public static void DrawLevels(string label, int index, ref bool category)
        {
            GUILayout.Space(10);
            var icon = category ? "▼ " + label : "▲ " + label;
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                category = !category;

            if (category)
            {
                DrawFloat("定量研磨:", ref VariableEx.ConfigGrindSet[index], ref VariableEx.InputGrindSet[index]);
                DrawFloat("定量加热:", ref VariableEx.ConfigHeatSet[index], ref VariableEx.InputHeatSet[index]);
                DrawPow("减速研磨:", ref VariableEx.ConfigGrindSpeed[index], ref VariableEx.InputGrindSpeed[index]);
                DrawPow("减速搅拌:", ref VariableEx.ConfigStirSpeed[index], ref VariableEx.InputStirSpeed[index]);
                DrawPow("减速加水:", ref VariableEx.ConfigLadleSpeed[index], ref VariableEx.InputLadleSpeed[index]);
                DrawPow("减速加热:", ref VariableEx.ConfigHeatSpeed[index], ref VariableEx.InputHeatSpeed[index]);
                DrawInt("批量炼药:", ref VariableEx.ConfigBrewMassive[index], ref VariableEx.InputBrewMassive[index]);
            }
        }
        #endregion

        #region 滑条渲染
        /// <summary>
        /// 绘制整数滑条
        /// </summary>
        public static void DrawInt(string label, ref ConfigEntry<int> config, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Variable.LabelStyle, GUILayout.Width(75));

            var value = Mathf.Log(config.Value, 10);
            var slideValue = GUILayout.HorizontalSlider(value, 0f, 2f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (slideValue != value)
            {
                config.Value = (int)Mathf.Pow(10, slideValue);
                input = ($"{config.Value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var inputValue = GUILayout.TextField(input.Item1, style);
            if (inputValue != input.Item1)
            {
                if (int.TryParse(inputValue, out var parsedValue))
                {
                    config.Value = Mathf.Max(parsedValue, 1);
                    if (parsedValue < 1)
                        input = ($"{config.Value}", false);
                    else
                        input = (inputValue, false);
                }
                else
                    input = (inputValue, true);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制指数滑条
        /// </summary>
        public static void DrawPow(string label, ref ConfigEntry<float> config, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Variable.LabelStyle, GUILayout.Width(75));

            var value = Mathf.Log(config.Value, 10);
            var slideValue = GUILayout.HorizontalSlider(value, 2f, -2f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (slideValue != value)
            {
                config.Value = Mathf.Pow(10, slideValue);
                input = ($"{config.Value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var inputValue = GUILayout.TextField(input.Item1, style);
            if (inputValue != input.Item1)
            {
                if (float.TryParse(inputValue, out var parsedValue))
                {
                    config.Value = Mathf.Clamp(parsedValue, 0f, 100f);
                    if (parsedValue < 0f || parsedValue > 100f)
                        input = ($"{config.Value}", false);
                    else
                        input = (inputValue, false);
                }
                else
                    input = (inputValue, true);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制浮点滑条
        /// </summary>
        public static void DrawFloat(string label, ref ConfigEntry<float> config, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Variable.LabelStyle, GUILayout.Width(75));

            var slideValue = GUILayout.HorizontalSlider(config.Value, 0f, 100f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (slideValue != config.Value)
            {
                config.Value = slideValue;
                input = ($"{config.Value}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var inputValue = GUILayout.TextField(input.Item1, style);
            if (inputValue != input.Item1)
            {
                if (float.TryParse(inputValue, out var parsedValue))
                {
                    config.Value = Mathf.Clamp(parsedValue, 0f, 100f);
                    if (parsedValue < 0f || parsedValue > 100f)
                        input = ($"{config.Value}", false);
                    else
                        input = (inputValue, false);
                }
                else
                    input = (inputValue, true);
            }
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
