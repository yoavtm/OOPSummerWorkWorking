using System.Collections.Generic;
using UnityEngine;

public class CardSprite : MonoBehaviour
{
    public List<Sprite> Cards = new List<Sprite>();

    public enum Suit { Clubs, Diamonds, Hearts, Spades }
    public Suit CardSuit;
    public int Value; // 2–14

    private SpriteRenderer rnd;

    void Awake() { rnd = GetComponent<SpriteRenderer>(); }

    public void ChangeSprite()
    {
        string valueName = Value switch
        {
            11 => "jack",
            12 => "queen",
            13 => "king",
            14 => "ace",
            _ => Value.ToString()
        };

        string targetName = CardSuit.ToString().ToLower() + "-" + valueName;

        foreach (Sprite s in Cards)
        {
            if (s != null && s.name.ToLower() == targetName)
            {
                rnd.sprite = s;
                return;
            }
        }
        Debug.LogWarning("No matching sprite found for: " + targetName);
    }
}
