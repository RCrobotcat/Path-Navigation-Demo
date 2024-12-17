using NaviPath;
using System.Collections.Generic;
#if UnityView
using UnityEngine;
#endif

// 导航路径查找器
namespace NaviFunnel
{
    public partial class NaviMap
    {
        #region A* Navigation
        NaviArea m_startArea;
        NaviArea m_endArea;

        readonly PriorityQueue<NaviArea> m_detectQueue = new PriorityQueue<NaviArea>(); // 待探测的区域优先队列(小顶堆)
        readonly List<NaviArea> m_finishedList = new List<NaviArea>(); // 已经探测过得区域列表
        List<NaviArea> m_PathList = new List<NaviArea>(); // 最终的寻路路径

        /// <summary>
        /// 通过A*算法计算多边形路径
        /// </summary>
        List<NaviArea> CalculatePolyPathByAStar(NaviArea startArea, NaviArea endArea)
        {
            m_startArea = startArea;
            m_endArea = endArea;

            m_detectQueue.Clear();
            m_finishedList.Clear();
            m_PathList.Clear();

            m_detectQueue.Enqueue(startArea);
            m_startArea.sumDistance = 0;

            while (m_detectQueue.Count > 0)
            {
                if (m_detectQueue.Contains(endArea))
                {
                    m_PathList = GetPathViaEndArea(endArea);
                    ShowPathAreaView?.Invoke(m_PathList); // 显示路径
                    m_finishedList.Add(endArea);
                    break;
                }

                NaviArea detectArea = m_detectQueue.Dequeue();
                if (!m_finishedList.Contains(detectArea))
                {
                    m_finishedList.Add(detectArea);
                }

                // 遍历当前区域的所有邻居区域
                for (int i = 0; i < detectArea.borderList.Count; i++)
                {
                    NaviBorder border = detectArea.borderList[i];
                    NaviArea neighborArea = areaArr[border.areaID_1 == detectArea.areaID ? border.areaID_2 : border.areaID_1];

                    DetectNeighborArea(detectArea, neighborArea);
                }
            }

            return m_PathList;
        }

        void DetectNeighborArea(NaviArea detectArea, NaviArea neighborArea)
        {
            if (!m_finishedList.Contains(neighborArea))
            {
                float neighborAreaDistance = detectArea.CalculateNaviAreaDistance(neighborArea);
                float newSumDistance = detectArea.sumDistance + neighborAreaDistance;
                // 如果邻居区域没有被探测过或者新的累计距离小于邻居区域的累计距离
                if (float.IsPositiveInfinity(neighborArea.sumDistance) || newSumDistance < neighborArea.sumDistance)
                {
                    neighborArea.preArea = detectArea;
                    neighborArea.sumDistance = newSumDistance;
                }

                if (!m_detectQueue.Contains(neighborArea))
                {
                    // f(n) = g(n) + h(n)
                    float targetDistance = neighborArea.CalculateNaviAreaDistance(m_endArea);
                    neighborArea.priority = neighborArea.sumDistance + targetDistance; // 优先级 = 累计距离 + 目标距离(A*算法估价函数)
                    m_detectQueue.Enqueue(neighborArea);
                }
            }
        }

        List<NaviArea> GetPathViaEndArea(NaviArea endArea)
        {
            List<NaviArea> pathList = new List<NaviArea>();
            NaviArea current = m_endArea;
            while (current.preArea != null)
            {
                pathList.Insert(0, current); // 插入到第一个位置
                current = current.preArea;
            }

            int startID, endID; // 起始区域ID和结束区域ID
            for (int i = 0; i < pathList.Count - 1; i++)
            {
                startID = pathList[i].areaID;
                endID = pathList[i + 1].areaID;
                string key;
                if (startID < endID)
                {
                    key = $"{startID}_{endID}";
                }
                else
                {
                    key = $"{endID}_{startID}";
                }

                pathList[i].targetBorder = GetNaviBorderByAreaIDKey(key);
            }

            return pathList;
        }

        void ResetAStarData()
        {
            for (int i = 0; i < m_finishedList.Count; i++)
            {
                m_finishedList[i].ResetAreaInfo();
            }

            List<NaviArea> list = m_detectQueue.ToList();
            for (int j = 0; j < list.Count; j++)
            {
                list[j].ResetAreaInfo();
            }
        }
        #endregion

        #region Funnel Navigation
        List<NaviVector> positionList = null; // 最终的路径点列表
        NaviVector funnelPos = NaviVector.Zero; // 漏斗顶点

        /// <summary>
        /// 计算漏斗路径(路径点列表)
        /// 基于多边形漏斗算法
        /// </summary>
        List<NaviVector> CalculateFunnelConnerPath(List<NaviArea> pathAreaList, NaviVector startPos, NaviVector endPos)
        {
            positionList = new List<NaviVector> { startPos }; // 第一个点是起始点
            funnelPos = startPos;

            // 初始化Funnel
            int initIndex = CalculateInitAreaID(pathAreaList);

            return positionList;
        }

        int CalculateInitAreaID(List<NaviArea> pathAreaList)
        {
            int initAreaID = -1;
            if (pathAreaList.Count == 0)
                return initAreaID;

            for (int i = 0; i < pathAreaList.Count; i++)
            {
                if (isFunnelInitAreaGood(pathAreaList[i]) && initAreaID == -1)
                {
                    initAreaID = i;
                }
            }

            return initAreaID;
        }

        /// <summary>
        /// 判断漏斗初始化区域是否合适
        /// </summary>
        bool isFunnelInitAreaGood(NaviArea initArea)
        {
            if (initArea.targetBorder == null)
                return false;

            int index_1 = initArea.targetBorder.vertexIndex_1;
            int index_2 = initArea.targetBorder.vertexIndex_2;
            NaviVector v1 = vertexArr[index_1] - funnelPos;
            NaviVector v2 = vertexArr[index_2] - funnelPos;

            // 显示漏斗射线
#if UnityView
            NaviView.ShowDebugLine(funnelPos, vertexArr[index_1], Color.green, 5);
            NaviView.ShowDebugLine(funnelPos, vertexArr[index_2], Color.green, 5);
#endif

            float crossXZ = NaviVector.CrossProductXZ(v1, v2);
            if (crossXZ < 0)
            {
                // TODO
                return true;
            }
            else if (crossXZ > 0)
            {
                // TODO
                return true;
            }
            else
            {
                this.Warn($"Funnel Init Area is not good! " +
                    $"Because the funnel vectors {v1} and {v2} " +
                    $"(funnel pos: {funnelPos}, for area: {initArea.areaID}) are collinear!");
                return false;
            }
        }

        void ResetFunnelData()
        {
            // TODO
        }
        #endregion
    }
}
