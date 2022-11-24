using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class GameStatus: MonoBehaviourPunCallbacks
{
    public enum Mode
    {
        STANDBY,
        PREPARE_CARD,
        PLAYING,
        POUSE,
        EMPTY,
    }
    public enum PlayingPhase
    {
        TURN_START,
        DRAW,
        OPERATE,
        SERVE,
        TURN_END,
    }
    public enum Turn
    {
        MASTER_CLIENT_TURN,
        NOT_MASTER_CLIENT_TURN,
    }


    public Mode m_nowMode { get; set; }
    public PlayingPhase m_gamePhase { get; private set; }
    public bool m_isModeEnd { get; protected set; }
    public Turn m_turn { get; protected set; }

    public GameStatus()
    {
        m_isModeEnd = true;
    }

    public void SetTurnRandom()
    {
        UnityEngine.Random.InitState(Environment.TickCount);
        int zeroOrOne = (int)(0.5f + UnityEngine.Random.value);
        bool isMasterTurn = (zeroOrOne == 0);
        if (isMasterTurn)
        {
            //m_turn = Turn.MASTER_CLIENT_TURN;
            photonView.RPC(nameof(SetTurnMasterClient), RpcTarget.AllBuffered);
        }
        else
        {
            //m_turn = Turn.NOT_MASTER_CLIENT_TURN;
            photonView.RPC(nameof(SetTurnNotMasterClient), RpcTarget.AllBuffered);
        }
        Debug.Log("Result of SetTurnRandom: m_turn == " + m_turn.ToString());
    }
    public bool IsMyTurn()
    {
        bool is_my_turn = false;
        if (PhotonNetwork.IsMasterClient)
        {
            is_my_turn = (m_turn == Turn.MASTER_CLIENT_TURN);
        }
        else
        {
            is_my_turn = (m_turn == Turn.NOT_MASTER_CLIENT_TURN);
        }
        return is_my_turn;
    }
    public void SwitchTurn()
    {
        if (m_turn == Turn.MASTER_CLIENT_TURN)
        {
            photonView.RPC(nameof(SetTurnNotMasterClient), RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC(nameof(SetTurnMasterClient), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void SetTurnMasterClient()
    {
        //Debug.Log("set turn to master");
        m_turn = Turn.MASTER_CLIENT_TURN;
    }

    [PunRPC]
    private void SetTurnNotMasterClient()
    {
        //Debug.Log("set turn to not master");
        m_turn = Turn.NOT_MASTER_CLIENT_TURN;
    }

}
