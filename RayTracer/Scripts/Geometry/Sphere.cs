using UnityEngine;

namespace ZXC.Geometry
{
    public struct SurfaceData
    {
        /// <summary>
        /// 法线
        /// </summary>
        public Vector3 normal;
        /// <summary>
        /// uv
        /// </summary>
        public Vector2 uv;
    }
    
    /// <summary>
    /// 球体
    /// </summary>
    public class Sphere
    {
        /// <summary>
        /// 半径
        /// </summary>
        public float Radius { get; private set; }
        /// <summary>
        /// 半径平方
        /// </summary>
        public float RadiusPow2 { get; private set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public float Transparency { get; private set; }
        /// <summary>
        /// 反射率
        /// </summary>
        public float Reflection { get; private set; }
        /// <summary>
        /// 球心
        /// </summary>
        public Vector3 Center { get; private set; }
        /// <summary>
        /// 表面颜色
        /// </summary>
        public Color SurfaceColor { get; private set; }
        /// <summary>
        /// 自发光颜色
        /// </summary>
        public Color EmissionColor { get; private set; }

        /// <summary>
        /// 不带自发光的
        /// </summary>
        /// <param name="center">球心</param>
        /// <param name="radius">半径</param>
        /// <param name="sColor">表面颜色</param>
        /// <param name="reflection">反光率</param>
        /// <param name="transparency">透明度</param>
        public Sphere(Vector3 center, float radius, Color sColor, float reflection, float transparency)
        {
            Center = center;
            Radius = radius;
            RadiusPow2 = radius * radius;
            SurfaceColor = sColor;
            Reflection = reflection;
            Transparency = transparency;
        }
        
        /// <summary>
        /// 带自发光
        /// </summary>
        /// <param name="center">球心</param>
        /// <param name="radius">半径</param>
        /// <param name="sColor">表面颜色</param>
        /// <param name="reflection">反光率</param>
        /// <param name="transparency">透明度</param>
        /// <param name="eColor">自发光颜色</param>
        public Sphere(Vector3 center, float radius, Color sColor, float reflection, float transparency, Color eColor)
            :this(center, radius, sColor, reflection, transparency)
        {
            EmissionColor = eColor;
        }

        // /// <summary>
        // /// 几何求解:计算光线与球的交点
        // /// </summary>
        // /// <param name="ray">光线</param>
        // /// <param name="t0">到近交点的距离</param>
        // /// <param name="t1">到远交点的距离</param>
        // /// <returns></returns>
        // public bool Intersect(Ray ray, ref float t0, ref float t1)
        // {
        //     var l = Center - ray.origin;
        //     var tca = Vector3.Dot(l, ray.direction);
        //     if (tca < 0) return false;
        //     var l2 = Vector3.Dot(l, l);
        //     var d2 = l2 - tca * tca;
        //     if (d2 > RadiusPow2) return false;
        //     var thc = Mathf.Sqrt(RadiusPow2 - d2);
        //     t0 = tca - thc;
        //     t1 = tca + thc;
        //     return true;
        //     保证x0小于x1
            // if (x0 > x1)
            // {
            //     var tmp = x1;
            //     x1 = x0;
            //     x0 = tmp;
            // }
            //
            // var t = 0f; //最近的交点距离
            //     if (x0 < 0)
            // {
            //     x0 = x1;
            //     if (x1 < 0) return false;
            // }
            //
            // t = x0;
            // return true;
        // }

        /// <summary>
        /// 解析求解：计算光线与球的交点
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool Intersect(Ray ray, out float t)
        {
            //转换为求一元二次方程的根f(x) = 0 求根公式、韦达定理
            //f(x) = ax^2 + bx + c 求出a，b，c即可
            //由(O + tD - C)^2 = R^2
            //有 D^2 * t^2 + 2 * D * (O - C) * t + (O - C) ^ 2 - R ^ 2 = 0 = f(t)
            //则 a = D^2 = 1, b = 2 * D * (O - C), c = (O - C) ^ 2 - R ^ 2
            var dir = ray.direction;
            var origin = ray.origin;
            var p = origin - Center;
            var a = 1;
            var b = 2 * Vector3.Dot(dir, p);
            var c = Vector3.Dot(p, p) - RadiusPow2;
            float x0 = 0f, x1 = 0f;
            t = 0f; //最近的交点距离
            if (!SolveQuadratic(a, b, c, ref x0, ref x1)) return false;

            //保证x0小于x1
            if (x0 > x1)
            {
                var tmp = x1;
                x1 = x0;
                x0 = tmp;
            }
            
            if (x0 < 0)
            {
                x0 = x1;
                if (x1 < 0) return false;
            }

            t = x0;
            return true;
        }

        /// <summary>
        /// 获取表面数据
        /// </summary>
        /// <param name="hit">表面某一点在世界坐标中的位置</param>
        /// <returns>表面数据</returns>
        public SurfaceData GetSurfaceData(Vector3 hit)
        {
            var normal = (hit - Center).normalized;
            
            //球的uv坐标可以看作是球极坐标θ和φ，θ[0 - Π], φ[0, 2Π]
            //极坐标转化为笛卡尔坐标系，公式为
            //x = r * sinθ * sinφ 
            //y = r * cosθ
            //z = r * sinθ * cosφ
            //推导过程见我的博客
            //因此，若得知某点相对于圆心的笛卡尔坐标，可逆向算出极坐标
            //有 cosθ = y / r
            //因此 θ = acos(y / r)
            //有 x / z = sinφ / cosφ = tanφ
            //因此 φ = atan(x / z)
            //又因为uv取值范围是[0, 1]所以需要把θ和φ的值clamp到[0,1]
            //不能直接用交点的世界坐标计算，因为极坐标是相对于圆心的
            //这里使用了法线，可以认为是再单位圆上操作，所以r = 1
            var uv = new Vector2
            {
                x = (1 + Mathf.Atan2(normal.z, normal.x) / Mathf.PI) * 0.5f, 
                y = Mathf.Acos(normal.y) / Mathf.PI
            };
            return new SurfaceData
            {
                normal = normal,
                uv = uv
            };
        }
        
        /// <summary>
        /// 求一元二次方程的根
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <returns></returns>
        private bool SolveQuadratic(float a, float b, float c, ref float x0, ref float x1)
        {
            //求根公式 x = -b +/-Sqrt(b*b - 4*a*c) / 2 * a 
            //韦达定理 x0 + x1 = -b/a    x1 * x2 = c/a
            //先求出判别式delta
            var delta = b * b - 4 * a * c;
            if (delta < 0) return false;
            if (Mathf.Approximately(delta, 0))
            {
                x0 = x1 = -0.5f * b / a; //0.5f * c / a
            }
            else
            {
                //为了避免catastrophic cancellation 将求根公式为如下公式   
                var q = b > 0 ? -0.5f * (b + Mathf.Sqrt(delta)) : -0.5f * (b - Mathf.Sqrt(delta));
                x0 = q / a;
                x1 = c / q;
            }

            return true;
        }
    }
}
