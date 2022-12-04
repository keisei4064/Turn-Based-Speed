using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System;

public class Button : MonoBehaviour
{
    static Color enabledButtonColor { get; } = new Color(0.3f, 0.3f, 0.3f, 1);
    static Color enabledTextColor { get; } = new Color(1, 1, 1, 1);
    static Color disabledButtonColor { get; } = new Color(0.3f, 0.3f, 0.3f, 0.4f);
    static Color disabledTextColor { get; } = new Color(1, 1, 1, 0.4f);

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
    public void ClearPressedBehave()
    {
        m_ButtonPressedBehave = null;
    }
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
        //this.GetComponent<Image>().color = enabledButtonColor;
        this.GetComponentInChildren<TextMeshProUGUI>().color = enabledTextColor;
    }
    public void Disable()
    {
        m_isEnabled = false;
        m_button.interactable = false;
        //this.GetComponent<Image>().color = disabledButtonColor;
        this.GetComponentInChildren<TextMeshProUGUI>().color = disabledTextColor;
    }
}
