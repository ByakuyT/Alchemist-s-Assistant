using PotionCraft.ManagersSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AlchAssV3
{
    public static class Geometry
    {
        #region 计算几何
        /// <summary>
        /// 计算线段到点的距离平方
        /// </summary>
        public static void SqrDisToPoint(Vector2 p0, Vector2 p1, Vector2 tar, bool isTp, out double dis, out Vector2 pos)
        {
            if (Math.Abs(p0.x - p1.x) < 1e-5 && Math.Abs(p0.y - p1.y) < 1e-5)
            {
                dis = (p0 - tar).sqrMagnitude;
                pos = p0;
                return;
            }

            if (isTp)
            {
                var dis0 = (p0 - tar).sqrMagnitude;
                var dis1 = (p1 - tar).sqrMagnitude;
                if (dis0 < dis1) { dis = dis0; pos = p0; }
                else { dis = dis1; pos = p1; }
                return;
            }

            var v = p1 - p0;
            var w = tar - p0;
            var t = Mathf.Clamp01(Vector2.Dot(w, v) / v.sqrMagnitude);
            pos = p0 + t * v;
            dis = (pos - tar).sqrMagnitude;
        }

        /// <summary>
        /// 计算线段与线段的交点
        /// </summary>
        public static void LineVsLine(Vector2 p0, Vector2 p1, Variable.Shape.Line line, out (Vector2, double)? pos)
        {
            pos = null;
            var q0 = new Vector2((float)line.X1, (float)line.Y1);
            var q1 = new Vector2((float)line.X2, (float)line.Y2);
            var p = p1 - p0;
            var q = q1 - q0;
            var Cpq = Cross(p, q);
            if (Math.Abs(Cpq) <= 0)
                return;

            var r = q0 - p0;
            var Crp = Cross(r, p);
            var Crq = Cross(r, q);
            var s = Crq / Cpq;
            var t = Crp / Cpq;
            if (s >= 0 && s < 1 && t >= 0 && t <= 1)
                pos = (p0 + (float)s * p, s);
        }

        /// <summary>
        /// 计算线段与圆的交点
        /// </summary>
        public static void LineVsCircle(Vector2 p0, Vector2 p1, Vector2 cen, double rad, out List<Vector2> pos, out List<double> ti)
        {
            pos = []; ti = [];
            if (rad <= 0)
                return;

            var d = p1 - p0;
            var f = p0 - cen;
            var a = d.sqrMagnitude;
            var b = 2 * Vector2.Dot(f, d);
            var c = f.sqrMagnitude - rad * rad;
            var delta = b * b - 4 * a * c;
            if (delta <= 0)
                return;

            var sqrtD = Math.Sqrt(delta);
            double[] ts = [(-b - sqrtD) / (2 * a), (-b + sqrtD) / (2 * a)];
            foreach (var t in ts)
                if (t >= 0 && t < 1)
                {
                    pos.Add(p0 + d * (float)t);
                    ti.Add(t);
                }
        }

        /// <summary>
        /// 计算线段与圆弧的交点
        /// </summary>
        public static void LineVsArc(Vector2 p0, Vector2 p1, Variable.Shape.Arc arc, out List<(Vector2, double)> pos)
        {
            pos = [];
            var cen = new Vector2((float)arc.X, (float)arc.Y);
            var rad = arc.R;
            LineVsCircle(p0, p1, cen, rad, out var points, out var ti);

            for (var i = 0; i < points.Count; i++)
            {
                var theta = Math.Atan2(points[i].y - arc.Y, points[i].x - arc.X);
                if (theta < 0) theta += Math.PI * 2;
                if (Between(theta, arc.StartAngle, arc.EndAngle))
                    pos.Add((points[i], ti[i]));
            }
        }

        /// <summary>
        /// 求解螺线相交查询的方程
        /// </summary>
        public static void FindRoots(Func<double, double> f, double x0, double x1, out List<(Vector2, double)> roots)
        {
            roots = [];
            var prevX = x0;
            var prevF = f(x0);

            var step = (int)Math.Ceiling((x1 - x0) * 100);
            for (var i = 1; i <= step; i++)
            {
                var x = Math.Min(x0 + i * 1e-2, x1);
                var fx = f(x);
                if (prevF * fx <= 0)
                {
                    var t = Brent(f, prevX, x);
                    if (roots.Count == 0 || Math.Abs(t - roots[roots.Count - 1].Item2) > 1e-5)
                    {
                        var r = t * Variable.VortexA;
                        var pos = new Vector2((float)(r * Math.Cos(t)), (float)(r * Math.Sin(t)));
                        roots.Add((pos, t));
                    }
                }
                prevX = x; prevF = fx;
            }
        }
        #endregion

        #region BVH查询
        /// <summary>
        /// 线段AABB相交
        /// </summary>
        public static bool LineAABB(Vector2 p0, Vector2 p1, Variable.Node node)
        {
            var d = p1 - p0;
            var tmin = 0.0;
            var tmax = 1.0;

            if (Mathf.Abs(d.x) < 1e-5)
            {
                if (p0.x < node.MinX || p0.x > node.MaxX)
                    return false;
            }
            else
            {
                var od = 1 / d.x;
                var t1 = (node.MinX - p0.x) * od;
                var t2 = (node.MaxX - p0.x) * od;
                if (t1 > t2)
                    (t1, t2) = (t2, t1);
                tmin = Math.Max(tmin, t1);
                tmax = Math.Min(tmax, t2);
                if (tmin > tmax)
                    return false;
            }

            if (Mathf.Abs(d.y) < 1e-5)
            {
                if (p0.y < node.MinY || p0.y > node.MaxY)
                    return false;
            }
            else
            {
                var od = 1 / d.y;
                var t1 = (node.MinY - p0.y) * od;
                var t2 = (node.MaxY - p0.y) * od;
                if (t1 > t2)
                    (t1, t2) = (t2, t1);
                tmin = Math.Max(tmin, t1);
                tmax = Math.Min(tmax, t2);
                if (tmin > tmax)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 圆AABB相交
        /// </summary>
        public static bool CircleAABB(double x, double y, double r, Variable.Node node)
        {
            var cx = Math.Max(node.MinX, Math.Min(x, node.MaxX));
            var cy = Math.Max(node.MinY, Math.Min(y, node.MaxY));
            var dx = x - cx;
            var dy = y - cy;
            return (dx * dx + dy * dy) <= (r * r);
        }

        /// <summary>
        /// 线段查询列表
        /// </summary>
        public static void LineBVH(Vector2 p0, Vector2 p1, List<Variable.Node> bvh, out List<int> result)
        {
            result = [];
            Stack<Variable.Node> stack = [];

            stack.Push(bvh[0]);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!LineAABB(p0, p1, node))
                    continue;

                switch (node)
                {
                    case Variable.Node.LeafNode leaf:
                        result.AddRange(leaf.Items);
                        break;

                    case Variable.Node.InternalNode inter:
                        if (inter.Left >= 0) stack.Push(bvh[inter.Left]);
                        if (inter.Right >= 0) stack.Push(bvh[inter.Right]);
                        break;
                }
            }
        }

        /// <summary>
        /// 圆查询列表
        /// </summary>
        public static void CircleBVH(double x, double y, double r, List<Variable.Node> bvh, out List<int> result)
        {
            result = [];
            Stack<Variable.Node> stack = [];

            stack.Push(bvh[0]);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!CircleAABB(x, y, r, node))
                    continue;

                switch (node)
                {
                    case Variable.Node.LeafNode leaf:
                        result.AddRange(leaf.Items);
                        break;

                    case Variable.Node.InternalNode inter:
                        if (inter.Left >= 0) stack.Push(bvh[inter.Left]);
                        if (inter.Right >= 0) stack.Push(bvh[inter.Right]);
                        break;
                }
            }
        }

        /// <summary>
        /// 矩形查询是否命中
        /// </summary>
        public static bool SquareBVH(double minx, double miny, double maxx, double maxy, List<Variable.Node> bvh)
        {
            Stack<Variable.Node> stack = [];

            stack.Push(bvh[0]);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node.MaxX < minx || node.MinX > maxx || node.MaxY < miny || node.MinY > maxy)
                    continue;

                switch (node)
                {
                    case Variable.Node.LeafNode:
                        return true;

                    case Variable.Node.InternalNode inter:
                        if (inter.Left >= 0) stack.Push(bvh[inter.Left]);
                        if (inter.Right >= 0) stack.Push(bvh[inter.Right]);
                        break;
                }
            }
            return false;
        }
        #endregion

        #region 交点计算处理
        /// <summary>
        /// 预处理目标范围检测
        /// </summary>
        public static void TargetRange(Vector2 p0, Vector2 p1, Vector2 cen, bool isTp, out List<Vector2> pos)
        {
            pos = [];
            if (Math.Abs(p0.x - p1.x) < 1e-5 && Math.Abs(p0.y - p1.y) < 1e-5)
                return;

            var targetRot = Variable.TargetEffect.transform.localEulerAngles.z;
            var devRot = Mathf.Abs(Mathf.DeltaAngle(Managers.RecipeMap.indicatorRotation.Value, targetRot));
            double[] targetRad = [1.53, 1.0 / 3.0 - devRot / 216.0, 1.0 / 18.0 - devRot / 216.0];

            for (var i = 0; i < 3; i++)
            {
                LineVsCircle(p0, p1, cen, targetRad[i], out var points, out _);
                pos.AddRange(points);
            }
            if (isTp)
                pos = pos.Count % 2 != 0 ? [p0] : [];
        }

        /// <summary>
        /// 预处理漩涡范围检测
        /// </summary>
        public static void VortexRange(Vector2 p0, Vector2 p1, Vector2 cen, double rad, bool isTp, out List<Vector2> pos)
        {
            pos = [];
            if (Math.Abs(p0.x - p1.x) < 1e-5 && Math.Abs(p0.y - p1.y) < 1e-5)
                return;

            LineVsCircle(p0, p1, cen, rad, out pos, out _);
            if (isTp)
                pos = pos.Count % 2 != 0 ? [p0] : [];
        }

        /// <summary>
        /// 预处理折线区域检测
        /// </summary>
        public static void DangerLine(Vector2 p0, Vector2 p1, string mapid, int index, bool isTp, out List<(Vector2, int, double, int)> pos)
        {
            pos = [];
            if (Math.Abs(p0.x - p1.x) < 1e-5 && Math.Abs(p0.y - p1.y) < 1e-5)
                return;

            List<(Vector2, int, double, int)> points = [];
            if (mapid != "Wine")
            {
                LineBVH(p0, p1, Variable.StrongBVHs[Variable.MapId[mapid]], out var items);
                foreach (var item in items)
                    switch (Variable.Strongs[Variable.MapId[mapid]][item])
                    {
                        case Variable.Shape.Line line:
                            LineVsLine(p0, p1, line, out var Pline);
                            if (Pline.HasValue) points.Add((Pline.Value.Item1, index, Pline.Value.Item2, 0));
                            break;
                        case Variable.Shape.Arc arc:
                            LineVsArc(p0, p1, arc, out var Parc);
                            points.AddRange(Parc.Select(x => (x.Item1, index, x.Item2, 0)));
                            break;
                    }
            }
            else
            {
                List<Variable.Shape>[] shapes = [Variable.Strongs[2], Variable.WeakWine, Variable.HealWine];
                List<Variable.Node>[] bvh = [Variable.StrongBVHs[2], Variable.WeakWineBVH, Variable.HealWineBVH];

                for (var i = 0; i < 3; i++)
                {
                    LineBVH(p0, p1, bvh[i], out var items);
                    foreach (var item in items)
                        switch (shapes[i][item])
                        {
                            case Variable.Shape.Line line:
                                LineVsLine(p0, p1, line, out var Pline);
                                if (Pline.HasValue) points.Add((Pline.Value.Item1, index, Pline.Value.Item2, i));
                                break;
                            case Variable.Shape.Arc arc:
                                LineVsArc(p0, p1, arc, out var Parc);
                                points.AddRange(Parc.Select(x => (x.Item1, index, x.Item2, i)));
                                break;
                        }
                }
            }

            points.Sort((x, y) => x.Item3.CompareTo(y.Item3));
            List<(Vector2, int, double, int)> pointsort = [];
            if (points.Count > 0)
            {
                pointsort.Add(points[0]);
                for (var i = 1; i < points.Count; i++)
                    if (points[i].Item4 != points[i - 1].Item4 || points[i].Item3 - points[i - 1].Item3 > 1e-5)
                        pointsort.Add(points[i]);
            }

            if (isTp)
                pos = points.Count % 2 != 0 ? [(p0, index, 0, 0)] : [];
            else
                pos = pointsort;
        }

        /// <summary>
        /// 预处理螺线区域检测
        /// </summary>
        public static void DangerSpiral(double cX, double cY, double rot, double maxT, double minT, string mapid, out List<(Vector2, double)> pos)
        {
            pos = [];
            CircleBVH(cX, cY, maxT * Variable.VortexA, Variable.StrongBVHs[Variable.MapId[mapid]], out var items);

            List<Variable.Shape> localShapes = [];
            foreach (var item in items)
                switch (Variable.Strongs[Variable.MapId[mapid]][item])
                {
                    case Variable.Shape.Line line:
                        TransToLocal(cX, cY, rot, line.X1, line.Y1, out var p1x, out var p1y);
                        TransToLocal(cX, cY, rot, line.X2, line.Y2, out var p2x, out var p2y);
                        localShapes.Add(new Variable.Shape.Line(p1x, p1y, p2x, p2y));
                        break;
                    case Variable.Shape.Arc arc:
                        TransToLocal(cX, cY, rot, arc.X, arc.Y, out var cx, out var cy);
                        var sa = (arc.StartAngle - rot) % (2 * Math.PI);
                        var ea = (arc.EndAngle - rot) % (2 * Math.PI);
                        if (sa < 0) sa += 2 * Math.PI;
                        if (ea < 0) ea += 2 * Math.PI;
                        localShapes.Add(new Variable.Shape.Arc(cx, cy, arc.R, sa, ea));
                        break;
                }

            var len = localShapes.Count;
            var roots = new List<(Vector2, double)>[len];
            var th0 = minT; var th1 = maxT;
            Parallel.For(0, len, i =>
            {
                roots[i] = [];
                switch (localShapes[i])
                {
                    case Variable.Shape.Line line:
                        var a = line.Y2 - line.Y1;
                        var b = line.X1 - line.X2;
                        var r = Math.Sqrt(a * a + b * b);
                        var K = 2 * Math.PI * (line.X2 * line.Y1 - line.X1 * line.Y2) / r;
                        var P = Math.Atan2(b, a);

                        FindRoots(x => x * Math.Cos(x - P) + K, th0, th1, out var Rline);
                        foreach (var root in Rline)
                        {
                            var x = root.Item1.x;
                            var y = root.Item1.y;
                            var xin = x >= Math.Min(line.X1, line.X2) && x <= Math.Max(line.X1, line.X2);
                            var yin = y >= Math.Min(line.Y1, line.Y2) && y <= Math.Max(line.Y1, line.Y2);
                            if (xin && yin)
                                roots[i].Add(root);
                        }
                        break;
                    case Variable.Shape.Arc arc:
                        var m = arc.X * arc.X + arc.Y * arc.Y;
                        var Q = Math.Atan2(arc.Y, arc.X);
                        var B = 4 * Math.PI * Math.PI * (m - arc.R * arc.R);
                        var A = -4 * Math.PI * Math.Sqrt(m);

                        FindRoots(x => x * x + A * x * Math.Cos(x - Q) + B, th0, th1, out var Rarc);
                        foreach (var root in Rarc)
                        {
                            var th = Math.Atan2(root.Item1.y - arc.Y, root.Item1.x - arc.X);
                            if (th < 0) th += Math.PI * 2;
                            if (Between(th, arc.StartAngle, arc.EndAngle))
                                roots[i].Add(root);
                        }
                        break;
                }
            });

            List<(Vector2, double)> points = [.. roots
                .Where(list => list != null)
                .SelectMany(list => list)
                .OrderByDescending(item => item.Item2)];
            if (points.Count > 0)
            {
                TransToGlobal(cX, cY, rot, points[0].Item1.x, points[0].Item1.y, out var nx0, out var ny0);
                pos.Add((new Vector2((float)nx0, (float)ny0), points[0].Item2));
                for (var i = 1; i < points.Count; i++)
                    if (points[i - 1].Item2 - points[i].Item2 > 1e-5)
                    {
                        TransToGlobal(cX, cY, rot, points[i].Item1.x, points[i].Item1.y, out var nxi, out var nyi);
                        pos.Add((new Vector2((float)nxi, (float)nyi), points[i].Item2));
                    }
            }
        }

        /// <summary>
        /// 后处理折线区域检测
        /// </summary>
        public static void DefeatLine(List<(Vector3, bool)> line, List<(Vector2, int, double, int)> points, double stlen, bool[] stin, string mapid, out Vector2 pos, out double dis)
        {
            pos = new Vector2(float.NaN, float.NaN); dis = 0.0;
            if (line.Count < 2)
                return;

            var cnt = line.Count - 1;
            double[] len = new double[cnt];
            double[] sum = new double[cnt];
            var curLen = (1 - stlen) * 2.5;
            var preDis = 0.0; var preLen = curLen;
            var gotPos = false; var gotDis = false;
            var id = 0;

            sum[0] = len[0] = line[1].Item2 ? 0 : Vector2.Distance(line[0].Item1, line[1].Item1);
            for (var i = 1; i < cnt; i++)
            {
                if (line[i + 1].Item2)
                    len[i] = 0;
                else
                    len[i] = Vector2.Distance(line[i].Item1, line[i + 1].Item1);
                sum[i] = len[i] + sum[i - 1];
            }

            if (mapid != "Wine")
            {
                var inter = stin[0];
                while (id < points.Count)
                {
                    if (gotDis && gotPos)
                        break;

                    var curDis = points[id].Item2 > 0 ? sum[points[id].Item2 - 1] : 0;
                    curDis += points[id].Item3 * len[points[id].Item2];
                    curLen = inter ? curLen + curDis - preDis : 0;

                    if (!gotPos && curLen >= 2.5)
                    {
                        var j = id > 0 ? points[id - 1].Item2 : 0;
                        while (sum[j] < preDis + 2.5 - preLen) j++;
                        var sumj = j > 0 ? sum[j - 1] : 0;
                        var t = (preDis + 2.5 - preLen - sumj) / len[j];
                        pos = line[j].Item1 + (float)t * (line[j + 1].Item1 - line[j].Item1);
                        gotPos = true;
                    }

                    if (inter && !gotDis)
                    {
                        dis = curLen;
                        gotDis = true;
                    }

                    preDis = curDis; preLen = curLen;
                    inter = !inter;
                    id++;
                }

                curLen = inter ? curLen + sum[cnt - 1] - preDis : 0;
                if (!gotPos && curLen >= 2.5)
                {
                    var j = id > 0 ? points[id - 1].Item2 : 0;
                    while (sum[j] < preDis + 2.5 - preLen) j++;
                    var sumj = j > 0 ? sum[j - 1] : 0;
                    var t = (preDis + 2.5 - preLen - sumj) / len[j];
                    pos = line[j].Item1 + (float)t * (line[j + 1].Item1 - line[j].Item1);
                }
                if (!gotDis)
                    dis = curLen;
            }
            else
            {
                var inter = stin;
                while (id < points.Count)
                {
                    if (gotDis && gotPos)
                        break;

                    var curDis = points[id].Item2 > 0 ? sum[points[id].Item2 - 1] : 0;
                    curDis += points[id].Item3 * len[points[id].Item2];
                    var disLen = curDis - preDis;
                    if (inter[2])
                        curLen -= disLen * 2.5;
                    if (inter[0])
                        curLen += disLen;
                    else if (inter[1])
                        curLen += disLen * 0.25;
                    else if (!inter[2])
                        curLen -= disLen * 0.1875;
                    curLen = Math.Max(0, curLen);
                    var coef = inter[0] ? 1.0 : 4.0;

                    if (!gotPos && curLen >= 2.5)
                    {
                        var j = id > 0 ? points[id - 1].Item2 : 0;
                        while (sum[j] < preDis + (2.5 - preLen) * coef) j++;
                        var sumj = j > 0 ? sum[j - 1] : 0;
                        var t = (preDis + (2.5 - preLen) * coef - sumj) / len[j];
                        pos = line[j].Item1 + (float)t * (line[j + 1].Item1 - line[j].Item1);
                        gotPos = true;
                    }

                    var coe1 = (inter[0] || inter[1]) && !inter[2] && points[id].Item4 == 2;
                    var coe2 = inter[0] && !inter[1] && !inter[2] && points[id].Item4 == 0;
                    var coe3 = !inter[0] && inter[1] && !inter[2] && points[id].Item4 == 1;
                    if ((coe1 || coe2 || coe3) && !gotDis)
                    {
                        dis = curLen;
                        gotDis = true;
                    }

                    preDis = curDis; preLen = curLen;
                    inter[points[id].Item4] = !inter[points[id].Item4];
                    id++;
                }

                var endLen = sum[cnt - 1] - preDis;
                if (inter[2])
                    curLen -= endLen * 2.5;
                if (inter[0])
                    curLen += endLen;
                else if (inter[1])
                    curLen += endLen * 0.25;
                else if (!inter[2])
                    curLen -= endLen * 0.1875;
                curLen = Math.Max(0, curLen);
                var enef = inter[0] ? 1.0 : 4.0;

                if (!gotPos && curLen >= 2.5)
                {
                    var j = id > 0 ? points[id - 1].Item2 : 0;
                    while (sum[j] < preDis + (2.5 - preLen) * enef) j++;
                    var sumj = j > 0 ? sum[j - 1] : 0;
                    var t = (preDis + (2.5 - preLen) * enef - sumj) / len[j];
                    pos = line[j].Item1 + (float)t * (line[j + 1].Item1 - line[j].Item1);
                }
                if (!gotDis)
                    dis = curLen;
            }
        }

        /// <summary>
        /// 后处理螺线区域检测
        /// </summary>
        public static void DefeatSpiral(double cX, double cY, double rot, double maxT, List<(Vector2, double)> points, double stlen, bool stin, out Vector2 pos, out double dis)
        {
            pos = new Vector2(float.NaN, float.NaN); dis = 0.0;
            var inter = stin; var curLen = (1 - stlen) * 2.5;
            var preDis = SpiralLength(maxT); var preLen = curLen;
            var gotPos = false; var gotDis = false;
            var id = 0;
            while (id < points.Count)
            {
                if (gotDis && gotPos)
                    break;

                var curDis = SpiralLength(points[id].Item2);
                curLen = inter ? curLen + preDis - curDis : 0;

                if (!gotPos && curLen >= 2.5)
                {
                    var t = Brent(x => SpiralLength(x) - preDis + 2.5 - preLen, points[id].Item2, id > 0 ? points[id - 1].Item2 : maxT);
                    var r = Variable.VortexA * t;
                    TransToGlobal(cX, cY, rot, r * Math.Cos(t), r * Math.Sin(t), out var px, out var py);
                    pos = new Vector2((float)px, (float)py);
                    gotPos = true;
                }

                if (inter && !gotDis)
                {
                    dis = curLen;
                    gotDis = true;
                }

                preDis = curDis; preLen = curLen;
                inter = !inter;
                id++;
            }

            curLen = inter ? curLen + preDis : 0;
            if (!gotPos && curLen >= 2.5)
            {
                var t = Brent(x => SpiralLength(x) - preDis + 2.5 - preLen, 0, id > 0 ? points[id - 1].Item2 : maxT);
                var r = Variable.VortexA * t;
                TransToGlobal(cX, cY, rot, r * Math.Cos(t), r * Math.Sin(t), out var px, out var py);
                pos = new Vector2((float)px, (float)py);
            }
            if (!gotDis)
                dis = curLen;
        }
        #endregion

        #region 路径缩放处理
        /// <summary>
        /// 寻找缩放点
        /// </summary>
        public static void ScalePathPoint(Vector2 p0, Vector2 p1, int index, bool isTp, out List<(Vector2, int, double)> pos)
        {
            pos = [];
            if (Math.Abs(p0.x - p1.x) < 1e-5 && Math.Abs(p0.y - p1.y) < 1e-5)
                return;

            LineBVH(p0, p1, Variable.SwampOilBVH, out var items);

            List<(Vector2, int, double)> points = [];
            foreach (var item in items)
                switch (Variable.SwampOil[item])
                {
                    case Variable.Shape.Line line:
                        LineVsLine(p0, p1, line, out var Pline);
                        if (Pline.HasValue) points.Add((Pline.Value.Item1, index, Pline.Value.Item2));
                        break;
                    case Variable.Shape.Arc arc:
                        LineVsArc(p0, p1, arc, out var Parc);
                        points.AddRange(Parc.Select(x => (x.Item1, index, x.Item2)));
                        break;
                }
            points.Sort((x, y) => x.Item3.CompareTo(y.Item3));
            List<(Vector2, int, double)> pointsort = [];
            if (points.Count > 0)
            {
                pointsort.Add(points[0]);
                for (var i = 1; i < points.Count; i++)
                    if (points[i].Item3 - points[i - 1].Item3 > 1e-5)
                        pointsort.Add(points[i]);
            }

            if (isTp)
                pos = pointsort.Count % 2 != 0 ? [(p0, index, 0)] : [];
            else
                pos = pointsort;
        }

        /// <summary>
        /// 生成沼泽缩放路径和交点
        /// </summary>
        public static void ScalePath(List<Vector3> lineP, bool stIn, Vector3 stSet, int index, bool isTp, out List<Vector3> lineC, out List<(Vector2, int, double)> pos, out bool edIn, out Vector3 edSet)
        {
            List<Vector3> LineC = [];
            List<(Vector2, int, double)> Pos = [];
            var inter = stIn; var offset = stSet;
            var scale = stIn ? 2f / 3f : 1f;
            if (lineP.Count > 1)
            {
                LineC.Add(lineP[0] + offset);
                HandlePath(lineP);
                edSet = LineC[LineC.Count - 1] - lineP[lineP.Count - 1];
            }
            else
                edSet = stSet;
            lineC = LineC; pos = Pos; edIn = inter;

            void HandlePath(List<Vector3> lineP)
            {
                var s = lineP[0];
                var lenPath = lineP.Count - 1;

                for (var i = 0; i < lenPath; i += 100)
                {
                    var lt = Math.Min(lenPath, i + 100);
                    var minx = -double.MaxValue; var maxx = double.MaxValue;
                    var miny = -double.MaxValue; var maxy = double.MaxValue;

                    for (var j = i; j <= lt; j++)
                    {
                        var x = lineP[j].x;
                        var y = lineP[j].y;
                        minx = Math.Min(minx, x); maxx = Math.Max(maxx, x);
                        miny = Math.Min(miny, y); maxy = Math.Max(maxy, y);
                    }
                    minx = s.x + (minx - s.x) * scale + offset.x; maxx = s.x + (maxx - s.x) * scale + offset.x;
                    miny = s.y + (miny - s.y) * scale + offset.y; maxy = s.y + (maxy - s.y) * scale + offset.y;

                    if (!SquareBVH(minx, miny, maxx, maxy, Variable.SwampOilBVH))
                    {
                        for (var j = i + 1; j <= lt; j++)
                            LineC.Add(s + (lineP[j] - s) * scale + offset);
                        continue;
                    }

                    for (var j = i; j < lt; j++)
                    {
                        var p0 = lineP[j];
                        var p1 = lineP[j + 1];
                        var q0 = s + (p0 - s) * scale + offset;
                        var q1 = s + (p1 - s) * scale + offset;
                        if (isTp)
                        {
                            var tmp = q0 + p1 - p0;
                            offset = offset - q0 + tmp;
                            q1 = tmp;
                        }

                        ScalePathPoint(q0, q1, j + index, isTp, out var points);
                        var splitPos = Vector3.zero;
                        while (points.Count > 0)
                        {
                            var pointPre = p0 + (p1 - p0) * (float)points[0].Item3;
                            splitPos = isTp ? p1 : pointPre;
                            if (!(Math.Abs(splitPos.x - s.x) < 1e-5 && Math.Abs(splitPos.y - s.y) < 1e-5))
                                break;
                            points.RemoveAt(0);
                        }

                        if (points.Count > 0)
                        {
                            Vector3 pointCur = points[0].Item1;
                            var scalePos = isTp ? q1 : pointCur;

                            if (!(Math.Abs(splitPos.x - p0.x) < 1e-5 && Math.Abs(splitPos.y - p0.y) < 1e-5))
                                LineC.Add(scalePos);

                            List<Vector3> newLineP = [];
                            if (!(Math.Abs(splitPos.x - p1.x) < 1e-5 && Math.Abs(splitPos.y - p1.y) < 1e-5))
                                newLineP.Add(splitPos);
                            for (var k = j + 1; k < lineP.Count; k++)
                                newLineP.Add(lineP[k]);

                            offset = scalePos - splitPos;
                            inter = !inter;
                            scale = inter ? 2f / 3f : 1f;

                            Pos.AddRange(points);
                            index += j + 1;
                            HandlePath(newLineP);
                            return;
                        }
                        else
                            LineC.Add(q1);
                    }
                }
            }
        }

        /// <summary>
        /// 后处理折线沼泽检测
        /// </summary>
        public static void SwampLine(List<(Vector3, bool)> line, List<(Vector2, int, double)> points, bool stin, out double dis)
        {
            dis = 0.0;
            if (line.Count < 2)
                return;

            var cnt = line.Count - 1;
            double[] len = new double[cnt];
            double[] sum = new double[cnt];
            var preDis = 0.0; var gotDis = false;
            var id = 0; var inter = stin;

            sum[0] = len[0] = line[1].Item2 ? 0 : Vector2.Distance(line[0].Item1, line[1].Item1);
            for (var i = 1; i < cnt; i++)
            {
                if (line[i + 1].Item2)
                    len[i] = 0;
                else
                    len[i] = Vector2.Distance(line[i].Item1, line[i + 1].Item1);
                sum[i] = len[i] + sum[i - 1];
            }

            while (id < points.Count)
            {
                var curDis = points[id].Item2 > 0 ? sum[points[id].Item2 - 1] : 0;
                curDis += points[id].Item3 * len[points[id].Item2];
                if (inter)
                {
                    dis = (curDis - preDis) * 1.5;
                    gotDis = true;
                    break;
                }

                preDis = curDis;
                inter = !inter;
                id++;
            }

            if (inter && !gotDis)
                dis = (sum[cnt - 1] - preDis) * 1.5;
        }
        #endregion

        #region 辅助判断
        /// <summary>
        /// 判断AABB是否与范围相交
        /// </summary>
        public static bool RangeAABB(double minx, double miny, double maxx, double maxy, Vector2 pos, double rad)
        {
            var cx = Math.Max(minx, Math.Min(pos.x, maxx));
            var cy = Math.Max(miny, Math.Min(pos.y, maxy));
            var dx = pos.x - cx;
            var dy = pos.y - cy;
            return (dx * dx + dy * dy) <= (rad * rad);
        }

        /// <summary>
        /// 判断AABB是否与区域相交
        /// </summary>
        public static bool DangerAABB(double minx, double miny, double maxx, double maxy, string mapid)
        {
            if (mapid != "Wine")
                return SquareBVH(minx, miny, maxx, maxy, Variable.StrongBVHs[Variable.MapId[mapid]]);
            else
            {
                List<Variable.Node>[] bvhs = [Variable.StrongBVHs[2], Variable.WeakWineBVH, Variable.HealWineBVH];
                foreach (var bvh in bvhs)
                    if (SquareBVH(minx, miny, maxx, maxy, bvh))
                        return true;
                return false;
            }
        }
        #endregion

        #region 辅助计算
        /// <summary>
        /// 计算二维向量叉积
        /// </summary>
        public static double Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// 判断角度是否在范围内
        /// </summary>
        public static bool Between(double theta, double start, double end)
        {
            if (start <= end)
                return start <= theta && theta <= end;
            return theta >= start || theta <= end;
        }

        /// <summary>
        /// 计算螺线弧长
        /// </summary>
        public static double SpiralLength(double theta)
        {
            var sq = Math.Sqrt(theta * theta + 1);
            return Variable.VortexA * (theta * sq + Math.Log(theta + sq)) / 2;
        }

        /// <summary>
        /// 转换为螺线本地坐标
        /// </summary>
        public static void TransToLocal(double cX, double cY, double rot, double px, double py, out double tx, out double ty)
        {
            var cR = Math.Cos(-rot);
            var sR = Math.Sin(-rot);
            var dx = px - cX;
            var dy = py - cY;
            tx = dx * cR - dy * sR;
            ty = dx * sR + dy * cR;
        }

        /// <summary>
        /// 转换为整体坐标
        /// </summary>
        public static void TransToGlobal(double cX, double cY, double rot, double tx, double ty, out double px, out double py)
        {
            var cR = Math.Cos(rot);
            var sR = Math.Sin(rot);
            var dx = tx * cR - ty * sR;
            var dy = tx * sR + ty * cR;
            px = dx + cX;
            py = dy + cY;
        }

        /// <summary>
        /// Brent算法求根
        /// </summary>
        public static double Brent(Func<double, double> f, double a, double b)
        {
            var fa = f(a); var fb = f(b);
            if (Math.Abs(fa) < 1e-5) return a;
            if (Math.Abs(fb) < 1e-5) return b;
            if (Math.Abs(fa) < Math.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }

            var c = a; var fc = fa; var d = c;
            var mflag = true;
            for (int iter = 0; iter < 100; iter++)
            {
                if (Math.Abs(fb) < 1e-5 || Math.Abs(b - a) < 1e-5)
                    return b;

                double s;
                if (fa != fc && fb != fc && fa != fb)
                    s = a * fb * fc / ((fa - fb) * (fa - fc)) + b * fa * fc / ((fb - fa) * (fb - fc)) + c * fa * fb / ((fc - fa) * (fc - fb));
                else
                    s = b - fb * (b - a) / (fb - fa);

                var co1 = s < (3 * a + b) / 4 || s > b;
                var co2 = mflag && Math.Abs(s - b) >= Math.Abs(b - c) / 2;
                var co3 = !mflag && Math.Abs(s - b) >= Math.Abs(c - d) / 2;
                var co4 = mflag && Math.Abs(b - c) < 1e-5;
                var co5 = !mflag && Math.Abs(c - d) < 1e-5;
                if (co1 || co2 || co3 || co4 || co5)
                {
                    s = (a + b) / 2;
                    mflag = true;
                }
                else
                    mflag = false;

                var fs = f(s);
                d = c; c = b; fc = fb;
                if (Math.Sign(fa) == Math.Sign(fs))
                {
                    a = s; fa = fs;
                }
                else
                {
                    b = s; fb = fs;
                }

                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    (a, b) = (b, a);
                    (fa, fb) = (fb, fa);
                }
            }
            return b;
        }
        #endregion
    }
}
