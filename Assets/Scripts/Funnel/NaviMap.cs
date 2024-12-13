using System;
using System.Collections.Generic;

// 导航逻辑地图
namespace NaviFunnel
{
    public class NaviMap
    {
        readonly List<int[]> indexArrList;
        readonly NaviVector[] vertexArr;
        readonly NaviArea[] areaArr;

        public static Action<NaviVector, int> ShowAreaIDView;

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
            // TODO
        }
    }
}
