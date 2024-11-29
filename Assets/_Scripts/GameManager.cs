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

    private FearCard playerSelectedCard; // ���ѡ��Ŀ���
    private FearCard monsterSelectedCard; // ����ѡ��Ŀ���

    //-------------------------------------------------------
    public Animator monsterAnimator;
    public TugOfWarUI gameboard;
    private int playerScore;
    private int monsterScore;

    //��ʼ��
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

        // ��ʼ�����Ƴ�
        InitDecks(player, monster, 1, 5);

        // ��ʼ��˫������
        player.InitCards(playDecks);
        monster.InitCards(monsterDecks);
        RefreshPlayerCardUI(player);

        // ��ʼ��˫������
        InitGameItems(player, monster);

        ChangeToNextPhase();
    }
    #region �׶��߼�
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
        Debug.Log($"�غ� {currentRound} ��ʼ");

        //����
        //���ĻغϿ�ʼ˫������2������
        if (currentRound == 4)
        {
            Debug.Log("˫����õ���");
            // ���ӵ����߼�������չ��
            for (int i = 0; i < 2; i++)
            {
                player.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
            }
            for (int i = 0; i < 2; i++)
            {
                monster.AddItem(itemTemplete[Random.Range(0, itemTemplete.Count)]);
            }
        }
        //��һ�׶�
        currentPhase = GamePhase.Cover;
        ChangeToNextPhase();
    }

    private void CoverPhase()
    {
        Debug.Log("���ƽ׶ο�ʼ");

        cardConfirmBtn.image.color = Color.white;

        //���ȷ��
        cardConfirmBtn.onClick.AddListener(OnClickedConfirmButton);

    }

    private void OnClickedConfirmButton()
    {
        //���ѡ��
        playerSelectedCard = cardTargetArea.GetAreaCard();

        if (playerSelectedCard == null)
        {
            Debug.Log("δѡ���ƣ���ѡ���ƺ�ȷ�ϣ�");
            return;
        }

        Debug.Log($"���ѡ���˿��ƣ�{playerSelectedCard.cardName}");

        CardTun cardTun = cardTargetArea.GetComponentInChildren<CardTun>();
        if (cardTun != null)
        {
            cardTun.StartFront();
        }
        else
        {
            Debug.LogWarning("ѡ�еĿ���û�й��� CardTun �ű�");
        }
        //����ѡ��
        List<FearCard> monsterCards = monster.GetCards();
        monsterSelectedCard = monsterCards[Random.Range(0, monsterCards.Count)];
        Debug.Log($"����ѡ���˿��ƣ�{monsterSelectedCard.cardName}");

        // ���á�ȷ�ϡ���ť���������
        cardConfirmBtn.onClick.RemoveListener(OnClickedConfirmButton);
        cardConfirmBtn.image.color = Color.gray;

        

        // ������һ�׶�
        currentPhase = GamePhase.Item;

        //���Խ���
        //currentPhase = GamePhase.Resolve;

        ChangeToNextPhase();
    }

    /*    private void ItemPhase()
        {
            Debug.Log("���߽׶ο�ʼ");

            //�ж�����

            //������֣�ѭ��ѡ����ߣ�������Ч
            //���ȷ������

            //�������֣�ѭ��ѡ����ߣ���Ч
            //��һ�׶�
        }*/

    private void ItemPhase()
    {
        Debug.Log("���߽׶ο�ʼ");

        // ���ѡ����߲���Ч���߼�������չ��
        //HandlePlayerItems();

        // ����ѡ��ʹ�õĵ��߻���
        List<GameItem> monsterItems = monster.GetItems();
        if (monsterItems.Count > 0)
        {
            // �������ʹ��һ������
            GameItem selectedItem = monsterItems[Random.Range(0, monsterItems.Count)];
            Debug.Log($"����ʹ���˵��ߣ�{selectedItem.itemName}");
            // �����������صĶ���
            monster.PlaySpecialAnimation(selectedItem);
            // �ȴ�������ɺ��ٽ�����һ�׶�
            StartCoroutine(WaitForMonsterAnimation(selectedItem));

            // �Ƴ���ʹ�õĵ���
            monster.RemoveItem(selectedItem);
        }
        else
        {
            Debug.Log("����û�п��õĵ���");
        }
    }
    private IEnumerator WaitForMonsterAnimation(GameItem selectedItem)
    {
        // ��ȡ�����ĳ���ʱ��
        AnimatorStateInfo currentState = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = currentState.length;

        // �ȴ������������
        yield return new WaitForSeconds(3f * animationDuration);
        // �ص� Idle ״̬
        monsterAnimator.SetTrigger("TriggerIdle");
        // ������ɺ������һ�׶�
        currentPhase = GamePhase.Resolve;
        ChangeToNextPhase();
    }


    private void ResolvePhase()
    {
        Debug.Log("����׶ο�ʼ");

        int playerPoint = playerSelectedCard.point;
        int monsterPoint = monsterSelectedCard.point;

        if (playerPoint > monsterPoint)
        {
            monster.IncreaseFearValue(1);
            Debug.Log($"��һ�ʤ����������һ��־�ֵ ({monster.GetFearValue()})");
            gameboard.UpdateBars(++playerScore, --monsterScore);
        }
        else if (playerPoint < monsterPoint)
        {
            player.IncreaseFearValue(1);
            Debug.Log($"���˻�ʤ���������һ��־�ֵ ({player.GetFearValue()})");
            gameboard.UpdateBars(--playerScore, ++monsterScore);
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
        ChangeToNextPhase();

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
            ChangeToNextPhase();
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

            // ���ÿ�������
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

    //���ƽ׶�
    //���ѡ�ƣ�AIѡ�ƣ���һ�׶�

    //���߽׶�
    //���ֵ��ߣ���Ч��ֱ��ѡ����������ֵ��ߣ���Ч��ֱ��ѡ���������һ�׶�

    //����׶�
    //���߿�����Ч���ȵ㣬���㣬�۷�

    //�غϽ���
    //���߿�����Ч���Ƿ�ʤ�ܣ�������һ�غ�

    //ʤ�ܽ׶�
    //չʾ���������������ؿ�


}
