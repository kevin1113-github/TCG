using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    [SerializeField]
    public enum CardType
    {
        Character,
        Effect
    }

    public string cardName;
    public Sprite cardImage;
    public CardType cardType;

    public string description;
    public int health = 1;
    public int attack = 0;
    public int cost = 1;

    public int attackCount = 0;
}