using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New FearCard", menuName ="Card/FearCardSO")]
public class FearCardSO : ScriptableObject
{
    public string cardName;
    public Sprite sprite;
    public int point;

    public string description;

    public FearCardSO(string name, Sprite img, int point, string description)
    {
        this.cardName = name;
        this.sprite = img;
        this.point = point;
        this.description = description;
    }
}
