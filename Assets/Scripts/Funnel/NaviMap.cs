using System;
using System.Collections.Generic;

// 导航逻辑地图
namespace NaviFunnel
{
    public partial class NaviMap
    {
        readonly List<int[]> indexArrList;
        readonly NaviVector[] vertexArr;
        readonly NaviArea[] areaArr;

        readonly Dictionary<string, NaviBorder> naviBorderDic = new Dictionary<string, NaviBorder>(); // 通过两个顶点索引获取边界线: 不为共享边界线
        readonly Dictionary<string, NaviBorder> areaIDDic = new Dictionary<string, NaviBorder>(); // 通过两个区域ID获取边界线: 共享边界线

        public static Action<NaviVector, int> ShowAreaIDView;
        public static Action<List<NaviArea>> ShowPathAreaView;
        public static Action<List<NaviVector>> ShowInflectionPointView; // 显示拐点

        public NaviMap(List<int[]> indexArrList, NaviVector[] vertexArr)
        {
            this.indexArrList = indexArrList;
            this.vertexArr = vertexArr;

            areaArr = new NaviArea[indexArrList.Count];
            for (int i = 0; i < indexArrList.Count; i++)
            {
                areaArr[i] = new NaviArea(i, vertexArr, indexArrList[i]);
                ShowAreaIDView?.Invoke(areaArr[i].center, i);
            }

            SetBasicData();
        }

        void SetBasicData()
        {
            for (int areaID = 0; areaID < indexArrList.Count; areaID++)
            {
                int[] indexArr = indexArrList[areaID];
                for (int vertexIndex = 0; vertexIndex < indexArr.Length; vertexIndex++)
                {
                    int startIndex = indexArr[vertexIndex];
                    int endIndex;
                    if (vertexIndex < indexArr.Length - 1)
                        endIndex = indexArr[vertexIndex + 1];
                    else
                        endIndex = indexArr[0];

                    string key;
                    if (startIndex < endIndex)
                        key = $"{startIndex}_{endIndex}";
                    else
                        key = $"{endIndex}_{startIndex}";

                    // 如果已经存在边界线: 是共享边界线
                    if (naviBorderDic.TryGetValue(key, out NaviBorder border))
                    {
                        border.isShared = true;
                        border.areaID_2 = areaID;
                        if (border.areaID_1 < border.areaID_2)
                        {
                            key = $"{border.areaID_1}_{border.areaID_2}";
                        }
                        else
                        {
                            key = $"{border.areaID_2}_{border.areaID_1}";
                        }
                        areaIDDic.Add(key, border);
                    }
                    else // 如果已经存在这条边界线: 不是共享边界线或者是第一次遍历到这条边界线
                    {
                        border = new NaviBorder()
                        {
                            areaID_1 = areaID,
                            vertexIndex_1 = startIndex,
                            vertexIndex_2 = endIndex
                        };
                        naviBorderDic.Add(key, border);
                    }
                }
            }

            // 从 naviBorderDic 中移除命名重复的边界线
            List<string> singleList = new List<string>();
            foreach (var item in naviBorderDic)
            {
                if (!item.Value.isShared)
                {
                    singleList.Add(item.Key);
                }
            }
            for (int i = 0; i < singleList.Count; i++)
            {
                naviBorderDic.Remove(singleList[i]);
            }

            for (int areaID = 0; areaID < areaArr.Length; areaID++)
            {
                areaArr[areaID].borderList = GetNaviBorderListByAreaID(areaID);
            }
        }

        /// <summary>
        /// 计算导航路径
        /// </summary>
        public List<NaviVector> CalculateNaviPath(NaviVector start, NaviVector end)
        {
            int startAreaID = GetNaviAreaIDByPos(start);
            int endAreaID = GetNaviAreaIDByPos(end);
            if (startAreaID == -1)
            {
                this.Error($"No start area found in {start}.");
                return null;
            }
            else
            {
                this.Log($"Start areaID: {startAreaID}");
            }

            if (endAreaID == -1)
            {
                this.Error($"No end area found in {end}.");
                return null;
            }
            else
            {
                this.Log($"End areaID: {endAreaID}");

                NaviArea startArea = areaArr[startAreaID];
                NaviArea endArea = areaArr[endAreaID];
                List<NaviArea> areaPath = CalculatePolyPathByAStar(startArea, endArea);
                List<NaviVector> inflectionPointList = null;
                // TODO
                return inflectionPointList;
            }
        }

        List<NaviBorder> GetNaviBorderListByAreaID(int areaID)
        {
            List<NaviBorder> borderList = new List<NaviBorder>();
            foreach (var item in naviBorderDic)
            {
                if (item.Value.areaID_1 == areaID || item.Value.areaID_2 == areaID)
                {
                    borderList.Add(item.Value);
                }
            }
            return borderList;
        }

        /// <summary>
        /// 通过位置获取导航区域ID
        /// </summary>
        public int GetNaviAreaIDByPos(NaviVector position)
        {
            int areaID = -1;

            // TODO
            return areaID;
        }

        /// <summary>
        /// 判断一个点是否在XZ平面上的某个线段上
        /// </summary>
        /// <param name="p1">线段的顶点1</param>
        /// <param name="p2">线段的顶点2</param>
        /// <param name="point">要判断的点</param>
        /// <returns></returns>
        bool PointOnXZSegment(NaviVector p1, NaviVector p2, NaviVector point)
        {
            NaviVector v1 = p1 - point;
            NaviVector v2 = p2 - point;

            bool isCollinear = NaviVector.CrossProductXZ(v1, v2) == 0; // 两向量是否在XZ平面上共线
            bool isNoProjectionLength = NaviVector.DotXZ(v1, v2) <= 0; // 两向量是否方向相反或在端点上(相反才有可能在线段上)

            return isCollinear && isNoProjectionLength;
        }
    }
}
