// 导航区域
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
    }
}
