using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Trush : Deck
{
    public GameObject mask;

    private void Awake()
    {
        m_cards = new List<Card>();
        m_canDrop = false;
        DisableReceiveDrop();
        m_isFront = true;
        DisableMask();
    }

    //public void DoDiscardAnim(bool ifTurnover)
    //{
    //    if (ifTurnover)
    //    {
    //        AnimationQueue.Instance.AddAnimToLastIndex(
    //            m_cards[m_cards.Count - 1].Anim_StraightLineMoveWithTurnOver(this.transform.position));
    //    }
    //    else
    //    {
    //        AnimationQueue.Instance.AddAnimToLastIndex(
    //            m_cards[m_cards.Count - 1].Anim_StraightLineMove(this.transform.position));
    //    }
    //}

    // AddCardRPCÇÃÉâÉbÉpä÷êî
    override public void AddCard(Card card, bool doAnim = true, bool doSync = true)
    {
        if (doSync)
        {
            photonView.RPC(nameof(AddCardRPC), RpcTarget.All, card, doAnim);
        }
        else
        {
            AddCardRPC(card, doAnim);
        }
    }

    [PunRPC]
    override public void AddCardRPC(Card card, bool doAnim)
    {
        mask.transform.SetAsLastSibling();
        //base.AddCard(card, doAnim);
        base.AddCardRPC(card, doAnim);
    }

    public void EnableMask()
    {
        mask.SetActive(true);
    }
    public void DisableMask()
    {
        mask.SetActive(false);
    }
}
