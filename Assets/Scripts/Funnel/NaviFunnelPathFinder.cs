using NaviPath;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        /// <summary>
        /// 漏斗边移动类型枚举
        /// </summary>
        enum FunnelShirkEnum
        {
            None,
            LeftToLeft,
            LeftToCenter,
            LeftToRight,
            RightToRight,
            RightToCenter,
            RightToLeft
        }

        /// <summary>
        /// 当前漏斗所在的左索引号
        /// </summary>
        int curLeftIndex = -1;
        /// <summary>
        /// 当前漏斗所在的右索引号
        /// </summary>
        int curRightIndex = -1;

        /// <summary>
        /// 漏斗左极限所在索引
        /// </summary>
        int leftLimitIndex = -1;
        /// <summary>
        /// 漏斗右极限所在索引
        /// </summary>
        int rightLimitIndex = -1;

        /// <summary>
        /// 漏斗左极限向量
        /// </summary>
        NaviVector leftLimitDir = NaviVector.Zero;
        /// <summary>
        /// 漏斗右极限向量
        /// </summary>
        NaviVector rightLimitDir = NaviVector.Zero;

        /// <summary>
        /// 左即将要检测的位置索引
        /// </summary>
        int leftCheckIndex = -1;
        /// <summary>
        /// 右即将要检测的位置索引
        /// </summary>
        int rightCheckIndex = -1;
        /// <summary>
        /// 左即将要检测的向量
        /// </summary>
        NaviVector leftCheckDir = NaviVector.Zero;
        /// <summary>
        /// 右即将要检测的向量
        /// </summary>
        NaviVector rightCheckDir = NaviVector.Zero;

        List<NaviVector> positionList = null; // 最终的路径点列表(拐点列表)
        NaviVector funnelPos = NaviVector.Zero; // 漏斗顶点

        readonly List<int> leftConnerList = new List<int>(); // 左拐点缓存列表: LeftToLeft才会添加
        readonly List<int> rightConnerList = new List<int>(); // 右拐点缓存列表: RightToRight才会添加

        /// <summary>
        /// 计算漏斗路径(路径点列表)
        /// 基于多边形漏斗算法
        /// </summary>
        List<NaviVector> CalculateFunnelConnerPath(List<NaviArea> pathAreaList, NaviVector startPos, NaviVector endPos)
        {
            positionList = new List<NaviVector> { startPos }; // 第一个点是起始点
            funnelPos = startPos;
            leftConnerList.Clear();
            rightConnerList.Clear();

            // 初始化Funnel
            int initIndex = CalculateInitAreaID(pathAreaList); // 第一个有效漏斗
            if (initIndex == -1)
            {
                positionList.Add(endPos);
                return positionList;
            }
            this.LogCyan($"FirstAreaID: {pathAreaList[initIndex].areaID}");

            FunnelShirkEnum leftFSE, rightFSE;
            for (int i = initIndex + 1; i < pathAreaList.Count; i++)
            {
                NaviArea area = pathAreaList[i];
                if (i == pathAreaList.Count - 1)
                {
                    // TODO
                }
                else
                {
                    // 计算要进行检测的索引号和漏斗检测向量(检测漏斗)
                    CalculateCheckingFunnel(area);
                    leftFSE = CalculateLeftFunnelChange();
                    rightFSE = CalculateRightFunnelChange();
                    if (leftFSE == FunnelShirkEnum.LeftToLeft)
                    {
                        if (!leftConnerList.Contains(leftCheckIndex))
                        {
                            leftConnerList.Add(leftCheckIndex); // 添加到缓存列表
                        }
                    }
                    if (rightFSE == FunnelShirkEnum.RightToRight)
                    {
                        if (!rightConnerList.Contains(rightCheckIndex))
                        {
                            rightConnerList.Add(rightCheckIndex); // 添加到缓存列表
                        }
                    }

                    #region Left
                    switch (leftFSE)
                    {
                        case FunnelShirkEnum.None:
                            leftLimitIndex = leftCheckIndex;
                            break;
                        case FunnelShirkEnum.LeftToCenter:
                            leftLimitIndex = leftCheckIndex;
                            leftLimitDir = leftCheckDir;
                            leftConnerList.Clear();
                            break;
                        case FunnelShirkEnum.LeftToRight:
                            // 计算极限变更
                            CalculateLeftToRightLimitIndex();
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Right
                    switch (rightFSE)
                    {
                        case FunnelShirkEnum.None:
                            rightLimitIndex = rightCheckIndex;
                            break;
                        case FunnelShirkEnum.RightToCenter:
                            rightLimitIndex = rightCheckIndex;
                            rightLimitDir = rightCheckDir;
                            rightConnerList.Clear();
                            break;
                        case FunnelShirkEnum.RightToLeft:
                            // 计算极限变更
                            CalculateRightToLeftLimitIndex();
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
            }

            return positionList;
        }

        /// <summary>
        /// 计算要进行检测的索引号和漏斗检测向量(检测漏斗)
        /// </summary>
        void CalculateCheckingFunnel(NaviArea area)
        {
            int checkIndex_1 = area.targetBorder.vertexIndex_1;
            int checkIndex_2 = area.targetBorder.vertexIndex_2;
            NaviVector checkV1 = vertexArr[checkIndex_1] - funnelPos;
            NaviVector checkV2 = vertexArr[checkIndex_2] - funnelPos;

            // Debug Show Funnel Check Line
            /*#if UnityView
                        NaviView.ShowDebugLine(funnelPos, vertexArr[checkIndex_1], Color.cyan, 5);
                        NaviView.ShowDebugLine(funnelPos, vertexArr[checkIndex_2], Color.cyan, 5);
            #endif*/

            int offset = 0;
            int count = area.indexArr.Length;
            for (int i = 0; i < count; i++)
            {
                if (curLeftIndex == area.indexArr[i])
                {
                    offset = i;
                    break;
                }
            }
            for (int i = 0; i < count; i++)
            {
                int curIndex = area.indexArr[(i + offset) % count];
                if (curIndex == checkIndex_1)
                {
                    leftCheckIndex = checkIndex_1;
                    leftCheckDir = checkV1;
                    rightCheckIndex = checkIndex_2;
                    rightCheckDir = checkV2;
                    break;
                }
                else if (curIndex == checkIndex_2)
                {
                    leftCheckIndex = checkIndex_2;
                    leftCheckDir = checkV2;
                    rightCheckIndex = checkIndex_1;
                    rightCheckDir = checkV1;
                    break;
                }
                else
                {
                    this.Log($"loop index: {i + offset}");
                }
            }

            if (leftLimitDir == NaviVector.Zero)
                leftLimitDir = leftCheckDir;
            if (rightLimitDir == NaviVector.Zero)
                rightLimitDir = rightCheckDir;
        }

        int CalculateInitAreaID(List<NaviArea> pathAreaList)
        {
            int initAreaID = -1;
            if (pathAreaList.Count == 0)
                return initAreaID;

            for (int i = 0; i < pathAreaList.Count; i++)
            {
                if (isFunnelInitAreaGood(pathAreaList[i]))
                {
                    initAreaID = i;
                    break;
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

            float crossXZ = NaviVector.CrossProductXZ(v1, v2);
            if (crossXZ < 0)
            {
                curLeftIndex = index_1;
                curRightIndex = index_2;
                leftLimitIndex = index_1;
                rightLimitIndex = index_2;
                leftLimitDir = v1;
                rightLimitDir = v2;
                return true;
            }
            else if (crossXZ > 0)
            {
                curLeftIndex = index_2;
                curRightIndex = index_1;
                leftLimitIndex = index_2;
                rightLimitIndex = index_1;
                leftLimitDir = v2;
                rightLimitDir = v1;
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

        /// <summary>
        /// 计算漏斗左边的变化
        /// </summary>
        FunnelShirkEnum CalculateLeftFunnelChange()
        {
            FunnelShirkEnum leftFSE;
            float crossXZ_left = NaviVector.CrossProductXZ(leftLimitDir, leftCheckDir);
            if (crossXZ_left > 0) // leftCheckDir在leftLimitDir的逆时针方向
            {
                leftFSE = FunnelShirkEnum.LeftToLeft;
            }
            else if (crossXZ_left == 0) // 共线
            {
                leftFSE = FunnelShirkEnum.None;
            }
            else // leftCheckDir在leftLimitDir的顺时针方向
            {
                float crossXZ_right = NaviVector.CrossProductXZ(rightLimitDir, leftCheckDir);
                if (crossXZ_right > 0) // leftCheckDir在rightLimitDir的逆时针方向
                {
                    leftFSE = FunnelShirkEnum.LeftToCenter;
                }
                else // leftCheckDir在rightLimitDir的顺时针方向
                {
                    leftFSE = FunnelShirkEnum.LeftToRight;
                }
            }

            return leftFSE;
        }
        /// <summary>
        /// 计算漏斗右边的变化
        /// </summary>
        FunnelShirkEnum CalculateRightFunnelChange()
        {
            FunnelShirkEnum rightFSE;
            float crossXZ_left = NaviVector.CrossProductXZ(rightLimitDir, rightCheckDir);
            if (crossXZ_left < 0) // rightCheckDir在rightLimitDir的顺时针方向
            {
                rightFSE = FunnelShirkEnum.RightToRight;
            }
            else if (crossXZ_left == 0) // 共线
            {
                rightFSE = FunnelShirkEnum.None;
            }
            else // rightCheckDir在rightLimitDir的逆时针方向
            {
                float crossXZ_right = NaviVector.CrossProductXZ(leftLimitDir, rightCheckDir);
                if (crossXZ_right < 0) // rightCheckDir在leftLimitDir的顺时针方向
                {
                    rightFSE = FunnelShirkEnum.RightToCenter;
                }
                else // rightCheckDir在leftLimitDir的逆时针方向
                {
                    rightFSE = FunnelShirkEnum.RightToLeft;
                }
            }

            return rightFSE;
        }

        /// <summary>
        /// 左向向右移动
        /// </summary>
        void CalculateLeftToRightLimitIndex()
        {
            funnelPos = vertexArr[rightLimitIndex]; // 漏斗位置更新
            positionList.Add(funnelPos);

            bool updateLimit = false; // 防止特殊情况
            int connerIndex = 0;
            NaviVector rightLimitDirNormalized = NaviVector.NormalizedXZ(rightLimitDir);
            while (connerIndex < rightConnerList.Count)
            {
                float rad = float.MaxValue;
                for (int i = connerIndex; i < rightConnerList.Count; i++)
                {
                    NaviVector v = NaviVector.NormalizedXZ(vertexArr[rightConnerList[i]] - funnelPos);
                    float currentRad = MathF.Abs(NaviVector.GetAngleXZ(rightLimitDirNormalized, v));
                    if (currentRad <= rad)
                    {
                        connerIndex = i;
                        rad = currentRad;
                    }
                }

                updateLimit = true; // 防止特殊情况

                // 漏斗位置更新, 极限向量更新
                rightLimitIndex = rightConnerList[connerIndex];
                rightLimitDir = vertexArr[rightLimitIndex] - funnelPos;
                leftLimitIndex = leftCheckIndex;
                leftLimitDir = vertexArr[leftLimitIndex] - funnelPos;

                float cross = NaviVector.CrossProductXZ(leftLimitDir, rightLimitDir);
                if (cross > 0) // rightLimitDir在leftLimitDir的逆时针方向(漏斗不合法)
                {
                    funnelPos = vertexArr[rightLimitIndex];
                    positionList.Add(funnelPos);
                    connerIndex++;
                    if (connerIndex >= rightConnerList.Count)
                    {
                        rightLimitIndex = -1;
                        rightLimitDir = NaviVector.Zero;

                        leftLimitDir = vertexArr[leftLimitIndex] - funnelPos;
                    }
                }
                else // 漏斗合法
                {
                    for (int i = 0; i < connerIndex; i++)
                    {
                        rightConnerList.RemoveAt(0);
                    }
                    break;
                }
            }

            if (!updateLimit)
            {
                // 重置右极限和左极限
                rightLimitIndex = -1;
                rightLimitDir = NaviVector.Zero;
                leftLimitIndex = leftCheckIndex;
                leftLimitDir = vertexArr[leftLimitIndex] - funnelPos;
            }
        }

        /// <summary>
        /// 右向向左移动
        /// </summary>
        void CalculateRightToLeftLimitIndex()
        {
            funnelPos = vertexArr[leftLimitIndex]; // 漏斗位置更新
            positionList.Add(funnelPos);

            bool updateLimit = false; // 防止特殊情况
            int connerIndex = 0;
            NaviVector leftLimitDirNormalized = NaviVector.NormalizedXZ(leftLimitDir);
            while (connerIndex < leftConnerList.Count)
            {
                float rad = float.MaxValue;
                for (int i = connerIndex; i < leftConnerList.Count; i++)
                {
                    NaviVector v = NaviVector.NormalizedXZ(vertexArr[leftConnerList[i]] - funnelPos);
                    float currentRad = MathF.Abs(NaviVector.GetAngleXZ(leftLimitDirNormalized, v));
                    if (currentRad <= rad)
                    {
                        connerIndex = i;
                        rad = currentRad;
                    }
                }

                updateLimit = true; // 防止特殊情况

                // 漏斗位置更新, 极限向量更新
                leftLimitIndex = leftConnerList[connerIndex];
                leftLimitDir = vertexArr[leftLimitIndex] - funnelPos;
                rightLimitIndex = rightCheckIndex;
                rightLimitDir = vertexArr[rightLimitIndex] - funnelPos;

                float cross = NaviVector.CrossProductXZ(rightLimitDir, leftLimitDir);
                if (cross > 0) // leftLimitDir在rightLimitDir的逆时针方向(漏斗不合法)
                {
                    funnelPos = vertexArr[leftLimitIndex];
                    positionList.Add(funnelPos);
                    connerIndex++;
                    if (connerIndex >= leftConnerList.Count)
                    {
                        leftLimitIndex = -1;
                        leftLimitDir = NaviVector.Zero;

                        rightLimitDir = vertexArr[rightLimitIndex] - funnelPos;
                    }
                }
                else // 漏斗合法
                {
                    for (int i = 0; i < connerIndex; i++)
                    {
                        leftConnerList.RemoveAt(0);
                    }
                    break;
                }
            }

            if (!updateLimit)
            {
                // 重置左极限和右极限
                leftLimitIndex = -1;
                leftLimitDir = NaviVector.Zero;
                rightLimitIndex = rightCheckIndex;
                rightLimitDir = vertexArr[rightLimitIndex] - funnelPos;
            }
        }

        void ResetFunnelData()
        {
            // TODO
        }
        #endregion
    }
}
