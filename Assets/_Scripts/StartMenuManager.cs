using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public Button startButton;  // 开始按钮
    public Button aboutButton;  // 关于按钮
    public Button exitButton;   // 退出按钮
    public GameObject aboutPanel;  // 关于弹出窗口
    public Sprite curror;

    // Start is called before the first frame update
    void Awake()
    {
        startButton = transform.Find("ButtonLayout/Start").GetComponent<Button>();
        aboutButton = transform.Find("ButtonLayout/About").GetComponent<Button>();
        exitButton = transform.Find("ButtonLayout/Exit").GetComponent<Button>();

        // 给按钮添加点击事件
        startButton.onClick.AddListener(OnStartButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        aboutButton.onClick.AddListener(OnAboutButtonClicked);

        aboutPanel = transform.Find("AboutPanel").gameObject;
        // 初始化时隐藏关于弹出窗口
        aboutPanel.SetActive(false);
    }

    // 点击开始按钮，加载下一个场景
    private void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainScene"); 
    }

    // 点击退出按钮，关闭游戏
    private void OnExitButtonClicked()
    {
        Application.Quit();  // 退出游戏
    }

    // 点击关于按钮，显示开发者名单的弹出窗口
    private void OnAboutButtonClicked()
    {
        aboutPanel.SetActive(true);  // 显示关于窗口
    }

    // 关闭关于窗口
    public void CloseAboutPanel()
    {
        aboutPanel.SetActive(false);  // 隐藏关于窗口
    }
}
