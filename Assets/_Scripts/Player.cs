using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : ICardManager
{
    public string name;
    private List<FearCard> handingCards = new List<FearCard>();
    private List<GameItem> items = new List<GameItem>();

    private int fearValue = 0;
    //private bool isBanned = false;

    //初始化
    public Player(string name) 
    {
        this.name = name;
    }
    public void AddCard(FearCard card)
    {
        handingCards.Add(card);
    }

    public void InitCards(List<FearCard> deck)
    {
        deck = deck.OrderBy(card => card.point).ToList();
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
