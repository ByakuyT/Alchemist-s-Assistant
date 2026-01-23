using UnityEngine;
using UnityEngine.InputSystem;

namespace AlchAssV3
{
    public static class UIWindow
    {
        #region 主体渲染
        /// <summary>
        /// 生成样式
        /// </summary>
        public static void InitStyles()
        {
            Variable.WindowTexture = new(1, 1);
            Variable.WindowTexture.SetPixel(0, 0, new(0.12f, 0.12f, 0.12f, 0.95f));
            Variable.WindowTexture.Apply();

            Variable.WindowStyle = new(GUI.skin.window);
            Variable.WindowStyle.normal.background = Variable.WindowTexture;
            Variable.WindowStyle.onNormal.background = Variable.WindowTexture;
            Variable.WindowStyle.normal.textColor = Color.cyan;
            Variable.WindowStyle.onNormal.textColor = Color.cyan;
            Variable.WindowStyle.font = Variable.Font;
            Variable.WindowStyle.fontSize = 20;

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
            Variable.DeleteButtonStyle.padding = new(10, 10, 5, 5);
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
        public static void Resizing()
        {
            var ResizeRect = new Rect(Variable.WindowRect.xMax - 25, Variable.WindowRect.yMax - 25, 25, 25);
            var mousePos = Mouse.current.position.ReadValue();
            mousePos.y = Screen.height - mousePos.y;

            if (Mouse.current.leftButton.wasPressedThisFrame && ResizeRect.Contains(mousePos))
            {
                Variable.IsResizing = true;
                Variable.LastMousePosition = mousePos;
            }
            if (Variable.IsResizing)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    var delta = mousePos - Variable.LastMousePosition;
                    Variable.WindowRect.width = Mathf.Max(400, Variable.WindowRect.width + delta.x);
                    Variable.WindowRect.height = Mathf.Max(400, Variable.WindowRect.height + delta.y);
                    Variable.LastMousePosition = mousePos;
                }
                else
                    Variable.IsResizing = false;
            }
        }

        /// <summary>
        /// 绘制窗口
        /// </summary>
        public static void DrawWindow(int _)
        {
            GUILayout.Space(8);
            Variable.ScrollPosition = GUILayout.BeginScrollView(Variable.ScrollPosition);

            DrawEnables();
            DrawDisplays();
            DrawActions();
            DrawCustomList();
            DrawExpansion();

            GUILayout.EndScrollView();
            GUILayout.Space(5);
            GUI.DragWindow(new Rect(0, 0, Variable.WindowRect.width, 30));
        }
        #endregion

        #region 元素渲染
        /// <summary>
        /// 绘制功能选项
        /// </summary>
        public static void DrawEnables()
        {
            var icon = Variable.EnableExpand ? "▼ 功能选项" : "▲ 功能选项";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                Variable.EnableExpand = !Variable.EnableExpand;

            if (Variable.EnableExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                Variable.EnablePathLine = GUILayout.Toggle(Variable.EnablePathLine, "路径切向线", Variable.ToggleStyle);
                Variable.EnableLadleLine = GUILayout.Toggle(Variable.EnableLadleLine, "加水方向线", Variable.ToggleStyle);
                Variable.EnableEffectLine = GUILayout.Toggle(Variable.EnableEffectLine, "效果径向线", Variable.ToggleStyle);
                Variable.EnableVortexLine = GUILayout.Toggle(Variable.EnableVortexLine, "漩涡径向线", Variable.ToggleStyle);
                Variable.EnableTangentLine = GUILayout.Toggle(Variable.EnableTangentLine, "漩涡切向线", Variable.ToggleStyle);
                Variable.EnableCustomLine = GUILayout.Toggle(Variable.EnableCustomLine, "自定义方向线", Variable.ToggleStyle);
                Variable.EnablePathCurve = GUILayout.Toggle(Variable.EnablePathCurve, "路径曲线", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                Variable.EnableVortexCurve = GUILayout.Toggle(Variable.EnableVortexCurve, "漩涡曲线", Variable.ToggleStyle);
                Variable.EnableEffectRange = GUILayout.Toggle(Variable.EnableEffectRange, "效果范围", Variable.ToggleStyle);
                Variable.EnableVortexRange = GUILayout.Toggle(Variable.EnableVortexRange, "漩涡范围", Variable.ToggleStyle);
                Variable.EnableDangerSimulation = GUILayout.Toggle(Variable.EnableDangerSimulation, "骷髅区模拟", Variable.ToggleStyle);
                Variable.EnableSwampSimulation = GUILayout.Toggle(Variable.EnableSwampSimulation, "沼泽区模拟", Variable.ToggleStyle);
                Variable.EnableTransparency = GUILayout.Toggle(Variable.EnableTransparency, "透明瓶身", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                Function.UpdateDoFromEnable();
            }
            GUILayout.Space(10);
        }

        /// <summary>
        /// 绘制显示选项
        /// </summary>
        public static void DrawDisplays()
        {
            var icon = Variable.DisplayExpand ? "▼ 显示选项" : "▲ 显示选项";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                Variable.DisplayExpand = !Variable.DisplayExpand;

            if (Variable.DisplayExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                Variable.DisplaySalt = GUILayout.Toggle(Variable.DisplaySalt, "旋转盐量", Variable.ToggleStyle);
                Variable.DisplayStage = GUILayout.Toggle(Variable.DisplayStage, "搅拌阶段", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                Variable.DisplayPolar = GUILayout.Toggle(Variable.DisplayPolar, "极坐标", Variable.ToggleStyle);
                Variable.DisplayOffset = GUILayout.Toggle(Variable.DisplayOffset, "碰撞体位置", Variable.ToggleStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }

        /// <summary>
        /// 绘制即时功能
        /// </summary>
        public static void DrawActions()
        {
            var icon = Variable.ActionExpand ? "▼ 即时功能" : "▲ 即时功能";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                Variable.ActionExpand = !Variable.ActionExpand;

            if (Variable.ActionExpand)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("开启所有窗口", Variable.ButtonStyle))
                    Function.OpenDebugWindows();
                if (GUILayout.Button("保存窗口位置", Variable.ButtonStyle))
                    Function.SaveDebugWindowPos();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }

        /// <summary>
        /// 绘制自定义方向线管理
        /// </summary>
        public static void DrawCustomList()
        {
            var icon = Variable.CustomListExpand ? "▼ 自定义方向线管理" : "▲ 自定义方向线管理";
            if (GUILayout.Button(icon, Variable.CategoryStyle))
                Variable.CustomListExpand = !Variable.CustomListExpand;

            if (Variable.CustomListExpand)
            {
                if (GUILayout.Button("+ 添加方向线", Variable.ButtonStyle))
                {
                    Variable.CustomLineDirections.Add(0f);
                    Variable.CustomLineHovers.Add(false);
                    Variable.Inputs.Add(("0", false));
                }

                for (int i = 0; i < Variable.CustomLineDirections.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{i + 1}.", Variable.LabelStyle, GUILayout.Width(25));

                    var slideValue = GUILayout.HorizontalSlider(Variable.CustomLineDirections[i], 0f, 360f, Variable.SliderStyle, new(GUI.skin.horizontalSliderThumb));
                    if (slideValue != Variable.CustomLineDirections[i])
                    {
                        Variable.CustomLineDirections[i] = slideValue;
                        Variable.Inputs[i] = ($"{slideValue}", false);
                    }

                    var style = Variable.Inputs[i].Item2 ? Variable.TextFieldErrorStyle : Variable.TextFieldStyle;
                    var inputValue = GUILayout.TextField(Variable.Inputs[i].Item1, style);
                    if (inputValue != Variable.Inputs[i].Item1)
                    {
                        if (float.TryParse(inputValue, out var parsedValue))
                        {
                            Variable.CustomLineDirections[i] = Mathf.Clamp(parsedValue, 0f, 360f);
                            if (parsedValue < 0f || parsedValue > 360f)
                                Variable.Inputs[i] = ($"{Variable.CustomLineDirections[i]}", true);
                            else
                                Variable.Inputs[i] = (inputValue, false);
                        }
                        else
                            Variable.Inputs[i] = (inputValue, true);
                    }

                    if (GUILayout.Button("删除", Variable.DeleteButtonStyle))
                    {
                        Variable.CustomLineDirections.RemoveAt(i);
                        Variable.CustomLineHovers.RemoveAt(i);
                        Variable.Inputs.RemoveAt(i);
                        if (Variable.TargetLineIndex == i) Variable.TargetLineIndex = -1;
                        else if (Variable.TargetLineIndex > i) Variable.TargetLineIndex--;
                        i--;
                    }
                    GUILayout.EndHorizontal();

                    if (Event.current.type == EventType.Repaint)
                        Variable.CustomLineHovers[i] = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
                }
            }
        }

        /// <summary>
        /// 留待扩展的接口
        /// </summary>
        public static void DrawExpansion() { }
        #endregion
    }
}
