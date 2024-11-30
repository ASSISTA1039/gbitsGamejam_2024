using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Player player = new Player();
    public Player monster = new Player();
    public TugOfWarUI tugUI;

    public List<FearCard> playDecks = new List<FearCard>();
    public List<FearCard> monsterDecks = new List<FearCard>();
    public List<GameItem> itemPool = new List<GameItem>();
    public Transform[] cardSlots;
    public Transform[] itemSlots;

    public GameObject cardPrefab;
    public GameObject itemPrefab;
    public CardTargetArea cardTargetArea;
    public Button confirmBtn;
    public Button finishBtn;

    public TextMeshProUGUI playerCardDisplay;
    public TextMeshProUGUI monsterCardDisplay;
    public TextMeshProUGUI roundDisplay;
    public TextMeshProUGUI playerValueTMP;
    public TextMeshProUGUI monsterValueTMP;

    public Animator playerAnimator; 
    public Animator monsterAnimator;
    public AudioSource audioSource;  // AudioSource for playing sound effects
    public AudioClip dealCardSound; // Sound effect for dealing cards
    public AudioClip chooseCardSound; // Sound effect for choosing card
    public AudioClip chooseItemSound; // Sound effect for choosing item
    public AudioClip comparePointsSound; // Sound effect for comparing points

    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard playerSelectedCard; // ���ѡ��Ŀ���
    private FearCard monsterSelectedCard; // ����ѡ��Ŀ���

    //��ʼ��
    private void Awake()
    {
        cardPrefab = Resources.Load<GameObject>("CardTemplete");
        itemPrefab = Resources.Load<GameObject>("ItemTemplete");
        audioSource = GetComponent<AudioSource>();

        DOTween.Init();
        InitGame();


    }

    private void InitGame()
    {
        currentRound = 1;

        // ��ʼ�����Ƴ�
        InitDecks(player, monster, 1, 5);

        // ��ʼ��˫������
        player.InitCards(playDecks);
        monster.InitCards(monsterDecks);
        //RefreshPlayerCardUI(player);

        // ��ʼ��˫������
        InitGameItems(player, monster);
        //RefreshPlayerItemUI(player, null);

        currentPhase = GamePhase.Start;
        RunPhase();
    }

    private void RunPhase()
    {
        roundDisplay.text = $"回合数：{currentRound}";
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
        playerCardDisplay.text = $"玩家的卡牌点数：无";
        monsterCardDisplay.text = $"敌人的卡牌点数：无";
        //����
        //���ĻغϿ�ʼ˫������2������
        if (currentRound == 3)
        {
            Debug.Log("双方获得道具");
            // ���ӵ����߼�������չ��
            for (int i = 0; i < 2; i++)
            {
                player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
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
        // ���ŷ��ƶ���
        //playerAnimator.SetTrigger("DealCards"); // Trigger "DealCards" animation
        //monsterAnimator.SetTrigger("DealCards");

        //���ص���UI��Ȼ����ʾ����UI
        HidePlayerCardUI();
        HidePlayerItemUI();

        List<FearCard> cards = player.GetCards();
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
                cardUI.gameObject.SetActive(true);
                cardUI.gameObject.transform.position = cardTargetArea.transform.position;
                cardUI.transform.DOMove(cardSlots[i].position, 1f).SetEase(Ease.InOutQuad);
                yield return new WaitForSeconds(1f); // �ӳ�һ�£�ȷ��ÿ�ſ����е�ʱ�们��
            }

        }

        //// ���ƶ�����ÿ�ſ��ƴ� cardTargetArea ��������Ӧ�� cardSlots
        //for (int i = 0; i < cardSlots.Length; i++)
        //{
        //    var card = player.GetCards()[i];
        //    var cardObj = Instantiate(cardPrefab, cardTargetArea.transform.position, Quaternion.identity);
        //    cardObj.GetComponent<FearCardUI>().SetUI(card, cardSlots[i]);
            
        //}

        // ���ŷ�����Ч
        //audioSource.PlayOneShot(dealCardSound);

        yield return new WaitForSeconds(2f); // �ȴ���������Ч�������

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
        if(cardTargetArea.GetAreaCard() == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后 确认！");
            return;
        }
        //���ѡ��
        playerSelectedCard = cardTargetArea.GetAreaCard();
        Debug.Log($"玩家选择了卡牌：{playerSelectedCard.cardName}");

        CardTun cardTun = cardTargetArea.GetComponentInChildren<CardTun>();
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
        //if(confirmBtn.interactable == true)
        //{
        //    CoverPhaseConfirm();
        //}

        if (playerSelectedCard == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后 结束！");
            return;
        }

        Debug.Log($"玩家选择了卡牌：{playerSelectedCard.cardName}");

        //����ѡ��
        List<FearCard> monsterCards = monster.GetCards();
        monsterSelectedCard = monsterCards[Random.Range(0, monsterCards.Count)];
        Debug.Log($"敌人选择了卡牌：{monsterSelectedCard.cardName}");

        playerCardDisplay.text = $"玩家的卡牌点数：{playerSelectedCard.point}";
        monsterCardDisplay.text = $"敌人的卡牌点数：{monsterSelectedCard.point}";

        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        // ������һ�׶�
        currentPhase = GamePhase.Item;

        RunPhase();
    }

    public FearCard GetPlayerSelectedCard()
    {
        return playerSelectedCard;
    }

    public FearCard GetMonsterSelectedCard()
    {
        return monsterSelectedCard;
    }

    #endregion

    #region 道具阶段
    private void ItemPhase()
    {
        Debug.Log("道具阶段开始");

        // ȷ�����ֹ��򣺼���ż���غϵ������֣������غ��������
        bool isPlayerTurn = currentRound % 2 != 0;

        EnableButton(finishBtn, true, OnClickedFinishButton);
        // ��ʼ����ʹ�ý׶�
        StartCoroutine(HandleItemPhase(isPlayerTurn));

        // ��ʾ���� UI
        RefreshPlayerItemUI(player);

    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        Debug.Log($"{(isPlayerTurn ? "玩家" : "敌人")}先手开始道具阶段");

        // �����ʹ�õ���
        while (isPlayerTurn)
        {
            yield return StartCoroutine(HandlePlayerItemUsage());
            if (player.GetItems().Count == 0 || !finishBtn.interactable)// ���û�е��������
            {
                while(true)
                {
                    yield return StartCoroutine(HandleEnemyItemUsage());
                    if (monster.GetItems().Count == 0 || StartCoroutine(HandleEnemyItemUsage()) == null) break; // ����û�е��������
                }
            }; 

        }

        // ����ʹ�õ���
        while (!isPlayerTurn)
        {
            yield return StartCoroutine(HandleEnemyItemUsage());
            if (monster.GetItems().Count == 0 || StartCoroutine(HandleEnemyItemUsage()) == null)// ����û�е��������
            {
                while (true)
                {
                    yield return StartCoroutine(HandlePlayerItemUsage());
                    if (player.GetItems().Count == 0 || !finishBtn.interactable) break;// ���û�е��������
                }
            }; 

        }

        // �����Һ͵��˶�ʹ������ߣ�������һ�׶�
        currentPhase = GamePhase.Resolve;
        RunPhase();
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
            selectedItem = cardTargetArea.GetAreaItem(); // ��ȡ���ѡ��ĵ���
            if (selectedItem != null)
            {
                Debug.Log($"玩家使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
                playerSelectedCard = cards[0];
                monsterSelectedCard = cards[1];
                player.RemoveItem(selectedItem);
                itemUsed = true;

                //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
                //audioSource.PlayOneShot(chooseItemSound);

                playerCardDisplay.text = $"玩家的卡牌点数：{playerSelectedCard.point}";
                monsterCardDisplay.text = $"敌人的卡牌点数：{monsterSelectedCard.point}";
            }
        });

        // �ȴ����ȷ��
        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
    }

    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("敌人的道具回合");

        List<GameItem> enemyItems = monster.GetItems();
        if (enemyItems.Count > 0)
        {
            GameItem selectedItem = null;

            // AIѡ����ߵ��߼�
            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"敌人使用道具：{selectedItem.itemName}");
                List<FearCard> cards = selectedItem.Use(monster, player, monsterSelectedCard, playerSelectedCard);
                monsterSelectedCard = cards[0];
                playerSelectedCard = cards[1];
                monster.RemoveItem(selectedItem);

                //monsterAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
                //audioSource.PlayOneShot(chooseItemSound);

                playerCardDisplay.text = $"玩家的卡牌点数：{playerSelectedCard.point}";
                monsterCardDisplay.text = $"敌人的卡牌点数：{monsterSelectedCard.point}";
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
        yield return new WaitForSeconds(1f);
    }

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
            int playerCardValue = playerSelectedCard.point; // �����л�ȡ��ҵ�ǰ���Ƶ����ĺ���
            int monsterCardValue = monsterSelectedCard.point; // �����л�ȡ���˵�ǰ���Ƶ����ĺ���

            if (playerCardValue > monsterCardValue)
            {
                // ���ݲ��ѡ���Ƿ���׳����͵����ǿ��
                return ChooseBoostOrSwap(playerCardValue, monsterCardValue);
            }
            else
            {
                // ���ݲ��ѡ�������߻�����
                return ChooseRewindOrSkip();
            }
        }

        return null;
    }

    private GameItem ChooseBoostOrSwap(int playerCardValue, int monsterCardValue)
    {
        // ѡ��׳����͵������
        if (Mathf.Abs(playerCardValue - monsterCardValue) <= 3)
        {
            return monster.GetItems().Find(item => item.itemName == "壮胆"); // �ҵ�׳������
        }
        else
        {
            return monster.GetItems().Find(item => item.itemName == "偷换"); // �ҵ�͵������
        }
    }

    private GameItem ChooseRewindOrSkip()
    {
        // ���û�к��ʵĵ��ߣ���ѡ����������
        Debug.Log("敌人没有合适的道具, 选择道具(悔棋)");
        return monster.GetItems().Find(item => item.itemName == "悔棋"); // �ҵ��������
    }

    private GameItem SelectOtherEnemyItems(List<GameItem> enemyItems)
    {
        // ѡ�������ǿ��ӵ��ߵĲ��ԣ��������ѡ��
        Debug.Log("敌人随机选择道具");
        return enemyItems[Random.Range(0, enemyItems.Count)];
    }

    public GameItem MonsterEvaluateItem(Monster monster)
    {
        List<GameItem> items = monster.GetItems();
        List<string> strings = new List<string>();
        GameItem evaluateItem;
        for (int i = 0; i < items.Count; i++)
        {
            strings.Add(items[i].itemName);
        }

        if (items.Contains(new PeekItem()))
        {
            evaluateItem = new PeekItem();

        }
        else if (items.Contains(new ForceChangeCardItem()))
        {
            evaluateItem = new ForceChangeCardItem();
        }
        else
        {
            evaluateItem = null;
        }

        return evaluateItem;
    }

    private void ItemPhaseConfirm()
    {
        //playerSelectedCard = cardTargetArea.GetAreaCard();
        EnableButton(confirmBtn, false, null);
    }

    private void ItemPhaseRun()
    {
        Debug.Log("玩家确认结束道具使用阶段");
        if (cardTargetArea.GetAreaItem() != null)
        {
            GameItem selectedItem = cardTargetArea.GetAreaItem();
            Debug.Log($"玩家使用道具：{selectedItem.itemName}");
            List<FearCard> cards = selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
            playerSelectedCard = cards[0];
            monsterSelectedCard = cards[1];
            player.RemoveItem(selectedItem);

            //playerAnimator.SetTrigger($"Trigger{selectedItem.itemName}");
            //audioSource.PlayOneShot(chooseItemSound);

            playerCardDisplay.text = $"玩家的卡牌点数：{playerSelectedCard.point}";
            monsterCardDisplay.text = $"敌人的卡牌点数：{monsterSelectedCard.point}";
        }

        // ����ȷ�ϰ�ť
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        // �������� UI
        RefreshPlayerItemUI(player, null);

        // ������һ�׶�
        currentPhase = GamePhase.Resolve;
        RunPhase();
    }

    private void HandleItemClicked(GameItem item)
    {
        Debug.Log($"玩家点击使用道具：{item.itemName}");

        // ִ�е���Ч��
        List<FearCard> cards = item.Use(player, monster, playerSelectedCard, monsterSelectedCard);
        playerSelectedCard = cards[0];
        monsterSelectedCard = cards[1];

        // �Ƴ���ҵ���
        player.RemoveItem(item);

        // ���� UI
        RefreshPlayerItemUI(player);
    }
    #endregion

    #region 结算阶段
    private void ResolvePhase()
    {
        Debug.Log("结算阶段开始");
        Debug.Log($"玩家选择的卡为({playerSelectedCard.cardName})，点数为({playerSelectedCard.point})");

        int playerPoint = playerSelectedCard.point;
        int monsterPoint = monsterSelectedCard.point;

        if (playerPoint > monsterPoint)
        {
            monster.IncreaseFearValue(1);
            playerAnimator.SetTrigger($"Trigger{playerSelectedCard.cardName}");
            monsterAnimator.SetTrigger($"TriggerHurt");

            // 播放比较点数音效
            audioSource.PlayOneShot(comparePointsSound);
            Debug.Log($"玩家获胜，敌人增加一点恐惧值 ({monster.GetFearValue()})");
        }
        else if (playerPoint < monsterPoint)
        {
            player.IncreaseFearValue(1);
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            playerAnimator.SetTrigger($"TriggerHurt");
            Debug.Log($"敌人获胜，玩家增加一点恐惧值 ({player.GetFearValue()})");
        }
        else
        {
            player.IncreaseFearValue(1);
            monster.IncreaseFearValue(1);

            playerAnimator.SetTrigger($"Trigger{playerSelectedCard.cardName}");
            monsterAnimator.SetTrigger($"Trigger{monsterSelectedCard.cardName}");
            playerAnimator.SetTrigger($"TriggerHurt");
            monsterAnimator.SetTrigger($"TriggerHurt");
            Debug.Log($"平局，双方都增加1恐惧值({player.GetFearValue()}):({monster.GetFearValue()})");
        }

        playerValueTMP.text = $"玩家的恐惧值：{player.GetFearValue()}";
        monsterValueTMP.text = $"敌人的恐惧值：{monster.GetFearValue()}";

        player.UseCard(playerSelectedCard);
        monster.UseCard(monsterSelectedCard);

        cardTargetArea.ClearReadyToUseCard();

        RefreshPlayerCardUI(player);

        // ������һ�׶�
        currentPhase = GamePhase.End;
        RunPhase();

        //��ȡ˫������
        //�ȵ㣬С��һ�����ӿ־�ֵ
        //��һ�׶�
    }

    private void EndPhase()
    {
        Debug.Log("回合结束阶段");
        tugUI.UpdateBars(4 + monster.GetFearValue(), 4 + player.GetFearValue());
        // �ж��Ƿ���һ���־�ֵ�ﵽ3
        if (player.GetFearValue() >= 3)
        {
            Debug.Log("玩家失败！");
            // 失败逻辑
        }
        else if (monster.GetFearValue() >= 3)
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
            // ʹ��һ���򵥵ļ�Ȩ�߼������м���ʸ��ߵ������
            float randomValue = Random.Range(0f, 1f); // ����һ��[0, 1]�ĸ�����

            // (��ʼֵa����ֵֹb����ֵ����t) ����a = 0, b = 1, t = 0, �򷵻�ֵΪ0
            int point = Mathf.RoundToInt(Mathf.Lerp(minPoint, maxPoint, randomValue * randomValue)); // ƽ��Ȩ��ʹ�м����

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


    public void InitDecks(Player player, Player monster,int startPoint, int endPoint)
    {
        List<int> points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"FireCard";
            playDecks.Add(new FearCard(cardName, points[i]));
        }
        player.ResetFearValue();

        points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"GhostCard";
            monsterDecks.Add(new FearCard(cardName, points[i]));
            Debug.Log($"敌人卡{i}的点数：{points[i]}");
        }
        monster.ResetFearValue();
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
            itemUI.SetUI(item, itemSlots[i], HandleItemClicked);

        }
    }

    public void RefreshPlayerItemUI(Player player, System.Action<GameItem> callback)
    {
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
            itemUI.SetUI(item, itemSlots[i], callback);

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

    public void InitGameItems(Player player, Player monster)
    {
        itemPool.Add(new PeekItem());
        itemPool.Add(new ChangeCardItem());
        itemPool.Add(new TauntItem());
        itemPool.Add(new EncourageItem());
        itemPool.Add(new SwapCardPointsItem());

        for (int i = 0; i < 5; i++)
        {
            player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
        }
        for (int i = 0; i < 5; i++)
        {
            monster.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
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
