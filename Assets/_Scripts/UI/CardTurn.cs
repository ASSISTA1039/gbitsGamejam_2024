using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum CardState
{
    Front,
    Back
}

public class CardTun : MonoBehaviour
{

    public GameObject mFront;
    public GameObject mBack;
    public CardState mCardState = CardState.Front;  //���Ƶ�ǰ��״̬�������滹�Ǳ���
    public float mTime = 0.5f;
    public bool isActive = false;       //true��������ִ�з�ת�����������

    // ��ʼ�����ƽǶȣ�����mCardState
    public void Init()
    {
        if (mCardState == CardState.Front)
        {
            mFront.transform.eulerAngles = Vector3.zero;
            mBack.transform.eulerAngles = new Vector3(0, 90, 0);
        }
        else
        {
            mBack.transform.eulerAngles = Vector3.zero;
            mFront.transform.eulerAngles = new Vector3(0, 90, 0);
        }
    }

    void Start()
    {
        Init();
    }

    //��ת
    public void StartBack()
    {
        if (isActive)
        {
            return;
        }
        StartCoroutine(ToBack());
    }
    //ת��
    public void StartFront()
    {
        if (isActive)
        {
            return;
        }
        StartCoroutine(ToFront());
    }

    IEnumerator ToBack()
    {
        isActive = true;
        mFront.transform.DORotate(new Vector3(0, 90, 0), mTime);
        for (float i = mTime; i >= 0; i -= Time.deltaTime)
        {
            yield return 0;
        }
        mBack.transform.DORotate(new Vector3(0, 0, 0), mTime);
        isActive = false;
    }

    IEnumerator ToFront()
    {
        isActive = true;
        mBack.transform.DORotate(new Vector3(0, 90, 0), mTime);
        for (float i = mTime; i >= 0; i -= Time.deltaTime)
        {
            yield return 0;
        }
        mFront.transform.DORotate(new Vector3(0, 0, 0), mTime);
        isActive = false;
    }

}