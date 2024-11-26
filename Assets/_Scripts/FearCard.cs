using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearCard 
{
    public string cardName;
    public Sprite sprite;
    public int point;

    public string description;
    //public FearCardSO fearCardData;
    public bool isUsed;

    //public FearCard( FearCardSO fearCardData )
    //{
    //    this.fearCardData = fearCardData;
    //    this.isUsed = false;
    //}

    public FearCard(string name, Sprite img, int point, string description)
    {
        this.cardName = name;
        this.sprite = img;
        this.point = point;
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
