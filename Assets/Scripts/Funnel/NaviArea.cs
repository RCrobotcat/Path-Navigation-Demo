// 导航区域
using System.Collections.Generic;
using UnityEditor;

namespace NaviFunnel
{
    public class NaviArea
    {
        public NaviVector[] vertexArr;
        public int[] indexArr;
        public int areaID;

        public NaviVector min = new NaviVector(float.MaxValue, float.MaxValue, float.MaxValue);
        public NaviVector max = new NaviVector(float.MinValue, float.MinValue, float.MinValue);
        public NaviVector center = NaviVector.Zero;
        public List<NaviBorder> borderList;

        public NaviVector start = NaviVector.Zero;
        public float priority; // 当前区域的寻路优先级
        public float sumDistance = float.PositiveInfinity; // 走到当前区域时的累计距离, 默认为无穷大
        public NaviArea preArea = null; // 前一个区域

        public NaviArea(int areaID, NaviVector[] vertexArr, int[] indexArr)
        {
            this.areaID = areaID;
            this.vertexArr = vertexArr;
            this.indexArr = indexArr;

            for (int i = 0; i < indexArr.Length; i++)
            {
                NaviVector v = vertexArr[indexArr[i]];

                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;

                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;

                center += v;
            }

            center = new NaviVector(center.x / indexArr.Length, center.y / indexArr.Length, center.z / indexArr.Length);
        }

        /// <summary>
        /// 计算区域之间的距离
        /// </summary>
        public float CalculateNaviAreaDistance(NaviArea neighborArea)
        {
            int[] indexArr = neighborArea.indexArr;
            float sqareDistance = float.MaxValue;
            for (int i = 0; i < indexArr.Length; i++)
            {
                NaviVector v = vertexArr[indexArr[i]];
                float newDistance = NaviVector.SqareDistance(center, v);
                if (newDistance < sqareDistance)
                {
                    sqareDistance = newDistance;
                    neighborArea.start = v;
                }
            }
            return sqareDistance;
        }

        /// <summary>
        /// 基于中心点计算区域之间的距离
        /// </summary>
        public float CalculateNaviAreaDistanceByCenter(NaviArea neighborArea)
        {
            return NaviVector.SqareDistance(center, neighborArea.center);
        }
    }
}
