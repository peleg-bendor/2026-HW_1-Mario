using UnityEngine;

public class StrikesManager : MonoBehaviour
{
    public static event System.Action<int> OnStrikeCountChanged;

    // Raised the instant strikes hit 0. GameEndManager is the only subscriber, and it
    // doesn't reload the scene until its own display timer runs out - so unlike the old
    // deferred-to-Update() flag this replaced, firing this inline is safe even though
    // PlayerDeath is an independent subscriber to the same OnHazardCollision event with
    // no guaranteed order against this one: nothing here calls SceneManager.LoadScene(),
    // so there's nothing left to race.
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
        // Capped at startingStrikes rather than a separate hardcoded max - the ceiling
        // is meant to match the starting amount, not some independent number.
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
