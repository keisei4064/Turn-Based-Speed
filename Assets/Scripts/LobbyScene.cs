using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    private const int MaxPlayerPerRoom = 2;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NickName = "Player";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayerPerRoom)
            {
                //PhotonNetwork.CurrentRoom.IsOpen = false;

                PhotonNetwork.IsMessageQueueRunning = false;
                Invoke(nameof(LoadGameScene), 0f);
            }
        }
    }

    private void LoadGameScene()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
