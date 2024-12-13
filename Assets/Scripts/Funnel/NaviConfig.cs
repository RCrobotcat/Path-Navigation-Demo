using System.Collections.Generic;

// 导航配置数据
namespace NaviFunnel
{
    public class NaviConfig
    {
        public List<int[]> indexArrList; // 所有多边形的索引数据
        public NaviVector[] vertexArr; // 所有点的数据
    }
}
