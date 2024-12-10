using UnityEngine;
using UnityEngine.UI;

namespace NaviPath
{
    public enum ViewState
    {
        Walk,
        Block,
        StatingPoint,
        EndingPoint,
        Checking,
        DirctionDebug,
        PathResult
    }

    // 单个区块显示
    public class BlockView : BlockViewBase
    {
        BlockLogic m_blockLogic;

        public Image stateImg; // 区块状态(黑色: 障碍、白色: 可走...)
        public Text indexInfo;
        public Text distanceInfo;

        public Transform arrowRoot; // 箭头

        public void InitBlockView(BlockLogic blockLogic)
        {
            m_blockLogic = blockLogic;
            name = $"block_({m_blockLogic.XIndex},{m_blockLogic.YIndex})"; // 设置名字
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = new Vector3(blockLogic.XIndex * 100, blockLogic.YIndex * 100);
            indexInfo.text = $"{m_blockLogic.XIndex},{m_blockLogic.YIndex}";

            blockLogic.OnViewChange = (state) =>
            {
                SetViewInfo(state);
            };

            OnClickDown = () =>
            {
                if (Input.GetMouseButton(0))
                {
                    if (m_blockLogic.Walkable)
                    {
                        PathFindingRoot.Instance.OnClickBlockItem(m_blockLogic);
                    }
                    else
                    {
                        Debug.LogWarning("This block is not walkable! Cannot be a starting point or an ending point!");
                    }
                }
                else
                {
                    OperationEnum op = PathFindingRoot.Instance.operation;
                    if (op == OperationEnum.Prepare)
                    {
                        m_blockLogic.SetWalkableState(!blockLogic.Walkable);
                    }
                    else
                    {
                        Debug.LogWarning("Cannot change the walkable state of a block now!");
                    }
                }
            };

            OnEnter = () =>
            {
                if (Input.GetMouseButton(1))
                {
                    OperationEnum op = PathFindingRoot.Instance.operation;
                    if (op == OperationEnum.Prepare)
                    {
                        m_blockLogic.SetWalkableState(!blockLogic.Walkable);
                    }
                    else
                    {
                        Debug.LogWarning("Cannot change the walkable state of a block now!");
                    }
                }
            };
        }

        void SetViewInfo(ViewState state)
        {
            switch (state)
            {
                case ViewState.Walk:
                    stateImg.color = Color.white;
                    distanceInfo.gameObject.SetActive(false);
                    arrowRoot.gameObject.SetActive(false);
                    break;
                case ViewState.Block:
                    stateImg.color = Color.black;
                    distanceInfo.gameObject.SetActive(false);
                    arrowRoot.gameObject.SetActive(false);
                    break;
                case ViewState.StatingPoint:
                    stateImg.color = Color.blue;
                    break;
                case ViewState.EndingPoint:
                    stateImg.color = Color.red;
                    break;
                case ViewState.Checking:
                    stateImg.color = Color.magenta;
                    distanceInfo.gameObject.SetActive(true);
                    distanceInfo.text = m_blockLogic.sumDistance.ToString();
                    arrowRoot.gameObject.SetActive(false);
                    break;
                case ViewState.DirctionDebug:
                    ShowSearchDirection(Color.yellow);
                    distanceInfo.text = m_blockLogic.sumDistance.ToString();
                    break;
                case ViewState.PathResult:
                    ShowSearchDirection(Color.green);
                    distanceInfo.text = m_blockLogic.sumDistance.ToString();
                    break;
                default:
                    break;
            }
        }

        void ShowSearchDirection(Color color)
        {
            if (m_blockLogic.preBlock != null)
            {
                stateImg.color = color;
                arrowRoot.gameObject.SetActive(true);
                int deltaX = m_blockLogic.XIndex - m_blockLogic.preBlock.XIndex;
                int deltaY = m_blockLogic.YIndex - m_blockLogic.preBlock.YIndex;
                float angle = Vector3.SignedAngle(Vector3.right, new Vector3(deltaX, deltaY, 0), Vector3.forward);
                arrowRoot.localEulerAngles = new Vector3(0, 0, angle);
            }
            else
            {
                arrowRoot.gameObject.SetActive(false);
            }
        }
    }
}