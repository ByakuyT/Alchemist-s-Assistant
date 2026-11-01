using System;
using PotionCraft.LocalizationSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssV4;

public static class UIWindow
{
    #region 渲染主体

    /// <summary>
    /// 生成样式
    /// </summary>
    public static void InitStyles()
    {
        Variable.MenuTexture = new(1, 1);
        Variable.MenuTexture.SetPixel(0, 0, new(0.12f, 0.12f, 0.12f, 0.95f));
        Variable.MenuTexture.Apply();
        Variable.MenuStyle = new(GUI.skin.window);
        Variable.MenuStyle.normal.background = Variable.MenuTexture;
        Variable.MenuStyle.onNormal.background = Variable.MenuTexture;
        Variable.MenuStyle.normal.textColor = Color.cyan;
        Variable.MenuStyle.onNormal.textColor = Color.cyan;
        Variable.MenuStyle.font = Variable.Font;
        Variable.MenuStyle.fontSize = 20;
        Variable.CategoryStyle = new(GUI.skin.button);
        Variable.CategoryStyle.normal.textColor = new(1.0f, 0.8f, 0.3f);
        Variable.CategoryStyle.hover.textColor = new(1.0f, 0.8f, 0.3f);
        Variable.CategoryStyle.font = Variable.Font;
        Variable.CategoryStyle.fontSize = 18;
        Variable.CategoryStyle.alignment = TextAnchor.MiddleLeft;
        Variable.CategoryStyle.padding = new(10, 10, 5, 5);
        Variable.ButtonStyle = new(GUI.skin.button);
        Variable.ButtonStyle.hover.textColor = Color.green;
        Variable.ButtonStyle.font = Variable.Font;
        Variable.ButtonStyle.margin = new(30, 30, 5, 5);
        Variable.ButtonStyle.padding = new(10, 10, 5, 5);
        Variable.DeleteButtonStyle = new(GUI.skin.button);
        Variable.DeleteButtonStyle.normal.textColor = Color.red;
        Variable.DeleteButtonStyle.hover.textColor = Color.red;
        Variable.DeleteButtonStyle.font = Variable.Font;
        Variable.DeleteButtonStyle.fontSize = 14;
        Variable.DeleteButtonStyle.margin = new(0, 30, 5, 0);
        Variable.DeleteButtonStyle.padding = new(10, 10, 6, 4);
        Variable.DeleteButtonStyle.fixedWidth = 50;
        Variable.ToggleStyle = new(GUI.skin.toggle);
        Variable.ToggleStyle.onNormal.textColor = new(0.4f, 0.8f, 1.0f);
        Variable.ToggleStyle.onHover.textColor = new(0.4f, 0.8f, 1.0f);
        Variable.ToggleStyle.font = Variable.Font;
        Variable.ToggleStyle.margin = new(30, 30, 6, 4);
        Variable.ToggleStyle.padding = new(25, 0, -1, 0);
        Variable.LabelStyle = new(GUI.skin.label);
        Variable.LabelStyle.font = Variable.Font;
        Variable.LabelStyle.margin = new(30, 10, 5, 5);
        Variable.AxisStyle = new(GUI.skin.label);
        Variable.AxisStyle.font = Variable.Font;
        Variable.AxisStyle.margin = new(0, 5, 5, 5);
        Variable.AxisStyle.fixedWidth = 10;
        Variable.SliderStyle = new(GUI.skin.horizontalSlider);
        Variable.SliderStyle.margin = new(0, 5, 14, 0);
        Variable.TextFieldStyle = new(GUI.skin.textField);
        Variable.TextFieldStyle.normal.textColor = Color.green;
        Variable.TextFieldStyle.font = Variable.Font;
        Variable.TextFieldStyle.fontSize = 14;
        Variable.TextFieldStyle.margin = new(0, 10, 7, 0);
        Variable.TextFieldStyle.fixedWidth = 70;
        Variable.TextFieldErrorStyle = new(GUI.skin.textField);
        Variable.TextFieldErrorStyle.normal.textColor = Color.red;
        Variable.TextFieldErrorStyle.font = Variable.Font;
        Variable.TextFieldErrorStyle.fontSize = 14;
        Variable.TextFieldErrorStyle.margin = new(0, 10, 7, 0);
        Variable.TextFieldErrorStyle.fixedWidth = 70;
    }

    /// <summary>
    /// 设置窗口大小
    /// </summary>
    public static void ResizeMenu()
    {
        var rect = new Rect(Variable.MenuRect.xMax - 25, Variable.MenuRect.yMax - 25, 25, 25);
        var mousePos = Mouse.current.position.ReadValue();
        mousePos.y = Screen.height - mousePos.y;
        if (Mouse.current.leftButton.wasPressedThisFrame && rect.Contains(mousePos))
        {
            Variable.IsResizing = true;
            Variable.LastMousePos = mousePos;
        }

        if (!Variable.IsResizing) return;
        if (Mouse.current.leftButton.isPressed)
        {
            var delta = mousePos - Variable.LastMousePos;
            Variable.MenuRect.width = Mathf.Max(450, Variable.MenuRect.width + delta.x);
            Variable.MenuRect.height = Mathf.Max(600, Variable.MenuRect.height + delta.y);
            Variable.LastMousePos = mousePos;
        }
        else
            Variable.IsResizing = false;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    public static void DrawMenu(int _)
    {
        GUILayout.Space(8);
        Variable.ScrollPosition = GUILayout.BeginScrollView(Variable.ScrollPosition);
        DrawEnables();
        DrawDisplays();
        DrawActions();
        DrawHandoff();
        DrawCustomLineList();
        DrawCustomMarkerList();
        DrawExpansion();
        GUILayout.EndScrollView();
        GUILayout.Space(5);
        GUI.DragWindow(new(0, 0, Variable.MenuRect.width, 30));
    }

    #endregion

    #region 渲染元素

    /// <summary>
    /// 功能选项
    /// </summary>
    private static void DrawEnables()
    {
        var icon = $"{(Variable.EnableExpand ? "▼" : "▲")} {LocalizationManager.GetText("渲染选项")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.EnableExpand = !Variable.EnableExpand;
        if (Variable.EnableExpand)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            Variable.EnablePathLine = GUILayout.Toggle(Variable.EnablePathLine, LocalizationManager.GetText("路径切向线"),
                Variable.ToggleStyle);
            Variable.EnableLadleLine = GUILayout.Toggle(Variable.EnableLadleLine, LocalizationManager.GetText("加水方向线"),
                Variable.ToggleStyle);
            Variable.EnableEffectLine = GUILayout.Toggle(Variable.EnableEffectLine,
                LocalizationManager.GetText("效果径向线"), Variable.ToggleStyle);
            Variable.EnableVortexLine = GUILayout.Toggle(Variable.EnableVortexLine,
                LocalizationManager.GetText("漩涡径向线"), Variable.ToggleStyle);
            Variable.EnableTangentLine = GUILayout.Toggle(Variable.EnableTangentLine,
                LocalizationManager.GetText("漩涡切向线"), Variable.ToggleStyle);
            Variable.EnableCustomLine = GUILayout.Toggle(Variable.EnableCustomLine,
                LocalizationManager.GetText("自定义方向线"), Variable.ToggleStyle);
            Variable.EnableCustomMarker = GUILayout.Toggle(Variable.EnableCustomMarker,
                LocalizationManager.GetText("自定义标记点"), Variable.ToggleStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            Variable.EnablePathCurve = GUILayout.Toggle(Variable.EnablePathCurve, LocalizationManager.GetText("路径曲线"),
                Variable.ToggleStyle);
            Variable.EnableVortexCurve = GUILayout.Toggle(Variable.EnableVortexCurve,
                LocalizationManager.GetText("漩涡曲线"), Variable.ToggleStyle);
            Variable.EnableEffectRange = GUILayout.Toggle(Variable.EnableEffectRange,
                LocalizationManager.GetText("效果范围"), Variable.ToggleStyle);
            Variable.EnableVortexRange = GUILayout.Toggle(Variable.EnableVortexRange,
                LocalizationManager.GetText("漩涡范围"), Variable.ToggleStyle);
            Variable.EnableDangerSimulation = GUILayout.Toggle(Variable.EnableDangerSimulation,
                LocalizationManager.GetText("骷髅区模拟"), Variable.ToggleStyle);
            Variable.EnableSwampSimulation = GUILayout.Toggle(Variable.EnableSwampSimulation,
                LocalizationManager.GetText("沼泽区模拟"), Variable.ToggleStyle);
            Variable.EnableTransparency = GUILayout.Toggle(Variable.EnableTransparency,
                LocalizationManager.GetText("透明瓶身"), Variable.ToggleStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            Function.UpdateDoFromEnable();
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制显示选项
    /// </summary>
    private static void DrawDisplays()
    {
        var icon = $"{(Variable.DisplayExpand ? "▼" : "▲")} {LocalizationManager.GetText("显示选项")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.DisplayExpand = !Variable.DisplayExpand;
        if (Variable.DisplayExpand)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            Variable.DisplaySalt = GUILayout.Toggle(Variable.DisplaySalt, LocalizationManager.GetText("盐量数据"),
                Variable.ToggleStyle);
            Variable.DisplayStage = GUILayout.Toggle(Variable.DisplayStage, LocalizationManager.GetText("搅拌阶段"),
                Variable.ToggleStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            var displayPolar = GUILayout.Toggle(Variable.DisplayPolar, LocalizationManager.GetText("极坐标"),
                Variable.ToggleStyle);
            if (Variable.DisplayPolar != displayPolar)
            {
                Variable.DisplayPolar = displayPolar;
                Function.UpdateCustomMarkerPositionType();
            }

            Variable.DisplayOffset = GUILayout.Toggle(Variable.DisplayOffset, LocalizationManager.GetText("碰撞体位置"),
                Variable.ToggleStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制窗口管理
    /// </summary>
    private static void DrawActions()
    {
        var icon = $"{(Variable.ActionExpand ? "▼" : "▲")} {LocalizationManager.GetText("窗口管理")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.ActionExpand = !Variable.ActionExpand;
        if (Variable.ActionExpand)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LocalizationManager.GetText("恢复所有窗口"), Variable.ButtonStyle))
                Function.RestoreDebugWindows();
            if (GUILayout.Button(LocalizationManager.GetText("保存窗口布局"), Variable.ButtonStyle))
                Function.SaveDebugWindowPos();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制最优交接点计算
    /// </summary>
    private static void DrawHandoff()
    {
        var icon = $"{(Variable.HandoffExpand ? "▼" : "▲")} {LocalizationManager.GetText("最优交接点计算")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.HandoffExpand = !Variable.HandoffExpand;
        if (Variable.HandoffExpand)
        {
            var run = Variable.HandoffRunning;
            var busy = run >= 0;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            if (GUILayout.Button(LocalizationManager.GetText(run == 0 ? "计算中…" : "加水 → 路径"), Variable.ButtonStyle) &&
                !busy)
                PathSolve.StartLadleToPath();
            if (GUILayout.Button(LocalizationManager.GetText(run == 1 ? "计算中…" : "路径 → 加水"), Variable.ButtonStyle) &&
                !busy)
                PathSolve.StartPathToLadle();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button(LocalizationManager.GetText(run == 2 ? "计算中…" : "漩涡 → 路径"), Variable.ButtonStyle) &&
                !busy)
                PathSolve.StartVortexToPath();
            if (GUILayout.Button(LocalizationManager.GetText(run == 3 ? "计算中…" : "漩涡 → 加水"), Variable.ButtonStyle) &&
                !busy)
                PathSolve.StartVortexToLadle();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Space(21);
            if (GUILayout.Button("CLR", Variable.DeleteButtonStyle) && !busy)
                Variable.HandoffPositions = [null, null];
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制自定义方向线管理
    /// </summary>
    private static void DrawCustomLineList()
    {
        var icon = $"{(Variable.CustomLineListExpand ? "▼" : "▲")} {LocalizationManager.GetText("自定义方向线管理")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.CustomLineListExpand = !Variable.CustomLineListExpand;
        if (Variable.CustomLineListExpand)
        {
            if (GUILayout.Button($"+ {LocalizationManager.GetText("添加方向线")}", Variable.ButtonStyle))
            {
                Variable.CustomLineDirections.Add(0);
                Variable.CustomLineHovers.Add(false);
                Variable.CustomLineInputs.Add(("0", false));
                Localization.UpdateCustomLineLabelWidth();
            }

            for (var i = 0; i < Variable.CustomLineDirections.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i + 1}.", Variable.LabelStyle, GUILayout.Width(Variable.CustomLineLabelWidth));
                var slideValue = GUILayout.HorizontalSlider((float)Variable.CustomLineDirections[i], -180, 180,
                    Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
                if (slideValue != (float)Variable.CustomLineDirections[i])
                {
                    Variable.CustomLineDirections[i] = slideValue;
                    Variable.CustomLineInputs[i] = ($"{slideValue}", false);
                }

                var style = Variable.CustomLineInputs[i].Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
                var inputValue = GUILayout.TextField(Variable.CustomLineInputs[i].Item1, style);
                if (inputValue != Variable.CustomLineInputs[i].Item1)
                {
                    if (double.TryParse(inputValue, out var parsedValue) && Algebra.IsFinite(parsedValue))
                    {
                        var clamped = Algebra.Clamp(parsedValue, -180, 180);
                        Variable.CustomLineDirections[i] = clamped;
                        Variable.CustomLineInputs[i] = (
                            parsedValue == clamped ? inputValue : $"{clamped}", false);
                    }
                    else
                        Variable.CustomLineInputs[i] = (inputValue, true);
                }

                if (GUILayout.Button("DEL", Variable.DeleteButtonStyle))
                {
                    Variable.CustomLineDirections.RemoveAt(i);
                    Variable.CustomLineHovers.RemoveAt(i);
                    Variable.CustomLineInputs.RemoveAt(i);
                    if (Variable.TargetLineIndex == i) Variable.TargetLineIndex = -1;
                    else if (Variable.TargetLineIndex > i) Variable.TargetLineIndex--;
                    i--;
                    Localization.UpdateCustomLineLabelWidth();
                }

                GUILayout.EndHorizontal();
                if (Event.current.type == EventType.Repaint)
                    Variable.CustomLineHovers[i] = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
            }
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制自定义标记点管理
    /// </summary>
    private static void DrawCustomMarkerList()
    {
        var icon = $"{(Variable.CustomMarkerListExpand ? "▼" : "▲")} {LocalizationManager.GetText("自定义标记点管理")}";
        if (GUILayout.Button(icon, Variable.CategoryStyle))
            Variable.CustomMarkerListExpand = !Variable.CustomMarkerListExpand;
        if (Variable.CustomMarkerListExpand)
        {
            if (GUILayout.Button($"+ {LocalizationManager.GetText("添加标记点")}", Variable.ButtonStyle))
            {
                Variable.CustomMarkerPositions.Add(Variable.Vec2D.Zero);
                Variable.CustomMarkerHovers.Add(false);
                Variable.CustomMarkerInputs.Add(("0", "0", false, false));
                Localization.UpdateCustomMarkerLabelWidth();
            }

            for (var i = 0; i < Variable.CustomMarkerPositions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(Variable.CustomMarkerLabelWidth));
                GUILayout.Space(21);
                GUILayout.Label($"{i + 1}.", Variable.LabelStyle);
                GUILayout.Space(11);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                DrawMarkerInput(i, true);
                DrawMarkerInput(i, false);
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(50));
                GUILayout.Space(21);
                if (GUILayout.Button("DEL", Variable.DeleteButtonStyle))
                {
                    Variable.CustomMarkerPositions.RemoveAt(i);
                    Variable.CustomMarkerHovers.RemoveAt(i);
                    Variable.CustomMarkerInputs.RemoveAt(i);
                    if (Variable.TargetMarkerIndex == i) Variable.TargetMarkerIndex = -1;
                    else if (Variable.TargetMarkerIndex > i) Variable.TargetMarkerIndex--;
                    i--;
                    Localization.UpdateCustomMarkerLabelWidth();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                if (Event.current.type == EventType.Repaint)
                    Variable.CustomMarkerHovers[i] =
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
            }
        }

        GUILayout.Space(10);
    }

    /// <summary>
    /// EXmod扩展的接口
    /// </summary>
    public static void DrawExpansion()
    {
    }

    #endregion

    #region 渲染辅助

    /// <summary>
    /// 绘制标记点坐标输入
    /// </summary>
    private static void DrawMarkerInput(int index, bool isFirst)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(GetMarkerAxisLabel(isFirst), Variable.AxisStyle);
        var (min, max) = GetMarkerAxisRange(isFirst);
        var axisValue = isFirst ? Variable.CustomMarkerPositions[index].X : Variable.CustomMarkerPositions[index].Y;
        var slideValue = GUILayout.HorizontalSlider((float)axisValue, min, max, Variable.SliderStyle,
            new(GUI.skin.horizontalSliderThumb));
        if (slideValue != (float)axisValue)
        {
            SetMarkerAxisValue(index, isFirst, slideValue);
            SetMarkerInput(index, isFirst, $"{slideValue}", false);
        }

        var style = (isFirst ? Variable.CustomMarkerInputs[index].Item3 : Variable.CustomMarkerInputs[index].Item4)
            ? Variable.TextFieldErrorStyle
            : Variable.TextFieldStyle;
        var input = isFirst ? Variable.CustomMarkerInputs[index].Item1 : Variable.CustomMarkerInputs[index].Item2;
        var inputValue = GUILayout.TextField(input, style);
        if (inputValue != input)
        {
            if (double.TryParse(inputValue, out var parsedValue) && Algebra.IsFinite(parsedValue))
            {
                var clamped = Algebra.Clamp(parsedValue, min, max);
                SetMarkerAxisValue(index, isFirst, clamped);
                SetMarkerInput(index, isFirst, parsedValue == clamped ? inputValue : $"{clamped}", false);
            }
            else
                SetMarkerInput(index, isFirst, inputValue, true);
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 获取标记点坐标轴名称
    /// </summary>
    private static string GetMarkerAxisLabel(bool isFirst)
    {
        return Variable.DisplayPolar ? isFirst ? "R" : "θ" : isFirst ? "X" : "Y";
    }

    /// <summary>
    /// 获取标记点坐标轴调节范围
    /// </summary>
    private static (float min, float max) GetMarkerAxisRange(bool isFirst)
    {
        return !Variable.DisplayPolar ? (-80f, 80f) : isFirst ? (0f, 115f) : (-180f, 180f);
    }

    /// <summary>
    /// 设置标记点坐标轴数值
    /// </summary>
    private static void SetMarkerAxisValue(int index, bool isFirst, double value)
    {
        var position = Variable.CustomMarkerPositions[index];
        Variable.CustomMarkerPositions[index] =
            isFirst ? new Variable.Vec2D(value, position.Y) : new Variable.Vec2D(position.X, value);
    }

    /// <summary>
    /// 设置标记点坐标输入
    /// </summary>
    private static void SetMarkerInput(int index, bool isFirst, string value, bool error)
    {
        var (first, second, firstError, secondError) = Variable.CustomMarkerInputs[index];
        Variable.CustomMarkerInputs[index] =
            isFirst ? (value, second, error, secondError) : (first, value, firstError, error);
    }

    #endregion
}