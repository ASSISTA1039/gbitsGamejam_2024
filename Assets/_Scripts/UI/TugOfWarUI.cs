using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TugOfWarUI : MonoBehaviour
{
    public RectTransform redBar;   // 红色部分
    public RectTransform blueBar;  // 蓝色部分
    public float totalWidth = 1920; // 总宽度

    public float aScore = 4f;
    public float bScore = 4f;
    private float totalScore = 8f;
    void Start()
    {
        UpdateBars(aScore, bScore);
    }

    public void UpdateBars(float playerScore, float monsterScore)
    {
        aScore = playerScore;
        bScore = monsterScore;

        float redWidth = (aScore / totalScore) * totalWidth;
        float blueWidth = (bScore / totalScore) * totalWidth;

        StartCoroutine(SmoothUpdate(redWidth, blueWidth));
    }

    //public float GetAScore() => aScore;
    //public float GetBScore() => bScore;

    IEnumerator SmoothUpdate(float targetAWidth, float targetBWidth)
    {
        float duration = 0.5f; // 动画持续时间
        float elapsed = 0f;

        Vector2 initialRedSize = redBar.sizeDelta;
        Vector2 initialBlueSize = blueBar.sizeDelta;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);  // 防止值超过1

            redBar.sizeDelta = Vector2.Lerp(initialRedSize, new Vector2(targetAWidth, redBar.sizeDelta.y), t);
            blueBar.sizeDelta = Vector2.Lerp(initialBlueSize, new Vector2(targetBWidth, blueBar.sizeDelta.y), t);

            yield return null;
        }
        redBar.sizeDelta = new Vector2(targetAWidth, redBar.sizeDelta.y);
        blueBar.sizeDelta = new Vector2(targetBWidth, blueBar.sizeDelta.y);
    }
}
