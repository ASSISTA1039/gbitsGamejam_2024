using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster:ICardManager
{
    private List<FearCard> handingCards = new List<FearCard>();
    private static List<GameItem> items = new List<GameItem>();

    private int fearValue = 0;
    private bool isBanned = false;

    #region 卡牌操作
    public void AddCard(FearCard card)
    {
        handingCards.Add(card);
    }

    public void InitCards(List<FearCard> deck)
    {
        handingCards.Clear();
        foreach (FearCard card in deck)
        {
            if (card != null)
            {
                card.isUsed = false;
                AddCard(card);
            }
        }
    }

    public void RemoveCard(FearCard card)
    {
        card.isUsed = true;
    }

    public void UseCard(FearCard card)
    {
        //TODO:卡牌使用效果

        Debug.Log("使用卡牌：" + card.cardName + "点数为：" + card.point);

        RemoveCard(card);
    }

    public List<FearCard> GetCards()
    {
        return handingCards;
    }
    #endregion

    #region 恐惧值操作
    public void ResetFearValue()
    {
        fearValue = 0;
    }

    public void IncreaseFearValue(int point)
    {
        fearValue += point;
    }

    public int GetFearValue()
    {
        return fearValue;
    }
    #endregion

    #region 道具操作
    public void AddItem(GameItem item)
    {
        items.Add(item);
    }

    public List<GameItem> GetItems()
    {
        return items;
    }

    public void RemoveItem(GameItem item)
    {
        items.Remove(item);
    }
    #endregion


}
