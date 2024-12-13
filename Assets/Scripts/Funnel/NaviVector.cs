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
