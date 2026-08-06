using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the "how does the game end" sequence: pick a message, reload right away, and hand
// the message to whatever GameEndMessageManager exists after the reload. StrikesManager and
// Portal only report that their own condition happened - they don't know this class exists,
// same as every other publisher/subscriber pair in this project (SC_Death -> StrikesManager,
// StrikePowerUp -> StrikesManager, etc.).
public class GameEndManager : MonoBehaviour
{
    [SerializeField] private Color gameOverColor = Color.red;
    [SerializeField] private Color gameWonColor = Color.green;

    private const string GameOverMessage = "GAME OVER";
    private const string GameWonMessage = "GAME WON";

    // None until either event fires; then locked to whichever outcome actually won.
    private enum EndReason { None, GameOver, GameWon }
    private EndReason reason = EndReason.None;

    // Set right before the reload below, read back once by GameEndMessageManager after the
    // reload. Static fields aren't tied to any scene, so this carries straight across
    // SceneManager.LoadScene() within the same Play session - no DontDestroyOnLoad, no
    // second Canvas needed just to keep a 1-second message alive across a reload.
    private static string pendingMessage;
    private static Color pendingColor;
    private static bool hasPendingMessage;

    public static bool TryConsumePendingMessage(out string message, out Color color)
    {
        message = pendingMessage;
        color = pendingColor;
        bool wasPending = hasPendingMessage;
        hasPendingMessage = false; // consumed - won't reappear on some later, unrelated reload
        return wasPending;
    }

    private void OnEnable()
    {
        StrikesManager.OnGameOver += HandleGameOver;
        Portal.OnGameWon += HandleGameWon;
    }

    private void OnDisable()
    {
        StrikesManager.OnGameOver -= HandleGameOver;
        Portal.OnGameWon -= HandleGameWon;
    }

    private void HandleGameOver()
    {
        // A win reached the same frame always beats a death on the same frame - Peleg's
        // call. Once the game is already ending as a win, a death can't downgrade it.
        if (reason == EndReason.GameWon)
            return;

        BeginEnding(EndReason.GameOver, GameOverMessage, gameOverColor);
    }

    private void HandleGameWon()
    {
        if (reason == EndReason.GameWon)
            return; // already ending as a win, nothing to change

        BeginEnding(EndReason.GameWon, GameWonMessage, gameWonColor);
    }

    private void BeginEnding(EndReason newReason, string message, Color color)
    {
        reason = newReason;
        pendingMessage = message;
        pendingColor = color;
        hasPendingMessage = true;
        Debug.Log(newReason == EndReason.GameWon ? "Game won - message pending" : "Game over - message pending");
    }

    private void Update()
    {
        if (reason == EndReason.None)
            return;

        // Reloads right away, same as StrikesManager/Portal always did - no artificial
        // delay. The message shows after the reload instead of before it, which is what
        // actually fixes the "game resets twice" feeling: PlayerDeath's instant
        // reposition-to-start and the reload happen close together again, and the message
        // just rides along on top of the already-restarted level for its own second.
        Debug.Log(reason == EndReason.GameWon ? "Game won - restarting" : "Game over - restarting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
