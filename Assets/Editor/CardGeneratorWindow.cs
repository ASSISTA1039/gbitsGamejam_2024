using UnityEditor;
using UnityEngine;

public class CardGeneratorWindow : EditorWindow
{
    private string folderPath = "Assets/Card/CardSO"; // 保存路径
    private string spriteFolderPath = "Assets/Card/CardSprites"; // Sprite资源路径
    private int cardCount = 5; // 卡牌数量
    private string baseName = "卡"; // 基础名称
    private int startValue = 1; // 初始恐惧值
    private Sprite[] spriteArray;

    [MenuItem("Tools/Card Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<CardGeneratorWindow>("Card Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("卡牌生成器", EditorStyles.boldLabel);

        // 输入保存路径
        folderPath = EditorGUILayout.TextField("保存路径", folderPath);

        // 输入Sprite文件夹路径
        spriteFolderPath = EditorGUILayout.TextField("Sprite资源路径", spriteFolderPath);

        // 输入卡牌数量
        cardCount = EditorGUILayout.IntField("卡牌数量", cardCount);

        // 输入基础名称
        baseName = EditorGUILayout.TextField("卡牌基础名称", baseName);

        // 输入初始恐惧值
        startValue = EditorGUILayout.IntField("初始恐惧值", startValue);

        EditorGUILayout.LabelField("选择Sprite资源：");
        int spriteCount = EditorGUILayout.IntField("Sprite数量", spriteArray != null ? spriteArray.Length : 0);
        if (spriteArray == null || spriteArray.Length != spriteCount)
        {
            spriteArray = new Sprite[spriteCount];
        }

        for (int i = 0; i < spriteCount; i++)
        {
            spriteArray[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", spriteArray[i], typeof(Sprite), false);
        }

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

        // 加载所有Sprite
        Sprite[] sprites = LoadAllSprites(spriteFolderPath);

        // 生成卡牌
        for (int i = 0; i < cardCount; i++)
        {
            // 创建一个新的ScriptableObject实例
            FearCardSO newCard = ScriptableObject.CreateInstance<FearCardSO>();

            // 设置属性
            newCard.cardName = $"{baseName}_{i + 1}";
            newCard.point = startValue + i;

            // 如果有足够的Sprite，绑定到卡牌
            if (i < sprites.Length)
            {
                newCard.sprite = sprites[i];
            }

            // 保存为.asset文件
            string assetPath = $"{folderPath}/{newCard.cardName}.asset";
            AssetDatabase.CreateAsset(newCard, assetPath);
        }

        // 刷新资源数据库
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"成功生成 {cardCount} 张卡牌！");
    }

    // 加载指定路径下的所有Sprite
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
