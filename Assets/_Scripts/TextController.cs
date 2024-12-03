using UnityEngine;
using TMPro;
using System.Collections;

public class TextScroller : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;  // TextMeshProUGUI 组件
    public float scrollSpeed = 50f;  // 滚动速度
    private float curHeight = 0;
    float startPosition;
    float endPosition;

    private RectTransform rectTransform;
    private bool isScrolling = false;  // 控制是否有正在进行的滚动

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        rectTransform = textMeshPro.GetComponent<RectTransform>();
        textMeshPro.text = "";  // 初始化时文本为空
        startPosition = rectTransform.anchoredPosition.y;
    }

    public void Scroll()
    {
        // 只有当没有正在进行的滚动时才开始新的滚动
        if (!isScrolling)
        {
            StartCoroutine(ScrollText());
        }
    }

    private IEnumerator ScrollText()
    {
        yield return new WaitUntil(() => isScrolling == false);
        isScrolling = true;  // 标记为正在滚动

        // 获取文本的高度
        float textHeight = textMeshPro.preferredHeight;

        // 获取文本框的高度
        float textBoxHeight = rectTransform.rect.height;

        // 如果文本高度大于文本框高度，则开始滚动
        if (textHeight > textBoxHeight)
        {
            // 计算超出部分的高度
            float overflowHeight = textHeight - textBoxHeight;

            // 计算每次滚动的距离，确保不超过超出部分的高度
            float scrollAmount = Mathf.Min(overflowHeight, textBoxHeight);

            // 计算目标位置
            endPosition = startPosition + scrollAmount;

            // 更新 curHeight
            curHeight += scrollAmount;

            // 确保不会滚动超过文本的底部
            if (curHeight > overflowHeight)
            {
                curHeight = overflowHeight;
                endPosition = startPosition + overflowHeight;  // 防止超出范围
            }

            float journeyLength = Mathf.Abs(endPosition - startPosition);
            float journeyCovered = 0f;

            while (journeyCovered < journeyLength)
            {
                // 计算滚动的进度
                journeyCovered += scrollSpeed * Time.deltaTime;

                // 使用 Lerp 进行平滑滚动
                float newPosition = Mathf.Lerp(startPosition, endPosition, journeyCovered / journeyLength);

                // 更新文本位置
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newPosition);

                yield return null;
            }

            // 确保滚动完后，文本位置为结束位置
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, endPosition);
        }
        isScrolling = false;  // 标记为滚动结束
    }
}
