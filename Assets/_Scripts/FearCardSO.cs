using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New FearCard", menuName ="Card/FearCardSO")]
public class FearCardSO : ScriptableObject
{
    public string cardName;
    public Sprite background;
    public Sprite artSprite;
    public Sprite back;
    public int minpoint;
    public int maxpoint;
    public AudioClip musicFX;

    //以胜者卡牌为准，回合结束胜方播放胜者卡牌胜利文案，败者播放胜者卡牌失败文案
    public string victoryDescription;
    public string defeatDescription;
    //平局则梦兽（玩家）先播放梦兽卡牌平局文案，然后女孩（敌人）播放女孩卡牌平局文案
    public string tieDescription;

    public FearCardSO(string name, Sprite background, Sprite img, string description, AudioClip musicFX)
    {
        this.cardName = name;
        this.background = background;
        this.artSprite = img;
        this.victoryDescription = description;
        this.musicFX = musicFX;
    }
}
