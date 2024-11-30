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
    private bool isSnappingToTarget = false; // �Ƿ�����������Ŀ������
    public Vector3 targetPosition;
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI pointTMP;
    public TextMeshProUGUI descriptionTMP;

    public Sprite artSprite;

    public FearCard card;

    //------------------------------
    public CardTun cardTun;

    private void Awake()
    {
        titleTMP = transform.Find("Front/Title/Text").GetComponent<TextMeshProUGUI>();
        pointTMP = transform.Find("Front/Point/Text").GetComponent<TextMeshProUGUI>();
        descriptionTMP = transform.Find("Front/Description/Text").GetComponent<TextMeshProUGUI>();
        artSprite = transform.Find("Front/ImageMask/Image").GetComponent<Image>().sprite;

        cardTun = gameObject.GetComponent<CardTun>();
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
        //cardTun = gameObject.GetComponent<CardTun>();
        //if (cardTun != null)
        //{
        //    cardTun.StartFront();
        //}
        //else
        //{
        //    Debug.LogWarning("ѡ�еĿ���û�й��� CardTun �ű�");
        //}
    }

    public void SnapToTarget(Transform areaTransform)
    {
        // ������Ŀ������
        areaParent = areaTransform;
        isSnappingToTarget = true;
        
        //if (cardTun != null)
        //{
        //    cardTun.StartBack();
        //}
        //else
        //{
        //    Debug.LogWarning("ѡ�еĿ���û�й��� CardTun �ű�");
        //}
    }

    public void ResetPosition()
    {
        // ���ÿ���λ��
        isSnappingToTarget = false;
    }
}
