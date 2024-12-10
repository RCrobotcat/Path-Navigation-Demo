// 区块构成的地图
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace NaviPath
{
    public class BlockMap
    {
        int m_xCount;
        int m_yCount;

        BlockLogic[,] m_blockArr; // 二维数组[,] 三维数组[,,]

        public void InitMap(int xCount, int yCount)
        {
            // 初始化地图
            m_xCount = xCount;
            m_yCount = yCount;
            m_blockArr = new BlockLogic[xCount, yCount];

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    BlockLogic blockLogic = new();
                    blockLogic.InitBlockLogic(i, j);
                    m_blockArr[i, j] = blockLogic;
                }
            }
        }

        public void UpdateMapData()
        {
            for (int i = 0; i < m_xCount; i++)
            {
                for (int j = 0; j < m_yCount; j++)
                {
                    if (m_blockArr[i, j].Walkable)
                    {
                        m_blockArr[i, j].neighborsList = GetNeighborList(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// 计算路径, 通过传入的路径查找器计算路径
        /// </summary>
        public Task<List<BlockLogic>> CalculatePath(BlockLogic start, BlockLogic end, BasePathFinder pathFinder)
        {
            return pathFinder.CalculatePath(start, end);
        }

        readonly Vector2[] allDirs = { 
            // Clockwise
            new Vector2(-1,1),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(1,-1),
            new Vector2(0,-1),
            new Vector2(-1,-1),
            new Vector2(-1,0)
        };
        /// <summary>
        /// 获取邻居(上下左右八个方向邻居)列表
        /// </summary>
        List<BlockLogic> GetNeighborList(int x, int y)
        {
            List<BlockLogic> list = new();
            for (int i = 0; i < allDirs.Length; i++)
            {
                int _x = x + (int)allDirs[i].X;
                int _y = y + (int)allDirs[i].Y;
                if (InSideBorder(_x, _y) && m_blockArr[_x, _y].Walkable)
                {
                    list.Add(m_blockArr[_x, _y]);
                }
            }
            return list;
        }
        bool InSideBorder(int x, int y) // 判断是否在边界内
        {
            return x >= 0 && x < m_xCount && y >= 0 && y < m_yCount;
        }
    }
}
