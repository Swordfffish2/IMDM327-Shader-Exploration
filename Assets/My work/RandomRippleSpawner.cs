using System.Collections.Generic;
using UnityEngine;
public class RandomRippleSpawner : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Camera targetCamera;
    public Material rippleMaterial;

    [Header("Random ripple placement")]
    [Range(1, 8)] public int maxRipples = 3;
    [Min(0.1f)] public float minQuadSize = 1.0f;
    [Min(0.1f)] public float maxQuadSize = 2.0f;
    [Min(0.1f)] public float rippleLifetime = 5f;
    [Min(0.1f)] public float minSpawnInterval = 0.7f;
    [Min(0.1f)] public float maxSpawnInterval = 1.2f;
    [Min(0.0f)] public float minimumSeparation = 1.8f;
    [Range(0.0f, 0.4f)] public float viewportMargin = 0.15f;

    [Header("Must match Shader Graph")]
    public string phaseReference = "_PhaseOffset";
    [Min(0.001f)] public float shaderTimeMultiplier = 0.2f;

    private readonly List<GameObject> activeRipples = new List<GameObject>();
    private MaterialPropertyBlock propertyBlock;
    private float nextSpawnTime;
    private const float ripplePlaneZ = 0f;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || rippleMaterial == null)
        {
            Debug.LogError("RandomRippleSpawner: assign a Camera and Ripple_Emitter Material.");
            enabled = false;
            return;
        }

        if (!targetCamera.orthographic)
            Debug.LogWarning("RandomRippleSpawner: an Orthographic camera is recommended.");

        if (!rippleMaterial.HasProperty(phaseReference))
            Debug.LogWarning("RandomRippleSpawner: shader is missing Float property " + phaseReference + ". Ripples may animate in sync.");

        nextSpawnTime = Time.time;
    }

    private void Update()
    {
        activeRipples.RemoveAll(ripple => ripple == null);
        if (Time.time < nextSpawnTime) return;

        if (activeRipples.Count < maxRipples) SpawnRipple();
        float low = Mathf.Min(minSpawnInterval, maxSpawnInterval);
        float high = Mathf.Max(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + Random.Range(low, high);
    }

    private void SpawnRipple()
    {
        float small = Mathf.Max(0.1f, Mathf.Min(minQuadSize, maxQuadSize));
        float large = Mathf.Max(small, Mathf.Max(minQuadSize, maxQuadSize));
        float randomSize = Random.Range(small, large);

        Vector3 position = Vector3.zero;
        bool found = false;
        float cameraDistance = Mathf.Abs(ripplePlaneZ - targetCamera.transform.position.z);

        float marginX = viewportMargin;
        float marginY = viewportMargin;
        if (targetCamera.orthographic)
        {
            float worldHeight = 2f * targetCamera.orthographicSize;
            float worldWidth = worldHeight * targetCamera.aspect;
            marginX = Mathf.Max(marginX, randomSize / (2f * worldWidth));
            marginY = Mathf.Max(marginY, randomSize / (2f * worldHeight));
        }
        if (marginX >= 0.5f || marginY >= 0.5f) return;

        for (int attempt = 0; attempt < 25; attempt++)
        {
            float u = Random.Range(marginX, 1f - marginX);
            float v = Random.Range(marginY, 1f - marginY);
            position = targetCamera.ViewportToWorldPoint(new Vector3(u, v, cameraDistance));
            position.z = ripplePlaneZ;

            found = true;
            foreach (GameObject existing in activeRipples)
            {
                if (existing != null)
                {
                    float visualSeparation = 0.46f * (randomSize + existing.transform.localScale.x);
                    float requiredSeparation = Mathf.Max(minimumSeparation, visualSeparation);
                    if (Vector2.Distance(position, existing.transform.position) < requiredSeparation)
                    {
                        found = false;
                        break;
                    }
                }
            }
            if (found) break;
        }

        if (!found) return;

        GameObject ripple = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ripple.name = "Ripple (runtime)";
        ripple.transform.position = position;
        ripple.transform.localScale = new Vector3(randomSize, randomSize, 1f);
        ripple.transform.SetParent(transform, true);

        MeshRenderer meshRenderer = ripple.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = rippleMaterial;

        propertyBlock.Clear();
        propertyBlock.SetFloat(phaseReference, -Time.time * shaderTimeMultiplier);
        meshRenderer.SetPropertyBlock(propertyBlock);

        Collider collider = ripple.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        activeRipples.Add(ripple);
        Destroy(ripple, rippleLifetime);
    }
}
