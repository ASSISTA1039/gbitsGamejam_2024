using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameItem
{
    public string itemName; // ��������
    public abstract void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
}

public class PeekItem : GameItem
{
    public PeekItem() { itemName = "����"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log("ʹ�ÿ��ӵ��ߣ����˿�����Ϣ���£�");
        foreach (var card in monster.GetCards())
        {
            Debug.Log($"���˿��ƣ�{card.cardName}, ����ֵ��{card.point}");
        }
    }
}

public class ChangeCardItem : GameItem
{
    public ChangeCardItem() { itemName = "����"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> playerDeck = player.GetCards();
        playerDeck.Add(playerCard); // �ѵ�ǰ���ƷŻؿ���
        playerCard = playerDeck[Random.Range(0, playerDeck.Count)]; // ������¿�
        Debug.Log($"ʹ�û������ߣ���һ������¿���{playerCard.cardName}, ����ֵ��{playerCard.point}");
    }
}

public class TauntItem : GameItem
{
    public TauntItem() { itemName = "����"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"ʹ�ü�����ߣ����˿��ƣ�{monsterCard.cardName} ����ֵ -3");
        monsterCard.point = Mathf.Max(1, monsterCard.point - 3);
    }
}

public class EncourageItem : GameItem
{
    public EncourageItem() { itemName = "׳��"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"ʹ��׳�����ߣ���ҿ��ƣ�{playerCard.cardName} ����ֵ +3");
        playerCard.point = Mathf.Min(9, playerCard.point + 3);
    }
}

public class SwapCardPointsItem : GameItem
{
    public SwapCardPointsItem() { itemName = "�������˵���"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        int temp = playerCard.point;
        playerCard.point = monsterCard.point;
        monsterCard.point = temp;
        Debug.Log($"ʹ�ý������˵������ߣ���ҿ��Ʊ�Ϊ��{playerCard.point}�����˿��Ʊ�Ϊ��{monsterCard.point}");
    }
}

public class ForceChangeCardItem : GameItem
{
    public ForceChangeCardItem() { itemName = "ǿ�Ȼ���"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> monsterDeck = monster.GetCards();
        monsterDeck.Add(monsterCard); // �ѵ�ǰ���ƷŻؿ���
        monsterCard = monsterDeck[Random.Range(0, monsterDeck.Count)]; // ���˻��¿�
        Debug.Log($"ʹ��ǿ�Ȼ������ߣ����˻������¿���{monsterCard.cardName}, ����ֵ��{monsterCard.point}");
    }
}

