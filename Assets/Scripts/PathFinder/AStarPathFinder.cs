// A*寻路算法
namespace NaviPath
{
    public class AStarPathFinder : BasePathFinder
    {
        protected override void DetectNeighborBlocks(BlockLogic detectBlock, BlockLogic neighborBlock)
        {
            if (!m_finishedList.Contains(neighborBlock) && !m_detectQueue.Contains(neighborBlock))
            {
                // 正在检测区块，显示为检测中
                neighborBlock.OnViewChange(ViewState.Checking);
                float neighborDistance = BlockLogic.GetLogicBlockDistance(detectBlock, neighborBlock);
                float newDistance = detectBlock.sumDistance + neighborDistance;
                if (!float.IsPositiveInfinity(neighborBlock.sumDistance) // neighborblock还没被检测过 
                    || neighborBlock.sumDistance > newDistance)
                {
                    neighborBlock.preBlock = detectBlock;
                    neighborBlock.sumDistance = newDistance;
                }

                // 优先级 = 起点到当前点的总距离 + 当前点到终点的估算距离
                // 估价函数f(n) = g(n) + h(n)
                if (!m_detectQueue.Contains(neighborBlock))
                {
                    // f(n) = g(n) + h(n)
                    neighborBlock.priority = neighborBlock.sumDistance + BlockLogic.GetLogicBlockDistance(neighborBlock, m_endBlock);
                    m_detectQueue.Enqueue(neighborBlock);
                }

                // 检测完后显示方向指向信息
                neighborBlock.OnViewChange(ViewState.DirctionDebug);
            }
        }
    }
}
