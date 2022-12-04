using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SyncManager : MonoBehaviourPunCallbacks
{
    //シングルトン実装
    private static SyncManager instance;
    public static SyncManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SyncManager)FindObjectOfType(typeof(SyncManager));
                if (instance == null)
                {
                    Debug.Log(" SyncManager Instance Error ");
                }
            }
            return instance;
        }
    }
    //-------------------------------------------------
    [SerializeField]
    Card[] m_cards = new Card[54];
    private int CalcCardsIndex(int num, Card.Suit suit)
    {
        return 13 * (int)suit + num - 1;
    }

    public void RegistCardInstance(Card card)
    {
        int index = card.m_sync_id;

        Debug.Assert(index < 54 & index >= 0);

        m_cards[index] = card;

        //Debug.Log("SyncManager.m_cards[]の中身を順に出力します");
        //foreach(var c in m_cards)
        //{
        //    Debug.Log("\t" + c.name);
        //}
        //Debug.Log("合計" + m_cards.Length + "枚");
    }

    public Card GetCardInstance(int sync_id)
    {
        Card c = m_cards[sync_id];
        Debug.Assert(c != null);

        return c;
    }

    public void Log(string str)
    {
        photonView.RPC(nameof(LogRPC), RpcTarget.AllBuffered, str);
    }
    [PunRPC]
    public void LogRPC(string str)
    {
        Debug.Log(str);
    }

}
