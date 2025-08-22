// GameManager.cs  (only relevant diffs added)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // NEW: hover pause flag
    public bool IsAnimatingHand { get; private set; } = false;

    [Header("Config")]
    [SerializeField] private int MAXROUNDS = 7;
    [SerializeField] private int initialHandSize = 7;
    [SerializeField] private GameObject cardPrefab;

    [Header("Anchors")]
    [SerializeField] private Transform p1Anchor;
    [SerializeField] private Transform p2Anchor;
    [SerializeField] private Transform p1SelectedAnchor;
    [SerializeField] private Transform p2SelectedAnchor;

    [Header("Layout")]
    [SerializeField] private float spacing = 1.4f;
    [SerializeField] private float moveDuration = 0.2f;

    [Header("Timing")]
    [SerializeField] private float holdInCenterSeconds = 0.6f;

    [Header("Winning Piles UI")]
    [SerializeField] private WinningPileUI p1WinPileUI;
    [SerializeField] private WinningPileUI p2WinPileUI;

    [Header("End Game UI")]
    [SerializeField] private WinnerUI winnerUI;

    private Deck deck;
    private HumanPlayer player1;
    private HumanPlayer player2;
    private int currentRound;

    private readonly List<CardView> p1Views = new();
    private readonly List<CardView> p2Views = new();

    private CardView p1PlayedView;
    private CardView p2PlayedView;

    private bool gameOver;

    void Awake()
    {
        Instance = this;

        deck = new Deck();
        player1 = new HumanPlayer("Player 1", initialHandSize, Deck.GetDeckSize());
        player2 = new HumanPlayer("Player 2", initialHandSize, Deck.GetDeckSize());
        currentRound = 0;
        gameOver = false;
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (gameOver && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        deck.Shuffle();
        DealInitialCards();
        SpawnAndLayoutHands();

        if (p1WinPileUI) p1WinPileUI.ResetToZero();
        if (p2WinPileUI) p2WinPileUI.ResetToZero();
        if (winnerUI) winnerUI.Hide();

        StartCoroutine(MainLoop());
    }

    void DealInitialCards()
    {
        for (int i = 0; i < initialHandSize; i++)
        {
            player1.ReceiveCard(deck.Pull());
            player2.ReceiveCard(deck.Pull());
        }
    }

    IEnumerator MainLoop()
    {
        while (currentRound < MAXROUNDS)
        {
            p1PlayedView = null;
            p2PlayedView = null;

            yield return StartCoroutine(WaitForP1Selection());
            yield return StartCoroutine(BotPlayRandom());
            yield return StartCoroutine(ResolveRound());

            currentRound++;
        }

        DisplayScore();
        gameOver = true;
    }

    IEnumerator WaitForP1Selection()
    {
        while (p1PlayedView == null)
        {
            if (LeftClickDown())
            {
                CardView v = CardUnderMouseTopmost();
                if (v != null && v.Owner == player1 && p1Views.Contains(v))
                {
                    if (!player1.RemoveFromHand(v.Data)) yield break;
                    p1Views.Remove(v);

                    // >>> BEGIN hover pause
                    IsAnimatingHand = true;
                    yield return StartCoroutine(MoveTo(v.transform, p1SelectedAnchor.position, moveDuration));
                    p1PlayedView = v;
                    LayoutHand(p1Views, p1Anchor, false); // reflow hand
                    // small end-of-frame so hover sees final positions
                    yield return null;
                    IsAnimatingHand = false;
                    // <<< END hover pause
                }
            }
            yield return null;
        }
    }

    IEnumerator BotPlayRandom()
    {
        if (p2Views.Count == 0) yield break;

        int idx = Random.Range(0, p2Views.Count);
        CardView v = p2Views[idx];

        if (!player2.RemoveFromHand(v.Data)) yield break;
        p2Views.RemoveAt(idx);

        // >>> BEGIN hover pause (covers human row too, for safety)
        IsAnimatingHand = true;
        yield return StartCoroutine(MoveTo(v.transform, p2SelectedAnchor.position, moveDuration));
        p2PlayedView = v;
        LayoutHand(p2Views, p2Anchor, true);
        yield return null;
        IsAnimatingHand = false;
        // <<< END hover pause
    }

    IEnumerator ResolveRound()
    {
        if (p1PlayedView == null || p2PlayedView == null) yield break;

        Card c1 = p1PlayedView.Data;
        Card c2 = p2PlayedView.Data;

        int winner = CompareCards(c1, c2);

        if (winner == 0)
            Debug.Log($"Tie: P1 played {c1}, P2 played {c2}");
        else if (winner == 1)
            Debug.Log($"Player 1 has the higher card. P1 played {c1}, P2 played {c2}");
        else
            Debug.Log($"Player 2 has the higher card. P1 played {c1}, P2 played {c2}");

        if (winner == 1)
            p1PlayedView.GetComponent<CardWinGlow>()?.PlayGlow(holdInCenterSeconds);
        else if (winner == 2)
            p2PlayedView.GetComponent<CardWinGlow>()?.PlayGlow(holdInCenterSeconds);

        yield return new WaitForSeconds(holdInCenterSeconds);

        if (winner == 1)
            player1.WinRound(new[] { c1, c2 });
        else if (winner == 2)
            player2.WinRound(new[] { c1, c2 });
        else
            deck.ReturnToDeck(new[] { c1, c2 });

        if (p1WinPileUI) p1WinPileUI.SetSum(player1.GetWonValueSum());
        if (p2WinPileUI) p2WinPileUI.SetSum(player2.GetWonValueSum());

        yield return StartCoroutine(FadeAndDestroy(p1PlayedView.gameObject, 0.15f));
        yield return StartCoroutine(FadeAndDestroy(p2PlayedView.gameObject, 0.15f));
    }

    public int CompareCards(Card c1, Card c2)
    {
        if (c1.GetValue() > c2.GetValue()) return 1;
        if (c1.GetValue() < c2.GetValue()) return 2;
        return 0;
    }

    public void DisplayScore()
    {
        int s1 = player1.GetWonValueSum();
        int s2 = player2.GetWonValueSum();
        string msg = s1 > s2 ? "You Won!" : s2 > s1 ? "You Lose" : "Tie game!";
        Debug.Log($"P1 sum {s1}, P2 sum {s2}. {msg}");
        if (winnerUI) winnerUI.Show(msg);
    }

    void SpawnAndLayoutHands()
    {
        foreach (var v in p1Views) if (v) Destroy(v.gameObject);
        foreach (var v in p2Views) if (v) Destroy(v.gameObject);
        p1Views.Clear(); p2Views.Clear();

        foreach (var c in player1.GetHandSnapshot())
            p1Views.Add(SpawnCard(c, player1));
        foreach (var c in player2.GetHandSnapshot())
            p2Views.Add(SpawnCard(c, player2));

        LayoutHand(p1Views, p1Anchor, false);
        LayoutHand(p2Views, p2Anchor, true);
    }

    CardView SpawnCard(Card c, HumanPlayer owner)
    {
        var go = Instantiate(cardPrefab);
        var view = go.GetComponent<CardView>();
        view.Init(c, owner);
        return view;
    }

    void LayoutHand(List<CardView> views, Transform anchor, bool topRow)
    {
        int n = views.Count;
        float startX = -spacing * (n - 1) * 0.5f;

        for (int i = 0; i < n; i++)
        {
            var v = views[i];
            if (!v) continue;
            v.transform.position = anchor.position + new Vector3(startX + i * spacing, 0f, 0f);

            var sr = v.GetComponent<SpriteRenderer>();
            if (sr) sr.sortingOrder = i + (topRow ? 100 : 0);
        }
    }

    bool LeftClickDown()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    CardView CardUnderMouseTopmost()
    {
        if (Camera.main == null) return null;

        Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        float z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z));
        Vector2 p = new Vector2(world.x, world.y);

        var hits = Physics2D.OverlapPointAll(p);
        CardView best = null;
        int bestOrder = int.MinValue;

        foreach (var h in hits)
        {
            var v = h.GetComponent<CardView>();
            var sr = h.GetComponent<SpriteRenderer>();
            if (v != null && sr != null && sr.sortingOrder > bestOrder)
            {
                best = v;
                bestOrder = sr.sortingOrder;
            }
        }
        return best;
    }

    IEnumerator MoveTo(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(start, target, a);
            yield return null;
        }
        t.position = target;
    }

    IEnumerator FadeAndDestroy(GameObject go, float duration)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        Color start = sr ? sr.color : Color.white;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            if (sr) sr.color = new Color(start.r, start.g, start.b, 1f - a);
            yield return null;
        }
        Destroy(go);
    }
}
