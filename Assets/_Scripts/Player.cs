using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ICardManager
{
    private static List<FearCard> handingCards = new List<FearCard>();
    private static List<GameItem> items = new List<GameItem>();
    private static List<FearCard> usedCards = new List<FearCard>();

    private int fearValue = 0;
    private bool isBanned = false;

    //��ʼ��
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
        //handingCards.Remove(card);
        //usedCards.Add(card);
    }

    public void UseCard(FearCard card)
    {
        //TODO:����ʹ��Ч��

        Debug.Log("ʹ�ÿ��ƣ�" +  card.cardName + "����Ϊ��" + card.point);
        
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
    //��Ƭѡ��
    //����ʹ��

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
