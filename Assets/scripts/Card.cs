using System;
using UnityEngine;
public enum Suit { Clubs, Diamonds, Hearts, Spades }
public class Card
{
    private Suit suit;
    private int value;
    public Card(Suit suit, int value)
    {
        this.suit = suit;
        this.value = value;
    }
    public int GetValue()
    {
        return value;
    }
    public Suit GetSuit()
    {
        return suit;
    }

    public override String ToString()
    {
        string name;
        if (value == 11) name = "Jack";
        else if (value == 12) name = "Queen";
        else if (value == 13) name = "King";
        else if (value == 14) name = "Ace";
        else name = value.ToString();
        switch (value)
        {
            case 11:
                name = "Jack";
                break;
            case 12:
                name = "Queen";
                break;
            case 13:
                name = "King";
                break;
            case 14:
                name = "Ace";
                break;
            default:
                name = value.ToString();
                break;
        }
        return $"{name} of {suit.ToString()}"; 
    }
}
