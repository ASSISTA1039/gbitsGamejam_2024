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

    public Sprite artSprite;
    public Action<GameItem> onClickedAction;

    private void Awake()
    {
        artSprite = transform.Find("Image").GetComponent<Image>().sprite;
    }

    public void SetUI(GameItem item, Transform originTransform, Action<GameItem> callback)
    {
        this.item = item;
        this.artSprite = item.sprite;
        this.onClickedAction = callback;

        originalParent = originTransform;
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
            CardTargetArea area = areaParent.gameObject.GetComponent<CardTargetArea>();
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
}
