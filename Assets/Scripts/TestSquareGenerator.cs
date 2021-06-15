using UnityEngine;

/// <summary>
/// TestSquareGenerator class defines a spawner of game objects that spawn prefabs
/// </summary>
public class TestSquareGenerator : MonoBehaviour
{
    // Prefab to spawn
    public GameObject prefab;

    private float spawnDelay = 2f;
    private float delayGap = 0.3f;

    private float delay;
    private float counter = 0f;

    /// <summary>
    /// At Start, inits the delay value
    /// </summary>
    private void Start()
    {
        delay = spawnDelay + Random.Range(-delayGap, delayGap);
    }

    /// <summary>
    /// At Update, spawn new prefabs if counter exceeds delay
    /// </summary>
    private void Update()
    {
        counter += Time.deltaTime;
        if(counter > delay)
        {
            Instantiate(prefab,transform.position,Quaternion.identity);
            counter = 0;
            delay = spawnDelay + Random.Range(-delayGap, delayGap);
        }
    }
}
