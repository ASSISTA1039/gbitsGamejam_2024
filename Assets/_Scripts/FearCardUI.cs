using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FearCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent; // 记录卡牌原始父物体
    public Transform areaParent; // 记录卡牌原始父物体
    //public Canvas parentCanvas; // 父物体的Canvas，用于处理拖动
    private bool isSnappingToTarget = false; // 是否正在吸附到目标区域
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
        //originalPosition = transform.position; // 记录开始拖动时的位置
        //originalParent = transform.parent; // 记录原始父物体
        //transform.SetParent(parentCanvas.transform); // 使卡牌在Canvas下，以便拖动
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // 跟随鼠标移动
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnappingToTarget)
        {
            transform.SetParent(areaParent);
            transform.position = areaParent.position;
            CardTargetArea area = areaParent.gameObject.GetComponent<CardTargetArea>();
            //当确认区域没有放置卡牌时才能拖入卡牌，一回合只能使用一张卡
            area.GetReadyToUseCard(this);
        }
        else
        {
            ReturnToHand();
        }
    }

    public void ReturnToHand()
    {
        transform.SetParent(originalParent); // 恢复父物体
        transform.position = originalParent.position;
    }

    public void SnapToTarget(Transform areaTransform)
    {
        // 吸附到目标区域
        areaParent = areaTransform;
        isSnappingToTarget = true;
    }

    public void ResetPosition()
    {
        // 重置卡牌位置
        isSnappingToTarget = false;
    }
}
