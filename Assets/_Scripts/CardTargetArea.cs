using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTargetArea : MonoBehaviour
{
    private FearCardUI readyToUseCardUI = null;

    public FearCard GetAreaCard()
    {
        return readyToUseCardUI.card;
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
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Card")) // 如果卡牌离开目标区域
        {
            FearCardUI cardUI = other.GetComponent<FearCardUI>();
            if (cardUI != null && cardUI == readyToUseCardUI )
            {
                // 恢复卡牌原位
                cardUI.ResetPosition();
                readyToUseCardUI = null;
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

    public void ClearReadyToUseCard()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        readyToUseCardUI = null;
    }
}
