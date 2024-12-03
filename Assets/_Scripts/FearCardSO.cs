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

    public string description;

    public FearCardSO(string name, Sprite background, Sprite img, string description, AudioClip musicFX)
    {
        this.cardName = name;
        this.background = background;
        this.artSprite = img;
        this.description = description;
        this.musicFX = musicFX;
    }
}
