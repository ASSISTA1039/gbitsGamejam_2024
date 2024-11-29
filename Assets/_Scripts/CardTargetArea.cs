using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTargetArea : MonoBehaviour
{
    private FearCardUI readyToUseCardUI = null;
    private GameItemUI readyToUseItemUI = null;

    public FearCard GetAreaCard()
    {
        return readyToUseCardUI != null ? readyToUseCardUI.card : null;
    }    
    public GameItem GetAreaItem()
    {
        return readyToUseItemUI != null ? readyToUseItemUI.item : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Card")) // �������Ŀ��������ǿ���
        {
            FearCardUI cardUI = other.GetComponent<FearCardUI>();
            if (cardUI != null)
            {
                // �������Ƶ�Ŀ������
                cardUI.SnapToTarget(transform);
            }
        }

        if (other.CompareTag("Item")) // �������Ŀ��������ǿ���
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null)
            {
                // �������ߵ�Ŀ������
                itemUI.SnapToTarget(transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Card")) // ��������뿪Ŀ������
        {
            FearCardUI cardUI = other.GetComponent<FearCardUI>();
            if (cardUI != null && cardUI == readyToUseCardUI)
            {
                // �ָ�����ԭλ
                cardUI.ResetPosition();
                readyToUseCardUI = null;
            }
        }

        if (other.CompareTag("Item")) // ��������뿪Ŀ������
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null && itemUI == readyToUseItemUI )
            {
                // �ָ�����ԭλ
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
