using System;
using UnityEngine;
using UnityEngine.EventSystems;

// UI触控封装类
public class BlockViewBase : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    protected Action OnClickDown;
    protected Action OnEnter;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickDown?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke();
    }
}

