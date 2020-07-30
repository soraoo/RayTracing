using System;
using System.IO;
using UnityEngine;
using ZXC.Geometry;

namespace ZXC.RayTracing
{
    /// <summary>
    /// 光线追踪设置
    /// </summary>
    public struct Options
    {
        /// <summary>
        /// 图片像素宽
        /// </summary>
        public int width;
        /// <summary>
        /// 图片像素高
        /// </summary>
        public int height;
        /// <summary>
        /// 最大递归深度
        /// </summary>
        public float maxDepth;
    }
    
    /// <summary>
    /// 光线追踪器
    /// </summary>
    public class RayTracer
    {
        public Options options;
        
        public Camera camera;

        public void Render(Sphere[] spheres)
        {
            var width = options.width;
            var height = options.height;
            var aspect = width / height;
            var fov = camera.fieldOfView;
            var nearClipPlane = camera.nearClipPlane;
            var scale = Mathf.Tan(fov * 0.5f) * nearClipPlane;
            var z = -nearClipPlane;
            var rayOrin = camera.transform.position;
            
            //输出用的数组
            var frame = new Color[width * height];
            
            //获取相机坐标到世界坐标的变换矩阵
            var c2w = camera.cameraToWorldMatrix;
            
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    //将光栅坐标变换到世界坐标
                    //1.先变换到相机坐标系
                    var cx = (2 * (x + 0.5f) / width - 1) * aspect * scale;
                    var cy = (1 - 2 * (y + 0.5f) / height) * scale;
                    //2.构造向量
                    var cp = new Vector3(cx, cy, z);
                    //3.乘矩阵变换到世界坐标系
                    var p = c2w.MultiplyPoint3x4(cp);
                    var dir = (p - rayOrin).normalized;
                    var ray = new Ray(rayOrin, dir);
                    frame[x + y * width] = CastRay(ray, spheres);
                }
            }
            
            WriteToDisk(width, height, frame);
        }

        private bool Trace(Ray ray, Sphere[] spheres, out float near, out Sphere sphere)
        {
            near = Mathf.Infinity;
            sphere = null;
            for (var i = 0; i < spheres.Length; i++)
            {
                if (spheres[i].Intersect(ray, out var t) && t < near)
                {
                    near = t;
                    sphere = spheres[i];
                }
            }

            return sphere != null;
        }

        private Color CastRay(Ray ray, Sphere[] spheres)
        {
            var color = camera.backgroundColor;
            if (Trace(ray, spheres, out var t, out var sphere))
            {
                var hit = ray.origin + t * ray.direction;
                var surfaceData = sphere.GetSurfaceData(hit);
                var normal = surfaceData.normal;
                var uv = surfaceData.uv;
                //拉伸uv，repeat，这样便可以形成棋盘格
                var scale = 4f; 
                var pattern = (uv.x * scale % 1f > 0.5f) ^ (uv.y * scale % 1f > 0.5f);
                var facingRatio = Mathf.Max(0f, Vector3.Dot(normal, -ray.direction));
                color = facingRatio * Mix(sphere.SurfaceColor, sphere.SurfaceColor * 0.8f, pattern ? 1f : 0);
            }
            return color;
        }
        
        private Color Mix(Color a, Color b, float min)
        {
            return a * (1 - min) + b * min;
        }

        private void WriteToDisk(int w, int h, Color[] frame)
        {
            using (var sw = new StreamWriter("tracer.ppm"))
            {
                sw.WriteLine("P3");
                sw.WriteLine($"{w} {h}");
                sw.WriteLine("255");
                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++)
                    {
                        var color = frame[x + y * w];
                        sw.Write(Math.Floor(color.r * 255));
                        sw.Write(" ");
                        sw.Write(Mathf.Floor(color.g * 255));
                        sw.Write(" ");
                        sw.Write(Mathf.Floor(color.b * 255));
                        sw.Write(" ");
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}