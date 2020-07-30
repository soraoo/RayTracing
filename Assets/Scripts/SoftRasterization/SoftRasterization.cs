using System;
using System.IO;
using UnityEngine;

namespace ZXC
{
    /// <summary>
    /// 软光栅器
    /// </summary>
    public class SoftRasterization
    {
        public static void CreateTriangle()
        {
            //顶点坐标
            var v0 = new Vector2(491.407f, 411.407f);
            var v1 = new Vector2(148.593f, 68.5928f);
            var v2 = new Vector2(148.593f, 411.407f);
            
            //颜色
            var c0 = new Color(1, 0, 0);
            var c1 = new Color(0, 1, 0);
            var c2 = new Color(0, 0, 1);

            //宽高
            uint w = 512;
            uint h = 512;

            var buffer = new uint[w * h * 3];
            var area = Mathf.Abs(EdgeTest(v0, v1, v2));
            for (var i = 0; i < h; i ++)
            {
                for (var j = 0; j < w; j++)
                {
                    var p = new Vector2(i + 0.5f, j + 0.5f);
                    var w0 = EdgeTest(v0, p, v1);
                    var w1 = EdgeTest(v1, p, v2);
                    var w2 = EdgeTest(v2, p, v0);
                    var edge0 = v2 - v1;
                    var edge1 = v0 - v2;
                    var edge2 = v1 - v0;
                    var overlaps = true;
                    overlaps &= Mathf.Approximately(w0 , 0) ? Mathf.Approximately(edge0.y, 0) && edge0.x > 0 || edge0.y > 0 : w0 > 0;
                    overlaps &= Mathf.Approximately(w1 , 0) ? Mathf.Approximately(edge1.y, 0) && edge1.x > 0 || edge1.y > 0 : w1 > 0;
                    overlaps &= Mathf.Approximately(w2 , 0) ? Mathf.Approximately(edge2.y, 0) && edge2.x > 0 || edge2.y > 0 : w2 > 0;
                    if (!overlaps) continue;
                    w0 /= area;
                    w1 /= area;
                    w2 /= area;
                    var r = w0 * c0.r + w1 * c1.r + w2 * c2.r;
                    var g = w0 * c0.g + w1 * c1.g + w2 * c2.g;
                    var b = w0 * c0.b + w1 * c1.b + w2 * c2.b;

                    buffer[3 * (j + w * i) + 0] = (uint)(r * 255);
                    buffer[3 * (j + w * i) + 1] = (uint)(g * 255);
                    buffer[3 * (j + w * i) + 2] = (uint)(b * 255);
                }
            }

            using (var sr = new StreamWriter("triangle.ppm"))
            {
                sr.WriteLine("P3");
                sr.WriteLine($"{w} {h}");
                sr.WriteLine("255");
                for (var i = 0; i < h; i ++)
                {
                    for (var j = 0; j < w; j++)
                    {
                        sr.Write(buffer[3 * (j + w * i) + 0]);
                        sr.Write(" ");
                        sr.Write(buffer[3 * (j + w * i) + 1]);
                        sr.Write(" ");
                        sr.Write(buffer[3 * (j + w * i) + 2]);
                        sr.Write(" ");
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private static float EdgeTest(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return (v1.x - v0.x) * (v2.y - v0.y) - (v1.y - v0.y) * (v2.x - v0.x);
        }
    }
}