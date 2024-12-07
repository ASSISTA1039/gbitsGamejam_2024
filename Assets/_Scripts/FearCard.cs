using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FearCard 
{
    public string cardName;
    public Sprite background;
    public Sprite artSprite;
    public Sprite back;
    public int point;

    //以胜者卡牌为准，回合结束胜方播放胜者卡牌胜利文案，败者播放胜者卡牌失败文案
    public string victoryDescription;
    public string defeatDescription;
    //平局则梦兽（玩家）先播放梦兽卡牌平局文案，然后女孩（敌人）播放女孩卡牌平局文案
    public string tieDescription;
    public FearCardSO fearCardData;

    public bool isUsed;

    public FearCard(FearCardSO fearCardData)
    {
        this.fearCardData = fearCardData;
        this.isUsed = false;
    }

    public FearCard(string name, Sprite background, Sprite art, Sprite back, int point, string victoryDescription, string defeatDescription, string tieDescription)
    {
        this.cardName = name;
        this.background = background;
        this.artSprite = art;
        this.point = point;
        this.back = back;
        this.victoryDescription = victoryDescription;
        this.defeatDescription = defeatDescription;
        this.tieDescription = tieDescription;
    }

    public FearCard(string name, int point)
    {
        this.cardName = name;
        this.point = point;

        //this.sprite = null;
        this.victoryDescription = "description";
    }
}
