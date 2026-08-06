using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// Spawns a configured enemy prefab on a repeating timer, capped at how many of its own
// spawned instances are still alive. Doesn't know or care what kind of enemy it's making -
// a second spawner for a different enemy later is just a second GameObject running this same
// script with a different Inspector-assigned prefab, not a subclass.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxAlive = 3;

    // What this spawner has made that's still alive. Pruned of destroyed entries before every
    // cap check instead of tracked via an event - a spawned enemy dying however it dies (axe,
    // fireball, anything added later) just quietly stops counting, with no need for this
    // script to know how enemies die or to be notified about it.
    private readonly List<GameObject> spawned = new List<GameObject>();

    private CancellationTokenSource cancellationTokenSource;

    // Real elapsed time, logged alongside game time purely so the two stay comparable - the
    // gap between them is what the first-interval bug turned out to be. Fully qualified rather
    // than "using System.Diagnostics;", which would make every Debug.Log below ambiguous
    // against System.Diagnostics.Debug.
    private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Start()
    {
        stopwatch.Start();
        StartSpawning();
    }

    void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
    }

    // async void (not async Task) because Start() is a synchronous Unity lifecycle method and
    // can't await anything - same reasoning Lesson 5's own EnemySpawner.cs uses for the same
    // shape of call.
    private async void StartSpawning()
    {
        cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await SpawnLoopAsync(cancellationTokenSource.Token);
        }
        catch (System.OperationCanceledException)
        {
            // Expected every time the scene reloads - OnDestroy() cancels the token on the way
            // out. Not an error, so it doesn't go through Debug.LogError below; a spawner
            // "erroring" on every ordinary game-over/game-won reload would be misleading noise.
            Debug.Log("Enemy spawner stopped: " + gameObject.name);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in enemy spawn loop: " + ex.Message);
        }
    }

    private async Task SpawnLoopAsync(CancellationToken cancellationToken)
    {
        // Spawns immediately on entry, then waits - not the other way around - so the level
        // doesn't sit at zero spawned enemies for the first interval after it loads.
        while (this != null && !cancellationToken.IsCancellationRequested)
        {
            TrySpawn();
            await WaitGameSecondsAsync(spawnInterval, cancellationToken);
        }
    }

    // Waits in Unity's own game time rather than real time, which is what Task.Delay would do.
    // Task.Delay keeps counting real seconds even while the game isn't running a single frame,
    // and the editor can sit frozen for seconds right after Play is pressed while it warms up.
    // A delay that expires during that freeze is already spent by the time anything reaches the
    // screen, so the first two spawns appeared almost together no matter what the interval was
    // set to. Counting frames the game actually ran fixes that at the source: this only
    // advances while there is something to see.
    private async Task WaitGameSecondsAsync(float seconds, CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Task.Yield resumes on Unity's main thread on the next frame, so this loop runs
            // once per rendered frame and stalls completely whenever the game does.
            await Task.Yield();

            if (this == null)
                return;

            elapsed += Time.deltaTime;
        }
    }

    private void TrySpawn()
    {
        spawned.RemoveAll(enemy => enemy == null);

        if (spawned.Count >= maxAlive)
        {
            Debug.Log("Enemy spawn skipped - already at cap (" + maxAlive + "): " + gameObject.name + TimingSuffix());
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.Log("Enemy spawn skipped - no prefab assigned: " + gameObject.name);
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        spawned.Add(enemy);
        Debug.Log("Enemy spawned: " + enemy.name + " (" + spawned.Count + "/" + maxAlive + " from " + gameObject.name + ")" + TimingSuffix());
    }

    private string TimingSuffix()
    {
        return " at " + Time.time.ToString("F3") + "s game / "
             + stopwatch.Elapsed.TotalSeconds.ToString("F3") + "s real";
    }
}
