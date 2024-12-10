using System.Collections.Generic;
using System.Threading.Tasks;

// 寻路计算器的基类
namespace NaviPath
{
    public abstract class BasePathFinder
    {
        protected BlockLogic m_startBlock;
        protected BlockLogic m_endBlock;

        protected Queue<BlockLogic> m_detectQueue = new Queue<BlockLogic>(); // 等待检测的队列
        protected List<BlockLogic> m_finishedList = new List<BlockLogic>(); // 已经检测过的列表
        protected List<BlockLogic> m_pathList = new List<BlockLogic>(); // 路径最终区块列表

        /// <summary>
        /// 计算路径
        /// </summary>
        public async Task<List<BlockLogic>> CalculatePath(BlockLogic start, BlockLogic end)
        {
            m_startBlock = start;
            m_endBlock = end;
            m_detectQueue.Clear();
            m_finishedList.Clear();
            m_pathList.Clear();

            m_detectQueue.Enqueue(start);

            m_startBlock.sumDistance = 0;

            while (m_detectQueue.Count > 0)
            {
                if (m_detectQueue.Contains(m_endBlock))
                {
                    // 找到路径
                    m_pathList = GetPathList();
                    break;
                }

                BlockLogic detectBlock = m_detectQueue.Dequeue();
                if (!m_finishedList.Contains(detectBlock))
                {
                    m_finishedList.Add(detectBlock);
                }

                for (int i = 0; i < detectBlock.neighborsList.Count; i++)
                {
                    BlockLogic neighbor = detectBlock.neighborsList[i];
                    await Task.Delay(PathFindingRoot.Instance.searchDelay);
                    DetectNeighborBlocks(detectBlock, neighbor);
                }
            }

            return m_pathList;
        }

        protected abstract void DetectNeighborBlocks(BlockLogic detectBlock, BlockLogic neighborBlock);

        protected List<BlockLogic> GetPathList()
        {
            List<BlockLogic> pathList = new List<BlockLogic>();
            BlockLogic current = m_endBlock;
            while (current.preBlock != null)
            {
                pathList.Insert(0, current); // 插入到第一个位置
                current = current.preBlock;
            }
            return pathList;
        }
    }
}
