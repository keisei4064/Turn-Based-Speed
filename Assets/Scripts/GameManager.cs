using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    //シングルトン実装
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (GameManager)FindObjectOfType(typeof(GameManager));
                if (instance == null)
                {
                    Debug.Log(" GameManager Instance Error ");
                }
            }
            return instance;
        }
    }
    //-------------------------------------------------

    public GameObject m_RootDeckObj;
    public GameObject m_MyDeckObj;
    public GameObject m_OppoDeckObj;
    public GameObject m_RightTrushObj;
    public GameObject m_LeftTrushObj;
    public GameObject m_MyHandObj;
    public GameObject m_OppoHandObj;
    public GameObject m_UIManagerObj;
    public GameObject m_GameStatusObj;
    static Deck m_RootDeck;
    static Deck m_MyDeck;
    static Deck m_OppoDeck;
    static Trush m_RightTrush;
    static Trush m_LeftTrush;
    static Hand m_MyHand;
    static Hand m_OppoHand;
    static UIManager m_UIManager;
    static public GameStatus m_gameStatus;

    public Image m_ImageCardPrefab;
    public Canvas m_TopLayerCanvas;


    GameManagerState m_nowState;
    static GameManagerState m_nextState;

    List<MouseHoverBehave> lastHits = new List<MouseHoverBehave>(); //Hover挙動

    private void Awake()
    {
        // TODO: 確認不十分　いちいち書くのめんどい
        if (m_RootDeckObj == null ||
            m_MyDeckObj == null ||
            m_OppoDeckObj == null ||
            m_RightTrushObj == null ||
            m_LeftTrushObj == null ||
            m_MyHandObj == null ||
            m_OppoDeckObj == null)
        {
            throw new NullReferenceException("Game Object instance is null.");
        }

        m_RootDeck = m_RootDeckObj.GetComponent<Deck>();
        m_MyDeck = m_MyDeckObj.GetComponent<Deck>();
        m_OppoDeck = m_OppoDeckObj.GetComponent<Deck>();
        m_RightTrush = m_RightTrushObj.GetComponent<Trush>();
        m_LeftTrush = m_LeftTrushObj.GetComponent<Trush>();
        m_MyHand = m_MyHandObj.GetComponent<Hand>();
        m_OppoHand = m_OppoHandObj.GetComponent<Hand>();
        m_UIManager = m_UIManagerObj.GetComponent<UIManager>();
        m_gameStatus = m_GameStatusObj.GetComponent<GameStatus>();

        //Cardのシリアライズルール登録
        Card.RegisterSerializeRule();
    }

    // Start is called before the first frame update
    void Start()
    {
        Card.LoadImages();
        m_nowState = new PrepareCardState(m_ImageCardPrefab);
        m_nowState.Enter();

        PhotonNetwork.IsMessageQueueRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        m_nowState.Update();
        if (m_nextState != null)
        {
            m_nowState.Exit();
            m_nowState = m_nextState;
            m_nowState.Enter();
            m_nextState = null;
        }

        // マウスのHover処理
        if (lastHits.Count != 0) //前回の分 離れたこととする
        {
            foreach (var hit in lastHits)
            {
                hit.OnMouseNotHover();
            }
        }
        lastHits.Clear();

        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        foreach (var hit in Physics2D.RaycastAll(targetPos, Vector2.zero))
        {
            MouseHoverBehave hoverBehaveComponent;
            if (hit.collider.TryGetComponent<MouseHoverBehave>(out hoverBehaveComponent))
            {
                hoverBehaveComponent.OnMouseHover();
                lastHits.Add(hoverBehaveComponent);
            }
        }
    }

    private void FixedUpdate()
    {
        AnimationQueue.Instance.DoAnimation();

        //3倍速
        //AnimationQueue.Instance.DoAnimation();
        //AnimationQueue.Instance.DoAnimation();
        //AnimationQueue.Instance.DoAnimation();
    }

    private void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
    }


    // 内部クラスとしてStateパターンを実装 ------------------------------------------------------------------------------------------------------------------------------
    abstract class GameManagerState
    {
        abstract public void Enter();
        abstract public void Update();
        abstract public void Exit();
    }

    class PrepareCardState : GameManagerState
    {
        Image m_ImageCardPrefab;

        public PrepareCardState(Image imageCardPrefab)
        {
            m_ImageCardPrefab = imageCardPrefab;
        }


        public override void Enter()
        {
            m_gameStatus.m_nowMode = GameStatus.Mode.PREPARE_CARD;
            Debug.Log("Gamemode: PREPARE_CARD");

            WorkQueue.Instance.StopNotMasterClient();
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("自身がマスタークライアントです");

                // やる処理をすべて登録しておく
                WorkQueue.Instance.EnqueueOnceRunFuncs(
                        MakeAllCardsToRootDeck,
                        () =>
                        {
                            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                            m_RootDeck.Shuffle();
                        },
                        MakeMyOppoDeck,
                        MakeInitialHands,
                        MakeInitialTrash,

                        // TODO: ?????????n???
                        m_gameStatus.SetTurnRandom,

                        WorkQueue.Instance.RestartRPC
                    );
            }
            else
            {
                Debug.Log("自身はマスタークライアントではありません");
                // 自分の場とするオブジェクトの入れ替え
                (m_MyDeck, m_OppoDeck) = (m_OppoDeck, m_MyDeck);
                (m_RightTrush, m_LeftTrush) = (m_LeftTrush, m_RightTrush);
                (m_MyHand, m_OppoHand) = (m_OppoHand, m_MyHand);
                (m_MyDeck.transform.position, m_OppoDeck.transform.position) = (m_OppoDeck.transform.position, m_MyDeck.transform.position);
                (m_RightTrush.transform.position, m_LeftTrush.transform.position) = (m_LeftTrush.transform.position, m_RightTrush.transform.position);
                (m_MyHand.transform.position, m_OppoHand.transform.position) = (m_OppoHand.transform.position, m_MyHand.transform.position);
                (m_MyHand.transform.rotation, m_OppoHand.transform.rotation) = (m_OppoHand.transform.rotation, m_MyHand.transform.rotation);
            }



            // 次のState登録
            WorkQueue.Instance.EnqueueOnceRunFunc(
                () =>
                {
                    m_nextState = new PlayingState();
                });
        }

        public override void Update()
        {
            WorkQueue.Instance.RunFunc();
        }
        public override void Exit()
        {
        }

        // -------------------------------------------------------------------------------------------------------
        void MakeAllCardsToRootDeck()
        {
            Debug.Log("Making All Root Deck's Cards");

            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            for (Card.Suit s = Card.Suit.Club; s <= Card.Suit.Spade; s++)
            {
                for (int i = 1; i <= 13; i++)
                {
                    //Image newCardImageObj = Image.Instantiate(m_ImageCardPrefab);
                    Image newCardImageObj = PhotonNetwork.Instantiate("ImageCard", Vector3.zero, Quaternion.identity).GetComponent<Image>();

                    Card newCard = newCardImageObj.GetComponent<Card>();

                    newCard.Initialize(s, i);
                    newCard.name = s.ToString() + "_" + i.ToString();

                    m_RootDeck.AddCard(newCard, false, true);
                }
            }
            //Image joker1_imageObj = Image.Instantiate(m_ImageCardPrefab);
            //Image joker2_imageObj = Image.Instantiate(m_ImageCardPrefab);
            Image joker1_imageObj = PhotonNetwork.Instantiate("ImageCard", Vector3.zero, Quaternion.identity).GetComponent<Image>();
            Image joker2_imageObj = PhotonNetwork.Instantiate("ImageCard", Vector3.zero, Quaternion.identity).GetComponent<Image>();
            Card joker1 = joker1_imageObj.GetComponent<Card>();
            Card joker2 = joker2_imageObj.GetComponent<Card>();
            joker1.Initialize(Card.Suit.Joker, 1, true);
            joker2.Initialize(Card.Suit.Joker, 2, true);
            joker1.name = "joker_1";
            joker2.name = "joker_2";

            m_RightTrush.AddCard(joker1, false, true);
            m_LeftTrush.AddCard(joker2, false, true);


            // 位置の整列
            foreach (Card card in m_RootDeck.m_cards)
            {
                card.SetTransformPositionToParent();
            }
            joker1.SetTransformPositionToParent();
            joker2.SetTransformPositionToParent();
            m_RootDeck.SetViewOrder();
        }

        //配る
        void MakeMyOppoDeck()
        {
            //Debug.Log("Making MyDeck and OppoDeck");
            SyncManager.Instance.Log("Making MyDeck and OppoDeck");

            int nLoop = m_RootDeck.m_cards.Count / 2;
            Vector3 myDeckPositon = m_MyDeck.transform.position;
            Vector3 oppoDeckPositon = m_OppoDeck.transform.position;
            var animList = new List<AnimationQueue.MethodAndWaitFrames>();

            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            for (int i = 0; i < nLoop; i++)
            {
                Card card1 = m_RootDeck.DrawCard();
                Card card2 = m_RootDeck.DrawCard();
                int waitFrames = 10 + i;
                m_MyDeck.AddCardWithDelay(card1, waitFrames);
                m_OppoDeck.AddCardWithDelay(card2, waitFrames);
            }

            //ジョーカー
            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            m_MyDeck.AddCard(m_RightTrush.DrawCard());
            m_OppoDeck.AddCard(m_LeftTrush.DrawCard());

            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            m_MyDeck.Shuffle();
            m_OppoDeck.Shuffle();
            SyncManager.Instance.Log("shuffled");
        }

        void MakeInitialHands()
        {
            Debug.Log("Making Initial Hands");
            for (int i = 0; i < Hand.INITIAL_CARDS_NUM; i++)
            {
                AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                m_MyHand.AddCard(m_MyDeck.DrawCard());
                m_OppoHand.AddCard(m_OppoDeck.DrawCard());
            }
        }

        void MakeInitialTrash()
        {
            Debug.Log("Making Initial Trash");
            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            m_RightTrush.AddCard(m_MyDeck.DrawCard());
            m_LeftTrush.AddCard(m_OppoDeck.DrawCard());

            //ジョーカーが出てしまった場合
            Deck dealDeck = m_MyDeck;
            Trush dealTrush = m_RightTrush;
            for (int i = 0; i < 2; ++i)
            {
                while (dealTrush.GetTopCard().m_suit == Card.Suit.Joker)
                {
                    Debug.Log("joker is invalid for initial Trush.");

                    if (dealTrush.GetTopCard().m_suit == Card.Suit.Joker)
                    {
                        Card joker = dealTrush.DrawCard();
                        AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                        Debug.Log("joker.m_isFront: " + joker.m_isFront);
                        dealDeck.AddCard(joker, true);

                        AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                        dealDeck.Shuffle();

                        //引き直し
                        AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                        Card newDrawCard = dealDeck.DrawCard();

                        dealTrush.AddCard(newDrawCard, true);
                    }
                }
                dealDeck = m_OppoDeck;
                dealTrush = m_LeftTrush;
            }
        }
        //bool RemakeTrushUntilNotJoker()
        //{
        //    //ジョーカーが出てしまった場合
        //    Deck dealDeck = m_MyDeck;
        //    Trush dealTrush = m_RightTrush;
        //    for (int i = 0; i < 2; ++i)
        //    {
        //        while (dealTrush.GetTopCard().m_suit == Card.Suit.Joker)
        //        {
        //            Debug.Log("joker is invalid for initial Trush.");

        //            if (dealTrush.GetTopCard().m_suit == Card.Suit.Joker)
        //            {
        //                Card joker = dealTrush.DrawCard();
        //                AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
        //                Debug.Log("joker.m_isFront: " + joker.m_isFront);
        //                dealDeck.AddCard(joker, true);

        //                AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
        //                dealDeck.Shuffle();

        //                //引き直し
        //                AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
        //                Card newDrawCard = dealDeck.DrawCard();

        //                dealTrush.AddCard(newDrawCard, true);
        //            }
        //        }
        //        dealDeck = m_OppoDeck;
        //        dealTrush = m_LeftTrush;
        //    }
        //}

        void PutObjectToAppropriatePosition()
        {

        }
    }

    class PlayingState : GameManagerState
    {
        bool m_drawedToTrushLastTurn;
        bool m_drawedToTrushLastMyTurn;
        bool m_drawedToTrushLastOppoTurn;

        public PlayingState()
        {
        }
        //------------------------------------------------------------------------------------

        public override void Enter()
        {
            m_gameStatus.m_nowMode = GameStatus.Mode.PLAYING;
            Debug.Log("Gamemode: PLAYING");
            WorkQueue.Instance.EnqueueOnceRunFunc(StartTurn);
            m_LeftTrush.EnableMask();
            m_RightTrush.EnableMask();
            m_drawedToTrushLastTurn = false;
            m_drawedToTrushLastMyTurn = false;
            m_drawedToTrushLastOppoTurn = false;

            m_UIManager.DrawButton.RegistPressedBehave(OnPushedDrawButton);
            m_UIManager.DiscardButton.RegistPressedBehave(OnPushedDiscardButton);
            m_UIManager.CombineButton.RegistPressedBehave(OnPushedCombineButton);
            m_UIManager.CompressButton.RegistPressedBehave(OnPushedCompressButton);
            m_UIManager.TurnEndButton.RegistPressedBehave(OnPushedTurnEndButton);
            m_UIManager.BackButton.RegistPressedBehave(OnPushedBackButton);
        }
        public override void Update()
        {
            WorkQueue.Instance.RunFunc();
        }
        public override void Exit()
        {
        }

        // --------------------------------------------------------------------------------------
        Deck m_handlingDeck;
        Hand m_handlingHand;
        Trush m_mainTrush, m_subTrush;
        void StartTurn()
        {
            Debug.Log("Start Turn");
            m_UIManager.UpdateTurnView();
            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
            if (m_gameStatus.IsMyTurn())
            {
                m_handlingDeck = m_MyDeck;
                m_handlingHand = m_MyHand;
                m_mainTrush = m_RightTrush;
                m_subTrush = m_LeftTrush;
                m_drawedToTrushLastTurn = m_drawedToTrushLastMyTurn;
                m_drawedToTrushLastMyTurn = false;
                AnimationQueue.Instance.AddAnimToLastIndex(m_UIManager.Anim_Transition("My Turn"));
            }
            else
            {
                m_handlingDeck = m_OppoDeck;
                m_handlingHand = m_OppoHand;
                m_mainTrush = m_LeftTrush;
                m_subTrush = m_RightTrush;
                m_drawedToTrushLastTurn = m_drawedToTrushLastOppoTurn;
                m_drawedToTrushLastOppoTurn = false;
                AnimationQueue.Instance.AddAnimToLastIndex(m_UIManager.Anim_Transition("Opponent Turn"));
            }

            // 自分のターンがくるまで待機
            if (!m_gameStatus.IsMyTurn())
            {
                Debug.Log("not my turn");

                WorkQueue.Instance.EnqueueLoopFunc(WaitMyTurn);

                return;
            }

            SetButtoninteraction();
        }

        void DrawFromDeck()
        {
            Debug.Log("DrawFromDeck");

            Debug.Assert(m_handlingDeck.m_cards.Count != 0);
            Card topCard = m_handlingDeck.GetTopCard();
            Transform deckTransform = topCard.transform.parent.parent;

            topCard.EnableDrag();

            if (!m_drawedToTrushLastTurn)
            {
                topCard.RegistBeginDragObserver(m_mainTrush.EnableReceiveDrop);
                topCard.RegistEndDragObserver(m_mainTrush.DisableReceiveDrop);
                topCard.RegistBeginDragObserver(m_subTrush.EnableReceiveDrop);
                topCard.RegistEndDragObserver(m_subTrush.DisableReceiveDrop);
            }
            if (m_handlingHand.CanAddCard())
            {
                topCard.RegistBeginDragObserver(m_handlingHand.EnableReceiveDrop);
                topCard.RegistEndDragObserver(m_handlingHand.DisableReceiveDrop);
            }
            Card.EnqueueHappenHandlingObserver(topCard.DisableDrag);
            Card.EnqueueHappenHandlingObserver(topCard.ClearDragObserverList);


            WorkQueue.Instance.Stop();
            Card.EnqueueHappenHandlingObserver(WorkQueue.Instance.Restart);
            Card.EnqueueHappenHandlingObserver(TurnEnd);
            Card.EnqueueHappenHandlingObserver(m_UIManager.BackButton.Disable);

            Card.EnqueueHappenHandlingObserver(() =>
            {
                if (topCard.GetParentHoldCardObject() == m_handlingHand) //手札にドローしたとき
                {
                    int drawNum = Hand.INITIAL_CARDS_NUM - m_handlingHand.m_cards.Count;
                    if (drawNum > m_handlingDeck.m_cards.Count) drawNum = m_handlingDeck.m_cards.Count;
                    if (drawNum > 0)
                    {
                        for (int i = 0; i < drawNum; i++)
                        {
                            AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                            m_handlingHand.AddCard(m_handlingDeck.DrawCard());
                        }
                    }
                }

                bool drawedToTrush =
                    topCard.GetParentHoldCardObject() == m_mainTrush || topCard.GetParentHoldCardObject() == m_subTrush;
                if (m_gameStatus.IsMyTurn())
                {
                    m_drawedToTrushLastMyTurn = drawedToTrush;
                }
                else
                {
                    m_drawedToTrushLastOppoTurn = drawedToTrush;
                }
            });

            // WorkQueue.Instance.EnqueueOnceRunFunc(TurnEnd);
        }

        bool isFirstDiscard;
        void DiscardPhase()
        {
            Debug.Log("Discard Phase");

            // バースト
            if (m_handlingHand.m_cards.Count == 0 && m_handlingDeck.m_cards.Count != 0)
            {
                Debug.Log("Burst happen.");

                // INITIAL_CARDS_NUMの枚数まで自動で引く
                int drawNum = Hand.INITIAL_CARDS_NUM;
                if (drawNum > m_handlingDeck.m_cards.Count) drawNum = m_handlingDeck.m_cards.Count;
                for (int i = 0; i < drawNum; i++)
                {
                    AnimationQueue.Instance.CreateNewEmptyAnimListToEnd();
                    m_handlingHand.AddCard(m_handlingDeck.DrawCard());
                }
                WorkQueue.Instance.EnqueueOnceRunFunc(DiscardPhase);
                return;
            }
            // 決着
            if (m_handlingDeck.m_cards.Count == 0 && m_handlingHand.m_cards.Count == 0)
            {
                GameManager.Instance.StateTransitionToResultState();
                return;
            }


            if (isFirstDiscard && m_handlingDeck.m_cards.Count == 0) //山札がもうないとき
            {
                Debug.Log("No cards in Deck");
                foreach (var card in m_handlingHand.m_cards)
                {
                    card.EnableDrag();

                    card.RegistBeginDragObserver(m_mainTrush.EnableReceiveDrop);
                    card.RegistEndDragObserver(m_mainTrush.DisableReceiveDrop);
                    card.RegistBeginDragObserver(m_subTrush.EnableReceiveDrop);
                    card.RegistEndDragObserver(m_subTrush.DisableReceiveDrop);
                    Card.EnqueueHappenHandlingObserver(card.DisableDrag);
                    Card.EnqueueHappenHandlingObserver(card.ClearDragObserverList);
                }
            }
            else //通常時
            {
                SetContinuousRelation(isFirstDiscard);
            }

            // isFirstDiscard = false;

            bool canDiscard = false;
            foreach (var card in m_handlingHand.m_cards)
            {
                if (card.m_canDrag) canDiscard = true;
            }

            if (canDiscard)
            {
                Card.EnqueueHappenHandlingObserver(() =>
                {
                    isFirstDiscard = false;
                    WorkQueue.Instance.EnqueueOnceRunFunc(DiscardPhase);
                    m_UIManager.BackButton.Disable();
                });

                m_UIManager.TurnEndButton.Enable();
            }
            else
            {
                // 強制ターンエンド
                WorkQueue.Instance.EnqueueOnceRunFunc(TurnEnd);
            }

        }

        void CombinePhase()
        {
            Debug.Log("Combine Phase");
            m_handlingHand.SetAllSingleCardsMode(Card.MODE.WAIT_COMBINE);
            Card.ClearHappenHandlingObserver();
            Card.EnqueueHappenHandlingObserver(m_UIManager.BackButton.Disable);

            foreach (var card in m_handlingHand.m_cards)
            {
                if (card.m_mode != Card.MODE.WAIT_COMBINE || card.m_suit == Card.Suit.Joker)
                    continue;
                card.EnableDrag();
                Card.EnqueueHappenHandlingObserver(card.DisableDrag);
                Card.EnqueueHappenHandlingObserver(card.ClearDragObserverList);
                foreach (var targetCard in m_handlingHand.m_cards)
                {
                    if (targetCard.m_mode != Card.MODE.WAIT_COMBINE || targetCard.m_suit == Card.Suit.Joker)
                        continue;
                    if (card != targetCard)
                    {
                        card.RegistBeginDragObserver(targetCard.EnableReceiveDrop);
                        card.RegistEndDragObserver(targetCard.DisableReceiveDrop);
                    }
                }
            }

            m_UIManager.TurnEndButton.Enable();

            Card.EnqueueHappenHandlingObserver(() =>
            {
                m_handlingHand.SetAllWaitCardModeToSingle();
                //Debug.Log("GetSingleCardNum(): " + m_handlingHand.GetCanCombineOrCompressCardNum());

                if (m_handlingHand.GetCanCombineOrCompressCardNum() >= 2)
                {
                    WorkQueue.Instance.EnqueueOnceRunFunc(CombinePhase);
                }
                else
                {
                    WorkQueue.Instance.EnqueueOnceRunFunc(TurnEnd);
                }
            });
        }

        void CompressPhase()
        {
            Debug.Log("Compress Phase");
            Card.ClearHappenHandlingObserver();
            Card.EnqueueHappenHandlingObserver(m_UIManager.BackButton.Disable);

            if (!CanCompress())
            {
                foreach (Card card in m_handlingHand.m_cards)
                {
                    if (card.m_mode == Card.MODE.COMPRESSING)
                        card.SetMode(Card.MODE.COMPRESSED);
                }
                WorkQueue.Instance.EnqueueOnceRunFunc(TurnEnd);
                return;
            }

            foreach (Card card1 in m_handlingHand.m_cards)
            {
                foreach (Card card2 in m_handlingHand.m_cards)
                {
                    if (card2.CanCompressTothis(card1))
                    {
                        card1.SetMode(Card.MODE.WAIT_COMPRESS);
                    }
                }
            }

            foreach (var card in m_handlingHand.m_cards)
            {
                if (card.m_mode != Card.MODE.WAIT_COMPRESS)
                    continue;
                card.EnableDrag();
                Card.EnqueueHappenHandlingObserver(card.DisableDrag);
                Card.EnqueueHappenHandlingObserver(card.ClearDragObserverList);
                foreach (var targetCard in m_handlingHand.m_cards)
                {
                    if (targetCard.CanCompressTothis(card))
                    {
                        card.RegistBeginDragObserver(targetCard.EnableReceiveDrop);
                        card.RegistEndDragObserver(targetCard.DisableReceiveDrop);
                    }
                }
            }

            m_UIManager.TurnEndButton.Enable();

            Card.EnqueueHappenHandlingObserver(() =>
            {
                m_handlingHand.SetAllWaitCardModeToSingle();
                Debug.Log("GetSingleCardNum(): " + m_handlingHand.GetCanCombineOrCompressCardNum());

                WorkQueue.Instance.EnqueueOnceRunFunc(CompressPhase);
            });
        }

        Card LastLeftTrush, lastRightTrush;
        void SetContinuousRelation(bool isFirst)
        {
            //2回目以降は1回目の時出した方にのみ出せる
            bool isLeftEnabled = true;
            bool isRightEnabled = true;
            if (!isFirst)
            {
                isLeftEnabled = LastLeftTrush != m_LeftTrush.GetTopCard();
                isRightEnabled = lastRightTrush != m_RightTrush.GetTopCard();
            }

            foreach (var card in m_handlingHand.m_cards)
            {
                //Debug.Log("setting " + card.name + " 's continuous relation.");

                bool isContinuousWithLeft = card.IsContinuous(m_LeftTrush.GetTopCard());
                bool isContinuousWithRight = card.IsContinuous(m_RightTrush.GetTopCard());

                isContinuousWithLeft &= isLeftEnabled;
                isContinuousWithRight &= isRightEnabled;

                if (isContinuousWithLeft || isContinuousWithRight)
                {
                    //Debug.Log(card.name + " is continuous.");

                    card.EnableDrag();
                    Card.EnqueueHappenHandlingObserver(card.DisableDrag);
                    Card.EnqueueHappenHandlingObserver(card.ClearDragObserverList);
                }
                if (isContinuousWithLeft)
                {
                    card.RegistBeginDragObserver(m_LeftTrush.EnableReceiveDrop);
                    card.RegistEndDragObserver(m_LeftTrush.DisableReceiveDrop);
                }
                if (isContinuousWithRight)
                {
                    card.RegistBeginDragObserver(m_RightTrush.EnableReceiveDrop);
                    card.RegistEndDragObserver(m_RightTrush.DisableReceiveDrop);
                }
            }
            LastLeftTrush = m_LeftTrush.GetTopCard();
            lastRightTrush = m_RightTrush.GetTopCard();
        }

        bool CanCompress()
        {
            bool canCompress = false;
            foreach (Card card in m_handlingHand.m_cards)
            {
                if (card.m_mode == Card.MODE.COMBINED || card.m_mode == Card.MODE.COMPRESSED) continue;
                int num = card.m_num;
                foreach (Card compareCard in m_handlingHand.m_cards)
                {
                    if (card.m_suit == Card.Suit.Joker || compareCard.m_suit == Card.Suit.Joker) continue;
                    if (card == compareCard) continue;
                    if (compareCard.m_mode == Card.MODE.COMBINED || compareCard.m_mode == Card.MODE.COMPRESSED ||
                        compareCard.m_mode == Card.MODE.COMPRESSING)
                        continue;
                    if (num == compareCard.m_num)
                    {
                        canCompress = true;
                        break;
                    }
                }
            }
            return canCompress;
        }

        void TurnEnd()
        {
            Debug.Log("Turn End");

            m_UIManager.TurnEndButton.Disable();
            m_gameStatus.SwitchTurn();
            WorkQueue.Instance.EnqueueOnceRunFunc(StartTurn);
            WorkQueue.Instance.RestartRPC();
        }

        bool WaitMyTurn()
        {
            if ((m_MyDeck.m_cards.Count == 0 && m_MyHand.m_cards.Count == 0) ||
                (m_OppoDeck.m_cards.Count == 0 && m_OppoHand.m_cards.Count == 0)) return true;
            bool isMyTurn = m_gameStatus.IsMyTurn();
            if (isMyTurn)
            {
                WorkQueue.Instance.EnqueueOnceRunFunc(StartTurn);
            }
            return isMyTurn;
        }

        void OnPushedDrawButton()
        {
            CommonPressedBehave();
            WorkQueue.Instance.EnqueueOnceRunFunc(DrawFromDeck);
        }
        void OnPushedDiscardButton()
        {
            CommonPressedBehave();
            isFirstDiscard = true;
            WorkQueue.Instance.EnqueueOnceRunFunc(DiscardPhase);
        }
        void OnPushedCombineButton()
        {
            CommonPressedBehave();
            WorkQueue.Instance.EnqueueOnceRunFunc(CombinePhase);
        }
        void OnPushedCompressButton()
        {
            CommonPressedBehave();
            WorkQueue.Instance.EnqueueOnceRunFunc(CompressPhase);
        }
        void OnPushedTurnEndButton()
        {
            CommonPressedBehave();
            ClearCardsBehave();
            WorkQueue.Instance.EnqueueOnceRunFunc(TurnEnd);

            m_UIManager.BackButton.Disable();
        }
        void OnPushedBackButton()
        {
            ClearCardsBehave();
            SetButtoninteraction();
        }

        void CommonPressedBehave()
        {
            WorkQueue.Instance.Restart();

            m_UIManager.DrawButton.Disable();
            m_UIManager.DiscardButton.Disable();
            m_UIManager.CombineButton.Disable();
            m_UIManager.CompressButton.Disable();
            m_UIManager.TurnEndButton.Disable();

            m_UIManager.BackButton.Enable();
        }

        void SetButtoninteraction()
        {
            // DrawButton
            if (m_handlingDeck.m_cards.Count != 0 && !(m_drawedToTrushLastTurn && !m_handlingHand.CanAddCard()))
            {
                m_UIManager.DrawButton.Enable();
            }

            // DiscardButton
            bool canDiscard = false;
            foreach (var card in m_handlingHand.m_cards)
            {
                if (card.IsContinuous(m_LeftTrush.GetTopCard())) canDiscard = true;
                if (card.IsContinuous(m_RightTrush.GetTopCard())) canDiscard = true;
            }
            if (m_handlingDeck.m_cards.Count == 0) canDiscard = true;
            if (canDiscard)
            {
                m_UIManager.DiscardButton.Enable();
            }

            // CombineButton
            if (m_handlingHand.GetCanCombineOrCompressCardNum() >= 2)
            {
                m_UIManager.CombineButton.Enable();
            }

            // CompressButton
            if (CanCompress())
            {
                m_UIManager.CompressButton.Enable();
            }

            // TurnEndButton
            m_UIManager.TurnEndButton.Enable();

            m_UIManager.BackButton.Disable();
        }

        void ClearCardsBehave()
        {
            m_handlingHand.SetAllWaitCardModeToSingle();
            foreach (Card card in m_handlingHand.m_cards)
            {
                if (card.m_mode == Card.MODE.COMPRESSING)
                    card.SetMode(Card.MODE.COMPRESSED);
            }
            foreach (var card in m_handlingHand.m_cards)
            {
                card.DisableDrag();
                card.ClearDragObserverList();
            }
            Card topCard = m_handlingDeck.GetTopCard();
            topCard.DisableDrag();
            topCard.ClearDragObserverList();
            Card.ClearHappenHandlingObserver();
        }
    }

    class ResultState : GameManagerState
    {
        bool m_isPlayerWinner;

        public ResultState(bool isPlayerWinner)
        {
            m_isPlayerWinner = isPlayerWinner;
        }
        //------------------------------------------------------------------------------------
        public override void Enter()
        {
            m_UIManager.ShowResult(m_isPlayerWinner);
        }
        public override void Update()
        {
            WorkQueue.Instance.RunFunc();
        }

        public override void Exit()
        {
            throw new NotImplementedException();
        }
    }

    void StateTransitionToResultState()
    {
        photonView.RPC(nameof(StateTransitionToResultStateRPC), RpcTarget.AllBuffered);
    }
    [PunRPC]
    void StateTransitionToResultStateRPC()
    {
        bool isPlayerWinner = m_gameStatus.IsMyTurn();
        WorkQueue.Instance.EnqueueOnceRunFunc(() =>
        {
            m_nextState = new ResultState(isPlayerWinner);
        });
        return;
    }
}
