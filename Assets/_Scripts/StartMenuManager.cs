using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartMenuManager : MonoBehaviour
{
    public Button startButton;  // ��ʼ��ť
    public Button aboutButton;  // ���ڰ�ť
    public Button exitButton;   // �˳���ť
    public GameObject aboutPanel;  // ���ڵ�������
    public Sprite cursor;

    public AudioSource bgm;
    public VideoPlayer videoPlayer; // 用于播放视频
    public RawImage rawImage; // 显示视频的RawImage
    public VideoClip startClip;     // 开场视频

    // Start is called before the first frame update
    void Awake()
    {
        startButton = transform.Find("ButtonLayout/Start").GetComponent<Button>();
        aboutButton = transform.Find("ButtonLayout/About").GetComponent<Button>();
        exitButton = transform.Find("ButtonLayout/Exit").GetComponent<Button>();

        // ����ť���ӵ���¼�
        startButton.onClick.AddListener(OnStartButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        aboutButton.onClick.AddListener(OnAboutButtonClicked);

        aboutPanel = transform.Find("AboutPanel").gameObject;
        aboutPanel.SetActive(false);
        bgm = transform.Find("BGM").GetComponent<AudioSource>();
    }

    // �����ʼ��ť��������һ������
    private void OnStartButtonClicked()
    {
        PlayStartVideo();
    }

    // ����˳���ť���ر���Ϸ
    private void OnExitButtonClicked()
    {
        Application.Quit();  // �˳���Ϸ
    }

    // ������ڰ�ť����ʾ�����������ĵ�������
    private void OnAboutButtonClicked()
    {
        aboutPanel.SetActive(true);  // ��ʾ���ڴ���
    }

    // �رչ��ڴ���
    public void CloseAboutPanel()
    {
        aboutPanel.SetActive(false);  // ���ع��ڴ���
    }

    #region 视频播放
    private bool isVideoPlayed;

    private void PlayStartVideo()
    {
        PlayVideo(startClip);
    }

    // 播放视频的通用方法
    private void PlayVideo(VideoClip clip)
    {
        bgm.Stop();
        isVideoPlayed = false;
        // 设置 VideoPlayer 的视频资源
        videoPlayer.clip = clip;

        // 显示视频 RawImage
        rawImage.gameObject.SetActive(true);
        rawImage.transform.SetAsLastSibling();
        // 播放视频
        videoPlayer.Play();

        // 等待视频播放结束
        videoPlayer.loopPointReached += EndOfVideo; // 注册回调函数

        // 禁用其他 UI 控件，防止在播放视频时进行操作
        DisableUIElements();
    }

    // 视频播放结束后的回调函数
    private void EndOfVideo(VideoPlayer vp)
    {
        // 视频播放完成后，恢复 UI 控件
        EnableUIElements();
        rawImage.gameObject.SetActive(false);
        isVideoPlayed = true;

        SceneManager.LoadScene("MainScene");
        // 在播放完视频后，可以执行后续的逻辑，比如进入下一场景、重启游戏等
        Debug.Log("视频播放完毕");
    }

    // 禁用 UI 元素
    private void DisableUIElements()
    {
        // 禁用游戏中的其他 UI 元素，防止操作
        // 比如禁用按钮，文本，等
        startButton.interactable = false;
        aboutButton.interactable = false;
        exitButton.interactable = false;
    }

    // 启用 UI 元素
    private void EnableUIElements()
    {
        // 恢复 UI 元素的交互
        startButton.interactable = true;
        aboutButton.interactable = true;
        exitButton.interactable = true;
    }
    #endregion
}
