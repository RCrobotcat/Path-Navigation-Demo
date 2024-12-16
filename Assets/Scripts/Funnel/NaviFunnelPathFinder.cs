using NaviPath;
using System.Collections.Generic;

// 导航路径查找器
namespace NaviFunnel
{
    public partial class NaviMap
    {
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
    }
}
