using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("梦兽牌库管理")]
    public List<FearCardSO> monsterDecksSO;
    public List<FearCard> monsterDecks = new List<FearCard>();//梦兽牌库
    [Header("女孩牌库管理")]
    public List<FearCardSO> girlDecksSO;
    public List<FearCard> girlDecks = new List<FearCard>();//女孩牌库

    public List<GameItem> itemPool = new List<GameItem>();//道具池

    [Header("界面UI管理")]
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
    public FearCardUI girlCardUI;//女孩展示的卡牌
    public GameItemUI girlItemUI;//女孩展示的道具
    public Button confirmBtn;
    public Button finishBtn;
    public TugOfWarUI tugUI;//背景UI移动
    private bool isDealCards;//是否处于发牌中
    private bool isDealItems;//是否处于发道具中
    private bool isGirlPeek;//女孩是否窥视过
    [Header("文本UI管理")]
    public TextMeshProUGUI monsterPointText;
    private int monsterSurplusPoint = 25;
    private int monsterCurrentPoint = 0;
    public TextMeshProUGUI girlPointText;
    private int girlSurplusPoint = 25;
    private int girlCurrentPoint = 0;
    public TextMeshProUGUI monsterValueText;
    public TextMeshProUGUI girlValueText;

    public CardTun roundDisplay;
    public Sprite[] roundSprites;
    public TextMeshProUGUI roundText;

    public TextMeshProUGUI contentDisplay;
    private bool isInfoDisplaying;//控制文字框显示

    public Animator girlAnimator; 
    public Animator monsterAnimator;
    [Header("素材管理")]
    public SpriteManager spritesMap;
    public AudioManager audioManager;

    //游戏阶段控制
    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard monsterSelectedCard; //梦兽选择的卡牌数据
    private FearCard girlSelectedCard; //女孩选择的卡牌数据

    //是否使用完毕道具
    private bool isMonsterUsedItem = false;
    private bool isGirlUsedItem = false;
    [Header("视频管理")]
    public VideoPlayer videoPlayer; // 用于播放视频
    public RawImage rawImage; // 显示视频的RawImage
    public VideoClip victoryClip;    // 胜利视频
    public VideoClip defeatClip;     // 失败视频
    public VideoClip startClip;     // 开场视频


    //��ʼ��
    private void Awake()
    {
        monsterCardArea = transform.Find("MonsterCardArea").GetComponent<CardTargetArea>();
        monsterItemArea = transform.Find("MonsterItemArea").GetComponent<ItemTargetArea>();
        monsterTransform = transform.Find("Panel/Monster").GetComponent<Transform>();
        girlTransform = transform.Find("Panel/Girl").GetComponent<Transform>();
        girlCardArea = transform.Find("GirlCardArea").GetComponent<Transform>();
        girlItemArea = transform.Find("GirlItemArea").GetComponent<Transform>();
        girlAnimator = girlTransform.gameObject.GetComponent<Animator>();

        monsterPointText = transform.Find("MonsterSurplus/Point").GetComponent<TextMeshProUGUI>();
        girlPointText = transform.Find("GirlSurplus/Point").GetComponent<TextMeshProUGUI>();
        monsterValueText = transform.Find("MonsterValue/Text").GetComponent<TextMeshProUGUI>();
        girlValueText = transform.Find("GirlValue/Text").GetComponent<TextMeshProUGUI>();

        roundDisplay = transform.Find("Round").GetComponent<CardTun>();
        roundText = transform.Find("Round/Num").GetComponent<TextMeshProUGUI>();

        contentDisplay = transform.Find("DialogPanel/Text").GetComponent<TextMeshProUGUI>();

        spritesMap = transform.Find("SpriteManager").GetComponent<SpriteManager>();
        audioManager = transform.Find("AudioManager").GetComponent<AudioManager>();

        DOTween.Init();
        InitGame();

    }

    private void InitGame()
    {
        //素材初始化，一次性配置后使用map根据string映射素材
        spritesMap.InitMap();
        audioManager.InitMap();

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);
        currentRound = 1;

        //初始化双方牌库，点数分布为1～9
        InitDecks(monster, girl, 1, 9);

        //初始化授牌
        monster.InitCards(monsterDecks);
        girl.InitCards(girlDecks);

        //初始化道具
        InitGameItems(monster, girl);

        //下一阶段
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
        contentDisplay.text = $"回合{currentRound} 开始";
        
        roundText.text = $"当前回合：{currentRound}";
        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";
        monsterValueText.text = $"你的恐惧值：{monster.GetFearValue()}";
        girlValueText.text = $"对方恐惧值：{girl.GetFearValue()}";

        isGirlPeek = false;

        if (currentRound == 3)
        {
            Debug.Log("双方获得道具");
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
    }

    private IEnumerator PlayDealCardsAnimation()
    {
        StartCoroutine(StartInfoDisplay(contentDisplay.text));
        yield return new WaitUntil(() => isInfoDisplaying == false);

        isDealCards = true;

        List<FearCard> cards = monster.GetCards();
        if (cards.Count == 0)
        {
            yield return null;
        }

        for (int i = 0; i < cards.Count; ++i)
        {
            FearCard card = cards[i];

            FearCardUI cardUI = null;
            if (cardSlots[i].childCount == 0)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{card.cardName}");
                cardUI = Instantiate(cardPrefab).GetComponent<FearCardUI>();
                cardUI.transform.SetParent(cardSlots[i], false );
            }
            else
            {
                cardUI = cardSlots[i].GetChild(0).GetComponent<FearCardUI>();
            }

            cardUI.SetUI(card, cardSlots[i]);

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
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        yield return new WaitForSecondsRealtime(1f);

        isDealCards = false;

        yield return new WaitUntil(() => isDealCards == false);
        currentPhase = GamePhase.Cover;
        RunPhase();
    }
    #endregion

    #region 盖牌阶段
    private void CoverPhase()
    {
        Debug.Log("盖牌阶段开始");
        //monsterCardArea.GetComponent<BoxCollider2D>().enabled = true;
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
        //monsterCardArea.GetComponent<BoxCollider2D>().enabled = false;
        monsterCardArea.transform.GetChild(1).GetComponent<FearCardUI>().enabled = false;

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
        //monsterCardArea.GetComponent<BoxCollider2D>().enabled = true;

        if (monsterSelectedCard == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后 结束！");
            return;
        }

        Debug.Log($"玩家选择了卡牌：{monsterSelectedCard.cardName}");

        //女孩随机出牌
        List<FearCard> girlCards = girl.GetCards();
        girlSelectedCard = girlCards[Random.Range(0, girlCards.Count)];
        Debug.Log($"敌人选择了卡牌：{girlSelectedCard.cardName}");

        monsterCurrentPoint = monsterSelectedCard.point;
        girlCurrentPoint = girlSelectedCard.point;

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        StartCoroutine(PlayGirlSelectedCardAnimation(girlSelectedCard));

    }

    private IEnumerator PlayGirlSelectedCardAnimation(FearCard enemyCard)
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

    #endregion

    #region 道具阶段
    private void ItemPhase()
    {
        Debug.Log("道具阶段开始");

        isMonsterUsedItem = false;
        isGirlUsedItem = false;
        //发道具动画
        //StartCoroutine(PlayDealItemsAnimation());
        // ȷ�����ֹ��򣺼���ż���غϵ������֣������غ��������
        bool isPlayerTurn = currentRound % 2 != 0;


        // ��ʼ����ʹ�ý׶�
        StartCoroutine(HandleItemPhase(isPlayerTurn));

    }

    private IEnumerator PlayDealItemsAnimation()
    {
        isDealItems = true;

        List<GameItem> items = monster.GetItems();
        if (items.Count == 0)
        {
            yield return null;
        }

        for (int i = 0; i < items.Count; ++i)
        {
            GameItem item = items[i];

            GameItemUI itemUI = null;
            if (itemSlots[i].childCount == 0)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
            }
            else
            {
                itemUI = itemSlots[i].GetChild(0).GetComponent<GameItemUI>();
            }
            // ��ȡ��ǰ�����е� GameIteUI�����û�У�ʵ����һ����

            itemUI.gameObject.SetActive(true);
            // ���õ���UI����
            itemUI.gameObject.transform.localPosition = Vector3.zero;
            itemUI.SetUI(item, itemSlots[i]);
            itemUI.SetTooltipText(item.itemName, item.description);
            itemUI.gameObject.transform.position = monsterTransform.transform.position;
            itemUI.transform.DOMove(itemSlots[i].position, 1f).SetEase(Ease.InOutQuad);
            yield return new WaitForSecondsRealtime(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
        }

        isDealItems = false;
        yield return null;
    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        DisplayPlayerCardUI(false);
        contentDisplay.text = $"{(isPlayerTurn ? $"{monster.name}" : $"{girl.name}")}先手开始道具阶段";
        StartCoroutine(StartInfoDisplay(contentDisplay.text));
        yield return new WaitUntil(() => isInfoDisplaying == false);

        if (isPlayerTurn)
        {
            StartCoroutine(PlayDealItemsAnimation());
            yield return new WaitUntil(() => isDealItems == false);
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
                    if (girl.GetItems().Count == 0 || isGirlUsedItem) break; // ����û�е��������
                }
            }; 

        }

        // ����ʹ�õ���
        while (!isPlayerTurn)
        {
            yield return StartCoroutine(HandleEnemyItemUsage());
            if (girl.GetItems().Count == 0 || isGirlUsedItem)// ����û�е��������
            {
                StartCoroutine(PlayDealItemsAnimation());
                yield return new WaitUntil(() => isDealItems == false);

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
                List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard, contentDisplay);
                monsterSelectedCard = cards[0];
                girlSelectedCard = cards[1];
                contentDisplay.text = selectedItem.displayString;
                if(selectedItem.itemName == "鬼手" || selectedItem.itemName == "抓娃娃爪子")
                {
                    monsterCurrentPoint = monsterSelectedCard.point;
                    girlCurrentPoint = girlSelectedCard.point;
                }

                monster.RemoveItem(selectedItem);
                itemUsed = true;

            }
        });

        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        monsterItemArea.ItemUIFlipBack(true);
        monsterItemArea.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSecondsRealtime(1.5f);
        monsterItemArea.ClearReadyToUseItem();
        audioManager.PlayItemSound(selectedItem.itemName);
        monsterItemArea.GetComponent<CircleCollider2D>().enabled = true;

        StartCoroutine(StartInfoDisplay(contentDisplay.text));
        yield return new WaitUntil(() => isInfoDisplaying == false);

        DisplayPlayerItemUI(true);
    }

    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("敌人的道具回合");
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        List<GameItem> enemyItems = girl.GetItems();
        if (enemyItems.Count > 0)
        {
            //contentDisplay.text = "对方思考中...";
            //StartCoroutine(StartInfoDisplay(contentDisplay.text));
            //yield return new WaitUntil(() => isInfoDisplaying == false);

            GameItem selectedItem = null;

            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"敌人使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(girl, monster, girlSelectedCard, monsterSelectedCard, contentDisplay);
                girlSelectedCard = cards[0];
                monsterSelectedCard = cards[1];
                contentDisplay.text = selectedItem.displayString;
                if (selectedItem.itemName == "鬼手" || selectedItem.itemName == "抓娃娃爪子")
                {
                    monsterCurrentPoint = monsterSelectedCard.point;
                    girlCurrentPoint = girlSelectedCard.point;
                }
                girl.RemoveItem(selectedItem);

                if (girlItemUI == null)
                {
                    girlItemUI = Instantiate(itemPrefab).GetComponent<GameItemUI>();
                }
                girlItemUI.gameObject.SetActive(true);
                girlItemUI.cardTun.mCardState = CardState.Back;
                girlItemUI.cardTun.Init();
                girlItemUI.SetUI(selectedItem, girlItemArea);
                girlItemUI.transform.SetParent(girlItemArea, false);
                girlItemUI.gameObject.transform.position = girlTransform.transform.position;
                girlItemUI.transform.DOMove(girlItemArea.position, 1f).SetEase(Ease.InOutQuad);
                yield return new WaitForSecondsRealtime(1.5f);
                girlItemUI.FlipBack(false);
                yield return new WaitForSecondsRealtime(1.5f);

                girlItemUI.gameObject.SetActive(false);
                audioManager.PlayItemSound(selectedItem.itemName);


                StartCoroutine(StartInfoDisplay(contentDisplay.text));
                yield return new WaitUntil(() => isInfoDisplaying == false);
            }
            else
            {
                Debug.Log("敌人没有合适的道具");
                isGirlUsedItem = true;
                yield return null;
            }
        }
        else
        {
            Debug.Log("敌人没有可用的道具");
            isGirlUsedItem = true;
            yield return null;
        }

    }

    #region AI道具决策
    private GameItem ChooseEnemyItem(List<GameItem> enemyItems)
    {
        GameItem itemToUse = null;

        if(isGirlPeek)
        {
            GameItem bestCounterItem = EvaluatePeekStrategy();
            if (bestCounterItem != null)
            {
                itemToUse = bestCounterItem;
            }
            return itemToUse;
        }

        GameItem peekItem = enemyItems.Find(item => item.itemName == "侦探眼睛"); // �ҵ����ӵ���
        if (peekItem != null && !isGirlPeek)
        {
            itemToUse = peekItem;
            Debug.Log("敌人使用侦探眼睛道具");
            isGirlPeek = true;
            return itemToUse;
        }
        else
        {
            //itemToUse = SelectOtherEnemyItems(enemyItems);
            itemToUse = null;
        }

        return null;
    }

    private GameItem EvaluatePeekStrategy()
    {
        int playerCardValue = monsterSelectedCard.point;
        int enemyCardValue = girlSelectedCard.point;

        if (playerCardValue > enemyCardValue)
        {
            return ChooseBoostOrSwap(playerCardValue, enemyCardValue);
        }
        else
        {
            return ChooseRewindOrSkip();
        }
    }

    private GameItem ChooseBoostOrSwap(int playerCardValue, int enemyCardValue)
    {
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
        Debug.Log("敌人没有合适的道具, 选择道具(鬼手)");
        return girl.GetItems().Find(item => item.itemName == "鬼手"); // �ҵ��������
    }

    private GameItem SelectOtherEnemyItems(List<GameItem> enemyItems)
    {
        Debug.Log("敌人随机选择道具");
        return enemyItems[Random.Range(0, enemyItems.Count)];
    }
    #endregion

    private void ItemPhaseConfirm()
    {
        EnableButton(confirmBtn, false, null);
    }

    private void ItemPhaseRun()
    {
        Debug.Log("玩家确认结束道具使用阶段");
        if (monsterItemArea.GetAreaItem() != null)
        {
            GameItem selectedItem = monsterItemArea.GetAreaItem();
            Debug.Log($"玩家使用道具：{selectedItem.itemName}");
            List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard, contentDisplay);
            monsterSelectedCard = cards[0];
            girlSelectedCard = cards[1];
            monster.RemoveItem(selectedItem);

            audioManager.PlayItemSound(selectedItem.itemName);

        }

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        currentPhase = GamePhase.Resolve;
        RunPhase();
    }
    #endregion

    #region 结算阶段
    private void ResolvePhase()
    {
        Debug.Log("结算阶段开始");
        Debug.Log($"玩家选择的卡为({monsterSelectedCard.cardName})，点数为({monsterSelectedCard.point})");
        //monsterCardArea.GetComponent<BoxCollider2D>().enabled = true;
        monsterCardArea.transform.GetChild(1).GetComponent<FearCardUI>().enabled = true;

        int playerPoint = monsterSelectedCard.point;
        int enemyPoint = girlSelectedCard.point;
        //卡牌数据受道具效果影响，更新出牌区卡牌点数和卡面
        monsterCardArea.UpdatePoint(playerPoint);
        girlCardUI.UpdatePoint(enemyPoint);
        monsterCardArea.UpdateSprite(monsterSelectedCard);
        girlCardUI.UpdateSprite(girlSelectedCard.background, girlSelectedCard.artSprite);

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

            //1.信息显示
            contentDisplay.text = $"{monster.name}获胜，{girl.name}增加一点恐惧值 ({girl.GetFearValue()})";
            StartCoroutine(StartInfoDisplay(contentDisplay.text));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //2.胜方文案
            StartCoroutine(StartInfoDisplay($"{monster.name}: {monsterSelectedCard.victoryDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //3.胜方动画
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitUntil(() => monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

            //4.败方文案
            StartCoroutine(StartInfoDisplay($"{girl.name}: {monsterSelectedCard.defeatDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //5.败方受击
            audioManager.PlayCardSound(monsterSelectedCard.cardName);
            girlAnimator.SetTrigger($"TriggerHurt");

        }
        else if (playerPoint < enemyPoint)
        {
            monster.IncreaseFearValue(1);

            //1.信息显示
            contentDisplay.text = $"{girl.name}获胜，{monster.name}增加一点恐惧值 ({monster.GetFearValue()})";
            StartCoroutine(StartInfoDisplay(contentDisplay.text));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //2.胜方文案
            StartCoroutine(StartInfoDisplay($"{girl.name}: {girlSelectedCard.victoryDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //3.胜方动画
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitUntil(() => girlAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            audioManager.PlayCardSound(girlSelectedCard.cardName);

            //4.败方文案
            StartCoroutine(StartInfoDisplay($"{monster.name}: {girlSelectedCard.defeatDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //5.败方受击
            audioManager.PlayCardSound(girlSelectedCard.cardName);
            monsterAnimator.SetTrigger($"TriggerHurt");

        }
        else
        {
            monster.IncreaseFearValue(1);
            girl.IncreaseFearValue(1);

            //1.信息显示
            contentDisplay.text = $"平局，双方都增加1恐惧值({monster.GetFearValue()}):({girl.GetFearValue()})";
            StartCoroutine(StartInfoDisplay(contentDisplay.text));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //2.平局文案
            StartCoroutine(StartInfoDisplay($"{monster.name}: {monsterSelectedCard.tieDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);
            StartCoroutine(StartInfoDisplay($"{girl.name}: {girlSelectedCard.tieDescription}"));
            yield return new WaitUntil(() => isInfoDisplaying == false);

            //3.玩家动画
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitForSecondsRealtime(2f);

            //4.败方受击
            girlAnimator.SetTrigger($"TriggerHurt");
            audioManager.PlayCardSound(girlSelectedCard.cardName);
            // 等待动画结束
            yield return new WaitForSecondsRealtime(2f);

            //5.败方动画
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            // 等待动画结束
            yield return new WaitForSecondsRealtime(2f);

            //6.玩家受击
            monsterAnimator.SetTrigger($"TriggerHurt");
            audioManager.PlayCardSound(monsterSelectedCard.cardName);
            // 等待动画结束
            yield return new WaitForSecondsRealtime(2f);

        }

        yield return new WaitForSecondsRealtime(2f);

        //清空出牌区
        monsterCardArea.ClearReadyToUseCard();
        for (int i = 1; i < girlCardArea.childCount; i++)
        {
            Destroy(girlCardArea.GetChild(i).gameObject);
        }

        currentPhase = GamePhase.End;
        RunPhase();
    }
    #endregion

    #region 回合结束
    private void EndPhase()
    {
        Debug.Log("回合结束阶段");
        //背景移动
        tugUI.UpdateBars(monster.GetFearValue() - girl.GetFearValue());

        //更新界面文本
        monsterSurplusPoint -= monsterCurrentPoint;
        girlSurplusPoint -= girlCurrentPoint;
        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";
        monsterValueText.text = $"你的恐惧值：{monster.GetFearValue()}";
        girlValueText.text = $"对方恐惧值：{girl.GetFearValue()}";

        //清空卡牌UI和道具UI，性能可优化
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].childCount == 0)
            {
                continue;
            }
            Destroy(cardSlots[i].GetChild(0).gameObject);
        }
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].childCount == 0)
            {
                continue;
            }
            Destroy(itemSlots[i].GetChild(0).gameObject);
        }

        //检测恐惧值是否足够结束游戏
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
        else if(monster.GetCards().Count == 0 && girl.GetCards().Count == 0)
        {
            if (monster.GetFearValue() > girl.GetFearValue())
            {
                Debug.Log("玩家失败！");
                // 失败逻辑
                PlayDefeatVideo();
            }
            else if (girl.GetFearValue() > monster.GetFearValue())
            {
                Debug.Log("敌人失败！");
                // 胜利逻辑
                PlayVictoryVideo();
            }
            else
            {
                StartCoroutine(StartInfoDisplay("恐惧值相同，谁也没有吓到谁，请重来一次吧~"));
                SceneManager.LoadScene("StartScene");
            }
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

    /// <summary>
    /// 生成player和enemy的牌库，牌库中最小的点数startPoint，最大的点数endPoint
    /// </summary>
    /// <param name="player">玩家</param>
    /// <param name="enemy">敌人</param>
    /// <param name="startPoint">最小点数</param>
    /// <param name="endPoint">最大点数</param>
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
        return new FearCard(cardData.cardName, cardData.background, cardData.artSprite, cardData.back, fearValue, cardData.victoryDescription, cardData.defeatDescription, cardData.tieDescription);
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

    /// <summary>
    /// 根据isDisplay控制卡牌UI的显示和关闭
    /// </summary>
    /// <param name="isDisplay"></param>
    public void DisplayPlayerCardUI(bool isDisplay)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].childCount == 0)
            {
                continue;
            }
            cardSlots[i].GetChild(0).gameObject.SetActive(isDisplay);
        }

    }

    /// <summary>
    /// 根据isDisplay控制道具UI的显示和关闭
    /// </summary>
    /// <param name="isDisplay"></param>
    public void DisplayPlayerItemUI(bool isDisplay)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].childCount == 0)
            {
                continue;
            }
            //Destroy(itemSlots[i].GetChild(0).gameObject);
            itemSlots[i].GetChild(0).gameObject.SetActive(isDisplay);
        }
    }

    private IEnumerator StartInfoDisplay(string content)
    {
        isInfoDisplaying = true;
        DisplayPlayerCardUI(false);
        DisplayPlayerItemUI(false);
        contentDisplay.transform.parent.gameObject.SetActive(true);

        //设置文本
        contentDisplay.text = content;

        //（可选）打字机效果
        //等待
        yield return new WaitForSecondsRealtime(2f);

        contentDisplay.transform.parent.gameObject.SetActive(false);
        //结束标志
        isInfoDisplaying = false;
    }

    public void InitGameItems(Player player, Player enemy)
    {
        itemPool.Add(new PeekItem("侦探眼睛", spritesMap.FindSprite("侦探眼睛"), spritesMap.FindSprite("BackItem"), "偷看对方当前的覆盖的牌的吓人点数是多少"));
        itemPool.Add(new ChangeCardItem("抓娃娃爪子", spritesMap.FindSprite("抓娃娃爪子"), spritesMap.FindSprite("BackItem"), "原先的卡片收进，随机换一张新卡出"));
        itemPool.Add(new ForceChangeCardItem("鬼手", spritesMap.FindSprite("鬼手"), spritesMap.FindSprite("BackItem"), "对方需要收走现在的卡，随机换一张"));
        itemPool.Add(new EncourageItem("壮胆", spritesMap.FindSprite("壮胆"), spritesMap.FindSprite("BackItem"), "自己此局已出卡牌的吓人值+3"));
        itemPool.Add(new DivinationItem("占卜", spritesMap.FindSprite("占卜"), spritesMap.FindSprite("BackItem"), "查看对方全部的牌中其中三张卡牌的点数是多少"));
        itemPool.Add(new SwapCardPointsItem("交互", spritesMap.FindSprite("交互"), spritesMap.FindSprite("BackItem"), "交换双方的牌的吓人点数"));

        for (int i = 0; i < 3; i++)
        {
            player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
        for (int i = 0; i < 3; i++)
        {
            enemy.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
    }

    /// <summary>
    /// 根据isEnabled控制button的启用和关闭，并分配点击事件onClickCallback
    /// </summary>
    /// <param name="button">按钮组件</param>
    /// <param name="isEnabled">是否启用按钮</param>
    /// <param name="onClickCallback">按钮点击事件</param>
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


    #region 视频播放
    private bool isVideoPlayed;

    // 播放胜利视频
    private void PlayVictoryVideo()
    {
        PlayVideo(victoryClip);
    }

    //播放失败视频
    private void PlayDefeatVideo()
    {
        PlayVideo(defeatClip);
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
    #endregion
}
