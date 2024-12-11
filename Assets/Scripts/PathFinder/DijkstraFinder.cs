// Dijkstra算法
namespace NaviPath
{
    public class DijkstraFinder : BasePathFinder
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

                if (!m_detectQueue.Contains(neighborBlock))
                {
                    neighborBlock.priority = neighborBlock.sumDistance;
                    m_detectQueue.Enqueue(neighborBlock);
                }

                // 检测完后显示方向指向信息
                neighborBlock.OnViewChange(ViewState.DirctionDebug);
            }
        }
    }
}
