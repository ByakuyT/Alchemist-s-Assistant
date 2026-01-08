using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using System;
using System.Linq;
using UnityEngine;
//using UnityEngine.PlayerLoop;

namespace AlchAssV3
{
    internal class Rendering
    {
        #region 生成材质
        /// <summary>
        /// 生成纯色材质
        /// </summary>
        public static Material CreateSolidMaterial()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixels([Color.white]);
            texture.Apply();
            return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
        }

        /// <summary>
        /// 生成虚线材质
        /// </summary>
        public static Material CreateDashedMaterial()
        {
            var texture = new Texture2D(10, 1);
            Color[] pixels = new Color[10];
            for (int x = 0; x < 10; x++)
                pixels[x] = x < 5 ? Color.white : Color.clear;
            texture.SetPixels(pixels);
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            return new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
        }

        /// <summary>
        /// 生成矩形精灵
        /// </summary>
        public static Sprite CreateSquareSprite()
        {
            var squareTexture = new Texture2D(1, 1);
            var pixels = new Color[1] { Color.white };
            squareTexture.SetPixels(pixels);
            squareTexture.Apply();
            squareTexture.filterMode = FilterMode.Point;
            return Sprite.Create(squareTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        /// <summary>
        /// 生成圆形精灵
        /// </summary>
        public static Sprite CreateRoundSprite()
        {
            var diameter = Variable.LineWidth.Value * 2;
            var textureSize = 500;
            var circleTexture = new Texture2D(textureSize, textureSize);
            var pixels = new Color[textureSize * textureSize];
            var center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);
            var radius = textureSize * 0.5;
            for (var y = 0; y < textureSize; y++)
                for (var x = 0; x < textureSize; x++)
                {
                    var pos = new Vector2(x, y);
                    var distance = Vector2.Distance(pos, center);
                    if (distance <= radius)
                        pixels[y * textureSize + x] = Color.white;
                    else
                        pixels[y * textureSize + x] = Color.clear;
                }
            circleTexture.SetPixels(pixels);
            circleTexture.Apply();
            circleTexture.filterMode = FilterMode.Bilinear;
            var pixelsPerUnit = textureSize / (float)diameter;
            return Sprite.Create(circleTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        /// <summary>
        /// 生成材质和精灵
        /// </summary>
        public static void CreateMaterialAndSprites()
        {
            Variable.SolidMaterial = CreateSolidMaterial();
            Variable.DashedMaterial = CreateDashedMaterial();
            Variable.RoundSprite = CreateRoundSprite();
            Variable.SquareSprite = CreateSquareSprite();
        }

        /// <summary>
        /// 生成曲线渲染器
        /// </summary>
        public static void InitLineRenderer(ref LineRenderer curve)
        {
            var obj = new GameObject("Renderer") { layer = Variable.BaseLayer };
            curve = obj.AddComponent<LineRenderer>();

            curve.textureMode = LineTextureMode.Tile;
            curve.useWorldSpace = true;
            curve.startWidth = (float)Variable.LineWidth.Value;
            curve.endWidth = (float)Variable.LineWidth.Value;
            curve.sortingLayerName = Variable.BaseSortingLayerName;
            curve.sortingOrder = Variable.BaseSortingOrder;
            curve.positionCount = 0;
            curve.enabled = false;
        }

        /// <summary>
        /// 生成精灵渲染器
        /// </summary>
        public static void InitSpriteRenderer(ref SpriteRenderer sprite)
        {
            var obj = new GameObject("Renderer") { layer = Variable.BaseLayer };
            sprite = obj.AddComponent<SpriteRenderer>();

            sprite.drawMode = SpriteDrawMode.Simple;
            sprite.maskInteraction = SpriteMaskInteraction.None;
            sprite.spriteSortPoint = SpriteSortPoint.Center;
            sprite.sortingLayerName = Variable.BaseSortingLayerName;
            sprite.sortingOrder = Variable.BaseSortingOrder + 1;
            sprite.enabled = false;
        }

        /// <summary>
        /// 更新曲线渲染器
        /// </summary>
        public static void UpdateLineRenderer(Material material, Color color, ref LineRenderer curve, Vector3[] points, bool loop)
        {
            curve.loop = loop;
            curve.material = material;
            curve.startColor = color;
            curve.endColor = color;
            curve.positionCount = points.Length;
            curve.SetPositions(points);
            curve.enabled = true;
        }

        /// <summary>
        /// 更新精灵渲染器
        /// </summary>
        public static void UpdateSpriteRenderer(Sprite material, Color color, ref SpriteRenderer sprite, Vector3 pos, float scale)
        {
            sprite.sprite = material;
            sprite.transform.position = pos;
            sprite.transform.localScale = new Vector3(scale, scale, 1);
            sprite.color = color;
            sprite.enabled = true;
        }
        #endregion

        #region 渲染对象
        /// <summary>
        /// 渲染透明瓶身
        /// </summary>
        public static void SetTransparency()
        {
            if (Variable.KeyMode != "Normal" || !Variable.SwitchKeyShortcuts[Variable.SwitchDictionaryKey.Transparency].Value.IsDown())
                return;

            var indicatorMapItems = UnityEngine.Object.FindObjectsByType<IndicatorMapItem>(FindObjectsSortMode.None);
            foreach (var indicatorMapItem in indicatorMapItems)
            {
                if (!Variable.SwitchTransparency.getState())
                {
                    var liquidAnimator = Traverse.Create(indicatorMapItem).Field("liquidColorChangeAnimator").GetValue();
                    if (liquidAnimator != null)
                    {
                        var upperContainer = Traverse.Create(liquidAnimator).Field("upperContainer").GetValue();
                        var lowerContainer = Traverse.Create(liquidAnimator).Field("lowerContainer").GetValue();
                        if (upperContainer != null)
                            Traverse.Create(upperContainer).Method("SetAlpha", 0f).GetValue();
                        if (lowerContainer != null)
                            Traverse.Create(lowerContainer).Method("SetAlpha", 0f).GetValue();
                    }
                    indicatorMapItem.backgroundSpriteRenderer.enabled = false;
                }
                else
                {
                    var liquidAnimator = Traverse.Create(indicatorMapItem).Field("liquidColorChangeAnimator").GetValue();
                    if (liquidAnimator != null)
                    {
                        var upperContainer = Traverse.Create(liquidAnimator).Field("upperContainer").GetValue();
                        var lowerContainer = Traverse.Create(liquidAnimator).Field("lowerContainer").GetValue();
                        if (upperContainer != null)
                            Traverse.Create(upperContainer).Method("SetAlpha", 1f).GetValue();
                        if (lowerContainer != null)
                            Traverse.Create(lowerContainer).Method("SetAlpha", 1f).GetValue();
                    }
                    indicatorMapItem.backgroundSpriteRenderer.enabled = true;
                }
            }
        }

        /// <summary>
        /// 渲染点
        /// </summary>
        public static void SetNodeRenderers()
        {
            bool[] ClosestEnables = [Variable.SwitchPathCurve.getState(), Variable.SwitchLadleLine.getState()];
            bool[] IntersectionEnables = [Variable.SwitchPathTargetPoint.getState(), Variable.SwitchLadleTargetPoint.getState(), Variable.SwitchPathVortexPoint.getState(), Variable.SwitchLadleTargetPoint.getState()];
            bool[] DangerEnables = [Variable.SwitchPathDangerPoint.getState(), Variable.SwitchLadleDangerPoint.getState(), Variable.SwitchVortexDangerPoint.getState()];
            var yDev = new Vector2(0, Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f);
             
            for (int i = 0; i < 2; i++)
            {
                if (ClosestEnables[i] && !float.IsNaN(Variable.ClosestPositions[i].x))
                {
                    var posDev = Variable.ClosestPositions[i] + yDev;
                    if (Variable.ClosestPointRenderers[i] == null)
                        InitSpriteRenderer(ref Variable.ClosestPointRenderers[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[8].Value, ref Variable.ClosestPointRenderers[i], posDev, (float)Variable.NodeSize.Value);
                }
                else if (Variable.ClosestPointRenderers[i] != null)
                    UnityEngine.Object.Destroy(Variable.ClosestPointRenderers[i].gameObject);
            }

            for (int i = 0; i < 4; i++)
            {
                if (IntersectionEnables[i] && Variable.IntersectionPositions[i].Count > 0)
                {
                    for (int j = 0; j < Variable.IntersectionPositions[i].Count; j++)
                    {
                        var posDev = Variable.IntersectionPositions[i][j] + yDev;

                        if (Variable.IntersectionPointRenderers[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.IntersectionPointRenderers[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.IntersectionPointRenderers[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.IntersectionPointRenderers[i][j] = point;
                        }
                    }

                    while (Variable.IntersectionPointRenderers[i].Count > Variable.IntersectionPositions[i].Count)
                    {
                        UnityEngine.Object.Destroy(Variable.IntersectionPointRenderers[i].Last().gameObject);
                        Variable.IntersectionPointRenderers[i].RemoveAt(Variable.IntersectionPointRenderers[i].Count - 1);
                    }
                }
                else
                {
                    foreach (var point in Variable.IntersectionPointRenderers[i])
                        UnityEngine.Object.Destroy(point.gameObject);
                    Variable.IntersectionPointRenderers[i].Clear();
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (DangerEnables[i] && !float.IsNaN(Variable.DefeatPositions[i].x))
                {
                    var posDev = Variable.DefeatPositions[i] + yDev;
                    if (Variable.DefeatPointRenderers[i] == null)
                        InitSpriteRenderer(ref Variable.DefeatPointRenderers[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[11].Value, ref Variable.DefeatPointRenderers[i], posDev, (float)Variable.NodeSize.Value);
                }
                else if (Variable.DefeatPointRenderers[i] != null)
                    UnityEngine.Object.Destroy(Variable.DefeatPointRenderers[i].gameObject);

                if (DangerEnables[i] && Variable.DangerPositions[i].Count > 0)
                {
                    for (int j = 0; j < Variable.DangerPositions[i].Count; j++)
                    {
                        var posDev = Variable.DangerPositions[i][j] + yDev;

                        if (Variable.DangerPointRenderers[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.DangerPointRenderers[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.DangerPointRenderers[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.DangerPointRenderers[i][j] = point;
                        }
                    }

                    while (Variable.DangerPointRenderers[i].Count > Variable.DangerPositions[i].Count)
                    {
                        UnityEngine.Object.Destroy(Variable.DangerPointRenderers[i].Last().gameObject);
                        Variable.DangerPointRenderers[i].RemoveAt(Variable.DangerPointRenderers[i].Count - 1);
                    }
                }
                else
                {
                    foreach (var point in Variable.DangerPointRenderers[i])
                        UnityEngine.Object.Destroy(point.gameObject);
                    Variable.DangerPointRenderers[i].Clear();
                }
            }

            if (Variable.SwitchSwampPoint.getState() && Variable.SwampPositions.Count > 0)
            {
                for (int i = 0; i < Variable.SwampPositions.Count; i++)
                {
                    var posDev = Variable.SwampPositions[i] + yDev;

                    if (Variable.SwampPointRenderers.Count <= i)
                    {
                        var point = new SpriteRenderer();
                        InitSpriteRenderer(ref point);
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                        Variable.SwampPointRenderers.Add(point);
                    }
                    else
                    {
                        var point = Variable.SwampPointRenderers[i];
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                        Variable.SwampPointRenderers[i] = point;
                    }
                }

                while (Variable.SwampPointRenderers.Count > Variable.SwampPositions.Count)
                {
                    UnityEngine.Object.Destroy(Variable.SwampPointRenderers.Last().gameObject);
                    Variable.SwampPointRenderers.RemoveAt(Variable.SwampPointRenderers.Count - 1);
                }
            }
            else
            {
                foreach (var point in Variable.SwampPointRenderers)
                    UnityEngine.Object.Destroy(point.gameObject);
                Variable.SwampPointRenderers.Clear();
            }
        }


        private static void getLineRendererInfo(int index, out Variable.Switch sw, out Color color)
        {
            switch (index) {
                case 0:
                    sw = Variable.SwitchPathLine;
                    color = Variable.Colors[0].Value;
                    break;
                case 1:
                    sw = Variable.SwitchLadleLine;
                    color = Variable.Colors[1].Value;
                    break;
                case 2:
                    sw = Variable.SwitchTargetLine;
                    color =Variable.Colors[2].Value;
                    break;
                case 3:
                    sw = Variable.SwitchVortexLine;
                    color=Variable.Colors[3].Value;
                    break;
                case 4:
                    sw = Variable.SwitchVortexCurve;
                    color=Variable.Colors[6].Value;
                    break;
                default:
                    //throw new System.Exception($"Invalid line renderer index {index}");
                    throw new IndexOutOfRangeException($"Invalid line renderer index {index}");
            }
        }


        /// <summary>
        /// 渲染直线
        /// </summary>
        public static void SetLineRenderers()
        {
            // hide original ladle line.
            Variable.BaseLadleRenderer.enabled = !Variable.SwitchLadleLine.getState();

            for(var i = 0; i < Variable.LineRenderers.Count(); i++)
            {
                try
                {
                    getLineRendererInfo(i, out var sw, out var color);
                    if (sw.getState())
                    {
                        Calculation.InitLine(Variable.LineDirections[i], out var points);
                        if (points.Length == 2)
                        {
                            if (Variable.LineRenderers[i] == null)
                                InitLineRenderer(ref Variable.LineRenderers[i]);
                            UpdateLineRenderer(Variable.SolidMaterial, color, ref Variable.LineRenderers[i], points, false);
                        }
                        else if (Variable.LineRenderers[i] != null)
                            UnityEngine.Object.Destroy(Variable.LineRenderers[i].gameObject);
                    }
                    else if (Variable.LineRenderers[i] != null)
                        UnityEngine.Object.Destroy(Variable.LineRenderers[i].gameObject);
                }
                catch(IndexOutOfRangeException)
                {
                    continue;
                }
            }


            for (int i = 0; i < Variable.AuxiliaryLineDirections.Count(); i++)
                {
                    if (!double.IsNaN(Variable.AuxiliaryLineDirections[i]))
                    {
                        Calculation.InitLine(Variable.AuxiliaryLineDirections[i], out var points);
                        if (points.Length == 2)
                        {
                            if (Variable.AuxiliaryLines[i] == null)
                            {
                                InitLineRenderer(ref Variable.AuxiliaryLines[i]);
                            }
                            UpdateLineRenderer(Variable.SolidMaterial, UnityEngine.Color.black, ref Variable.AuxiliaryLines[i], points, false);
                        }
                        else
                        {
                            if (Variable.AuxiliaryLines[i] != null)
                                UnityEngine.Object.Destroy(Variable.AuxiliaryLines[i].gameObject);
                        }
                    }
                    else
                    {
                        if (Variable.AuxiliaryLines[i] != null)
                            UnityEngine.Object.Destroy(Variable.AuxiliaryLines[i].gameObject);
                    }
                }
        }

        /// <summary>
        /// 渲染曲线
        /// </summary>
        public static void SetCurveRenderers()
        {
            HideOriginalPaths(!Variable.SwitchPathCurve.getState());

            if (Variable.SwitchPathCurve.getState() && Variable.PathGraphical.Count > 0)
            {
                for (int i = 0; i < Variable.PathGraphical.Count; i++)
                {
                    var points = Variable.PathGraphical[i].Item1;
                    var isTp = Variable.PathGraphical[i].Item2;
                    var material = isTp ? Variable.DashedMaterial : Variable.SolidMaterial;
                    var color = i % 2 == 0 ? Variable.Colors[5].Value : Variable.Colors[4].Value;

                    if (Variable.PathCurveRenderers.Count <= i)
                    {
                        var curve = new LineRenderer();
                        InitLineRenderer(ref curve);
                        UpdateLineRenderer(material, color, ref curve, points, false);
                        Variable.PathCurveRenderers.Add(curve);
                    }
                    else
                    {
                        var curve = Variable.PathCurveRenderers[i];
                        UpdateLineRenderer(material, color, ref curve, points, false);
                        Variable.PathCurveRenderers[i] = curve;
                    }
                }

                while (Variable.PathCurveRenderers.Count > Variable.PathGraphical.Count)
                {
                    UnityEngine.Object.Destroy(Variable.PathCurveRenderers.Last().gameObject);
                    Variable.PathCurveRenderers.RemoveAt(Variable.PathCurveRenderers.Count - 1);
                }
            }
            else
            {
                foreach (var line in Variable.PathCurveRenderers)
                    UnityEngine.Object.Destroy(line.gameObject);
                Variable.PathCurveRenderers.Clear();
            }

            if (Variable.SwitchVortexCurve.getState() && Variable.VortexGraphical.Length >= 2)
            {
                if (Variable.VortexCurveRenderer == null)
                    InitLineRenderer(ref Variable.VortexCurveRenderer);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[6].Value, ref Variable.VortexCurveRenderer, Variable.VortexGraphical, false);
            }
            else if (Variable.VortexCurveRenderer != null)
                UnityEngine.Object.Destroy(Variable.VortexCurveRenderer.gameObject);
        }

        /// <summary>
        /// 渲染范围圈
        /// </summary>
        public static void SetRangeRenderers()
        {
            if (Variable.SwitchTargetRange.getState() && Variable.TargetEffect != null)
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Variable.IndicatorRotation, targetRot));
                var yDev = Variable.CurrentMapID == "Water" ? -130f : Variable.CurrentMapID == "Oil" ? -283f : -391f;
                var posDev = new Vector2(targetPos.x, targetPos.y + yDev);
                double[] rads = [1.53, 1.0 / 3.0 - devRot / 216.0, 1.0 / 18.0 - devRot / 216.0];

                Calculation.InitRange(rads[0], targetPos.x, targetPos.y, out var pointsOut);
                if (Variable.TargetRangeRenderers[0] == null)
                    InitLineRenderer(ref Variable.TargetRangeRenderers[0]);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.TargetRangeRenderers[0], pointsOut, true);

                if (rads[1] > Variable.LineWidth.Value)
                {
                    if (Variable.TargetDiskRenderers[0] != null)
                        UnityEngine.Object.Destroy(Variable.TargetDiskRenderers[0].gameObject);
                    Calculation.InitRange(rads[1], targetPos.x, targetPos.y, out var pointsMid);
                    if (Variable.TargetRangeRenderers[1] == null)
                        InitLineRenderer(ref Variable.TargetRangeRenderers[1]);
                    UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.TargetRangeRenderers[1], pointsMid, true);
                }
                else if (rads[1] > 0)
                {
                    var scale = rads[1] / Variable.LineWidth.Value;
                    if (Variable.TargetRangeRenderers[1] != null)
                        UnityEngine.Object.Destroy(Variable.TargetRangeRenderers[1].gameObject);
                    if (Variable.TargetDiskRenderers[0] == null)
                        InitSpriteRenderer(ref Variable.TargetDiskRenderers[0]);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.Colors[7].Value, ref Variable.TargetDiskRenderers[0], posDev, (float)scale);
                }
                else
                {
                    if (Variable.TargetRangeRenderers[1] != null)
                        UnityEngine.Object.Destroy(Variable.TargetRangeRenderers[1].gameObject);
                    if (Variable.TargetDiskRenderers[0] != null)
                        UnityEngine.Object.Destroy(Variable.TargetDiskRenderers[0].gameObject);
                }

                if (rads[2] > 0)
                {
                    var scale = rads[2] / Variable.LineWidth.Value;
                    if (Variable.TargetDiskRenderers[1] == null)
                        InitSpriteRenderer(ref Variable.TargetDiskRenderers[1]);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.Colors[7].Value, ref Variable.TargetDiskRenderers[1], posDev, (float)scale);
                }
                else if (Variable.TargetDiskRenderers[1] != null)
                    UnityEngine.Object.Destroy(Variable.TargetDiskRenderers[1].gameObject);
            }
            else
            {
                foreach (var line in Variable.TargetRangeRenderers)
                    if (line != null)
                        UnityEngine.Object.Destroy(line.gameObject);
                foreach (var sprite in Variable.TargetDiskRenderers)
                    if (sprite != null)
                        UnityEngine.Object.Destroy(sprite.gameObject);
            }

            if (Variable.SwitchVortexRange.getState() && Variable.CurrentMapID != "Wine")
            {
                var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
                var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
                if (Variable.VortexIndex[mapindex] >= 0)
                {
                    var selVortex = vortexList[Variable.VortexIndex[mapindex]];
                    Calculation.InitRange(selVortex.r, selVortex.x, selVortex.y, out var points);
                    if (Variable.VortexRangeRenderer == null)
                        InitLineRenderer(ref Variable.VortexRangeRenderer);
                    UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.VortexRangeRenderer, points, true);
                }
                else if (Variable.VortexRangeRenderer != null)
                    UnityEngine.Object.Destroy(Variable.VortexRangeRenderer.gameObject);
            }
            else if (Variable.VortexRangeRenderer != null)
                UnityEngine.Object.Destroy(Variable.VortexRangeRenderer.gameObject);
        }
        #endregion

        #region 隐藏原生对象
        /// <summary>
        /// 隐藏原生路径
        /// </summary>
        public static void HideOriginalPaths(bool hide)
        {
            var hints = Managers.RecipeMap.path.fixedPathHints;
            foreach (var hint in hints)
                if (hint != null)
                {
                    var renderers = hint.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                        renderer.enabled = hide;
                }
        }
        #endregion
    }
}
