using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTargetArea : MonoBehaviour
{
    private FearCardUI readyToUseCardUI = null;
    private GameItemUI readyToUseItemUI = null;

    public FearCard GetAreaCard()
    {
        return readyToUseCardUI.card;
    }    
    public GameItem GetAreaItem()
    {
        return readyToUseItemUI.item;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Card")) // 如果进入目标区域的是卡牌
        {
            FearCardUI cardUI = other.GetComponent<FearCardUI>();
            if (cardUI != null)
            {
                // 吸附卡牌到目标区域
                cardUI.SnapToTarget(transform);
            }
        }

        if (other.CompareTag("Item")) // 如果进入目标区域的是卡牌
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null)
            {
                // 吸附道具到目标区域
                itemUI.SnapToTarget(transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Card")) // 如果卡牌离开目标区域
        {
            FearCardUI cardUI = other.GetComponent<FearCardUI>();
            if (cardUI != null && cardUI == readyToUseCardUI)
            {
                // 恢复卡牌原位
                cardUI.ResetPosition();
                readyToUseCardUI = null;
            }
        }

        if (other.CompareTag("Item")) // 如果卡牌离开目标区域
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null && itemUI == readyToUseItemUI )
            {
                // 恢复道具原位
                itemUI.ResetPosition();
                readyToUseItemUI = null;
            }
        }
    }

    public void GetReadyToUseCard(FearCardUI cardUI)
    {
        if (readyToUseCardUI != null)
        {
            readyToUseCardUI.ReturnToHand();
        }
        readyToUseCardUI = cardUI;
    }

    public void GetReadyToUseItem(GameItemUI itemUI)
    {
        if (readyToUseItemUI != null)
        {
            readyToUseItemUI.ReturnToHand();
        }
        readyToUseItemUI = itemUI;
    }

    public void ClearReadyToUseCard()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        readyToUseCardUI = null;
    }
}
