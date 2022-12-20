using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System;

public class Button : MonoBehaviour
{
    [SerializeField]
    Color enabledTextColor = new Color(1, 1, 1, 1);
    [SerializeField]
    Color disabledTextColor = new Color(1, 1, 1, 0.4f);
    [SerializeField]
    Color enabledBorderColor = new Color(1, 1, 1, 1);
    [SerializeField]
    Color disabledBorderColor = new Color(1, 1, 1, 0.4f);

    private Action m_ButtonPressedBehave;
    bool m_isEnabled;
    private UnityEngine.UI.Button m_button;

    private void Awake()
    {
        m_button = GetComponent<UnityEngine.UI.Button>();
        Debug.Assert(m_button != null);
        Disable();
    }

    public void RegistPressedBehave(Action action)
    {
        m_ButtonPressedBehave = action;
    }
    // public void ClearPressedBehave()
    // {
    //     m_ButtonPressedBehave = null;
    // }
    public void Pressed()
    {
        if (m_isEnabled == false) return;

        if (m_ButtonPressedBehave != null)
            m_ButtonPressedBehave();
    }
    public void Enable()
    {
        m_isEnabled = true;
        m_button.interactable = true;
        this.GetComponentInChildren<TextMeshProUGUI>().color = enabledTextColor;
        foreach (var border in this.GetComponentsInChildren<Image>())
        {
            border.color = enabledBorderColor;
        }
    }
    public void Disable()
    {
        m_isEnabled = false;
        m_button.interactable = false;
        this.GetComponentInChildren<TextMeshProUGUI>().color = disabledTextColor;
        foreach (var border in this.GetComponentsInChildren<Image>())
        {
            border.color = disabledBorderColor;
        }
    }
    public void SetIsInteractable(bool is_interactable)
    {
        if (is_interactable)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }
}
