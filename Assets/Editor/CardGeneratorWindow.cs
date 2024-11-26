using UnityEditor;
using UnityEngine;

public class CardGeneratorWindow : EditorWindow
{
    private string folderPath = "Assets/Card/CardSO"; // ����·��
    private string spriteFolderPath = "Assets/Card/CardSprites"; // Sprite��Դ·��
    private int cardCount = 5; // ��������
    private string baseName = "��"; // ��������
    private int startValue = 1; // ��ʼ�־�ֵ
    private Sprite[] spriteArray;

    [MenuItem("Tools/Card Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<CardGeneratorWindow>("Card Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("����������", EditorStyles.boldLabel);

        // ���뱣��·��
        folderPath = EditorGUILayout.TextField("����·��", folderPath);

        // ����Sprite�ļ���·��
        spriteFolderPath = EditorGUILayout.TextField("Sprite��Դ·��", spriteFolderPath);

        // ���뿨������
        cardCount = EditorGUILayout.IntField("��������", cardCount);

        // �����������
        baseName = EditorGUILayout.TextField("���ƻ�������", baseName);

        // �����ʼ�־�ֵ
        startValue = EditorGUILayout.IntField("��ʼ�־�ֵ", startValue);

        EditorGUILayout.LabelField("ѡ��Sprite��Դ��");
        int spriteCount = EditorGUILayout.IntField("Sprite����", spriteArray != null ? spriteArray.Length : 0);
        if (spriteArray == null || spriteArray.Length != spriteCount)
        {
            spriteArray = new Sprite[spriteCount];
        }

        for (int i = 0; i < spriteCount; i++)
        {
            spriteArray[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", spriteArray[i], typeof(Sprite), false);
        }

        if (GUILayout.Button("���ɿ���"))
        {
            CreateCards();
        }
    }

    private void CreateCards()
    {
        // ȷ��Ŀ���ļ��д���
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", folderPath.Replace("Assets/", ""));
        }

        // ��������Sprite
        Sprite[] sprites = LoadAllSprites(spriteFolderPath);

        // ���ɿ���
        for (int i = 0; i < cardCount; i++)
        {
            // ����һ���µ�ScriptableObjectʵ��
            FearCardSO newCard = ScriptableObject.CreateInstance<FearCardSO>();

            // ��������
            newCard.cardName = $"{baseName}_{i + 1}";
            newCard.point = startValue + i;

            // ������㹻��Sprite���󶨵�����
            if (i < sprites.Length)
            {
                newCard.sprite = sprites[i];
            }

            // ����Ϊ.asset�ļ�
            string assetPath = $"{folderPath}/{newCard.cardName}.asset";
            AssetDatabase.CreateAsset(newCard, assetPath);
        }

        // ˢ����Դ���ݿ�
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"�ɹ����� {cardCount} �ſ��ƣ�");
    }

    // ����ָ��·���µ�����Sprite
    private static Sprite[] LoadAllSprites(string folderPath)
    {
        string[] spriteGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        Sprite[] sprites = new Sprite[spriteGUIDs.Length];
        for (int i = 0; i < spriteGUIDs.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(spriteGUIDs[i]);
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }
        return sprites;
    }
}
