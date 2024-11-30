using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    public Player girl = new Player();
    public Player monster = new Player();
    public TugOfWarUI tugUI;

    public List<FearCardSO> girlDecksSO;
    public List<FearCardSO> monsterDecksSO;
    public List<FearCard> girlDecks = new List<FearCard>();
    public List<FearCard> monsterDecks = new List<FearCard>();
    public List<GameItem> itemPool = new List<GameItem>();
    public List<Sprite> itemSprites;
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

    public Animator girlAnimator; 
    public Animator monsterAnimator;
    public AudioSource audioSource;  // AudioSource for playing sound effects
    public AudioClip dealCardSound; // Sound effect for dealing cards
    public AudioClip chooseCardSound; // Sound effect for choosing card
    public AudioClip chooseItemSound; // Sound effect for choosing item
    public AudioClip comparePointsSound; // Sound effect for comparing points

    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard monsterSelectedCard; // ���ѡ��Ŀ���
    private FearCard girlSelectedCard; // ����ѡ��Ŀ���

    //��ʼ��
    private void Awake()
    {
        cardPrefab = Resources.Load<GameObject>("CardTemplete");
        itemPrefab = Resources.Load<GameObject>("ItemTemplete");
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

        audioSource = GetComponent<AudioSource>();

        DOTween.Init();
        InitGame();


    }

    private void InitGame()
    {
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);
        currentRound = 1;

        // ��ʼ�����Ƴ�
        InitDecks(monster, girl, 1, 5);

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
        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";
        //����
        //���ĻغϿ�ʼ˫������2������
        if (currentRound == 3)
        {
            Debug.Log("双方获得道具");
            // ���ӵ����߼�������չ��
            for (int i = 0; i < 2; i++)
            {
                girl.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
            }
            for (int i = 0; i < 2; i++)
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
                // ���ŷ�����Ч
                //audioSource.PlayOneShot(dealCardSound);
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

        monsterPointText.text = $"{monsterSurplusPoint}";
        girlPointText.text = $"{girlSurplusPoint}";

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        StartCoroutine(PlaySelectedCardAnimation(girlSelectedCard));

        monsterSurplusPoint -= monsterSelectedCard.point;
        girlSurplusPoint -= girlSelectedCard.point;
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

        // ��ʾ���� UI
        StartCoroutine(PlayDealItemsAnimation());

    }

    private IEnumerator PlayDealItemsAnimation()
    {
        HidePlayerCardUI();
        HidePlayerItemUI();

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
            itemUI.transform.DOMove(cardSlots[i].position, 1f).SetEase(Ease.InOutQuad);
            yield return new WaitForSecondsRealtime(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
        }

        // ȷ�����ֹ��򣺼���ż���غϵ������֣������غ��������
        bool isPlayerTurn = currentRound % 2 != 0;

        EnableButton(finishBtn, true, OnClickedFinishButton);
        // ��ʼ����ʹ�ý׶�
        StartCoroutine(HandleItemPhase(isPlayerTurn));
    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        Debug.Log($"{(isPlayerTurn ? "玩家" : "敌人")}先手开始道具阶段");

        // �����ʹ�õ���
        while (isPlayerTurn)
        {
            yield return StartCoroutine(HandlePlayerItemUsage());
            if (monster.GetItems().Count == 0 || !finishBtn.interactable)// ���û�е��������
            {
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
                while (true)
                {
                    yield return StartCoroutine(HandlePlayerItemUsage());
                    if (monster.GetItems().Count == 0 || !finishBtn.interactable) break;// ���û�е��������
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
        EnableButton(confirmBtn, true, () =>
        {
            selectedItem = monsterItemArea.GetAreaItem(); // ��ȡ���ѡ��ĵ���
            if (selectedItem != null)
            {
                Debug.Log($"玩家使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard);
                monsterSelectedCard = cards[0];
                girlSelectedCard = cards[1];
                monster.RemoveItem(selectedItem);
                itemUsed = true;

                
            }
        });

        monsterItemArea.ItemUIFlipBack(true);
        yield return new WaitForSecondsRealtime(1.5f);
        monsterItemArea.ClearReadyToUseItem();
        //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
        //audioSource.PlayOneShot(chooseItemSound);

        monsterPointText.text = $"{monsterSurplusPoint - monsterSelectedCard.point}";
        girlPointText.text = $"{girlSurplusPoint - girlSelectedCard.point}";
        // �ȴ����ȷ��
        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
    }

    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("敌人的道具回合");

        List<GameItem> enemyItems = girl.GetItems();
        if (enemyItems.Count > 0)
        {
            GameItem selectedItem = null;

            // AIѡ����ߵ��߼�
            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"敌人使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(girl, monster, girlSelectedCard, monsterSelectedCard);
                girlSelectedCard = cards[0];
                monsterSelectedCard = cards[1];
                girl.RemoveItem(selectedItem);

                if(girlItemUI == null)
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
                girlItemUI.FlipBack(true);
                yield return new WaitForSecondsRealtime(1.5f);

                girlItemUI.gameObject.SetActive(false);
                //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
                //audioSource.PlayOneShot(chooseItemSound);

                monsterPointText.text = $"{monsterSurplusPoint}";
                girlPointText.text = $"{girlSurplusPoint}";
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
        yield return new WaitForSecondsRealtime(1f);
    }

    #region AI道具决策
    private GameItem ChooseEnemyItem(List<GameItem> enemyItems)
    {
        // ������˵ĵ��߲���1�����Ӻ���������
        GameItem itemToUse = null;

        GameItem peekItem = enemyItems.Find(item => item.itemName == "窥视"); // �ҵ����ӵ���
        if (peekItem != null)
        {
            // ʹ�ÿ��ӵ���
            itemToUse = peekItem;
            Debug.Log("敌人使用窥视道具");

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
            return girl.GetItems().Find(item => item.itemName == "偷换"); // �ҵ�͵������
        }
    }

    private GameItem ChooseRewindOrSkip()
    {
        // ���û�к��ʵĵ��ߣ���ѡ����������
        Debug.Log("敌人没有合适的道具, 选择道具(悔棋)");
        return girl.GetItems().Find(item => item.itemName == "悔棋"); // �ҵ��������
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
            List<FearCard> cards = selectedItem.Use(monster, girl, monsterSelectedCard, girlSelectedCard);
            monsterSelectedCard = cards[0];
            girlSelectedCard = cards[1];
            monster.RemoveItem(selectedItem);

            //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
            //audioSource.PlayOneShot(chooseItemSound);

            monsterPointText.text = $"{monsterSurplusPoint}";
            girlPointText.text = $"{girlSurplusPoint}";
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

    private void HandleItemClicked(GameItem item)
    {
        Debug.Log($"玩家点击使用道具：{item.itemName}");

        // ִ�е���Ч��
        List<FearCard> cards = item.Use(girl, monster, monsterSelectedCard, girlSelectedCard);
        monsterSelectedCard = cards[0];
        girlSelectedCard = cards[1];

        // �Ƴ���ҵ���
        girl.RemoveItem(item);

        // ���� UI
        RefreshPlayerItemUI(girl);
    }
    #endregion

    #region 结算阶段
    private void ResolvePhase()
    {
        Debug.Log("结算阶段开始");
        Debug.Log($"玩家选择的卡为({monsterSelectedCard.cardName})，点数为({monsterSelectedCard.point})");

        int playerPoint = monsterSelectedCard.point;
        int enemyPoint = girlSelectedCard.point;

        StartCoroutine(PlayResolveAnimation(playerPoint, enemyPoint));

        //playerValueTMP.text = $"玩家的恐惧值：{monster.GetFearValue()}";
        //monsterValueTMP.text = $"敌人的恐惧值：{girl.GetFearValue()}";


        monster.UseCard(monsterSelectedCard);
        girl.UseCard(girlSelectedCard);



        RefreshPlayerCardUI(monster);

        
    }

    private IEnumerator PlayResolveAnimation(int playerPoint, int enemyPoint)
    {

        monsterCardArea.CardUIFlipBack(false);
        girlCardUI.FlipBack(false);
        yield return new WaitForSecondsRealtime(1f);

        if (playerPoint > enemyPoint)
        {
            girl.IncreaseFearValue(1);
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            yield return new WaitForSecondsRealtime(0.5f);
            girlAnimator.SetTrigger($"TriggerHurt");

            // 播放比较点数音效
            audioSource.PlayOneShot(comparePointsSound);
            Debug.Log($"玩家获胜，敌人增加一点恐惧值 ({girl.GetFearValue()})");
        }
        else if (playerPoint < enemyPoint)
        {
            monster.IncreaseFearValue(1);
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            yield return new WaitForSecondsRealtime(0.5f);
            monsterAnimator.SetTrigger($"TriggerHurt");
            Debug.Log($"敌人获胜，玩家增加一点恐惧值 ({monster.GetFearValue()})");
        }
        else
        {
            monster.IncreaseFearValue(1);
            girl.IncreaseFearValue(1);

            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            girlAnimator.SetTrigger($"Trigger{girlSelectedCard.cardName}");
            yield return new WaitForSecondsRealtime(0.5f);
            monsterAnimator.SetTrigger($"TriggerHurt");
            girlAnimator.SetTrigger($"TriggerHurt");
            Debug.Log($"平局，双方都增加1恐惧值({monster.GetFearValue()}):({girl.GetFearValue()})");
        }
        yield return new WaitForSecondsRealtime(0.5f);

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
        // �ж��Ƿ���һ���־�ֵ�ﵽ3
        if (monster.GetFearValue() >= 3)
        {
            Debug.Log("玩家失败！");
            // 失败逻辑
        }
        else if (girl.GetFearValue() >= 3)
        {
            Debug.Log("敌人失败！");
            // 胜利逻辑
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

        while (points.Count < cardCount)
        {
            int point = Random.Range(0, 10);

            if (point > 0 && point <= maxPoint && totalPoints - point >= minPoint * (cardCount - points.Count - 1))
            {
                points.Add(point);
                totalPoints -= point;
            }
        }

        // ����˳��ʹÿ�η����˳�����
        for (int i = 0; i < points.Count; i++)
        {
            int swapIndex = Random.Range(0, points.Count);
            (points[i], points[swapIndex]) = (points[swapIndex], points[i]);
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
        itemPool.Add(new PeekItem(itemSprites[0], itemSprites[6]));
        itemPool.Add(new ChangeCardItem(itemSprites[1], itemSprites[6]));
        itemPool.Add(new TauntItem(itemSprites[2], itemSprites[6]));
        itemPool.Add(new EncourageItem(itemSprites[3], itemSprites[6]));
        itemPool.Add(new SwapCardPointsItem(itemSprites[4], itemSprites[6]));

        for (int i = 0; i < 5; i++)
        {
            player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
        for (int i = 0; i < 5; i++)
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

}
