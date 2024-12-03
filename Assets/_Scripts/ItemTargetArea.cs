using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTargetArea : MonoBehaviour
{
    private GameItemUI readyToUseItemUI = null;

    public GameItem GetAreaItem()
    {
        return readyToUseItemUI != null ? readyToUseItemUI.item : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item")) // �������Ŀ��������ǿ���
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null)
            {
                // �������Ƶ�Ŀ������
                itemUI.SnapToTarget(transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item")) // ��������뿪Ŀ������
        {
            GameItemUI itemUI = other.GetComponent<GameItemUI>();
            if (itemUI != null && itemUI == readyToUseItemUI)
            {
                // �ָ�����ԭλ
                itemUI.ResetPosition();
                readyToUseItemUI = null;
            }
        }
    }

    public void GetReadyToUseItem(GameItemUI itemUI)
    {
        if (readyToUseItemUI != null)
        {
            readyToUseItemUI.ReturnToHand();
        }
        readyToUseItemUI = itemUI;
    }

    public void ClearReadyToUseItem()
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        readyToUseItemUI = null;
    }

    public void ItemUIFlipBack(bool istoback)
    {
        if (readyToUseItemUI == null)
        {
            return;
        }
        readyToUseItemUI.FlipBack(istoback);
    }
}
