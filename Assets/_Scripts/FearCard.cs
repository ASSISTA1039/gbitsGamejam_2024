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

    public string description;
    //public FearCardSO fearCardData;
    public bool isUsed;

    //public FearCard( FearCardSO fearCardData )
    //{
    //    this.fearCardData = fearCardData;
    //    this.isUsed = false;
    //}

    public FearCard(string name, Sprite background, Sprite art, Sprite back, int point, string description)
    {
        this.cardName = name;
        this.background = background;
        this.artSprite = art;
        this.point = point;
        this.back = back;
        this.description = description;
    }

    public FearCard(string name, int point)
    {
        this.cardName = name;
        this.point = point;

        //this.sprite = null;
        this.description = "description";
    }
}
