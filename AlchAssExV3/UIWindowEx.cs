using AlchAssV3;
using PotionCraft.ManagersSystem;
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
                VariableEx.EnableStirSet = GUILayout.Toggle(VariableEx.EnableStirSet, "定量搅拌", Variable.ToggleStyle);
                VariableEx.EnableLadleSet = GUILayout.Toggle(VariableEx.EnableLadleSet, "定量加水", Variable.ToggleStyle);
                VariableEx.EnableHeatSet = GUILayout.Toggle(VariableEx.EnableHeatSet, "定量加热", Variable.ToggleStyle);
                VariableEx.EnableGrindSpeed = GUILayout.Toggle(VariableEx.EnableGrindSpeed, "减速研磨", Variable.ToggleStyle);
                VariableEx.EnableStirSpeed = GUILayout.Toggle(VariableEx.EnableStirSpeed, "减速搅拌", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                VariableEx.EnableLadleSpeed = GUILayout.Toggle(VariableEx.EnableLadleSpeed, "减速加水", Variable.ToggleStyle);
                VariableEx.EnableHeatSpeed = GUILayout.Toggle(VariableEx.EnableHeatSpeed, "减速加热", Variable.ToggleStyle);
                VariableEx.EnableBrewMassive = GUILayout.Toggle(VariableEx.EnableBrewMassive, "批量炼药", Variable.ToggleStyle);
                VariableEx.EnableEdgeControl = GUILayout.Toggle(VariableEx.EnableEdgeControl, "漩涡制动", Variable.ToggleStyle);
                VariableEx.EnableClosestControl = GUILayout.Toggle(VariableEx.EnableClosestControl, "最近点制动", Variable.ToggleStyle);
                VariableEx.EnableProximityControl = GUILayout.Toggle(VariableEx.EnableProximityControl, "接近点制动", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制定量配置
        /// </summary>
        public static void DrawSets()
        {
            GUILayout.Space(10);
            var icon = VariableEx.SetExpand ? "▼ 定量搅拌加水设定" : "▲ 定量搅拌加水设定";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.SetExpand = !VariableEx.SetExpand;

            if (VariableEx.SetExpand)
            {
                var stage = Managers.RecipeMap.path.deletedGraphicsSegments + Managers.RecipeMap.path.segmentLengthToDeletePhysics;
                if (stage != VariableEx.StirStage)
                {
                    VariableEx.StirStage = stage;
                    VariableEx.StirSet = Mathf.Max(VariableEx.StirSetTarget - VariableEx.StirStage, 0f);
                    VariableEx.InputStirSet = ($"{VariableEx.StirSet}", false);
                }
                VariableEx.StirSet = DrawFloat("定量搅拌:", VariableEx.StirSet, ref VariableEx.InputStirSet, false, false, 0f, 100f);
                VariableEx.StirSetTarget = VariableEx.StirSet + VariableEx.StirStage;

                var indis = (Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset).magnitude;
                if (indis != VariableEx.LadleDistance)
                {
                    VariableEx.LadleDistance = indis;
                    VariableEx.LadleSet = Mathf.Max(VariableEx.LadleDistance - VariableEx.LadleSetTarget, 0f);
                    VariableEx.InputLadleSet = ($"{VariableEx.LadleSet}", false);
                }
                VariableEx.LadleSet = DrawFloat("定量加水:", VariableEx.LadleSet, ref VariableEx.InputLadleSet, false, false, 0f, 100f);
                VariableEx.LadleSetTarget = VariableEx.LadleDistance - VariableEx.LadleSet;
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
                VariableEx.ConfigGrindSet[index].Value = DrawFloat("定量研磨:", VariableEx.ConfigGrindSet[index].Value, ref VariableEx.InputGrindSet[index], false, true, 0f, 100f);
                VariableEx.ConfigHeatSet[index].Value = DrawFloat("定量加热:", VariableEx.ConfigHeatSet[index].Value, ref VariableEx.InputHeatSet[index], false, true, 0f, 100f);
                VariableEx.ConfigGrindSpeed[index].Value = DrawFloat("减速研磨:", VariableEx.ConfigGrindSpeed[index].Value, ref VariableEx.InputGrindSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigStirSpeed[index].Value = DrawFloat("减速搅拌:", VariableEx.ConfigStirSpeed[index].Value, ref VariableEx.InputStirSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigLadleSpeed[index].Value = DrawFloat("减速加水:", VariableEx.ConfigLadleSpeed[index].Value, ref VariableEx.InputLadleSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigHeatSpeed[index].Value = DrawFloat("减速加热:", VariableEx.ConfigHeatSpeed[index].Value, ref VariableEx.InputHeatSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigBrewMassive[index].Value = DrawInt("批量炼药:", VariableEx.ConfigBrewMassive[index].Value, ref VariableEx.InputBrewMassive[index]);
            }
        }
        #endregion

        #region 滑条渲染
        /// <summary>
        /// 绘制整数滑条
        /// </summary>
        public static int DrawInt(string label, int config, ref (string, bool) input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Variable.LabelStyle, GUILayout.Width(75));

            var value = Mathf.Log(config, 10);
            var slideValue = GUILayout.HorizontalSlider(value, 0f, 2f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (slideValue != value)
            {
                config = (int)Mathf.Pow(10, slideValue);
                input = ($"{config}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var inputValue = GUILayout.TextField(input.Item1, style);
            if (inputValue != input.Item1)
            {
                if (int.TryParse(inputValue, out var parsedValue))
                {
                    config = Mathf.Max(parsedValue, 1);
                    if (parsedValue < 1)
                        input = ($"{config}", false);
                    else
                        input = (inputValue, false);
                }
                else
                    input = (inputValue, true);
            }
            GUILayout.EndHorizontal();
            return config;
        }

        /// <summary>
        /// 绘制浮点滑条
        /// </summary>
        public static float DrawFloat(string label, float config, ref (string, bool) input, bool pow, bool clamp, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Variable.LabelStyle, GUILayout.Width(75));

            var value = pow ? Mathf.Log(config, 10) : config;
            var slideValue = GUILayout.HorizontalSlider(value, min, max, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
            if (slideValue != value)
            {
                config = pow ? Mathf.Pow(10, slideValue) : slideValue;
                input = ($"{config}", false);
            }

            var style = input.Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
            var inputValue = GUILayout.TextField(input.Item1, style);
            if (inputValue != input.Item1)
            {
                if (float.TryParse(inputValue, out var parsedValue))
                {
                    config = clamp ? Mathf.Clamp(parsedValue, 0f, 100f) : Mathf.Max(parsedValue, 0f);
                    if (parsedValue < 0f || (clamp && parsedValue > 100f))
                        input = ($"{config}", false);
                    else
                        input = (inputValue, false);
                }
                else
                    input = (inputValue, true);
            }
            GUILayout.EndHorizontal();
            return config;
        }
        #endregion
    }
}
