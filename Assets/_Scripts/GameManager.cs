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

    private FearCard playerSelectedCard; // ���ѡ��Ŀ���
    private FearCard monsterSelectedCard; // ����ѡ��Ŀ���

    //��ʼ��
    private void Awake()
    {
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
        RefreshPlayerCardUI(player);

        // ��ʼ��˫������
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

    #region �غϿ�ʼ
    private void StartRound()
    {
        Debug.Log($"�غ� {currentRound} ��ʼ");

        //����
        //���ĻغϿ�ʼ˫������2������
        if (currentRound == 3)
        {
            Debug.Log("˫����õ���");
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
        //��һ�׶�
        currentPhase = GamePhase.Cover;
        RunPhase();
    }
    #endregion

    #region ���ƽ׶�
    private void CoverPhase()
    {
        Debug.Log("���ƽ׶ο�ʼ");

        EnableButton(confirmBtn, true, OnClickedConfirmButton);
        EnableButton(finishBtn, true, OnClickedFinishButton);
    }

    private void CoverPhaseConfirm()
    {
        //���ѡ��
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
            Debug.Log("δѡ���ƣ���ѡ���ƺ�ȷ�ϣ�");
            return;
        }

        Debug.Log($"���ѡ���˿��ƣ�{playerSelectedCard.cardName}");

        //����ѡ��
        List<FearCard> monsterCards = monster.GetCards();
        monsterSelectedCard = monsterCards[Random.Range(0, monsterCards.Count)];
        Debug.Log($"����ѡ���˿��ƣ�{monsterSelectedCard.cardName}");


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

    #region ���߽׶�
    private void ItemPhase()
    {
        Debug.Log("���߽׶ο�ʼ");

        // ȷ�����ֹ��򣺼���ż���غϵ������֣������غ��������
        bool isPlayerTurn = currentRound % 2 != 0;

        EnableButton(finishBtn, true, OnClickedFinishButton);
        // ��ʼ����ʹ�ý׶�
        StartCoroutine(HandleItemPhase(isPlayerTurn));

        // ��ʾ���� UI
        RefreshPlayerItemUI(player);

        //if(isPlayerTurn)
        //{
        //    // ����ȷ�ϰ�ť
        //    EnableConfirmButton(confirmBtn, true, OnClickedConfirmButton);
        //    EnableConfirmButton(finishBtn, true, OnClickedFinishButton);

        //    while (finishBtn.interactable) continue;

        //    mon.IntoItemPhase();
        //}
        //else
        //{
        //    mon.IntoItemPhase();

        //    // ����ȷ�ϰ�ť
        //    EnableConfirmButton(confirmBtn, true, OnClickedConfirmButton);
        //    EnableConfirmButton(finishBtn, true, OnClickedFinishButton);
        //}

    }

    private IEnumerator HandleItemPhase(bool isPlayerTurn)
    {
        Debug.Log($"{(isPlayerTurn ? "���" : "����")}���ֿ�ʼ���߽׶�");
        while (true)
        {
            if (isPlayerTurn)
            {
                yield return StartCoroutine(HandlePlayerItemUsage());
                if (player.GetCards().Count == 0) break; // ���û�е��������
            }
            else
            {
                yield return StartCoroutine(HandleEnemyItemUsage());
                if (monster.GetCards().Count == 0) break; // ����û�е��������
            }

        }
    }

    private IEnumerator HandlePlayerItemUsage()
    {
        Debug.Log("��ҵĵ��߻غ�");

        // �ȴ����ѡ�����
        bool itemUsed = false;
        GameItem selectedItem = null;
        // ����ȷ�ϰ�ť
        EnableButton(confirmBtn, true, () =>
        {
            selectedItem = cardTargetArea.GetAreaItem(); // ��ȡ���ѡ��ĵ���
            if (selectedItem != null)
            {
                Debug.Log($"���ʹ�õ��ߣ�{selectedItem.itemName}");
                selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
                player.RemoveItem(selectedItem);
                itemUsed = true;
            }
        });

        // �ȴ����ȷ��
        yield return new WaitUntil(() => itemUsed);
        EnableButton(confirmBtn, false, null);
    }

    //private IEnumerator HandleEnemyItemUsage()
    //{
    //    Debug.Log("���˵ĵ��߻غ�");

    //    List<GameItem> enemyItems = monster.GetItems();
    //    if (enemyItems.Count > 0)
    //    {
    //        GameItem selectedItem = enemyItems[Random.Range(0, enemyItems.Count)];
    //        Debug.Log($"����ʹ�õ��ߣ�{selectedItem.itemName}");
    //        selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
    //        monster.RemoveItem(selectedItem);
    //    }
    //    else
    //    {
    //        Debug.Log("����û�п��õĵ���");
    //    }

    //    // ģ����˲������ӳ�
    //    yield return new WaitForSeconds(1f);
    //}
    private IEnumerator HandleEnemyItemUsage()
    {
        Debug.Log("���˵ĵ��߻غ�");

        List<GameItem> enemyItems = monster.GetItems();
        if (enemyItems.Count > 0)
        {
            GameItem selectedItem = null;

            // AIѡ����ߵ��߼�
            selectedItem = ChooseEnemyItem(enemyItems);

            if (selectedItem != null)
            {
                Debug.Log($"����ʹ�õ��ߣ�{selectedItem.itemName}");
                selectedItem.Use(player, monster, playerSelectedCard, monsterSelectedCard);
                monster.RemoveItem(selectedItem);
            }
            else
            {
                Debug.Log("����û�к��ʵĵ���");
            }
        }
        else
        {
            Debug.Log("����û�п��õĵ���");
        }

        // ģ����˲������ӳ�
        yield return new WaitForSeconds(1f);
    }

    private GameItem ChooseEnemyItem(List<GameItem> enemyItems)
    {
        // ������˵ĵ��߲���1�����Ӻ���������
        GameItem itemToUse = null;

        GameItem peekItem = enemyItems.Find(item => item.itemName == "����"); // �ҵ����ӵ���
        if (peekItem != null)
        {
            // ʹ�ÿ��ӵ���
            itemToUse = peekItem;
            Debug.Log("����ʹ�ÿ��ӵ���");

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
            return monster.GetItems().Find(item => item.itemName == "׳��"); // �ҵ�׳������
        }
        else
        {
            return monster.GetItems().Find(item => item.itemName == "͵��"); // �ҵ�͵������
        }
    }

    private GameItem ChooseRewindOrSkip()
    {
        // ���û�к��ʵĵ��ߣ���ѡ����������
        Debug.Log("����û�к��ʵĵ���, ѡ�����(����)");
        return monster.GetItems().Find(item => item.itemName == "����"); // �ҵ��������
    }

    private GameItem SelectOtherEnemyItems(List<GameItem> enemyItems)
    {
        // ѡ�������ǿ��ӵ��ߵĲ��ԣ��������ѡ��
        Debug.Log("�������ѡ�����");
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
        Debug.Log("���ȷ�Ͻ�������ʹ�ý׶�");

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
        Debug.Log($"��ҵ��ʹ�õ��ߣ�{item.itemName}");

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

    #region ����׶�
    private void ResolvePhase()
    {
        Debug.Log("����׶ο�ʼ");
        Debug.Log($"���ѡ��Ŀ�Ϊ({playerSelectedCard.cardName})������Ϊ({playerSelectedCard.point})");

        int playerPoint = playerSelectedCard.point;
        int monsterPoint = monsterSelectedCard.point;

        if (playerPoint > monsterPoint)
        {
            monster.IncreaseFearValue(1);
            Debug.Log($"��һ�ʤ����������һ��־�ֵ ({monster.GetFearValue()})");
        }
        else if (playerPoint < monsterPoint)
        {
            player.IncreaseFearValue(1);
            Debug.Log($"���˻�ʤ���������һ��־�ֵ ({player.GetFearValue()})");
        }
        else
        {
            monster.IncreaseFearValue(1);
            player.IncreaseFearValue(1);
            Debug.Log($"ƽ�֣�˫��������1�־�ֵ({player.GetFearValue()}):({monster.GetFearValue()})");
        }

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
        Debug.Log("�غϽ����׶�");

        // �ж��Ƿ���һ���־�ֵ�ﵽ3
        if (player.GetFearValue() >= 3)
        {
            Debug.Log("���ʧ�ܣ�");
            // ʧ���߼�
        }
        else if (monster.GetFearValue() >= 3)
        {
            Debug.Log("����ʧ�ܣ�");
            // ʤ���߼�
        }
        else
        {
            // ������һ�غ�
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


    public void InitDecks(Player player, Monster monster,int startPoint, int endPoint)
    {
        List<int> points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"��ҿ�{i}";
            playDecks.Add(new FearCard(cardName, points[i]));
        }
        player.ResetFearValue();

        points = RandomCardPoints(25, 5, startPoint, endPoint);
        for (int i = 0; i < points.Count; i++)
        {
            string cardName = $"���˿�{i}";
            monsterDecks.Add(new FearCard(cardName, points[i]));
            Debug.Log($"���˿�{i}�ĵ�����{points[i]}");
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
                Debug.Log($"ʵ������ {i} ���Ƶ� UI��{card.cardName}");
                cardUI = Instantiate(cardPrefab, cardSlots[i]).GetComponent<FearCardUI>();
            }

            // ���ÿ���UI����
            cardUI.SetUI(card, cardSlots[i]);

            // ���ݿ���״̬��ʾ������ UI
            if (card.isUsed)
            {
                Debug.Log($"���ص� {i} ����ʹ�ÿ��ƣ�{card.cardName}");
                cardUI.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"��ʾ�� {i} ��δʹ�ÿ��ƣ�{card.cardName}");
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
                Debug.Log($"ʵ������ {i} ���Ƶ� UI��{item.itemName}");
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
                Debug.Log($"ʵ������ {i} ���Ƶ� UI��{item.itemName}");
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