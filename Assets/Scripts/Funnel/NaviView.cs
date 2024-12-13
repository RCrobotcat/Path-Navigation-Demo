using NaviFunnel;
using UnityEngine;

// 导航计算结果展示
public class NaviView : MonoBehaviour
{
    public Transform AreaIDRoot;

    public void ShowAreaIDView(NaviVector position, int areaID)
    {
        var go = new GameObject { name = $"AreaID {areaID}" };
        go.transform.SetParent(AreaIDRoot);

        go.transform.position = position.ConvertToUnityVector();
        go.transform.localEulerAngles = new Vector3(90, 0, 0);
        var comp = go.AddComponent<TextMesh>();

        comp.text = areaID.ToString();
        comp.anchor = TextAnchor.MiddleCenter;
        comp.alignment = TextAlignment.Center;
        comp.color = Color.gray;
        comp.fontSize = 45;
        comp.fontStyle = FontStyle.Normal;

        go.transform.localScale = Vector3.one;
    }
}