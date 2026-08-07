using UnityEngine;
using UnityEngine.SceneManagement;

// Decides how the game ended and restarts it. StrikesManager and Portal only report their own
// condition and don't know this class exists, so neither of them owns any part of the ending.
public class GameEndManager : MonoBehaviour
{
    [SerializeField] private Color gameOverColor = Color.red;
    [SerializeField] private Color gameWonColor = Color.green;

    private const string GameOverMessage = "GAME OVER";
    private const string GameWonMessage = "GAME WON";

    // None until either event fires, then locked to whichever outcome won.
    private enum EndReason { None, GameOver, GameWon }
    private EndReason reason = EndReason.None;

    // Static, so it survives the scene reload below and can be read back by the freshly loaded
    // message display. That is the whole trick that keeps the message alive without a persisted
    // GameObject, a second Canvas, or DontDestroyOnLoad.
    private static string pendingMessage;
    private static Color pendingColor;
    private static bool hasPendingMessage;

    public static bool TryConsumePendingMessage(out string message, out Color color)
    {
        message = pendingMessage;
        color = pendingColor;
        bool wasPending = hasPendingMessage;
        // Consumed here, so a message can't reappear on some later, unrelated reload.
        hasPendingMessage = false;
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
        // A win reached on the same frame always beats a death: once the game is ending as a
        // win, a death can't downgrade it. The reverse is deliberately not true.
        if (reason == EndReason.GameWon)
            return;

        BeginEnding(EndReason.GameOver, GameOverMessage, gameOverColor);
    }

    private void HandleGameWon()
    {
        if (reason == EndReason.GameWon)
            return;

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

        // Reloads immediately, with no delay for the message. Waiting for it made the level
        // visibly reset twice, since PlayerDeath already snaps Mario back to the start the
        // instant he dies. The message rides on top of the restarted level instead.
        Debug.Log(reason == EndReason.GameWon ? "Game won - restarting" : "Game over - restarting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
