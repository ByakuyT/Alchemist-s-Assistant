using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using System.Linq;
using UnityEngine;

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
            if (!Variable.Keys[10].Value.IsDown())
                return;

            var indicatorMapItems = Object.FindObjectsByType<IndicatorMapItem>(FindObjectsSortMode.None);
            foreach (var indicatorMapItem in indicatorMapItems)
            {
                if (!Variable.DerivedEnables[8])
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
            bool[] ClosestEnables = [Variable.DerivedEnables[4], Variable.DerivedEnables[1]];
            bool[] IntersectionEnables = [Variable.DerivedEnables[9], Variable.DerivedEnables[10], Variable.DerivedEnables[11], Variable.DerivedEnables[12]];
            bool[] DangerEnables = [Variable.DerivedEnables[13], Variable.DerivedEnables[14], Variable.DerivedEnables[15]];
            var yDev = new Vector2(0, Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y);

            for (int i = 0; i < 2; i++)
            {
                if (ClosestEnables[i] && !float.IsNaN(Variable.ClosestPositions[i].x))
                {
                    var posDev = Variable.ClosestPositions[i] + yDev;
                    if (Variable.ClosestPoints[i] == null)
                        InitSpriteRenderer(ref Variable.ClosestPoints[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[8].Value, ref Variable.ClosestPoints[i], posDev, (float)Variable.NodeSize.Value);
                }
                else if (Variable.ClosestPoints[i] != null)
                    Object.Destroy(Variable.ClosestPoints[i].gameObject);
            }

            for (int i = 0; i < 4; i++)
            {
                if (IntersectionEnables[i] && Variable.IntersectionPositions[i].Count > 0)
                {
                    for (int j = 0; j < Variable.IntersectionPositions[i].Count; j++)
                    {
                        var posDev = Variable.IntersectionPositions[i][j] + yDev;

                        if (Variable.IntersectionPoints[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.IntersectionPoints[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.IntersectionPoints[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.IntersectionPoints[i][j] = point;
                        }
                    }

                    while (Variable.IntersectionPoints[i].Count > Variable.IntersectionPositions[i].Count)
                    {
                        Object.Destroy(Variable.IntersectionPoints[i].Last().gameObject);
                        Variable.IntersectionPoints[i].RemoveAt(Variable.IntersectionPoints[i].Count - 1);
                    }
                }
                else
                {
                    foreach (var point in Variable.IntersectionPoints[i])
                        Object.Destroy(point.gameObject);
                    Variable.IntersectionPoints[i].Clear();
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (DangerEnables[i] && !float.IsNaN(Variable.DefeatPositions[i].x))
                {
                    var posDev = Variable.DefeatPositions[i] + yDev;
                    if (Variable.DefeatPoints[i] == null)
                        InitSpriteRenderer(ref Variable.DefeatPoints[i]);
                    UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[11].Value, ref Variable.DefeatPoints[i], posDev, (float)Variable.NodeSize.Value);
                }
                else if (Variable.DefeatPoints[i] != null)
                    Object.Destroy(Variable.DefeatPoints[i].gameObject);

                if (DangerEnables[i] && Variable.DangerPositions[i].Count > 0)
                {
                    for (int j = 0; j < Variable.DangerPositions[i].Count; j++)
                    {
                        var posDev = Variable.DangerPositions[i][j] + yDev;

                        if (Variable.DangerPoints[i].Count <= j)
                        {
                            var point = new SpriteRenderer();
                            InitSpriteRenderer(ref point);
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.DangerPoints[i].Add(point);
                        }
                        else
                        {
                            var point = Variable.DangerPoints[i][j];
                            UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                            Variable.DangerPoints[i][j] = point;
                        }
                    }

                    while (Variable.DangerPoints[i].Count > Variable.DangerPositions[i].Count)
                    {
                        Object.Destroy(Variable.DangerPoints[i].Last().gameObject);
                        Variable.DangerPoints[i].RemoveAt(Variable.DangerPoints[i].Count - 1);
                    }
                }
                else
                {
                    foreach (var point in Variable.DangerPoints[i])
                        Object.Destroy(point.gameObject);
                    Variable.DangerPoints[i].Clear();
                }
            }

            if (Variable.DerivedEnables[16] && Variable.SwampPositions.Count > 0)
            {
                for (int i = 0; i < Variable.SwampPositions.Count; i++)
                {
                    var posDev = Variable.SwampPositions[i] + yDev;

                    if (Variable.SwampPoints.Count <= i)
                    {
                        var point = new SpriteRenderer();
                        InitSpriteRenderer(ref point);
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                        Variable.SwampPoints.Add(point);
                    }
                    else
                    {
                        var point = Variable.SwampPoints[i];
                        UpdateSpriteRenderer(Variable.SquareSprite, Variable.Colors[10].Value, ref point, posDev, (float)Variable.NodeSize.Value);
                        Variable.SwampPoints[i] = point;
                    }
                }

                while (Variable.SwampPoints.Count > Variable.SwampPositions.Count)
                {
                    Object.Destroy(Variable.SwampPoints.Last().gameObject);
                    Variable.SwampPoints.RemoveAt(Variable.SwampPoints.Count - 1);
                }
            }
            else
            {
                foreach (var point in Variable.SwampPoints)
                    Object.Destroy(point.gameObject);
                Variable.SwampPoints.Clear();
            }
        }

        /// <summary>
        /// 渲染直线
        /// </summary>
        public static void SetLineRenderers()
        {
            Variable.BaseLadleRenderer.enabled = !Variable.DerivedEnables[1];

            for (int i = 0; i < 4; i++)
            {
                if (Variable.DerivedEnables[i])
                {
                    Calculation.InitLine(Variable.LineDirections[i], out var points);
                    if (points.Length == 2)
                    {
                        if (Variable.Lines[i] == null)
                            InitLineRenderer(ref Variable.Lines[i]);
                        UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[i].Value, ref Variable.Lines[i], points, false);
                    }
                    else if (Variable.Lines[i] != null)
                        Object.Destroy(Variable.Lines[i].gameObject);
                }
                else if (Variable.Lines[i] != null)
                    Object.Destroy(Variable.Lines[i].gameObject);
            }
        }

        /// <summary>
        /// 渲染曲线
        /// </summary>
        public static void SetCurveRenderers()
        {
            HideOriginalPaths(!Variable.DerivedEnables[4]);

            if (Variable.DerivedEnables[4] && Variable.PathGraphical.Count > 0)
            {
                for (int i = 0; i < Variable.PathGraphical.Count; i++)
                {
                    var points = Variable.PathGraphical[i].Item1;
                    var isTp = Variable.PathGraphical[i].Item2;
                    var material = isTp ? Variable.DashedMaterial : Variable.SolidMaterial;
                    var color = i % 2 == 0 ? Variable.Colors[5].Value : Variable.Colors[4].Value;

                    if (Variable.PathCurves.Count <= i)
                    {
                        var curve = new LineRenderer();
                        InitLineRenderer(ref curve);
                        UpdateLineRenderer(material, color, ref curve, points, false);
                        Variable.PathCurves.Add(curve);
                    }
                    else
                    {
                        var curve = Variable.PathCurves[i];
                        UpdateLineRenderer(material, color, ref curve, points, false);
                        Variable.PathCurves[i] = curve;
                    }
                }

                while (Variable.PathCurves.Count > Variable.PathGraphical.Count)
                {
                    Object.Destroy(Variable.PathCurves.Last().gameObject);
                    Variable.PathCurves.RemoveAt(Variable.PathCurves.Count - 1);
                }
            }
            else
            {
                foreach (var line in Variable.PathCurves)
                    Object.Destroy(line.gameObject);
                Variable.PathCurves.Clear();
            }

            if (Variable.DerivedEnables[5] && Variable.VortexGraphical.Length >= 2)
            {
                if (Variable.VortexCurve == null)
                    InitLineRenderer(ref Variable.VortexCurve);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[6].Value, ref Variable.VortexCurve, Variable.VortexGraphical, false);
            }
            else if (Variable.VortexCurve != null)
                Object.Destroy(Variable.VortexCurve.gameObject);
        }

        /// <summary>
        /// 渲染范围圈
        /// </summary>
        public static void SetRangeRenderers()
        {
            if (Variable.DerivedEnables[6] && Variable.TargetEffect != null)
            {
                Vector2 targetPos = Variable.TargetEffect.transform.localPosition;
                var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
                var devRot = Mathf.Abs(Mathf.DeltaAngle(Variable.IndicatorRotation, targetRot));
                var yDev = Variable.BaseRenderPosition.y - Variable.IndicatorPosition.y;
                var posDev = new Vector2(targetPos.x, targetPos.y + yDev);
                double[] rads = [1.53, 1.0 / 3.0 - devRot / 216.0, 1.0 / 18.0 - devRot / 216.0];

                Calculation.InitRange(rads[0], targetPos.x, targetPos.y, out var pointsOut);
                if (Variable.TargetRanges[0] == null)
                    InitLineRenderer(ref Variable.TargetRanges[0]);
                UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.TargetRanges[0], pointsOut, true);

                if (rads[1] > Variable.LineWidth.Value)
                {
                    if (Variable.TargetDisks[0] != null)
                        Object.Destroy(Variable.TargetDisks[0].gameObject);
                    Calculation.InitRange(rads[1], targetPos.x, targetPos.y, out var pointsMid);
                    if (Variable.TargetRanges[1] == null)
                        InitLineRenderer(ref Variable.TargetRanges[1]);
                    UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.TargetRanges[1], pointsMid, true);
                }
                else if (rads[1] > 0)
                {
                    var scale = rads[1] / Variable.LineWidth.Value;
                    if (Variable.TargetRanges[1] != null)
                        Object.Destroy(Variable.TargetRanges[1].gameObject);
                    if (Variable.TargetDisks[0] == null)
                        InitSpriteRenderer(ref Variable.TargetDisks[0]);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.Colors[7].Value, ref Variable.TargetDisks[0], posDev, (float)scale);
                }
                else
                {
                    if (Variable.TargetRanges[1] != null)
                        Object.Destroy(Variable.TargetRanges[1].gameObject);
                    if (Variable.TargetDisks[0] != null)
                        Object.Destroy(Variable.TargetDisks[0].gameObject);
                }

                if (rads[2] > 0)
                {
                    var scale = rads[2] / Variable.LineWidth.Value;
                    if (Variable.TargetDisks[1] == null)
                        InitSpriteRenderer(ref Variable.TargetDisks[1]);
                    UpdateSpriteRenderer(Variable.RoundSprite, Variable.Colors[7].Value, ref Variable.TargetDisks[1], posDev, (float)scale);
                }
                else if (Variable.TargetDisks[1] != null)
                    Object.Destroy(Variable.TargetDisks[1].gameObject);
            }
            else
            {
                foreach (var line in Variable.TargetRanges)
                    if (line != null)
                        Object.Destroy(line.gameObject);
                foreach (var sprite in Variable.TargetDisks)
                    if (sprite != null)
                        Object.Destroy(sprite.gameObject);
            }

            if (Variable.DerivedEnables[7] && Variable.CurrentMapID != "Wine")
            {
                var mapindex = Variable.CurrentMapID == "Water" ? 0 : 1;
                var vortexList = mapindex == 0 ? Variable.Vortex_Water : Variable.Vortex_Oil;
                if (Variable.VortexIndex[mapindex] >= 0)
                {
                    var selVortex = vortexList[Variable.VortexIndex[mapindex]];
                    Calculation.InitRange(selVortex.r, selVortex.x, selVortex.y, out var points);
                    if (Variable.VortexRange == null)
                        InitLineRenderer(ref Variable.VortexRange);
                    UpdateLineRenderer(Variable.SolidMaterial, Variable.Colors[7].Value, ref Variable.VortexRange, points, true);
                }
                else if (Variable.VortexRange != null)
                    Object.Destroy(Variable.VortexRange.gameObject);
            }
            else if (Variable.VortexRange != null)
                Object.Destroy(Variable.VortexRange.gameObject);
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
