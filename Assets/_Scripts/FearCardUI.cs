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
    public TextMeshProUGUI pointTMP;

    public Image background;
    public Image artSprite;
    public Image back;

    public FearCard card;

    //------------------------------
    public CardTun cardTun;

    private void Awake()
    {
        pointTMP = transform.Find("Front/Point/Text").GetComponent<TextMeshProUGUI>();
        background = transform.Find("Front/BackGround").GetComponent<Image>();
        artSprite = transform.Find("Front/ImageMask/Image").GetComponent<Image>();
        back = transform.Find("Back").GetComponent<Image>();

        cardTun = gameObject.GetComponent<CardTun>();
    }

    public void SetUI(FearCard fearCard, Transform originTransform)
    {
        card = fearCard;
        pointTMP.text = fearCard.point.ToString();
        background.sprite = fearCard.background;
        artSprite.sprite = fearCard.artSprite;
        back.sprite = fearCard.back;

        originalParent = originTransform;
    }

    public void UpdatePoint(int point)
    {
        pointTMP.text = point.ToString();
    }

    public void UpdateSprite(Sprite background, Sprite art)
    {
        this.background.sprite = background;
        this.artSprite.sprite = art;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {

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
    }

    public void SnapToTarget(Transform areaTransform)
    {
        // ??????????????
        areaParent = areaTransform;
        isSnappingToTarget = true;
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
