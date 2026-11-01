using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject alienPrefab;

    public AlienCounter alienCounter;
    [SerializeField, Min(0f)] private float edgePadding = 0.3f;

    public List<GameObject> aliens = new List<GameObject> { };

    public Bounds FleetBounds { get; private set; }
    public float Width => FleetBounds.size.x;
    public float Height => FleetBounds.size.y;
    public bool HasAliens => aliens.Count > 0;

    float speed = GameSettings.InitialAlienSpeed;
    float leftBound = -17;
    float rightBound = 17;
    float downTime = .1f;
    bool movingDown = false;
    int defeatedAliens = 0;
    private Camera gameplayCamera;

    void Awake()
    {
        if (GameSession.Instance == null)
            gameObject.AddComponent<GameSession>();
    }

    void Start()
    {
        AlienBullet.ResetSpeed();
        Alien.ResetFireRate();
        gameplayCamera = Camera.main;

        int alienCount = LevelProgression.GetAlienCount(GameSession.Instance.Level);
        int rows = Mathf.CeilToInt((float)alienCount / LevelProgression.Columns);
        float visibleTopRowY = 10f;
        if (gameplayCamera != null)
        {
            float depth = gameplayCamera.WorldToViewportPoint(Vector3.zero).z;
            visibleTopRowY = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth)).y - 1f;
        }
        int offscreenRows = Mathf.Max(0, rows - GameSettings.InitialVisibleAlienRows);
        float topRowY = visibleTopRowY + offscreenRows * GameSettings.AlienRowSpacing;
        for (int index = 0; index < alienCount; index++)
        {
            int column = index % LevelProgression.Columns;
            int row = index / LevelProgression.Columns;
            GameObject alien = Instantiate<GameObject>(alienPrefab);
            alien.transform.position = new Vector3(2 * column - 12,
                topRowY - GameSettings.AlienRowSpacing * row, 0);
            aliens.Add(alien);
            alien.GetComponent<Alien>().fleet = this;
        }
        GiveRandomShields();
        RecalculateBounds();
        alienCounter.SetCount(aliens.Count);
    }

    private void GiveRandomShields()
    {
        List<GameObject> candidates = new List<GameObject>(aliens);
        int shieldCount = Mathf.CeilToInt((float)aliens.Count / LevelProgression.Columns);
        int count = Mathf.Min(shieldCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, candidates.Count);
            candidates[index].GetComponent<Alien>().GiveShield();
            candidates.RemoveAt(index);
        }
    }

    void FixedUpdate()
    {
        if (!HasAliens || !GameSession.Instance.IsPlaying)
        {
            return;
        }

        RecalculateBounds();
        UpdateScreenBounds();

        Vector3 movement;
        bool reachedEdge = false;
        if (movingDown)
        {
            movement = Vector3.down * Mathf.Abs(speed);
        }
        else
        {
            float remainingDistance = speed > 0
                ? rightBound - FleetBounds.max.x
                : FleetBounds.min.x - leftBound;
            float step = Mathf.Min(Mathf.Abs(speed), Mathf.Max(0f, remainingDistance));
            movement = Vector3.right * Mathf.Sign(speed) * step;
            reachedEdge = remainingDistance <= Mathf.Abs(speed);
        }

        foreach (GameObject alien in aliens)
        {
            alien.transform.position += movement;
        }

        RecalculateBounds();

        if (FleetBounds.min.y <= GameSession.Instance.DefeatLineY)
        {
            GameSession.Instance.FinishRound(false);
            return;
        }

        // Finish the horizontal step at the edge before descending and reversing.
        if (reachedEdge)
        {
            speed = -speed;
            movingDown = true;
            Invoke(nameof(startSidewaysMotion), downTime);
        }
    }

    private void UpdateScreenBounds()
    {
        if (gameplayCamera == null)
        {
            return;
        }

        float depth = gameplayCamera.WorldToViewportPoint(FleetBounds.center).z;
        leftBound = gameplayCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x + edgePadding;
        rightBound = gameplayCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth)).x - edgePadding;
    }


    public void AlienKilled(GameObject alien)
    {
        if (!GameSession.Instance.IsPlaying) return;
        // Only count an alien the first time it is removed.
        if (!aliens.Remove(alien))
        {
            return;
        }

        alienCounter.SetCount(aliens.Count);
        RecalculateBounds();

        GameSession.Instance.AlienDefeated(!HasAliens);

        defeatedAliens++;
        if (HasAliens && defeatedAliens % GameSettings.KillsPerShieldDrop == 0
            && Random.value < GameSettings.ShieldDropChance)
        {
            ShieldPickup.Spawn(alien.transform.position);
        }
        if (defeatedAliens > GameSettings.MaxSpeedIncreaseKills)
        {
            return;
        }

        speed *= GameSettings.AlienSpeedMultiplier;

        AlienBullet.IncreaseSpeed();
        Alien.IncreaseFireRate();
    }

    private void RecalculateBounds()
    {
        bool initialized = false;
        Bounds bounds = new Bounds();

        foreach (GameObject alien in aliens)
        {
            Bounds alienBounds = new Bounds(alien.transform.position, Vector3.zero);
            bool hasVisibleRenderer = false;
            Alien alienScript = alien.GetComponent<Alien>();
            foreach (Renderer visual in alien.GetComponentsInChildren<Renderer>())
            {
                // Shield effects do not change the formation's movement or defeat bounds.
                if (!visual.enabled || visual == alienScript.ShieldVisual) continue;
                if (!hasVisibleRenderer)
                {
                    alienBounds = visual.bounds;
                    hasVisibleRenderer = true;
                }
                else alienBounds.Encapsulate(visual.bounds);
            }

            if (!initialized)
            {
                bounds = alienBounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(alienBounds);
            }
        }

        // An empty fleet has zero width and height.
        FleetBounds = bounds;
    }

    void startSidewaysMotion()
    {
        movingDown = false;
    }
}
