using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchAssV4;

public static class Geometry
{
    #region 计算几何

    /// <summary>
    /// 计算线段与圆的交点参数
    /// </summary>
    private static void LineVsCircleRoots(Variable.Vec2D p0, Variable.Vec2D p1, Variable.Vec2D cen, double rad,
        out Variable.Vec2D d, out List<double> ti)
    {
        ti = [];
        d = p1 - p0;
        if (rad <= 0) return;
        var a = d.SqrMagnitude;
        if (a <= 0.0) return;
        var f = p0 - cen;
        var b = 2.0 * f.Dot(d);
        var c = f.SqrMagnitude - rad * rad;
        var delta = b * b - 4.0 * a * c;
        if (delta < 0) return;
        var sqrtD = Math.Sqrt(delta);
        var t0 = (-b - sqrtD) / (2.0 * a);
        var t1 = (-b + sqrtD) / (2.0 * a);
        if (t0 is >= 0 and < 1) ti.Add(t0);
        if (t1 is >= 0 and < 1 && t0 != t1) ti.Add(t1);
    }

    /// <summary>
    /// 计算线段与圆的交点
    /// </summary>
    public static void LineVsCircle(Variable.Vec2D p0, Variable.Vec2D p1, Variable.Vec2D cen, double rad,
        out List<Variable.Vec2D> pos, out List<double> ti)
    {
        LineVsCircleRoots(p0, p1, cen, rad, out var d, out ti);
        pos = [.. ti.Select(t => p0 + d * t)];
    }

    /// <summary>
    /// 计算线段与圆最近的交点
    /// </summary>
    public static void LineVsCircle(Variable.Vec2D p0, Variable.Vec2D p1, Variable.Vec2D cen, double rad,
        out double? ti)
    {
        LineVsCircleRoots(p0, p1, cen, rad, out _, out var ts);
        ti = ts.Count > 0 ? ts[0] : null;
    }

    #endregion

    #region 漩涡可达角

    /// <summary>
    /// 获取漩涡可达角度
    /// </summary>
    public static List<(double, double)> GetVortexReachableAngles(Variable.Vec2D pos, double r)
    {
        List<(double, double)> intervals = [];
        var d = pos - Variable.Offset;
        var alpha = Math.Atan2(d.Y, d.X);
        var kappa = Math.Acos(Algebra.Clamp(r / d.Magnitude, -1, 1));
        AddCircularArc(intervals, alpha - kappa, alpha + kappa);
        var s = Variable.Offset.Magnitude;
        var noPositive = s < r && s <= Variable.Spiral.A * (r - s) / (r + s);
        if (!noPositive)
        {
            var psi = Math.Atan2(Variable.Offset.Y, Variable.Offset.X);
            List<double> roots = [];
            foreach (var c in SolveCosineCubic(s, r, Variable.Spiral.A))
            {
                if (c is < -1.0 - 1e-10 or > 1.0 + 1e-10) continue;
                var eta = Math.Acos(Algebra.Clamp(c, -1.0, 1.0));
                if (VerifyRoot(eta, s, r, Variable.Spiral.A))
                    Algebra.AddUniqueAngleRoot(roots, psi - eta);
                if (eta > 1e-10 && VerifyRoot(-eta, s, r, Variable.Spiral.A))
                    Algebra.AddUniqueAngleRoot(roots, psi + eta);
            }

            if (Math.Abs(s - r) < 1e-10)
                Algebra.AddUniqueAngleRoot(roots, psi + Math.PI);
            roots.Sort();
            if (roots.Count > 1)
                for (var i = 0; i < roots.Count; i++)
                {
                    var start = roots[i];
                    var end = i + 1 < roots.Count ? roots[i + 1] : roots[0] + 2.0 * Math.PI;
                    if (end - start < 1e-10) continue;
                    var mid = Algebra.DeltaAngleRad(0, (start + end) * 0.5);
                    if (!GetSign(mid, r)) continue;
                    AddCircularArc(intervals, start, end);
                }
        }

        intervals.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        List<(double, double)> merged = [];
        foreach (var (l, u) in intervals)
        {
            if (merged.Count > 0 && l < merged.Last().Item2 + 1e-10)
            {
                var pre = merged.Last();
                if (u > pre.Item2)
                    merged[merged.Count - 1] = (pre.Item1, u);
            }
            else
                merged.Add((l, u));
        }

        return merged;
    }

    /// <summary>
    /// 计算漩涡可达角度区间的符号函数
    /// </summary>
    private static bool GetSign(double gamma, double r)
    {
        var e = new Variable.Vec2D(Math.Cos(gamma), Math.Sin(gamma));
        var q = Variable.Offset + e * r;
        var rho = q.Magnitude;
        if (rho < 1e-10) return false;
        return e.Cross(Variable.Offset) > Variable.Spiral.A * (r + e.Dot(Variable.Offset)) / rho;
    }

    /// <summary>
    /// 向角度区间加入一段圆弧
    /// </summary>
    private static void AddCircularArc(List<(double, double)> intervals, double startAngle, double endAngle)
    {
        var width = endAngle - startAngle;
        if (width < 1e-10) return;
        if (width > Math.PI * 2 - 1e-10)
        {
            intervals.Add((-Math.PI, Math.PI));
            return;
        }

        var start = Algebra.DeltaAngleRad(0, startAngle);
        var end = start + width;
        if (end <= Math.PI)
            intervals.Add((start, end));
        else
        {
            if (start < Math.PI - 1e-10)
                intervals.Add((start, Math.PI));
            var warppedEnd = end - Math.PI * 2;
            if (warppedEnd > -Math.PI - 1e-10)
                intervals.Add((-Math.PI, warppedEnd));
        }
    }

    /// <summary>
    /// 求解螺线三次方程
    /// </summary>
    private static List<double> SolveCosineCubic(double s, double R, double a)
    {
        var A = -2.0 * R * s * s * s;
        var B = -(R * R * s * s + a * a * s * s + s * s * s * s);
        var C = 2.0 * R * s * (s * s - a * a);
        var D = -R * R * a * a + R * R * s * s + s * s * s * s;
        return Algebra.SolveCubic(A, B, C, D);
    }

    /// <summary>
    /// 验证螺线方程的根
    /// </summary>
    private static bool VerifyRoot(double eta, double s, double rad, double a)
    {
        var cosEta = Math.Cos(eta);
        var sinEta = Math.Sin(eta);
        var cosHalf = Math.Cos(eta * 0.5);
        var rhoSqr = (rad - s) * (rad - s) + 4.0 * rad * s * cosHalf * cosHalf;
        if (rhoSqr < 1e-20) return false;
        var rho = Math.Sqrt(rhoSqr);
        var lhs = s * sinEta;
        var rhs = a * (rad + s * cosEta) / rho;
        return Math.Abs(lhs - rhs) < 1e-10;
    }

    #endregion
}