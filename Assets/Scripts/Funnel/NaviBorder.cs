// 导航区域的边界线
namespace NaviFunnel
{
    public class NaviBorder
    {
        public bool isShared = false; // 是否是共享边界线

        public int areaID_1 = -1; // 要连通的区域1
        public int areaID_2 = -1; // 要连通的区域2
        public int vertexIndex_1 = -1; // 这条边界线的顶点索引1
        public int vertexIndex_2 = -1; // 这条边界线的顶点索引2

        public override string ToString()
        {
            return $"Border: [{vertexIndex_1}, {vertexIndex_2}]";
        }
    }
}
