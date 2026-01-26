using AlchAssV3;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using UnityEngine;
using UnityEngine.UIElements;

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
            var icon = $"{(VariableEx.EnableExpand ? "▼" : "▲")} {LocalizationManager.GetText("控制选项")}";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                VariableEx.EnableExpand = !VariableEx.EnableExpand;

            if (VariableEx.EnableExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                VariableEx.EnableStirSet = GUILayout.Toggle(VariableEx.EnableStirSet, LocalizationManager.GetText("定量搅拌"), Variable.ToggleStyle);
                VariableEx.EnableLadleSet = GUILayout.Toggle(VariableEx.EnableLadleSet, LocalizationManager.GetText("定量加水"), Variable.ToggleStyle);
                VariableEx.EnableHeatSet = GUILayout.Toggle(VariableEx.EnableHeatSet, LocalizationManager.GetText("定量加热"), Variable.ToggleStyle);
                VariableEx.EnableGrindSet = GUILayout.Toggle(VariableEx.EnableGrindSet, LocalizationManager.GetText("定量研磨"), Variable.ToggleStyle);
                VariableEx.EnableStirSpeed = GUILayout.Toggle(VariableEx.EnableStirSpeed, LocalizationManager.GetText("减速搅拌"), Variable.ToggleStyle);
                VariableEx.EnableLadleSpeed = GUILayout.Toggle(VariableEx.EnableLadleSpeed, LocalizationManager.GetText("减速加水"), Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                VariableEx.EnableHeatSpeed = GUILayout.Toggle(VariableEx.EnableHeatSpeed, LocalizationManager.GetText("减速加热"), Variable.ToggleStyle);
                VariableEx.EnableGrindSpeed = GUILayout.Toggle(VariableEx.EnableGrindSpeed, LocalizationManager.GetText("减速研磨"), Variable.ToggleStyle);
                VariableEx.EnableBrewMassive = GUILayout.Toggle(VariableEx.EnableBrewMassive, LocalizationManager.GetText("批量酿造"), Variable.ToggleStyle);
                VariableEx.EnableEdgeControl = GUILayout.Toggle(VariableEx.EnableEdgeControl, LocalizationManager.GetText("漩涡制动"), Variable.ToggleStyle);
                VariableEx.EnableClosestControl = GUILayout.Toggle(VariableEx.EnableClosestControl, LocalizationManager.GetText("最近点制动"), Variable.ToggleStyle);
                VariableEx.EnableProximityControl = GUILayout.Toggle(VariableEx.EnableProximityControl, LocalizationManager.GetText("吸附点制动"), Variable.ToggleStyle);
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
            var icon = $"{(VariableEx.SetExpand ? "▼" : "▲")} {LocalizationManager.GetText("定量搅拌与加水设定")}";
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
                var stirval = DrawFloat("定量搅拌", VariableEx.StirSet, VariableEx.LabelWidthSets, ref VariableEx.InputStirSet, false, false, 0f, 100f);
                if (stirval != VariableEx.StirSet)
                {
                    VariableEx.StirSet = stirval;
                    VariableEx.StirSetTarget = VariableEx.StirSet + VariableEx.StirStage;
                }

                var indis = (Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition + Variable.Offset).magnitude;
                if (indis != VariableEx.LadleDistance)
                {
                    VariableEx.LadleDistance = indis;
                    VariableEx.LadleSet = Mathf.Max(VariableEx.LadleDistance - VariableEx.LadleSetTarget, 0f);
                    VariableEx.InputLadleSet = ($"{VariableEx.LadleSet}", false);
                }
                var ladleval = DrawFloat("定量加水", VariableEx.LadleSet, VariableEx.LabelWidthSets, ref VariableEx.InputLadleSet, false, false, 0f, 100f);
                if (ladleval != VariableEx.LadleSet)
                {
                    VariableEx.LadleSet = ladleval;
                    VariableEx.LadleSetTarget = VariableEx.LadleDistance - VariableEx.LadleSet;
                }
            }
        }

        /// <summary>
        /// 绘制变速配置
        /// </summary>
        public static void DrawLevels(string label, int index, ref bool category)
        {
            GUILayout.Space(10);
            var icon = $"{(category ? "▼" : "▲")} {LocalizationManager.GetText(label)}";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                category = !category;

            if (category)
            {
                VariableEx.ConfigHeatSet[index].Value = DrawFloat("定量加热", VariableEx.ConfigHeatSet[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputHeatSet[index], false, true, 0f, 100f);
                VariableEx.ConfigGrindSet[index].Value = DrawFloat("定量研磨", VariableEx.ConfigGrindSet[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputGrindSet[index], false, true, 0f, 100f);
                VariableEx.ConfigStirSpeed[index].Value = DrawFloat("减速搅拌", VariableEx.ConfigStirSpeed[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputStirSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigLadleSpeed[index].Value = DrawFloat("减速加水", VariableEx.ConfigLadleSpeed[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputLadleSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigHeatSpeed[index].Value = DrawFloat("减速加热", VariableEx.ConfigHeatSpeed[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputHeatSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigGrindSpeed[index].Value = DrawFloat("减速研磨", VariableEx.ConfigGrindSpeed[index].Value, VariableEx.LabelWidthLevels, ref VariableEx.InputGrindSpeed[index], true, true, 2f, -2f);
                VariableEx.ConfigBrewMassive[index].Value = DrawInt("批量酿造", VariableEx.ConfigBrewMassive[index].Value, ref VariableEx.InputBrewMassive[index]);
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
            GUILayout.Label($"{LocalizationManager.GetText(label)}:", Variable.LabelStyle, GUILayout.Width(VariableEx.LabelWidthLevels));

            var value = Mathf.Log10(config);
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
        public static float DrawFloat(string label, float config, float width, ref (string, bool) input, bool pow, bool clamp, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{LocalizationManager.GetText(label)}:", Variable.LabelStyle, GUILayout.Width(width));

            var value = pow ? Mathf.Log10(config) : config;
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

        #region 标签渲染
        /// <summary>
        /// 更新标签宽度
        /// </summary>
        public static void GetLabelWidth()
        {
            if (float.IsNaN(VariableEx.LabelWidthSets))
                VariableEx.LabelWidthSets = Localization.GetLabelWidth(["定量搅拌", "定量加水"], true);
            if (float.IsNaN(VariableEx.LabelWidthLevels))
                VariableEx.LabelWidthLevels = Localization.GetLabelWidth(["定量加热", "定量研磨", "减速搅拌", "减速加水", "减速加热", "减速研磨", "批量酿造"], true);
        }
        #endregion
    }
}
