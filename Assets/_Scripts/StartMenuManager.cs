using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public Button startButton;  // ��ʼ��ť
    public Button aboutButton;  // ���ڰ�ť
    public Button exitButton;   // �˳���ť
    public GameObject aboutPanel;  // ���ڵ�������
    public Sprite curror;

    // Start is called before the first frame update
    void Awake()
    {
        startButton = transform.Find("ButtonLayout/Start").GetComponent<Button>();
        aboutButton = transform.Find("ButtonLayout/About").GetComponent<Button>();
        exitButton = transform.Find("ButtonLayout/Exit").GetComponent<Button>();

        // ����ť��ӵ���¼�
        startButton.onClick.AddListener(OnStartButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        aboutButton.onClick.AddListener(OnAboutButtonClicked);

        aboutPanel = transform.Find("AboutPanel").gameObject;
        // ��ʼ��ʱ���ع��ڵ�������
        aboutPanel.SetActive(false);
    }

    // �����ʼ��ť��������һ������
    private void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainScene"); 
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
}
