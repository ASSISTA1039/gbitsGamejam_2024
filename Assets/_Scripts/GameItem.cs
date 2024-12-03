using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class GameItem
{
    public string itemName; // ��������
    public Sprite art;
    public Sprite back;
    public string description;
    public string displayString;
    public abstract List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context); // ����ʹ���߼�
    //public abstract List<FearCard> Use(Player player, Monster monster, FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
    //public abstract List<FearCard> Use(Player player,  FearCard playerCard, FearCard monsterCard); // ����ʹ���߼�
}

public class PeekItem : GameItem
{
    public PeekItem(string itemName, Sprite sprite, Sprite back, string description) {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        displayString = $"{player.name}使用侦探眼睛道具，{monster.name}选择的卡牌是：{monsterCard.cardName}, 吓人值：{monsterCard.point}";

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}
public class DivinationItem : GameItem
{
    public DivinationItem(string itemName, Sprite sprite, Sprite back, string description)
    {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        displayString = $"{player.name}使用{itemName}道具，{monster.name}卡牌信息如下：";
        var cards = monster.GetCards();
        for (int i = 0; i < 3; ++i)
        {
            if (i == cards.Count)
            {
                displayString += $"\n{monster.name}只有{i + 1}张卡牌";
                break;
            }
            FearCard card = cards[Random.Range(0, monster.GetCards().Count)];
            displayString += ($"\n{monster.name}卡牌第{i+1}张卡牌：{card.cardName}, 吓人值：{card.point}");
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
    public ChangeCardItem(string itemName, Sprite sprite, Sprite back, string description)
    {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        List<FearCard> playerDeck = player.GetCards();
        //player.AddCard(playerCard); // �ѵ�ǰ���ƷŻؿ���
        playerCard = playerDeck[Random.Range(0, playerDeck.Count)]; // ������������������¿�
        //player.UseCard(playerCard); 
        displayString = $"{player.name}使用抓娃娃爪子道具，换成了新卡{playerCard.cardName}, 新卡点数：{playerCard.point}";

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
    public EncourageItem(string itemName, Sprite sprite, Sprite back, string description)
    {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        displayString = $"{player.name}使用壮胆道具，{player.name}卡牌吓人值 +3";
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
    public SwapCardPointsItem(string itemName, Sprite sprite, Sprite back, string description)
    {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        int temp = playerCard.point;
        playerCard.point = monsterCard.point;
        monsterCard.point = temp;
        displayString = $"{player.name}使用交互道具，{player.name}卡牌变为：{playerCard.point}，敌人卡牌变为：{monsterCard.point}";

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
    public ForceChangeCardItem(string itemName, Sprite sprite, Sprite back, string description)
    {
        this.itemName = itemName;
        this.art = sprite;
        this.back = back;
        this.description = description;
    }

    public override List<FearCard> Use(Player player, Player monster, FearCard playerCard, FearCard monsterCard, TextMeshProUGUI context)
    {
        List<FearCard> monsterDeck = monster.GetCards();
        monsterDeck.Add(monsterCard); // �ѵ�ǰ���ƷŻؿ���
        monsterCard = monsterDeck[Random.Range(0, monsterDeck.Count)]; // ���˻��¿�
        displayString = $"{player.name}使用鬼手道具，{monster.name}换成了新卡：{monsterCard.cardName}, 吓人值：{monsterCard.point}";

        List<FearCard> res = new List<FearCard>
        {
            playerCard,
            monsterCard
        };

        return res;
    }
}

