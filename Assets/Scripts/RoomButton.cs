using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomButton : MonoBehaviour
{
    private const int MaxPlayers = 2;

    [SerializeField]
    private TextMeshProUGUI m_label = default;

    private MatchmakingView m_matchmakingView;
    private UnityEngine.UI.Button m_button;

    public string RoomName { get; private set; }

    public void Init(MatchmakingView parentView, int roomId)
    {
        m_matchmakingView = parentView;
        RoomName = $"Room{roomId}";

        m_button = GetComponent<UnityEngine.UI.Button>();
        m_button.interactable = false;
        m_button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        m_matchmakingView.OnJoiningRoom();

        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MaxPlayers;
        PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
    }

    public void SetPlayerCount(int playerCount)
    {
        m_label.text = $"{RoomName}\n{playerCount} / {MaxPlayers}";
        m_button.interactable = (playerCount < MaxPlayers);
    }
}
