using System;
using UnityEngine;

public class Deck
{
    private int topIndex;
    private Card[] cards;
    private const int DECK_SIZE = 52;
    private System.Random random;

    public Deck()
    {
        random = new System.Random();
        Reset();
    }

    private void Reset()
    {
        cards = new Card[DECK_SIZE];
        topIndex = 0;
        // fill deck 
        int index = 0;
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            for (int value = 2; value <= 14; value++)
            {
                cards[index++] = new Card(suit, value);
            }
        }
    }

    public void Shuffle()
    {
        for (int i = cards.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); 
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        topIndex = 0;
    }

    public Card Pull()
    { 
        if (topIndex < cards.Length)
        {
            return cards[topIndex++];
        }
        else
        {
            Debug.Log("Deck is empty!");
            return null;
        }
    }
    public static int GetDeckSize()
    {
        return DECK_SIZE;
    }
    public void ReturnToDeck(Card[] returned)
    {
        int remainingCount = cards.Length - topIndex;
        Card[] pool = new Card[remainingCount + returned.Length];
        Array.Copy(cards, topIndex, pool, 0, remainingCount);
        Array.Copy(returned, 0, pool, remainingCount, returned.Length);

        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        cards = pool;
        topIndex = 0;
    }

}
