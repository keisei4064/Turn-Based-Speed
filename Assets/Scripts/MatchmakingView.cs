using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class MatchmakingView : MonoBehaviourPunCallbacks
{
    private RoomList m_roomList = new RoomList();
    [SerializeField]
    private List<RoomButton> m_roomButtonList = new List<RoomButton>(5);
    private CanvasGroup m_canvasGroup;
    [SerializeField]
    private UnityEngine.UI.Button m_quickMatchButtonObj;
    private Button m_quickMatchButton;

    [SerializeField]
    public TextMeshProUGUI m_statusText;

    // Start is called before the first frame update
    void Start()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_canvasGroup.interactable = false;

        int roomId = 1;
        foreach (var button in m_roomButtonList)
        {
            button.Init(this, roomId++);
        }

        m_quickMatchButton = m_quickMatchButtonObj.GetComponent<Button>();
        m_quickMatchButton.RegistPressedBehave(OnQuickMatchButtonPushed);

        m_statusText.enabled = false;
    }

    public override void OnJoinedLobby()
    {
        m_canvasGroup.interactable = true;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        m_roomList.Update(roomList);

        bool is_exist_empty_room = false;
        foreach (var roomButton in m_roomButtonList)
        {
            if (m_roomList.TryGetRoomInfo(roomButton.RoomName, out var roomInfo))
            {
                roomButton.SetPlayerCount(roomInfo.PlayerCount);
            }
            else
            {
                roomButton.SetPlayerCount(0);
            }
            if (roomButton.IsAvailableToJoin())
            {
                is_exist_empty_room = true;
            }
        }

        m_quickMatchButton.SetIsInteractable(is_exist_empty_room);
    }

    public void OnJoiningRoom()
    {
        m_canvasGroup.interactable = false;
        m_quickMatchButton.Disable();
    }

    public override void OnJoinedRoom()
    {
        //gameObject.SetActive(false);
        m_statusText.enabled = true;
        m_statusText.text = "ëŒêÌëäéËÇë“Ç¡ÇƒÇ¢Ç‹Ç∑ÅB";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        m_canvasGroup.interactable = true;
    }

    public void OnQuickMatchButtonPushed()
    {

        foreach (var roomButton in m_roomButtonList)
        {
            if(roomButton.m_player_num == 1)
            {
                roomButton.OnButtonClick();
                return;
            }
        }
        foreach (var roomButton in m_roomButtonList)
        {
            // ãÛÇ¢ÇƒÇ¢ÇÈïîâÆÇÃÉ{É^ÉìÇ™âüÇ≥ÇÍÇΩÇ±Ç∆Ç…Ç∑ÇÈ
            if (roomButton.IsAvailableToJoin())
            {
                roomButton.OnButtonClick();
                return;
            }
        }
        Debug.LogError("QuickMatchÇ…é∏îs");
    }
}
