using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameItem
{
    public string itemName; // ��������
    public Sprite art;
    public Sprite back;
    public abstract List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
    //public abstract List<FearCard> Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
    //public abstract List<FearCard> Use(Player player,  FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
}

public class PeekItem : GameItem
{
    public PeekItem(Sprite sprite, Sprite back) {
        itemName = "窥视";
        this.art = sprite;
        this.back = back;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log("使用窥视道具，敌人卡牌信息如下：");
        foreach (var card in monster.GetCards())
        {
            Debug.Log($"窥视成功！敌人选择的卡牌是：{card.cardName}, 吓人值：{card.point}");
        }

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

public class ChangeCardItem : GameItem
{
    public ChangeCardItem(Sprite sprite, Sprite back) {
        itemName = "鬼脸";
        this.art = sprite;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> playerDeck = player.GetCards();
        //player.AddCard(playerCard); // �ѵ�ǰ���ƷŻؿ���
        playerCard = playerDeck[Random.Range(0, playerDeck.Count)]; // ������������������¿�
        //player.UseCard(playerCard); 
        Debug.Log($"ʹ玩家换成了新卡{playerCard.cardName}, 新卡点数：{playerCard.point}");

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

public class TauntItem : GameItem
{
    public TauntItem(Sprite sprite, Sprite back) {
        itemName = "讥讽";
        this.art = sprite;
        this.back = back;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"使用讥讽道具，敌人卡牌：{monsterCard.cardName} 吓人值 -3");
        monsterCard.point = Mathf.Max(1, monsterCard.point - 3);

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

public class EncourageItem : GameItem
{
    public EncourageItem(Sprite sprite, Sprite back) {
        itemName = "壮胆";
        this.art = sprite;
        this.back = back;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"使用壮胆道具，玩家卡牌：{playerCard.cardName} 吓人值 +3");
        playerCard.point = Mathf.Min(9, playerCard.point + 3);

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

public class SwapCardPointsItem : GameItem
{
    public SwapCardPointsItem(Sprite sprite, Sprite back) {
        itemName = "交换吓人点数";
        this.art = sprite;
        this.back = back;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        int temp = playerCard.point;
        playerCard.point = monsterCard.point;
        monsterCard.point = temp;
        Debug.Log($"使用交换吓人点数道具，玩家卡牌变为：{playerCard.point}，敌人卡牌变为：{monsterCard.point}");

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

public class ForceChangeCardItem : GameItem
{
    public ForceChangeCardItem(Sprite sprite, Sprite back) {
        itemName = "强迫换卡";
        this.art = sprite;
        this.back = back;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> monsterDeck = monster.GetCards();
        monsterDeck.Add(monsterCard); // �ѵ�ǰ���ƷŻؿ���
        monsterCard = monsterDeck[Random.Range(0, monsterDeck.Count)]; // ���˻��¿�
        Debug.Log($"使用强迫换卡道具，敌人换成了新卡：{monsterCard.cardName}, 吓人值：{monsterCard.point}");

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

