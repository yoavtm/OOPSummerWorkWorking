using System;

public class HumanPlayer
{
    private string name;
    private Card[] hand;
    private int handSize;
    private Card[] wonCards;
    private int wonSize;

    public HumanPlayer(string name, int maxHand, int maxWon)
    {
        this.name = name;
        hand = new Card[maxHand];
        handSize = 0;
        wonCards = new Card[maxWon];
        wonSize = 0;
    }

    public string GetName() => name;
    public int GetWonCount() => wonSize;
    public int GetHandCount() => handSize;

    public void ReceiveCard(Card card)
    {
        if (handSize >= hand.Length) { UnityEngine.Debug.LogError("Hand is full"); return; }
        hand[handSize++] = card;
    }

    public bool RemoveFromHand(Card card)
    {
        for (int i = 0; i < handSize; i++)
        {
            if (hand[i] == card)
            {
                for (int j = i; j < handSize - 1; j++) hand[j] = hand[j + 1];
                hand[--handSize] = null;
                return true;
            }
        }
        return false;
    }

    public Card[] GetHandSnapshot()
    {
        Card[] a = new Card[handSize];
        Array.Copy(hand, a, handSize);
        return a;
    }

    public void WinRound(Card[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
            wonCards[wonSize + i] = cards[i];
        wonSize += cards.Length;
    }

    public int GetWonValueSum()
    {
        int sum = 0;
        for (int i = 0; i < wonSize; i++) sum += wonCards[i].GetValue();
        return sum;
    }
}
