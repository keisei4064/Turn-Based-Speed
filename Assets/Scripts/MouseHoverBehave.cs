using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

// マウスを乗せた時の挙動を登録するインターフェイス的なクラス
public class MouseHoverBehave : MonoBehaviour
{
    [SerializeField]
    UnityEvent m_hoverBhaves = new UnityEvent();

    [SerializeField]
    UnityEvent m_notHoverBhaves = new UnityEvent();
    public void OnMouseHover()
    {
        if (!enabled) return;
        m_hoverBhaves.Invoke();
    }
    public void OnMouseNotHover()
    {
        if (!enabled) return;
        m_notHoverBhaves.Invoke();
    }
}
