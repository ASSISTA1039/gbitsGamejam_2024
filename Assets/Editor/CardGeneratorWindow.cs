using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CardGeneratorWindow : EditorWindow
{
    private string folderPath = "Assets/Card/CardSO"; // 目标文件夹
    private string spriteFolderPath = "Assets/Card/CardSprites"; // Sprite资源文件夹
    private int cardCount = 5; // 卡牌数量
    private string baseName = "Card"; // 卡牌基础名称
    private int startValue = 1; // 起始点数
    private Sprite[] spriteArray; // 卡牌背景和艺术图像

    private string[] descriptions = { "Description 1", "Description 2", "Description 3", "Description 4", "Description 5" }; // 卡牌描述

    [MenuItem("Tools/Card Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<CardGeneratorWindow>("Card Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Card Generator", EditorStyles.boldLabel);

        // 目标文件夹路径
        folderPath = EditorGUILayout.TextField("目标文件夹", folderPath);

        // Sprite资源文件夹路径
        spriteFolderPath = EditorGUILayout.TextField("Sprite资源文件夹", spriteFolderPath);

        // 卡牌数量
        cardCount = EditorGUILayout.IntField("卡牌数量", cardCount);

        // 卡牌基础名称
        baseName = EditorGUILayout.TextField("卡牌基础名称", baseName);

        // 起始点数
        startValue = EditorGUILayout.IntField("起始点数", startValue);

        EditorGUILayout.LabelField("选择Sprite资源");

        // 获取Sprite数量
        int spriteCount = EditorGUILayout.IntField("Sprite数量", spriteArray != null ? spriteArray.Length : 0);
        if (spriteArray == null || spriteArray.Length != spriteCount)
        {
            spriteArray = new Sprite[spriteCount];
        }

        // 显示Sprite选择框
        for (int i = 0; i < spriteCount; i++)
        {
            spriteArray[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", spriteArray[i], typeof(Sprite), false);
        }

        // 生成卡牌按钮
        if (GUILayout.Button("生成卡牌"))
        {
            CreateCards();
        }
    }

    private void CreateCards()
    {
        // 确保目标文件夹存在
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", folderPath.Replace("Assets/", ""));
        }

        // 获取所有Sprite资源
        Sprite[] sprites = LoadAllSprites(spriteFolderPath);

        // 创建卡牌
        for (int i = 0; i < cardCount; i++)
        {
            // 创建ScriptableObject实例
            FearCardSO newCard = ScriptableObject.CreateInstance<FearCardSO>();

            // 设置卡牌名称
            newCard.cardName = $"{baseName}_{i + 1}";
            newCard.minpoint = startValue + i;
            newCard.maxpoint = newCard.minpoint + Random.Range(1, 5); // 随机生成最大点数

            // 设置卡牌背景和艺术图像，如果有足够的Sprite
            if (i < sprites.Length)
            {
                newCard.background = sprites[i];
                newCard.artSprite = sprites[i]; // 假设背景和艺术图像是同一张图片
            }

            // 设置卡牌描述
            if (i < descriptions.Length)
            {
                newCard.victoryDescription = descriptions[i];
            }

            // 保存为.asset文件
            string assetPath = $"{folderPath}/{newCard.cardName}.asset";
            AssetDatabase.CreateAsset(newCard, assetPath);
        }

        // 刷新资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"成功生成 {cardCount} 张卡牌！");
    }

    // 加载指定文件夹下的所有Sprite资源
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
