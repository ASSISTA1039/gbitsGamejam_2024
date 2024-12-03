using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public Text answerText;              // 显示回答的文本框
    public float typingSpeed = 0.05f;    // 打字速度，控制每个字符显示的延迟时间
    private string fullText;             // 完整的回答文本
    private Coroutine typingCoroutine;   // 用于存储正在进行的打字协程

    // 启动打字机效果
    public void StartTyping(string text)
    {
        fullText = text;
        answerText.text = "";  // 重置文本框
        typingCoroutine = StartCoroutine(TypeText());
    }

    // 打字机效果实现
    private IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            answerText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // 停止打字机效果并立即显示完整文本
    public void ShowFullText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        answerText.text = fullText;
    }

    // 重置文本为初始状态
    public void ResetText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        answerText.text = "";  // 清空文本
    }

    private void Start()
    {
        // 为文本框添加点击事件
        answerText.GetComponentInParent<Button>().onClick.AddListener(ShowFullText);
    }
}
