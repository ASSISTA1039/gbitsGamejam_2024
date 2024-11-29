using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster:ICardManager
{
    private List<FearCard> handingCards = new List<FearCard>();
    private static List<GameItem> items = new List<GameItem>();

    private int fearValue = 0;
    //private bool isBanned = false;

    //-----------------------------
    private Animator animator;
    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    //初始化
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

    public void PlaySpecialAnimation(GameItem selectedItem)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator 未初始化");
            return;
        }
        switch (selectedItem.itemName)
        {
            case "讥讽":
                animator.SetTrigger("TriggerDentalCard");
                break;
            case "壮胆":
                animator.SetTrigger("TriggerJokerCard");
                break;
            case "交换吓人点数":
                animator.SetTrigger("TriggerDogCard");
                break;
            case "换卡":
                animator.SetTrigger("TriggerChristmasCard");
                break;
            case "窥视":
                animator.SetTrigger("TriggerGhostCard");
                break;
            default:
                Debug.Log("没有对应的动画触发器！");
                break;
        }
    }
}
