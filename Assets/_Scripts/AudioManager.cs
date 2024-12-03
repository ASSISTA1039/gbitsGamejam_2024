using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;
    public AudioSource audioSource;  // AudioSource for playing sound effects
    [Header("Ů��������Ч")]
    public AudioClip BatCardSound; // Sound effect for dealing cards
    public AudioClip BoomCardSound; // Sound effect for choosing card
    public AudioClip BowCardSound; // Sound effect for choosing item
    public AudioClip HammerCardSound; // Sound effect for comparing points
    public AudioClip FireCardSound; // Sound effect for comparing points

    [Header("���޿�����Ч")]
    public AudioClip ChristmasCardSound; // Sound effect for dealing cards
    public AudioClip DentalCardSound; // Sound effect for choosing card
    public AudioClip DogCardSound; // Sound effect for choosing item
    public AudioClip GhostCardSound; // Sound effect for comparing points
    public AudioClip JokerCardSound; // Sound effect for comparing points

    [Header("������Ч")]
    public AudioClip PeekItemSound; // Sound effect for dealing cards
    public AudioClip ChangeCardItemSound; // Sound effect for choosing card
    public AudioClip ForceChangeCardItemSound; // Sound effect for choosing item
    public AudioClip EncourageItemSound; // Sound effect for comparing points
    public AudioClip DivinationItemSound; // Sound effect for comparing points
    public AudioClip SwapCardPointsItemSound; // Sound effect for comparing points

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public Dictionary<string, AudioClip> map = new Dictionary<string, AudioClip>();

    public void InitMap()
    {
        map.Add("BatCard", BatCardSound);
        map.Add("BoomCard", BoomCardSound);
        map.Add("BowCard", BowCardSound);
        map.Add("HammerCard", HammerCardSound);
        map.Add("FireCard", FireCardSound);

        map.Add("ChristmasCard", ChristmasCardSound);
        map.Add("DentalCard", DentalCardSound);
        map.Add("DogCard", DogCardSound);
        map.Add("GhostCard", GhostCardSound);
        map.Add("JokerCard", JokerCardSound);

        map.Add("侦探眼睛", PeekItemSound);
        map.Add("抓娃娃爪子", ChangeCardItemSound);
        map.Add("鬼手", ForceChangeCardItemSound);
        map.Add("壮胆", EncourageItemSound);
        map.Add("占卜", DivinationItemSound);
        map.Add("交互", SwapCardPointsItemSound);
    }

    public void AddMap(string key, AudioClip value)
    {
        map.Add(key, value);
    }

    public AudioClip FindSound(string key)
    {
        return map[key];
    }
    // ������Ч�ķ���
    public void PlayCardSound(string cardName)
    {
        AudioClip clipToPlay = FindSound(cardName);

        // ����ҵ���Ӧ����Ч��������
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    // ���ŵ�����Ч
    public void PlayItemSound(string itemName)
    {
        AudioClip clipToPlay = FindSound(itemName);

        // ����ҵ���Ӧ����Ч��������
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
