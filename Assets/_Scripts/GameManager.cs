using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Player player = new Player();
    public Monster monster = new Monster();

    public List<FearCard> playDecks = new List<FearCard>();
    public List<FearCard> monsterDecks = new List<FearCard>();
    public List<GameItem> itemTemplete = new List<GameItem>();
    public Transform[] cardSlots;

    public GameObject cardPrefab;
    public CardTargetArea cardTargetArea;
    public Button cardConfirmBtn;

    private enum GamePhase { Start, Cover, Item, Resolve, End }
    private GamePhase currentPhase;
    private int currentRound;

    private FearCard playerSelectedCard; // 玩家选择的卡牌
    private FearCard monsterSelectedCard; // 敌人选择的卡牌

    //-------------------------------------------------------
    public Animator monsterAnimator;
    public TugOfWarUI gameboard;
    private int playerScore;
    private int monsterScore;

    //初始化
    private void Awake()
    {
        InitGame();
        playerScore = 4;
        monsterScore = 4;
        monster.SetAnimator(monsterAnimator);
    }

    private void InitGame()
    {
        currentRound = 1;
        currentPhase = GamePhase.Start;

        // 初始化卡牌池
        InitDecks(player, monster, 1, 5);

        // 初始化双方手牌
        player.InitCards(playDecks);
        monster.InitCards(monsterDecks);
        RefreshPlayerCardUI(player);

        // 初始化双方道具
        InitGameItems(player, monster);

        ChangeToNextPhase();
    }
    #region 阶段逻辑
    private void ChangeToNextPhase()
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

    private void StartRound()
    {
        Debug.Log($"回合 {currentRound} 开始");

        //发牌
        //第四回合开始双方各发2个道具
        if (currentRound == 4)
        {
            Debug.Log("双方获得道具");
            // 增加道具逻辑（待扩展）
            for (int i = 0; i < 2; i++)
            {
                player.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
            }
            for (int i = 0; i < 2; i++)
            {
                monster.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
            }
        }
        //下一阶段
        currentPhase = GamePhase.Cover;
        ChangeToNextPhase();
    }

    private void CoverPhase()
    {
        Debug.Log("盖牌阶段开始");

        cardConfirmBtn.image.color = Color.white;

        //玩家确认
        cardConfirmBtn.onClick.AddListener(OnClickedConfirmButton);

    }

    private void OnClickedConfirmButton()
    {
        //玩家选卡
        playerSelectedCard = cardTargetArea.GetAreaCard();

        if (playerSelectedCard == null)
        {
            Debug.Log("未选择卡牌，请选择卡牌后确认！");
            return;
        }

        Debug.Log($"玩家选择了卡牌：{playerSelectedCard.cardName}");

        CardTun cardTun = cardTargetArea.GetComponentInChildren<CardTun>();
        if (cardTun != null)
        {
            cardTun.StartFront();
        }
        else
        {
            Debug.LogWarning("选中的卡牌没有挂载 CardTun 脚本");
        }
        //敌人选卡
        List<FearCard> monsterCards = monster.GetCards();
        monsterSelectedCard = monsterCards[Random.Range(0, monsterCards.Count)];
        Debug.Log($"敌人选择了卡牌：{monsterSelectedCard.cardName}");

        // 禁用“确认”按钮，清除监听
        cardConfirmBtn.onClick.RemoveListener(OnClickedConfirmButton);
        cardConfirmBtn.image.color = Color.gray;

        

        // 进入下一阶段
        currentPhase = GamePhase.Item;

        //测试结算
        //currentPhase = GamePhase.Resolve;

        ChangeToNextPhase();
    }

    /*    private void ItemPhase()
        {
            Debug.Log("道具阶段开始");

            //判断先手

            //玩家先手，循环选择道具，道具生效
            //玩家确认用完

            //敌人先手，循环选择道具，生效
            //下一阶段
        }*/

    private void ItemPhase()
    {
        Debug.Log("道具阶段开始");

        // 玩家选择道具并生效的逻辑（待扩展）
        //HandlePlayerItems();

        // 敌人选择使用的道具或卡牌
        List<GameItem> monsterItems = monster.GetItems();
        if (monsterItems.Count > 0)
        {
            // 敌人随机使用一个道具
            GameItem selectedItem = monsterItems[Random.Range(0, monsterItems.Count)];
            Debug.Log($"敌人使用了道具：{selectedItem.itemName}");
            // 播放与道具相关的动画
            monster.PlaySpecialAnimation(selectedItem);
            // 等待动画完成后再进入下一阶段
            StartCoroutine(WaitForMonsterAnimation(selectedItem));

            // 移除已使用的道具
            monster.RemoveItem(selectedItem);
        }
        else
        {
            Debug.Log("敌人没有可用的道具");
        }
    }
    private IEnumerator WaitForMonsterAnimation(GameItem selectedItem)
    {
        // 获取动画的持续时间
        AnimatorStateInfo currentState = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = currentState.length;

        // 等待动画播放完成
        yield return new WaitForSeconds(3f * animationDuration);
        // 回到 Idle 状态
        monsterAnimator.SetTrigger("TriggerIdle");
        // 动画完成后进入下一阶段
        currentPhase = GamePhase.Resolve;
        ChangeToNextPhase();
    }


    private void ResolvePhase()
    {
        Debug.Log("结算阶段开始");

        int playerPoint = playerSelectedCard.point;
        int monsterPoint = monsterSelectedCard.point;

        if (playerPoint > monsterPoint)
        {
            monster.IncreaseFearValue(1);
            Debug.Log($"玩家获胜，敌人增加一点恐惧值 ({monster.GetFearValue()})");
            gameboard.UpdateBars(++playerScore, --monsterScore);
        }
        else if (playerPoint < monsterPoint)
        {
            player.IncreaseFearValue(1);
            Debug.Log($"敌人获胜，玩家增加一点恐惧值 ({player.GetFearValue()})");
            gameboard.UpdateBars(--playerScore, ++monsterScore);
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
        ChangeToNextPhase();

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
            ChangeToNextPhase();
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

            // 设置卡牌数据
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

    public void InitGameItems(Player player, Monster monster)
    {
        itemTemplete.Add(new PeekItem());
        itemTemplete.Add(new ChangeCardItem());
        itemTemplete.Add(new TauntItem());
        itemTemplete.Add(new EncourageItem());
        itemTemplete.Add(new SwapCardPointsItem());

        for (int i = 0; i < 3; i++)
        {
            player.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
        }
        for (int i = 0; i < 3; i++)
        {
            monster.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
        }
    }

    //盖牌阶段
    //玩家选牌，AI选牌，下一阶段

    //道具阶段
    //先手道具，生效，直到选择结束，后手道具，生效，直到选择结束，下一阶段

    //结算阶段
    //道具可能生效，比点，结算，扣分

    //回合结束
    //道具可能生效，是否胜败，否则下一回合

    //胜败阶段
    //展示，动画，结束，重开


}
