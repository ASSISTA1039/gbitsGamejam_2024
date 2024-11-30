using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FearCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent; // ??????????????????
    public Transform areaParent; // ??????????????????
    private bool isSnappingToTarget = false; // ??????????????????????
    public Vector3 targetPosition;
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI pointTMP;
    public TextMeshProUGUI descriptionTMP;

    public Image background;
    public Image artSprite;
    public Image back;

    public FearCard card;

    //------------------------------
    public CardTun cardTun;

    private void Awake()
    {
        titleTMP = transform.Find("Front/Title/Text").GetComponent<TextMeshProUGUI>();
        pointTMP = transform.Find("Front/Point/Text").GetComponent<TextMeshProUGUI>();
        descriptionTMP = transform.Find("Front/Description/Text").GetComponent<TextMeshProUGUI>();
        background = transform.Find("Front/BackGround").GetComponent<Image>();
        artSprite = transform.Find("Front/ImageMask/Image").GetComponent<Image>();
        back = transform.Find("Back").GetComponent<Image>();

        cardTun = gameObject.GetComponent<CardTun>();
    }

    public void SetUI(FearCard fearCard, Transform originTransform)
    {
        card = fearCard;
        titleTMP.text = fearCard.cardName;
        pointTMP.text = fearCard.point.ToString();
        descriptionTMP.text = fearCard.description;
        background.sprite = fearCard.background;
        artSprite.sprite = fearCard.artSprite;
        back.sprite = fearCard.back;

        originalParent = originTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //originalPosition = transform.position; // ????????????????????
        //originalParent = transform.parent; // ??????????????
        //transform.SetParent(parentCanvas.transform); // ????????Canvas????????????
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // ????????????
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnappingToTarget)
        {
            transform.SetParent(areaParent);
            transform.position = areaParent.position;
            CardTargetArea area = areaParent.gameObject.GetComponent<CardTargetArea>();
            //??????????????????????????????????????????????????????????
            area.GetReadyToUseCard(this);
        }
        else
        {
            ReturnToHand();
        }
    }

    public void ReturnToHand()
    {
        transform.SetParent(originalParent); // ??????????
        transform.position = originalParent.position;
        //cardTun = gameObject.GetComponent<CardTun>();
        //if (cardTun != null)
        //{
        //    cardTun.StartFront();
        //}
        //else
        //{
        //    Debug.LogWarning("?????????????????? CardTun ????");
        //}
    }

    public void SnapToTarget(Transform areaTransform)
    {
        // ??????????????
        areaParent = areaTransform;
        isSnappingToTarget = true;
        
        //if (cardTun != null)
        //{
        //    cardTun.StartBack();
        //}
        //else
        //{
        //    Debug.LogWarning("?????????????????? CardTun ????");
        //}
    }

    public void ResetPosition()
    {
        // ????????????
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
