using System;
#if UnityView
using UnityEngine;
#endif

// 导航计算专用向量
namespace NaviFunnel
{
    public struct NaviVector
    {
        public float x;
        public float y;
        public float z;
        public NaviVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NaviVector(float x, float z)
        {
            this.x = x;
            this.y = 0;
            this.z = z;
        }

#if UnityView
        public NaviVector(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
#endif

        public static NaviVector Zero
        {
            get
            {
                return new NaviVector(0, 0, 0);
            }
        }
        public static NaviVector One
        {
            get
            {
                return new NaviVector(1, 1, 1);
            }
        }

        public static NaviVector operator +(NaviVector a, NaviVector b)
        {
            return new NaviVector(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static NaviVector operator -(NaviVector a, NaviVector b)
        {
            return new NaviVector(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static NaviVector operator *(NaviVector a, float d)
        {
            return new NaviVector(a.x * d, a.y * d, a.z * d);
        }
        public static NaviVector operator /(NaviVector a, float d)
        {
            return new NaviVector(a.x / d, a.y / d, a.z / d);
        }
        public static bool operator ==(NaviVector a, NaviVector b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(NaviVector a, NaviVector b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        /// <summary>
        /// 计算两个向量之间的距离(平方)
        /// </summary>
        public static float SqareDistance(NaviVector v1, NaviVector v2)
        {
            return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
        }

        /// <summary>
        ///  二维向量的点乘(X和Z)
        ///  反映的是两个向量的方向是否一致
        ///  v1向量在v2向量的方向上的投影长度与v2向量的模的乘积
        /// </summary>
        public static float DotXZ(NaviVector v1, NaviVector v2)
        {
            return v1.x * v2.x + v1.z * v2.z;
        }

        /// <summary>
        /// 二维向量的叉乘(X和Z)
        /// 值为负数: v2在v1顺时针方向
        /// 值为正数: v2在v1逆时针方向
        /// 值为0: v2和v1共线
        /// 值的绝对值为两向量共起点构成的平行四边形的面积
        /// (x1,y1) x (x2,y2) = x1*y2 - x2*y1 => (x1,0,z1) x (x2,0,z2) = x1*z2 - x2*z1
        /// </summary>
        public static float CrossProductXZ(NaviVector v1, NaviVector v2)
        {
            return v1.x * v2.z - v1.z * v2.x;
        }

        /// <summary>
        /// 三维向量的叉乘(X,Y,Z)
        /// 得到的向量垂直于v1和v2构成的平面(法向量)
        /// </summary>
        public static NaviVector CrossProductXYZ(NaviVector v1, NaviVector v2)
        {
            return new NaviVector
            {
                x = v1.y * v2.z - v1.z * v2.y,
                y = v1.z * v2.x - v1.x * v2.z,
                z = v1.x * v2.y - v1.y * v2.x
            };
        }

        /// <summary>
        /// 归一化向量
        /// </summary>
        public static NaviVector NormalizedXZ(NaviVector v)
        {
            float length = Mathf.Sqrt(v.x * v.x + v.z * v.z);
            return new NaviVector
            {
                x = v.x / length,
                y = 0,
                z = v.z / length
            };
        }

        /// <summary>
        /// 计算两个向量之间的夹角(弧度)
        /// 传进来的v1和v2要为单位向量
        /// </summary>
        public static float GetAngleXZ(NaviVector v1, NaviVector v2)
        {
            float dot = DotXZ(v1, v2);
            float angle = MathF.Acos(dot);

            NaviVector cross = CrossProductXYZ(v1, v2);
            if (cross.y < 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public override bool Equals(object obj)
        {
            return obj is NaviVector vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
        public override string ToString()
        {
            return $"[{x}, {z}]";
        }
#if UnityView
        public Vector3 ConvertToUnityVector()
        {
            return new Vector3(x, y, z);
        }
#endif
    }
}
