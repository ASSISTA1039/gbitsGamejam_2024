using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ICardManager
{
    private List<FearCard> handingCards = new List<FearCard>();
    private List<GameItem> items = new List<GameItem>();
    private List<FearCard> usedCards = new List<FearCard>();

    private int fearValue = 0;
    //private bool isBanned = false;

    //初始化
    public void AddCard(FearCard card)
    {
        card.isUsed = false;
        handingCards.Add(card);
    }

    public void InitCards(List<FearCard> deck)
    {
        handingCards.Clear();
        foreach (FearCard card in deck)
        {
            if (card != null)
            {
                AddCard(card);

            }
        }
        
    }

    public void RemoveCard(FearCard card)
    {
        card.isUsed = true;
        handingCards.Remove(card);
        //usedCards.Add(card);
    }

    public void UseCard(FearCard card)
    {
        //TODO:卡牌使用效果

        Debug.Log("使用卡牌：" +  card.cardName + "点数为：" + card.point);
        
        RemoveCard(card);
    }

    public List<FearCard> GetCards()
    {
        return handingCards;
    }

    public void ResetFearValue()
    {
        fearValue = 0;
    }

    public int GetFearValue()
    {
        return fearValue;
    }

    public void IncreaseFearValue(int point)
    {
        fearValue += point;
    }
    //卡片选择
    //道具使用

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
}
