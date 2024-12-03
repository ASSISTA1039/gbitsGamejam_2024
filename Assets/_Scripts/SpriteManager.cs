using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoBehaviour
{
    public Sprite CardBackSprite; // Sprite effect for comparing points
    public Sprite ItemBackSprite; // Sprite effect for comparing points
    //[Header("Ů������ͼƬ")]
    //public Sprite BatCardSprite; // Sprite effect for dealing cards
    //public Sprite BoomCardSprite; // Sprite effect for choosing card
    //public Sprite BowCardSprite; // Sprite effect for choosing item
    //public Sprite HammerCardSprite; // Sprite effect for comparing points
    //public Sprite FireCardSprite; // Sprite effect for comparing points

    //[Header("���޿���ͼƬ")]
    //public Sprite ChristmasCardSprite; // Sprite effect for dealing cards
    //public Sprite DentalCardSprite; // Sprite effect for choosing card
    //public Sprite DogCardSprite; // Sprite effect for choosing item
    //public Sprite GhostCardSprite; // Sprite effect for comparing points
    //public Sprite JokerCardSprite; // Sprite effect for comparing points

    [Header("����ͼƬ")]
    public Sprite PeekItemSprite; // Sprite effect for dealing cards
    public Sprite ChangeCardItemSprite; // Sprite effect for choosing card
    public Sprite ForceChangeCardItemSprite; // Sprite effect for choosing item
    public Sprite EncourageItemSprite; // Sprite effect for comparing points
    public Sprite DivinationItemSprite; // Sprite effect for comparing points
    public Sprite SwapCardPointsItemSprite; // Sprite effect for comparing points
    
    public Dictionary<string, Sprite> map = new Dictionary<string, Sprite>();

    public void InitMap()
    {
        map.Add("BackCard", CardBackSprite);
        map.Add("BackItem", ItemBackSprite);

        //map.Add("BatCard", BatCardSprite);
        //map.Add("BoomCard", BoomCardSprite);
        //map.Add("BowCard", BowCardSprite);
        //map.Add("HammerCard", HammerCardSprite);
        //map.Add("FireCard", FireCardSprite);

        //map.Add("ChristmasCard", ChristmasCardSprite);
        //map.Add("DentalCard", DentalCardSprite);
        //map.Add("DogCard", DogCardSprite);
        //map.Add("GhostCard", GhostCardSprite);
        //map.Add("JokerCard", JokerCardSprite);

        map.Add("侦探眼睛", PeekItemSprite);
        map.Add("抓娃娃爪子", ChangeCardItemSprite);
        map.Add("鬼手", ForceChangeCardItemSprite);
        map.Add("壮胆", EncourageItemSprite);
        map.Add("占卜", DivinationItemSprite);
        map.Add("交互", SwapCardPointsItemSprite);
    }

    public void AddMap(string key, Sprite value)
    {
        map.Add(key, value);
    }

    public Sprite FindSprite(string key)
    {
        return map[key];
    }
}
