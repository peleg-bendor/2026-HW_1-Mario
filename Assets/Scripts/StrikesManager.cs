using UnityEngine;
using UnityEngine.SceneManagement;

public class StrikesManager : MonoBehaviour
{
    public static event System.Action<int> OnStrikeCountChanged;

    [SerializeField] private int startingStrikes = 3;

    private int strikesRemaining;

    // Set true the instant strikes hit 0. The actual scene reload happens in Update(),
    // not here - SceneManager.LoadScene() is blocking, and PlayerDeath is an independent
    // subscriber to the same OnHazardCollision event with no guaranteed order against this
    // one. Reloading inline could tear down the scene mid-dispatch and throw a
    // MissingReferenceException if PlayerDeath's handler ran after this one. Deferring to
    // Update() guarantees every subscriber for this hit has already run first.
    private bool strikesDepleted = false;

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
            strikesDepleted = true;
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

    private void Update()
    {
        if (strikesDepleted)
        {
            Debug.Log("Game over - restarting");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
