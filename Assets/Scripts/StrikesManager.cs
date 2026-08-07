using UnityEngine;

// Owns how many strikes Mario has left, and nothing else. It doesn't move him, doesn't draw
// the number, and doesn't restart the game - it only counts, and says so when the count hits
// zero.
public class StrikesManager : MonoBehaviour
{
    public static event System.Action<int> OnStrikeCountChanged;

    // Raised the instant strikes hit zero. Nothing here reloads the scene, so this can fire
    // inline even though PlayerDeath subscribes to the same hazard event with no guaranteed
    // order against this one - there is no reload left for the two of them to race.
    public static event System.Action OnGameOver;

    [SerializeField] private int startingStrikes = 3;

    private int strikesRemaining;

    private void OnEnable()
    {
        SC_Death.OnHazardCollision += OnHazardCollision;
        StrikePowerUp.OnStrikeGained += OnStrikeGained;
    }

    private void OnDisable()
    {
        SC_Death.OnHazardCollision -= OnHazardCollision;
        StrikePowerUp.OnStrikeGained -= OnStrikeGained;
    }

    private void Awake()
    {
        strikesRemaining = startingStrikes;
    }

    private void Start()
    {
        OnStrikeCountChanged?.Invoke(strikesRemaining);
    }

    private void OnHazardCollision()
    {
        strikesRemaining--;
        Debug.Log("Strike lost - " + strikesRemaining + " remaining");
        OnStrikeCountChanged?.Invoke(strikesRemaining);

        if (strikesRemaining <= 0)
            OnGameOver?.Invoke();
    }

    private void OnStrikeGained()
    {
        // Capped at the starting amount rather than a separate maximum, so the ceiling can't
        // drift away from the number Mario begins with.
        if (strikesRemaining >= startingStrikes)
        {
            Debug.Log("Strike pickup ignored - already at max (" + startingStrikes + ")");
            return;
        }

        strikesRemaining++;
        Debug.Log("Strike gained - " + strikesRemaining + " remaining");
        OnStrikeCountChanged?.Invoke(strikesRemaining);
    }
}
