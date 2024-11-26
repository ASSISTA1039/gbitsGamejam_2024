using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameItem
{
    public string itemName; // 道具名称
    public abstract void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard); // 道具使用逻辑
}

public class PeekItem : GameItem
{
    public PeekItem() { itemName = "窥视"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log("使用窥视道具，敌人卡牌信息如下：");
        foreach (var card in monster.GetCards())
        {
            Debug.Log($"敌人卡牌：{card.cardName}, 吓人值：{card.point}");
        }
    }
}

public class ChangeCardItem : GameItem
{
    public ChangeCardItem() { itemName = "换卡"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> playerDeck = player.GetCards();
        playerDeck.Add(playerCard); // 把当前卡牌放回卡组
        playerCard = playerDeck[Random.Range(0, playerDeck.Count)]; // 随机换新卡
        Debug.Log($"使用换卡道具，玩家换成了新卡：{playerCard.cardName}, 吓人值：{playerCard.point}");
    }
}

public class TauntItem : GameItem
{
    public TauntItem() { itemName = "讥讽"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"使用讥讽道具，敌人卡牌：{monsterCard.cardName} 吓人值 -3");
        monsterCard.point = Mathf.Max(1, monsterCard.point - 3);
    }
}

public class EncourageItem : GameItem
{
    public EncourageItem() { itemName = "壮胆"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        Debug.Log($"使用壮胆道具，玩家卡牌：{playerCard.cardName} 吓人值 +3");
        playerCard.point = Mathf.Min(9, playerCard.point + 3);
    }
}

public class SwapCardPointsItem : GameItem
{
    public SwapCardPointsItem() { itemName = "交换吓人点数"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        int temp = playerCard.point;
        playerCard.point = monsterCard.point;
        monsterCard.point = temp;
        Debug.Log($"使用交换吓人点数道具，玩家卡牌变为：{playerCard.point}，敌人卡牌变为：{monsterCard.point}");
    }
}

public class ForceChangeCardItem : GameItem
{
    public ForceChangeCardItem() { itemName = "强迫换卡"; }

    public override void Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard)
    {
        List<FearCard> monsterDeck = monster.GetCards();
        monsterDeck.Add(monsterCard); // 把当前卡牌放回卡组
        monsterCard = monsterDeck[Random.Range(0, monsterDeck.Count)]; // 敌人换新卡
        Debug.Log($"使用强迫换卡道具，敌人换成了新卡：{monsterCard.cardName}, 吓人值：{monsterCard.point}");
    }
}

