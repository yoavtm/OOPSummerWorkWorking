using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CardView : MonoBehaviour
{
    public Card Data { get; private set; }
    public HumanPlayer Owner { get; private set; }

    public void Init(Card data, HumanPlayer owner)
    {
        Data = data;
        Owner = owner;

        var cs = GetComponent<CardSprite>();
        if (cs != null)
        {
            cs.Value = data.GetValue();
            cs.CardSuit = (CardSprite.Suit)System.Enum.Parse(
                typeof(CardSprite.Suit), data.GetSuit().ToString());
            cs.ChangeSprite();
        }

        // Optional: only allow hover for Player 1
        var hover = GetComponent<CardHoverHighlighter>();
        if (hover != null) hover.onlyIfOwnerIsPlayer1 = true;
    }
}
