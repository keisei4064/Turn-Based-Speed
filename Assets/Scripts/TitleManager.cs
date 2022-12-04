using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_RuleButtonObj, m_TutorialButtonObj, m_SingleplayButtonObj, m_MultiplayButtonObj;
    Button m_RuleButton, m_TutorialButton, m_SingleplayButton, m_MultiplayButton;

    private void Awake()
    {
        m_RuleButton = m_RuleButtonObj.GetComponent<Button>();
        m_TutorialButton = m_TutorialButtonObj.GetComponent<Button>();
        m_SingleplayButton = m_SingleplayButtonObj.GetComponent<Button>();
        m_MultiplayButton = m_MultiplayButtonObj.GetComponent<Button>();

        m_MultiplayButton.Enable();
    }

    public void PushMultiplayButton()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
