using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class GameManager : MonoBehaviour
{
    public Player girl = new Player("女孩(对手)");
    public Player monster = new Player("梦兽(你)");
    public TugOfWarUI tugUI;

    public List<FearCardSO> girlDecksSO;
    public List<FearCardSO> monsterDecksSO;
    public List<FearCard> girlDecks = new List<FearCard>();
    public List<FearCard> monsterDecks = new List<FearCard>();
    public List<GameItem> itemPool = new List<GameItem>();
    public Transform[] cardSlots;
    public Transform[] itemSlots;

    public GameObject cardPrefab;
    public GameObject itemPrefab;
    public CardTargetArea monsterCardArea;
    public ItemTargetArea monsterItemArea;
    public Transform monsterTransform;
    public Transform girlTransform;
    public Transform girlCardArea;
    public Transform girlItemArea;
    public FearCardUI girlCardUI;
    public GameItemUI girlItemUI;
    public Button confirmBtn;
    public Button finishBtn;

    public TextMeshProUGUI monsterPointText;
    private int monsterSurplusPoint = 25;
    public TextMeshProUGUI girlPointText;
    private int girlSurplusPoint = 25;

    public CardTun roundDisplay;
    public Sprite[] roundSprites;
    public TextMeshProUGUI roundText;

    public TextMeshProUGUI contextDisplay;

    public Animator girlAnimator; 
    public Animator monsterAnimator;

    public SpriteManager spritesMap;
    public AudioManager audioManager;


    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard monsterSelectedCard; // ���ѡ��Ŀ���
    private FearCard girlSelectedCard; // ����ѡ��Ŀ���

    private bool isMonsterUsedItem = false;
    private bool isGirlUsedItem = false;

    public VideoPlayer videoPlayer; // 用于播放视频
    public RawImage rawImage; // 显示视频的RawImage
    public VideoClip victoryClip;    // 胜利视频
    public VideoClip defeatClip;     // 失败视频
    public VideoClip startClip;     // 开场视频

    public TextScroller scroller;

    //��ʼ��
    private async void Awake()
    {
        //cardPrefab = Resources.Load<GameObject>("CardTemplete");
        //itemPrefab = Resources.Load<GameObject>("ItemTemplete");
        monsterCardArea = transform.Find("MonsterCardArea").GetComponent<CardTargetArea>();
        monsterItemArea = transform.Find("MonsterItemArea").GetComponent<ItemTargetArea>();
        monsterTransform = transform.Find("Panel/Monster").GetComponent<Transform>();
        girlTransform = transform.Find("Panel/Girl").GetComponent<Transform>();
        girlCardArea = transform.Find("GirlCardArea").GetComponent<Transform>();
        girlItemArea = transform.Find("GirlItemArea").GetComponent<Transform>();
        girlAnimator = girlTransform.gameObject.GetComponent<Animator>();

        monsterPointText = transform.Find("MonsterSurplus/Point").GetComponent<TextMeshProUGUI>();
        girlPointText = transform.Find("GirlSurplus/Point").GetComponent<TextMeshProUGUI>();
        roundDisplay = transform.Find("Round").GetComponent<CardTun>();
        roundText = transform.Find("Round/Num").GetComponent<TextMeshProUGUI>();

        contextDisplay = transform.Find("ItemDisplay/Mask/Context").GetComponent<TextMeshProUGUI>();

        spritesMap = transform.Find("SpriteManager").GetComponent<SpriteManager>();
        audioManager = transform.Find("AudioManager").GetComponent<AudioManager>();

        scroller = contextDisplay.gameObject.GetComponent<TextScroller>();

        DOTween.Init();
        InitGame();


    }

    private void InitGame()
    {
        PlayVictoryVideo();
        spritesMap.InitMap();
        audioManager.InitMap();

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);
        currentRound = 1;

        // ��ʼ�����Ƴ�
        InitDecks(monster, girl, 1, 9);

        // ��ʼ��˫������
        monster.InitCards(monsterDecks);
        girl.InitCards(girlDecks);

        // ��ʼ��˫������
        InitGameItems(monster, girl);

        currentPhase = GamePhase.Start;
        RunPhase();
    }

    private void RunPhase()
    {
        roundDisplay.mFront.GetComponent<Image>().sprite = roundSprites[(currentRound - 1 + roundSprites.Length) % roundSprites.Length];
        roundDisplay.mBack.GetComponent<Image>().sprite = roundSprites[(currentRound - 1) % roundSprites.Length];
        roundDisplay.StartBack();
        switch (currentPhase)
        {
            case GamePhase.Start:
                StartRound();
                break;
            case GamePhase.Cover:
                CoverPhase();
                break;
            case GamePhase.Item:
                ItemPhase();
                break;
            case GamePhase.Resolve:
                ResolvePhase();
                break;
            case GamePhase.End:
                EndPhase();
                break;
        }
    }

    #region 开始阶段
    private void StartRound()
    {

        Debug.Log($"回合{currentRound} 开始");
        contextDisplay.text += $"回合{currentRound} 开始";
        
        roundText.text = $"当前回合：{currentRound}";
        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";
        //����
        //���ĻغϿ�ʼ˫������2������
        if (currentRound == 3)
        {
            Debug.Log("双方获得道具");
            // ���ӵ����߼�������չ��
            for (int i = 0; i < 3; i++)
            {
                girl.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
            }
            for (int i = 0; i < 3; i++)
            {
                monster.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
            }
        }
        StartCoroutine(PlayDealCardsAnimation());
        //��һ�׶�
        //currentPhase = GamePhase.Cover;
        //RunPhase();
    }

    private IEnumerator PlayDealCardsAnimation()
    {

        //���ص���UI��Ȼ����ʾ����UI
        HidePlayerCardUI();
        HidePlayerItemUI();
        yield return new WaitForSecondsRealtime(2f);

        List<FearCard> cards = monster.GetCards();
        if (cards.Count == 0)
        {
            yield return null;
        }

        for (int i = 0; i < cards.Count; ++i)
        {
            FearCard card = cards[i];

            // ��ȡ��ǰ�����е� FearCardUI�����û�У�ʵ����һ����
            FearCardUI cardUI = cardSlots[i].GetComponentInChildren<FearCardUI>();
            if (cardUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{card.cardName}");
                cardUI = Instantiate(cardPrefab).GetComponent<FearCardUI>();
                cardUI.transform.SetParent(cardSlots[i], false );
            }

            // ���ÿ���UI����
            cardUI.SetUI(card, cardSlots[i]);

            // ���ݿ���״̬��ʾ������ UI
            if (card.isUsed)
            {
                Debug.Log($"隐藏第 {i} 张已使用卡牌：{card.cardName}");
                cardUI.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"显示第 {i} 张未使用卡牌：{card.cardName}");
                cardUI.gameObject.transform.position = monsterTransform.transform.position;
                cardUI.gameObject.SetActive(true);
                cardUI.transform.DOMove(cardSlots[i].position, 1f).SetEase(Ease.InOutQuad);
                yield return new WaitForSecondsRealtime(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
            }

        }

        yield return new WaitForSecondsRealtime(1f); // �ȴ���������Ч�������

        // ���ƽ��������ѡ���ƽ׶�
        currentPhase = GamePhase.Cover;
        RunPhase();
    }
    #endregion

    #region 盖牌阶段
    private void CoverPhase()
    {
        Debug.Log("盖牌阶段开始");

        EnableButton(confirmBtn, true, OnClickedConfirmButton);
        EnableButton(finishBtn, true, OnClickedFinishButton);

    }

    private void CoverPhaseConfirm()
    {
        if(monsterCardArea.GetAreaCard() == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后 确认！");
            return;
        }
        //���ѡ��
        monsterSelectedCard = monsterCardArea.GetAreaCard();
        Debug.Log($"玩家选择了卡牌：{monsterSelectedCard.cardName}");


        CardTun cardTun = monsterCardArea.GetComponentInChildren<CardTun>();
        if (cardTun != null)
        {
            cardTun.StartBack();
        }
        else
        {
            Debug.Log("未选择卡牌，请选择卡牌后 结束！");
        }
        EnableButton(confirmBtn, false, null);
    }

    private void CoverPhaseRun()
    {
        if (monsterSelectedCard == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后 结束！");
            return;
        }

        Debug.Log($"玩家选择了卡牌：{monsterSelectedCard.cardName}");

        //����ѡ��
        List<FearCard> girlCards = girl.GetCards();
        girlSelectedCard = girlCards[Random.Range(0, girlCards.Count)];
        Debug.Log($"敌人选择了卡牌：{girlSelectedCard.cardName}");

        monsterSurplusPoint -= monsterSelectedCard.point;
        girlSurplusPoint -= girlSelectedCard.point;

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        StartCoroutine(PlaySelectedCardAnimation(girlSelectedCard));

    }

    private IEnumerator PlaySelectedCardAnimation(FearCard enemyCard)
    {
        girlCardUI = Instantiate(cardPrefab).GetComponent<FearCardUI>();
        girlCardUI.cardTun.mCardState = CardState.Back;
        girlCardUI.SetUI(enemyCard, girlCardArea);
        girlCardUI.transform.SetParent(girlCardArea, false);
        girlCardUI.gameObject.transform.position = girlTransform.transform.position;
        girlCardUI.transform.DOMove(girlCardArea.position, 1f).SetEase(Ease.InOutQuad);
        yield return new WaitForSecondsRealtime(1f+1f);

        // ������һ�׶�
        currentPhase = GamePhase.Item;
        RunPhase();
    }

    public FearCard GetMonsterSelectedCard()
    {
        return monsterSelectedCard;
    }

    public FearCard GetGirlSelectedCard()
    {
        return girlSelectedCard;
    }

    #endregion

    #region 道具阶段
    private void ItemPhase()
    {
        Debug.Log("道具阶段开始");

        isMonsterUsedItem = false;
        isGirlUsedItem = false;
        // ��ʾ���� UI
        StartCoroutine(PlayDealItemsAnimation());

    }

    private IEnumerator PlayDealItemsAnimation()
    {
        HidePlayerCardUI();
        HidePlayerItemUI();

        // ȷ�����ֹ��򣺼���ż���غϵ������֣������غ��������
        bool isPlayerTurn = currentRound % 2 != 0;


        // ��ʼ����ʹ�ý׶�
        StartCoroutine(HandleItemPhase(isPlayerTurn));

        yield return null;
    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        contextDisplay.text += ($"{(isPlayerTurn ? $"{monster.name}" : $"{girl.name}")}先手开始道具阶段");

        if (isPlayerTurn)
        {
            List<GameItem> items = monster.GetItems();
            if (items.Count == 0)
            {
                yield return null;
            }

            for (int i = 0; i < items.Count; ++i)
            {
                GameItem item = items[i];

                // ��ȡ��ǰ�����е� GameIteUI�����û�У�ʵ����һ����
                GameItemUI itemUI = itemSlots[i].GetComponentInChildren<GameItemUI>();
                if (itemUI == null)
                {
                    Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                    itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
                }

                // ���õ���UI����
                itemUI.gameObject.transform.localPosition = Vector3.zero;
                itemUI.SetUI(item, itemSlots[i]);
                itemUI.gameObject.transform.position = monsterTransform.transform.position;
                itemUI.transform.DOMove(itemSlots[i].position, 1f).SetEase(Ease.InOutQuad);
                yield return new WaitForSecondsRealtime(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
            }
        }

        // �����ʹ�õ���
        while (isPlayerTurn)
        {
            yield return StartCoroutine(HandlePlayerItemUsage());
            if (monster.GetItems().Count == 0 || isMonsterUsedItem)// ���û�е��������
            {
                isMonsterUsedItem = false;

                while(true)
                {
                    yield return StartCoroutine(HandleEnemyItemUsage());
                    if (girl.GetItems().Count == 0 || StartCoroutine(HandleEnemyItemUsage()) == null) break; // ����û�е��������
                }
            }; 

        }

        // ����ʹ�õ���
        while (!isPlayerTurn)
        {
            yield return StartCoroutine(HandleEnemyItemUsage());
            if (girl.GetItems().Count == 0 || StartCoroutine(HandleEnemyItemUsage()) == null)// ����û�е��������
            {
                List<GameItem> items = monster.GetItems();
                if (items.Count == 0)
                {
                    yield return null;
                }

                for (int i = 0; i < items.Count; ++i)
                {
                    GameItem item = items[i];

                    // ��ȡ��ǰ�����е� GameIteUI�����û�У�ʵ����һ����
                    GameItemUI itemUI = itemSlots[i].GetComponentInChildren<GameItemUI>();
                    if (itemUI == null)
                    {
                        Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                        itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
                    }

                    // ���õ���UI����
                    itemUI.gameObject.transform.localPosition = Vector3.zero;
                    itemUI.SetUI(item, itemSlots[i]);
                    itemUI.GetComponent<TooltipTrigger>().header = item.itemName;
                    itemUI.GetComponent<TooltipTrigger>().header = item.description;
                    itemUI.gameObject.transform.position = monsterTransform.transform.position;
                    itemUI.transform.DOMove(itemSlots[i].position, 1f).SetEase(Ease.InOutQuad);
                    yield return new WaitForSecondsRealtime(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
                }

                while (true)
                {
                    yield return StartCoroutine(HandlePlayerItemUsage());
                    if (monster.GetItems().Count == 0 || isMonsterUsedItem)
                    {
                        isMonsterUsedItem = false;

                        break;// ���û�е��������
                    }
                }
            }; 

        }

    }


    private IEnumerator HandlePlayerItemUsage()
    {
        Debug.Log("玩家的道具回合");


        // �ȴ����ѡ�����
        bool itemUsed = false;
        GameItem selectedItem = null;
        // ����ȷ�ϰ�ť
        EnableButton(finishBtn, true, OnClickedFinishButton);
        EnableButton(confirmBtn, true, () =>
        {
            selectedItem = monsterItemArea.GetAreaItem(); // ��ȡ���ѡ��ĵ���
            if (selectedItem != null)
            {
                Debug.Log($"玩家使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard, contextDisplay);
                monsterSelectedCard = cards[0];
                girlSelectedCard = cards[1];
                monster.RemoveItem(selectedItem);
                itemUsed = true;

                scroller.Scroll();
            }
        });

        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        monsterItemArea.ItemUIFlipBack(true);
        monsterItemArea.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSecondsRealtime(1.5f);
        monsterItemArea.ClearReadyToUseItem();
        //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
        audioManager.PlayItemSound(selectedItem.itemName);
        monsterItemArea.GetComponent<CircleCollider2D>().enabled = true;

    }

    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("敌人的道具回合");
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        List<GameItem> enemyItems = girl.GetItems();
        if (enemyItems.Count > 0)
        {
            GameItem selectedItem = null;

            // AIѡ����ߵ��߼�
            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"敌人使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(girl, monster, girlSelectedCard, monsterSelectedCard, contextDisplay);
                girlSelectedCard = cards[0];
                monsterSelectedCard = cards[1];
                girl.RemoveItem(selectedItem);

                scroller.Scroll();
                if (girlItemUI == null)
                {
                    girlItemUI = Instantiate(itemPrefab).GetComponent<GameItemUI>();
                }
                girlItemUI.gameObject.SetActive(true);
                girlItemUI.cardTun.mCardState = CardState.Back;
                girlItemUI.SetUI(selectedItem, girlItemArea);
                girlItemUI.transform.SetParent(girlItemArea, false);
                girlItemUI.gameObject.transform.position = girlTransform.transform.position;
                girlItemUI.transform.DOMove(girlItemArea.position, 1f).SetEase(Ease.InOutQuad);
                yield return new WaitForSecondsRealtime(1.5f);
                girlItemUI.FlipBack(false);
                yield return new WaitForSecondsRealtime(1.5f);

                girlItemUI.gameObject.SetActive(false);
                //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
                audioManager.PlayItemSound(selectedItem.itemName);

            }
            else
            {
                Debug.Log("敌人没有合适的道具");
                yield return null;
            }
        }
        else
        {
            Debug.Log("敌人没有可用的道具");
            yield return null;
        }

        // ģ����˲������ӳ�
        yield return new WaitForSecondsRealtime(3f);

    }

    #region AI道具决策
    private GameItem ChooseEnemyItem(List<GameItem> enemyItems)
    {
        // ������˵ĵ��߲���1�����Ӻ���������
        GameItem itemToUse = null;

        GameItem peekItem = enemyItems.Find(item => item.itemName == "侦探眼睛"); // �ҵ����ӵ���
        if (peekItem != null)
        {
            // ʹ�ÿ��ӵ���
            itemToUse = peekItem;
            Debug.Log("敌人使用侦探眼睛道具");

            // ���ݵ��˵Ĳ��Ծ���ʹ�ú��ֵ���
            GameItem bestCounterItem = EvaluatePeekStrategy(peekItem);
            if (bestCounterItem != null)
            {
                itemToUse = bestCounterItem;
            }
        }
        else
        {
            // ���û�п��ӵ��ߣ�������������ѡ�����
            itemToUse = SelectOtherEnemyItems(enemyItems);
        }

        return itemToUse;
    }

    private GameItem EvaluatePeekStrategy(GameItem peekItem)
    {
        // ���ڿ��ӽ��������ѡ������ʵĵ���
        if (peekItem != null)
        {
            // ���ݶԷ����Ƶ�����ѡ����ʵĵ��߲���
            int playerCardValue = monsterSelectedCard.point; // �����л�ȡ��ҵ�ǰ���Ƶ����ĺ���
            int enemyCardValue = girlSelectedCard.point; // �����л�ȡ���˵�ǰ���Ƶ����ĺ���

            if (playerCardValue > enemyCardValue)
            {
                // ���ݲ��ѡ���Ƿ���׳����͵����ǿ��
                return ChooseBoostOrSwap(playerCardValue, enemyCardValue);
            }
            else
            {
                // ���ݲ��ѡ�������߻�����
                return ChooseRewindOrSkip();
            }
        }

        return null;
    }

    private GameItem ChooseBoostOrSwap(int playerCardValue, int enemyCardValue)
    {
        // ѡ��׳����͵������
        if (Mathf.Abs(playerCardValue - enemyCardValue) <= 3)
        {
            return girl.GetItems().Find(item => item.itemName == "壮胆"); // �ҵ�׳������
        }
        else
        {
            return girl.GetItems().Find(item => item.itemName == "交互"); // �ҵ�͵������
        }
    }

    private GameItem ChooseRewindOrSkip()
    {
        // ���û�к��ʵĵ��ߣ���ѡ����������
        Debug.Log("敌人没有合适的道具, 选择道具(鬼手)");
        return girl.GetItems().Find(item => item.itemName == "鬼手"); // �ҵ��������
    }

    private GameItem SelectOtherEnemyItems(List<GameItem> enemyItems)
    {
        // ѡ�������ǿ��ӵ��ߵĲ��ԣ��������ѡ��
        Debug.Log("敌人随机选择道具");
        return enemyItems[Random.Range(0, enemyItems.Count)];
    }
    #endregion

    private void ItemPhaseConfirm()
    {
        //playerSelectedCard = cardTargetArea.GetAreaCard();
        EnableButton(confirmBtn, false, null);
    }

    private void ItemPhaseRun()
    {
        Debug.Log("玩家确认结束道具使用阶段");
        if (monsterItemArea.GetAreaItem() != null)
        {
            GameItem selectedItem = monsterItemArea.GetAreaItem();
            Debug.Log($"玩家使用道具：{selectedItem.itemName}");
            List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard, contextDisplay);
            monsterSelectedCard = cards[0];
            girlSelectedCard = cards[1];
            monster.RemoveItem(selectedItem);

            //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
            audioManager.PlayItemSound(selectedItem.itemName);


        }

        // ����ȷ�ϰ�ť
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        // �������� UI
        RefreshPlayerItemUI(monster);

        // ������һ�׶�
        currentPhase = GamePhase.Resolve;
        RunPhase();
    }
    #endregion

    #region 结算阶段
    private void ResolvePhase()
    {
        Debug.Log("结算阶段开始");
        Debug.Log($"玩家选择的卡为({monsterSelectedCard.cardName})，点数为({monsterSelectedCard.point})");

        int playerPoint = monsterSelectedCard.point;
        int enemyPoint = girlSelectedCard.point;
        monsterCardArea.UpdatePoint(monsterSelectedCard);
        girlCardUI.UpdatePoint(girlSelectedCard.point);

        StartCoroutine(PlayResolveAnimation(playerPoint, enemyPoint));

        monster.UseCard(monsterSelectedCard);
        girl.UseCard(girlSelectedCard);

    }

    private IEnumerator PlayResolveAnimation(int playerPoint, int enemyPoint)
    {

        monsterCardArea.CardUIFlipBack(false);
        girlCardUI.FlipBack(false);
        yield return new WaitForSecondsRealtime(2f);

        if (playerPoint > enemyPoint)
        {
            girl.IncreaseFearValue(1);
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitUntil(() => monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

            audioManager.PlayCardSound(monsterSelectedCard.cardName);
            girlAnimator.SetTrigger($"TriggerHurt");

            contextDisplay.text += ($"\n{monster.name}获胜，{girl.name}增加一点恐惧值 ({girl.GetFearValue()})");
        }
        else if (playerPoint < enemyPoint)
        {
            monster.IncreaseFearValue(1);
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitUntil(() => girlAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            audioManager.PlayCardSound(girlSelectedCard.cardName);
            monsterAnimator.SetTrigger($"TriggerHurt");

            contextDisplay.text += ($"\n{girl.name}获胜，{monster.name}增加一点恐惧值 ({monster.GetFearValue()})");
        }
        else
        {
            monster.IncreaseFearValue(1);
            girl.IncreaseFearValue(1);

            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitUntil(() => monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            yield return new WaitUntil(() => girlAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

            monsterAnimator.SetTrigger($"TriggerHurt");
            audioManager.PlayCardSound(monsterSelectedCard.cardName);
            // 等待动画结束
            yield return new WaitUntil(() => monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

            girlAnimator.SetTrigger($"TriggerHurt");
            audioManager.PlayCardSound(girlSelectedCard.cardName);
            // 等待动画结束
            yield return new WaitUntil(() => girlAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

            contextDisplay.text += ($"\n平局，双方都增加1恐惧值({monster.GetFearValue()}):({girl.GetFearValue()})");
        }
        yield return new WaitForSecondsRealtime(3f);

        monsterCardArea.ClearReadyToUseCard();
        for (int i = 1; i < girlCardArea.childCount; i++)
        {
            Destroy(girlCardArea.GetChild(i).gameObject);
        }
        // ������һ�׶�
        currentPhase = GamePhase.End;
        RunPhase();
    }

    private void EndPhase()
    {
        Debug.Log("回合结束阶段");
        tugUI.UpdateBars(4 + monster.GetFearValue(), 4 + girl.GetFearValue());
        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";


        // �ж��Ƿ���һ���־�ֵ�ﵽ3
        if (monster.GetFearValue() >= 3)
        {
            Debug.Log("玩家失败！");
            // 失败逻辑
            PlayDefeatVideo();
        }
        else if (girl.GetFearValue() >= 3)
        {
            Debug.Log("敌人失败！");
            // 胜利逻辑
            PlayVictoryVideo();
        }
        else
        {
            // 进入下一回合
            currentRound++;
            currentPhase = GamePhase.Start;
            RunPhase();
        }
    }
    #endregion

    private List<int> RandomCardPoints(int totalPoints, int cardCount, int minPoint, int maxPoint)
    {
        List<int> points = new List<int>();

        // 生成初始随机点数
        int remainingPoints = totalPoints;  // 剩余需要分配的点数
        for (int i = 0; i < cardCount - 1; i++)
        {
            // 每个点数在 minPoint 和 maxPoint 之间
            int randomPoint = Random.Range(minPoint, maxPoint + 1);
            points.Add(randomPoint);
            remainingPoints -= randomPoint;
        }

        // 最后一个点数，确保总和为 totalPoints
        // 并且点数范围仍在 minPoint 和 maxPoint 之间
        points.Add(remainingPoints);

        // 确保所有点数都在合法范围内
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] < minPoint)
            {
                points[i] = minPoint;
            }
            else if (points[i] > maxPoint)
            {
                points[i] = maxPoint;
            }
        }

        // 确保总和为 totalPoints
        int sum = points.Sum();
        int difference = totalPoints - sum;
        if (difference != 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                // 调整点数，确保总和为 totalPoints
                int adjustment = Mathf.Clamp(difference, minPoint - points[i], maxPoint - points[i]);
                points[i] += adjustment;
                difference -= adjustment;

                // 如果总和已经调整到目标，跳出循环
                if (difference == 0)
                {
                    break;
                }
            }
        }

        return points;
    }


    public void InitDecks(Player player, Player enemy,int startPoint, int endPoint)
    {
        // 生成梦兽卡牌
        List<int> points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            FearCard newCard = GenerateCardData(monsterDecksSO, points[i]);
            monsterDecks.Add(newCard);
        }

        // 生成小女孩卡牌
        points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            FearCard newCard = GenerateCardData(girlDecksSO, points[i]);
            girlDecks.Add(newCard);
        }

        player.ResetFearValue();
        enemy.ResetFearValue();
    }

    // 根据卡牌点数自动匹配背景、图案、数值
    private FearCard GenerateCardData(List<FearCardSO> decksSO, int fearValue)
    {
        // 查找对应卡牌的配置信息
        FearCardSO cardData = GetCardDataByFearValue(decksSO, fearValue);

        if (cardData == null)
        {
            Debug.LogError($"未找到对应 {fearValue} 吓人值的卡牌配置信息！");
            return null;
        }

        // 返回生成的卡牌数据
        return new FearCard(cardData.cardName, cardData.background, cardData.artSprite, cardData.back, fearValue, cardData.description);
    }

    // 根据卡牌的吓人值从配置文件中获取数据
    private FearCardSO GetCardDataByFearValue(List<FearCardSO> decksSO, int fearValue)
    {
        foreach (var cardData in decksSO)
        {
            if (cardData.minpoint <= fearValue && fearValue <= cardData.maxpoint)
            {
                return cardData;
            }
        }
        return null;  // 如果没有找到对应的卡牌，返回null
    }

    public void RefreshPlayerCardUI(Player player)
    {
        //���ص���UI��Ȼ����ʾ����UI
        HidePlayerItemUI();

        List<FearCard> cards = player.GetCards();
        if (cards.Count == 0)
        {
            return;
        }

        for (int i = 0; i < cards.Count; ++i)
        {
            FearCard card = cards[i];

            // ��ȡ��ǰ�����е� FearCardUI�����û�У�ʵ����һ����
            FearCardUI cardUI = cardSlots[i].GetComponentInChildren<FearCardUI>();
            if (cardUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{card.cardName}");
                cardUI = Instantiate(cardPrefab, cardSlots[i]).GetComponent<FearCardUI>();
            }

            // ���ÿ���UI����
            cardUI.SetUI(card, cardSlots[i]);

            // ���ݿ���״̬��ʾ������ UI
            if (card.isUsed)
            {
                Debug.Log($"隐藏第 {i} 张已使用卡牌：{card.cardName}");
                cardUI.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"显示第 {i} 张未使用卡牌：{card.cardName}");
                cardUI.gameObject.SetActive(true);
                cardUI.gameObject.transform.localPosition = Vector3.zero;
            }
        }


    }

    public void HidePlayerCardUI()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].childCount == 0)
            {
                continue;
            }
            Destroy(cardSlots[i].GetChild(0).gameObject);
        }

    }

    public void RefreshPlayerItemUI(Player player)
    {
        //���ؿ���UI��Ȼ����ʾ����UI
        HidePlayerCardUI();

        List<GameItem> items = player.GetItems();
        if (items.Count == 0)
        {
            return;
        }

        for (int i = 0; i < items.Count; ++i)
        {
            GameItem item = items[i];

            // ��ȡ��ǰ�����е� GameIteUI�����û�У�ʵ����һ����
            GameItemUI itemUI = itemSlots[i].GetComponentInChildren<GameItemUI>();
            if (itemUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
            }

            // ���õ���UI����
            itemUI.gameObject.transform.localPosition = Vector3.zero;
            itemUI.SetUI(item, itemSlots[i]);

        }
    }

    public void HidePlayerItemUI()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].childCount == 0)
            {
                continue;
            }
            Destroy(itemSlots[i].GetChild(0).gameObject);
        }

    }

    public void InitGameItems(Player player, Player enemy)
    {
        itemPool.Add(new PeekItem("侦探眼睛", spritesMap.FindSprite("侦探眼睛"), spritesMap.FindSprite("BackItem"), "偷看对方当前的覆盖的牌的吓人点数是多少"));
        itemPool.Add(new ChangeCardItem("抓娃娃爪子", spritesMap.FindSprite("抓娃娃爪子"), spritesMap.FindSprite("BackItem"), "原先的卡片收进，随机换一张新卡出"));
        itemPool.Add(new ForceChangeCardItem("鬼手", spritesMap.FindSprite("鬼手"), spritesMap.FindSprite("BackItem"), "对方需要收走现在的卡，随机换一张"));
        itemPool.Add(new EncourageItem("壮胆", spritesMap.FindSprite("壮胆"), spritesMap.FindSprite("BackItem"), "自己此局已出卡牌的吓人值+3"));
        itemPool.Add(new DivinationItem("占卜", spritesMap.FindSprite("占卜"), spritesMap.FindSprite("BackItem"), "查看对方全部的牌中其中三张卡牌的点数是多少"));
        itemPool.Add(new SwapCardPointsItem("交互", spritesMap.FindSprite("交互"), spritesMap.FindSprite("BackItem"), "交换双方的牌的吓人点数"));

        for (int i = 0; i < itemSlots.Length; i++)
        {
            player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
        for (int i = 0; i < itemSlots.Length; i++)
        {
            enemy.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
    }

    private void EnableButton(Button button, bool isEnabled, System.Action onClickCallback)
    {
        button.interactable = isEnabled;

        // ����ɵļ�����
        button.onClick.RemoveAllListeners();

        if (isEnabled)
        {
            button.onClick.AddListener(() => onClickCallback?.Invoke());
            button.image.color = Color.white; // ��ť����
        }
        else
        {
            button.image.color = Color.gray; // ��ť�䰵
        }
    }

    private void OnClickedConfirmButton()
    {
        switch (currentPhase)
        {
            case GamePhase.Cover:
                CoverPhaseConfirm();
                break;
            case GamePhase.Item:
                ItemPhaseConfirm();
                break;
            default:
                Debug.Log($"{currentPhase}�׶ε�� ȷ�� û������");
                break;
        }
    }

    private void OnClickedFinishButton()
    {
        switch (currentPhase)
        {
            case GamePhase.Cover:
                CoverPhaseRun();
                break;
            case GamePhase.Item:
                ItemPhaseRun();
                break;
            default:
                Debug.Log($"{currentPhase}�׶ε�� ��� û������");
                break;
        }
    }

    private bool isVideoPlayed;

    // 播放胜利视频
    private void PlayVictoryVideo()
    {
        PlayVideo(victoryClip);
        // 例如，结束视频后重新开始游戏或跳转到其他场景：
        //StartCoroutine(LoadYourAsyncScene());
    }

    //播放失败视频
    private void PlayDefeatVideo()
    {
        PlayVideo(defeatClip);
        // 例如，结束视频后重新开始游戏或跳转到其他场景：
        //StartCoroutine(LoadYourAsyncScene());
    }
    private void PlayStartVideo()
    {
        PlayVideo(startClip);
    }

    // 播放视频的通用方法
    private void PlayVideo(VideoClip clip)
    {
        isVideoPlayed = false;
        audioManager.bgm.Stop();
        // 设置 VideoPlayer 的视频资源
        videoPlayer.clip = clip;

        // 显示视频 RawImage
        rawImage.gameObject.SetActive(true);
        rawImage.transform.SetAsLastSibling();
        // 播放视频
        videoPlayer.Play();

        // 等待视频播放结束
        videoPlayer.loopPointReached += EndOfVideo; // 注册回调函数

        // 禁用其他 UI 控件，防止在播放视频时进行操作
        DisableUIElements();
    }

    // 视频播放结束后的回调函数
    private void EndOfVideo(VideoPlayer vp)
    {
        // 视频播放完成后，恢复 UI 控件
        EnableUIElements();
        rawImage.gameObject.SetActive(false);
        isVideoPlayed = true;

        SceneManager.LoadScene("StartScene");
        // 在播放完视频后，可以执行后续的逻辑，比如进入下一场景、重启游戏等
        Debug.Log("视频播放完毕");
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone && !isVideoPlayed)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    // 禁用 UI 元素
    private void DisableUIElements()
    {
        // 禁用游戏中的其他 UI 元素，防止操作
        // 比如禁用按钮，文本，等
        confirmBtn.interactable = false;
        finishBtn.interactable = false;
    }

    // 启用 UI 元素
    private void EnableUIElements()
    {
        // 恢复 UI 元素的交互
        confirmBtn.interactable = true;
        finishBtn.interactable = true;
    }
}
