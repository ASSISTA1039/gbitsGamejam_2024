using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FearCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent; // ��¼����ԭʼ������
    public Transform areaParent; // ��¼����ԭʼ������
    //public Canvas parentCanvas; // �������Canvas�����ڴ����϶�
    private bool isSnappingToTarget = false; // �Ƿ�����������Ŀ������
    public Vector3 targetPosition;

    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI pointTMP;
    public TextMeshProUGUI descriptionTMP;

    public Sprite artSprite;

    public FearCard card;

    private void Awake()
    {
        //parentCanvas = GetComponentInParent<Canvas>();
        titleTMP = transform.Find("Title/Text").GetComponent<TextMeshProUGUI>();
        pointTMP = transform.Find("Point/Text").GetComponent<TextMeshProUGUI>();
        descriptionTMP = transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();
        artSprite = transform.Find("ImageMask/Image").GetComponent<Image>().sprite;
    }

    public void SetUI(FearCard fearCard, Transform originTransform)
    {
        card = fearCard;
        titleTMP.text = fearCard.cardName;
        pointTMP.text = fearCard.point.ToString();
        descriptionTMP.text = fearCard.description;
        artSprite = fearCard.sprite;

        originalParent = originTransform;
    }

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
            area.GetReadyToUseCard(this);
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
