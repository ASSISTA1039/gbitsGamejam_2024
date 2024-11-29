using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster:ICardManager
{
    private List<FearCard> handingCards = new List<FearCard>();
    private static List<GameItem> items = new List<GameItem>();

    private int fearValue = 0;
    private bool isBanned = false;

    #region ���Ʋ���
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
        //TODO:����ʹ��Ч��

        Debug.Log("ʹ�ÿ��ƣ�" + card.cardName + "����Ϊ��" + card.point);

        RemoveCard(card);
    }

    public List<FearCard> GetCards()
    {
        return handingCards;
    }
    #endregion

    #region �־�ֵ����
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

    #region ���߲���
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
    
    public void PlaySpecialAnimation(GameItem selectedItem)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator δ��ʼ��");
            return;
        }
        switch (selectedItem.itemName)
        {
            case "����":
                animator.SetTrigger("TriggerDentalCard");
                break;
            case "׳��":
                animator.SetTrigger("TriggerJokerCard");
                break;
            case "�������˵���":
                animator.SetTrigger("TriggerDogCard");
                break;
            case "����":
                animator.SetTrigger("TriggerChristmasCard");
                break;
            case "����":
                animator.SetTrigger("TriggerGhostCard");
                break;
            default:
                Debug.Log("û�ж�Ӧ�Ķ�����������");
                break;
        }
    }

}
