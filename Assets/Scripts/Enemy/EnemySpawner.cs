using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// Spawns a configured enemy prefab on a repeating timer, capped at how many of its own spawns
// are still alive. It never learns what kind of enemy it makes, so a spawner for a different
// enemy is another GameObject running this same script with a different prefab, not a subclass.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxAlive = 3;

    // What this spawner has made that is still alive, pruned before every cap check rather than
    // tracked by a death event. An enemy dying however it dies just stops counting, so nothing
    // here needs to know how enemies die or be told when one does.
    private readonly List<GameObject> spawned = new List<GameObject>();

    private CancellationTokenSource cancellationTokenSource;

    void Start()
    {
        StartSpawning();
    }

    void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
    }

    // async void rather than async Task because Start() is a synchronous Unity lifecycle method
    // and has nothing to await this with.
    private async void StartSpawning()
    {
        cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await SpawnLoopAsync(cancellationTokenSource.Token);
        }
        catch (System.OperationCanceledException)
        {
            // Expected on every scene reload, since OnDestroy cancels the token on the way out.
            // Logged rather than reported as an error, because a spawner "failing" every time
            // the game restarts would be misleading noise.
            Debug.Log("Enemy spawner stopped: " + gameObject.name);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in enemy spawn loop: " + ex.Message);
        }
    }

    private async Task SpawnLoopAsync(CancellationToken cancellationToken)
    {
        // Spawns on entry and then waits, rather than the other way around, so the level doesn't
        // sit empty for a whole interval after it loads.
        while (this != null && !cancellationToken.IsCancellationRequested)
        {
            TrySpawn();
            await WaitGameSecondsAsync(spawnInterval, cancellationToken);
        }
    }

    // Waits in game time rather than real time. Task.Delay would keep counting real seconds
    // while the game isn't rendering a single frame, and the editor sits frozen for a moment
    // right after Play is pressed - long enough that the first wait was mostly spent before
    // anything reached the screen. Counting only frames the game actually ran fixes that.
    private async Task WaitGameSecondsAsync(float seconds, CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Task.Yield resumes on Unity's main thread next frame, so this loop runs once per
            // rendered frame and stalls completely whenever the game does.
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
            Debug.Log("Enemy spawn skipped - already at cap (" + maxAlive + "): " + gameObject.name);
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.Log("Enemy spawn skipped - no prefab assigned: " + gameObject.name);
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        spawned.Add(enemy);
        Debug.Log("Enemy spawned: " + enemy.name + " (" + spawned.Count + "/" + maxAlive + " from " + gameObject.name + ")");
    }
}
