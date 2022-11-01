using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncManager : MonoBehaviour
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
        int num = card.m_num;
        Card.Suit suit = card.m_suit;

        m_cards[CalcCardsIndex(num, suit)] = card;

        //Debug.Log("SyncManager.m_cards[]の中身を順に出力します");
        //foreach(var c in m_cards)
        //{
        //    Debug.Log("\t" + c.name);
        //}
        //Debug.Log("合計" + m_cards.Length + "枚");
    }

    public Card GetCardInstance(int num, Card.Suit suit)
    {
        Debug.Assert(num != 0);
        Card c = m_cards[CalcCardsIndex(num, suit)];
        Debug.Assert(c != null);

        return c;
    }
}
