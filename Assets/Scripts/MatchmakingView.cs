using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MatchmakingView : MonoBehaviourPunCallbacks
{
    private RoomList m_roomList = new RoomList();
    private List<RoomButton> m_roomButtonList = new List<RoomButton>();
    private CanvasGroup m_canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_canvasGroup.interactable = false;

        int roomId = 1;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<RoomButton>(out var roomButton))
            {
                roomButton.Init(this, roomId++);
                m_roomButtonList.Add(roomButton);
            }
        }
    }

    public override void OnJoinedLobby()
    {
        m_canvasGroup.interactable = true;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        m_roomList.Update(roomList);

        foreach(var roomButton in m_roomButtonList)
        {
            if (m_roomList.TryGetRoomInfo(roomButton.RoomName, out var roomInfo))
            {
                roomButton.SetPlayerCount(roomInfo.PlayerCount);
            }
            else
            {
                roomButton.SetPlayerCount(0);
            }
        }
    }

    public void OnJoiningRoom()
    {
        m_canvasGroup.interactable = false;
    }

    public override void OnJoinedRoom()
    {
        //gameObject.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        m_canvasGroup.interactable = true;
    }
}
