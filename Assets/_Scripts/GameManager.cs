using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Player player = new Player();
    public Monster monster = new Monster();

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

    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard playerSelectedCard; // 玩家选择的卡牌
    private FearCard monsterSelectedCard; // 敌人选择的卡牌

    //初始化
    private void Awake()
    {
        InitGame();
    }

    private void InitGame()
    {
        currentRound = 1;

        // 初始化卡牌池
        InitDecks(player, monster, 1, 5);

        // 初始化双方手牌
        player.InitCards(playDecks);
        monster.InitCards(monsterDecks);
        RefreshPlayerCardUI(player);

        // 初始化双方道具
        InitGameItems(player, monster);
        //RefreshPlayerItemUI(player, null);

        currentPhase = GamePhase.Start;
        RunPhase();
    }

    private void RunPhase()
    {
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

    #region 回合开始
    private void StartRound()
    {
        Debug.Log($"回合 {currentRound} 开始");

        //发牌
        //第四回合开始双方各发2个道具
        if (currentRound == 3)
        {
            Debug.Log("双方获得道具");
            // 增加道具逻辑（待扩展）
            for (int i = 0; i < 2; i++)
            {
                player.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
            }
            for (int i = 0; i < 2; i++)
            {
                monster.AddItem(itemPool[Random.Range(0, itemPool.Count)]);
            }
        }
        //下一阶段
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
        //玩家选卡
        playerSelectedCard = cardTargetArea.GetAreaCard();
        
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
            Debug.Log("未选择卡牌，请选择卡牌后确认！");
            return;
        }

        Debug.Log($"玩家选择了卡牌：{playerSelectedCard.cardName}");

        //敌人选卡
        List<FearCard> monsterCards = monster.GetCards();
        monsterSelectedCard = monsterCards[Random.Range(0, monsterCards.Count)];
        Debug.Log($"敌人选择了卡牌：{monsterSelectedCard.cardName}");


        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        // 进入下一阶段
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

        // 确定先手规则：假设偶数回合敌人先手，奇数回合玩家先手
        bool isPlayerTurn = currentRound % 2 != 0;

        EnableButton(finishBtn, true, OnClickedFinishButton);
        // 开始道具使用阶段
        StartCoroutine(HandleItemPhase(isPlayerTurn));

        // 显示道具 UI
        RefreshPlayerItemUI(player);

        //if(isPlayerTurn)
        //{
        //    // 启用确认按钮
        //    EnableConfirmButton(confirmBtn, true, OnClickedConfirmButton);
        //    EnableConfirmButton(finishBtn, true, OnClickedFinishButton);

        //    while (finishBtn.interactable) continue;

        //    mon.IntoItemPhase();
        //}
        //else
        //{
        //    mon.IntoItemPhase();

        //    // 启用确认按钮
        //    EnableConfirmButton(confirmBtn, true, OnClickedConfirmButton);
        //    EnableConfirmButton(finishBtn, true, OnClickedFinishButton);
        //}

    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        Debug.Log($"{(isPlayerTurn ? "玩家" : "敌人")}先手开始道具阶段");
        while (true)
        {
            if (isPlayerTurn)
            {
                yield return StartCoroutine(HandlePlayerItemUsage());
                if (player.GetCards().Count == 0) break; // 玩家没有道具则结束
            }
            else
            {
                yield return StartCoroutine(HandleEnemyItemUsage());
                if (monster.GetCards().Count == 0) break; // 敌人没有道具则结束
            }

        }
    }

    private IEnumerator HandlePlayerItemUsage()
    {
        Debug.Log("玩家的道具回合");

        // 等待玩家选择道具
        bool itemUsed = false;
        GameItem selectedItem = null;
        // 启用确认按钮
        EnableButton(confirmBtn, true, () =>
        {
            selectedItem = cardTargetArea.GetAreaItem(); // 获取玩家选择的道具
            if (selectedItem != null)
            {
                Debug.Log($"玩家使用道具：{selectedItem.itemName}");
                selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
                player.RemoveItem(selectedItem);
                itemUsed = true;
            }
        });

        // 等待玩家确认
        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
    }

    //private IEnumerator HandleEnemyItemUsage()
    //{
    //    Debug.Log("敌人的道具回合");

    //    List<GameItem> enemyItems = monster.GetItems();
    //    if (enemyItems.Count > 0)
    //    {
    //        GameItem selectedItem = enemyItems[Random.Range(0, enemyItems.Count)];
    //        Debug.Log($"敌人使用道具：{selectedItem.itemName}");
    //        selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
    //        monster.RemoveItem(selectedItem);
    //    }
    //    else
    //    {
    //        Debug.Log("敌人没有可用的道具");
    //    }

    //    // 模拟敌人操作的延迟
    //    yield return new WaitForSeconds(1f);
    //}
    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("敌人的道具回合");

        List<GameItem> enemyItems = monster.GetItems();
        if (enemyItems.Count > 0)
        {
            GameItem selectedItem = null;

            // AI选择道具的逻辑
            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"敌人使用道具：{selectedItem.itemName}");
                selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
                monster.RemoveItem(selectedItem);
            }
            else
            {
                Debug.Log("敌人没有合适的道具");
            }
        }
        else
        {
            Debug.Log("敌人没有可用的道具");
        }

        // 模拟敌人操作的延迟
        yield return new WaitForSeconds(1f);
    }

    private GameItem ChooseEnemyItem(List<GameItem> enemyItems)
    {
        // 假设敌人的道具策略1：窥视后作出决策
        GameItem itemToUse = null;

        GameItem peekItem = enemyItems.Find(item => item.itemName == "窥视"); // 找到窥视道具
        if (peekItem != null)
        {
            // 使用窥视道具
            itemToUse = peekItem;
            Debug.Log("敌人使用窥视道具");

            // 依据敌人的策略决定使用何种道具
            GameItem bestCounterItem = EvaluatePeekStrategy(peekItem);
            if (bestCounterItem != null)
            {
                itemToUse = bestCounterItem;
            }
        }
        else
        {
            // 如果没有窥视道具，按照其他策略选择道具
            itemToUse = SelectOtherEnemyItems(enemyItems);
        }

        return itemToUse;
    }

    private GameItem EvaluatePeekStrategy(GameItem peekItem)
    {
        // 基于窥视结果，敌人选择最合适的道具
        if (peekItem != null)
        {
            // 根据对方卡牌点数来选择合适的道具策略
            int playerCardValue = playerSelectedCard.point; // 假设有获取玩家当前卡牌点数的函数
            int monsterCardValue = monsterSelectedCard.point; // 假设有获取敌人当前卡牌点数的函数

            if (playerCardValue > monsterCardValue)
            {
                // 根据差距选择是否打出壮胆、偷换或强迫
                return ChooseBoostOrSwap(playerCardValue, monsterCardValue);
            }
            else
            {
                // 根据差距选择悔棋道具或跳过
                return ChooseRewindOrSkip();
            }
        }

        return null;
    }

    private GameItem ChooseBoostOrSwap(int playerCardValue, int monsterCardValue)
    {
        // 选择壮胆或偷换策略
        if (Mathf.Abs(playerCardValue - monsterCardValue) <= 3)
        {
            return monster.GetItems().Find(item => item.itemName == "壮胆"); // 找到壮胆道具
        }
        else
        {
            return monster.GetItems().Find(item => item.itemName == "偷换"); // 找到偷换道具
        }
    }

    private GameItem ChooseRewindOrSkip()
    {
        // 如果没有合适的道具，则选择悔棋或跳过
        Debug.Log("敌人没有合适的道具, 选择道具(悔棋)");
        return monster.GetItems().Find(item => item.itemName == "悔棋"); // 找到悔棋道具
    }

    private GameItem SelectOtherEnemyItems(List<GameItem> enemyItems)
    {
        // 选择其他非窥视道具的策略，例如随机选择
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

        // 禁用确认按钮
        EnableButton(confirmBtn, false, null);
        EnableButton(finishBtn, false, null);

        // 清理道具 UI
        RefreshPlayerItemUI(player, null);

        // 进入下一阶段
        currentPhase = GamePhase.Resolve;
        RunPhase();
    }

    private void HandleItemClicked(GameItem item)
    {
        Debug.Log($"玩家点击使用道具：{item.itemName}");

        // 执行道具效果
        List<FearCard> cards = item.Use(player, monster, playerSelectedCard, monsterSelectedCard);
        playerSelectedCard = cards[0];
        monsterSelectedCard = cards[1];

        // 移除玩家道具
        player.RemoveItem(item);

        // 更新 UI
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
            Debug.Log($"玩家获胜，敌人增加一点恐惧值 ({monster.GetFearValue()})");
        }
        else if (playerPoint < monsterPoint)
        {
            player.IncreaseFearValue(1);
            Debug.Log($"敌人获胜，玩家增加一点恐惧值 ({player.GetFearValue()})");
        }
        else
        {
            monster.IncreaseFearValue(1);
            player.IncreaseFearValue(1);
            Debug.Log($"平局，双方都增加1恐惧值({player.GetFearValue()}):({monster.GetFearValue()})");
        }

        player.UseCard(playerSelectedCard);
        monster.UseCard(monsterSelectedCard);

        cardTargetArea.ClearReadyToUseCard();

        RefreshPlayerCardUI(player);

        // 进入下一阶段
        currentPhase = GamePhase.End;
        RunPhase();

        //获取双方点数
        //比点，小的一方增加恐惧值
        //下一阶段
    }

    private void EndPhase()
    {
        Debug.Log("回合结束阶段");

        // 判断是否有一方恐惧值达到3
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
            // 使用一个简单的加权逻辑生成中间概率更高的随机数
            float randomValue = Random.Range(0f, 1f); // 生成一个[0, 1]的浮点数

            // (起始值a，终止值b，插值比例t) 例如a = 0, b = 1, t = 0, 则返回值为0
            int point = Mathf.RoundToInt(Mathf.Lerp(minPoint, maxPoint, randomValue * randomValue)); // 平方权重使中间更高

            if (point > 0 && point <= maxPoint && totalPoints - point >= minPoint * (cardCount - points.Count - 1))
            {
                points.Add(point);
                totalPoints -= point;
            }
        }

        // 打乱顺序，使每次分配的顺序随机
        for (int i = 0; i < points.Count; i++)
        {
            int swapIndex = Random.Range(0, points.Count);
            (points[i], points[swapIndex]) = (points[swapIndex], points[i]);
        }

        return points;
    }


    public void InitDecks(Player player, Monster monster,int startPoint, int endPoint)
    {
        List<int> points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"玩家卡{i}";
            playDecks.Add(new FearCard(cardName, points[i]));
        }
        player.ResetFearValue();

        points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"敌人卡{i}";
            monsterDecks.Add(new FearCard(cardName, points[i]));
            Debug.Log($"敌人卡{i}的点数：{points[i]}");
        }
        monster.ResetFearValue();
    }

    public void RefreshPlayerCardUI(Player player)
    {
        //隐藏道具UI，然后显示卡牌UI
        HidePlayerItemUI();

        List<FearCard> cards = player.GetCards();
        if (cards.Count == 0)
        {
            return;
        }

        for (int i = 0; i < cards.Count; ++i)
        {
            FearCard card = cards[i];

            // 获取当前卡槽中的 FearCardUI（如果没有，实例化一个）
            FearCardUI cardUI = cardSlots[i].GetComponentInChildren<FearCardUI>();
            if (cardUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{card.cardName}");
                cardUI = Instantiate(cardPrefab, cardSlots[i]).GetComponent<FearCardUI>();
            }

            // 设置卡牌UI数据
            cardUI.SetUI(card, cardSlots[i]);

            // 根据卡牌状态显示或隐藏 UI
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
        //隐藏卡牌UI，然后显示道具UI
        HidePlayerCardUI();

        List<GameItem> items = player.GetItems();
        if (items.Count == 0)
        {
            return;
        }

        for (int i = 0; i < items.Count; ++i)
        {
            GameItem item = items[i];

            // 获取当前卡槽中的 GameIteUI（如果没有，实例化一个）
            GameItemUI itemUI = itemSlots[i].GetComponentInChildren<GameItemUI>();
            if (itemUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
            }

            // 设置道具UI数据
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

            // 获取当前卡槽中的 GameIteUI（如果没有，实例化一个）
            GameItemUI itemUI = itemSlots[i].GetComponentInChildren<GameItemUI>();
            if (itemUI == null)
            {
                Debug.Log($"实例化第 {i} 张牌的 UI：{item.itemName}");
                itemUI = Instantiate(itemPrefab, itemSlots[i]).GetComponent<GameItemUI>();
            }

            // 设置道具UI数据
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

    public void InitGameItems(Player player, Monster monster)
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

        // 清除旧的监听器
        button.onClick.RemoveAllListeners();

        if (isEnabled)
        {
            button.onClick.AddListener(() => onClickCallback?.Invoke());
            button.image.color = Color.white; // 按钮亮起
        }
        else
        {
            button.image.color = Color.gray; // 按钮变暗
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
                Debug.Log($"{currentPhase}阶段点击 确认 没有作用");
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
                Debug.Log($"{currentPhase}阶段点击 完成 没有作用");
                break;
        }
    }

}
