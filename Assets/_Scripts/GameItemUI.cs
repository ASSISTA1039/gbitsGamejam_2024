using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GameItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent; // ��¼����ԭʼ������
    public Transform areaParent; // ��¼����ԭʼ������
    //public Canvas parentCanvas; // �������Canvas�����ڴ����϶�
    private bool isSnappingToTarget = false; // �Ƿ�����������Ŀ������
    public Vector3 targetPosition;

    public GameItem item;

    public Image art;
    public Image back;
    public CardTun cardTun;

    public TooltipTrigger tooltipTrigger;

    private void Awake()
    {
        art = transform.Find("Front/Image").GetComponent<Image>();
        back = transform.Find("Back").GetComponent<Image>();
        tooltipTrigger = GetComponent<TooltipTrigger>();

        cardTun = gameObject.GetComponent<CardTun>();
    }

    public void SetUI(GameItem item, Transform originTransform)
    {
        this.item = item;
        this.art.sprite = item.art;
        this.back.sprite = item.back;

        originalParent = originTransform;

    }

    public void SetTooltipText(string header, string context)
    {
        tooltipTrigger.header = header;
        tooltipTrigger.content = context;
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    Debug.Log($"������ߣ�{item.itemName}");
    //    onClickedAction?.Invoke(item);
    //}
    public void OnBeginDrag(PointerEventData eventData)
    {
        //originalPosition = transform.position; // ��¼��ʼ�϶�ʱ��λ��
        //originalParent = transform.parent; // ��¼ԭʼ������
        //transform.SetParent(parentCanvas.transform); // ʹ������Canvas�£��Ա��϶�
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // ��������ƶ�
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnappingToTarget)
        {
            transform.SetParent(areaParent);
            transform.position = areaParent.position;
            ItemTargetArea area = areaParent.gameObject.GetComponent<ItemTargetArea>();
            //��ȷ������û�з��ÿ���ʱ�������뿨�ƣ�һ�غ�ֻ��ʹ��һ�ſ�
            area.GetReadyToUseItem(this);
        }
        else
        {
            ReturnToHand();
        }
    }

    public void ReturnToHand()
    {
        transform.SetParent(originalParent); // �ָ�������
        transform.position = originalParent.position;
    }

    public void SnapToTarget(Transform areaTransform)
    {
        // ������Ŀ������
        areaParent = areaTransform;
        isSnappingToTarget = true;
    }

    public void ResetPosition()
    {
        // ���ÿ���λ��
        isSnappingToTarget = false;
    }

    public void FlipBack(bool istoback)
    {
        if (istoback)
        {
            cardTun.mCardState = CardState.Back;
            cardTun.StartBack();
        }
        else
        {
            cardTun.mCardState = CardState.Front;
            cardTun.StartFront();
        }
    }
}
