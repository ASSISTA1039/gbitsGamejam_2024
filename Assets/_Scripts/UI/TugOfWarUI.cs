using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TugOfWarUI : MonoBehaviour
{
    public RectTransform girlBar;   // 
    public RectTransform monsterBar;  // ��ɫ����
    public float totalWidth = 1920; // �ܿ���

    public float girlScore = 4f;
    public float monsterScore = 4f;
    private float totalScore = 8f;
    void Start()
    {
        UpdateBars(girlScore, monsterScore);
    }

    public void UpdateBars(float monsterScore, float girlScore)
    {
        this.monsterScore = monsterScore;
        this.girlScore = girlScore;

        float monsterWidth = (this.monsterScore / totalScore) * totalWidth;
        float girlWidth = (this.girlScore / totalScore) * totalWidth;
        if(girlWidth < monsterWidth)
        {
            girlBar.transform.SetAsFirstSibling();
        }
        else
        {
            monsterBar.transform.SetAsFirstSibling();
        }

        StartCoroutine(SmoothUpdate(girlWidth, monsterWidth));
    }

    public void UpdateBars(float diff)//梦兽的恐惧值减去小女孩的恐惧值后的差值
    {
        float monsterWidth = ((this.monsterScore - diff) / totalScore) * totalWidth;
        float girlWidth = ((this.girlScore + diff) / totalScore) * totalWidth;
        if (girlWidth < monsterWidth)
        {
            girlBar.transform.SetAsFirstSibling();
        }
        else
        {
            monsterBar.transform.SetAsFirstSibling();
        }

        StartCoroutine(SmoothUpdate(girlWidth, monsterWidth));
    }

    //public float GetAScore() => aScore;
    //public float GetBScore() => bScore;

    IEnumerator SmoothUpdate(float targetAWidth, float targetBWidth)
    {
        float duration = 1f; // ��������ʱ��
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);  // ��ֵֹ����1

            Vector2 initialRedSize = girlBar.sizeDelta;
            Vector2 initialBlueSize = monsterBar.sizeDelta;

            girlBar.sizeDelta = Vector2.Lerp(initialRedSize, new Vector2(targetAWidth, girlBar.sizeDelta.y), t);
            monsterBar.sizeDelta = Vector2.Lerp(initialBlueSize, new Vector2(targetBWidth, monsterBar.sizeDelta.y), t);

            yield return null;
        }
        girlBar.sizeDelta = new Vector2(targetAWidth, girlBar.sizeDelta.y);
        monsterBar.sizeDelta = new Vector2(targetBWidth, monsterBar.sizeDelta.y);
        yield return null;
    }
}
