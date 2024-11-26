using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardManager 
{
    void InitCards(List<FearCard> deck);
    void AddCard(FearCard card);
    void UseCard(FearCard card);
    void RemoveCard(FearCard card);

}
