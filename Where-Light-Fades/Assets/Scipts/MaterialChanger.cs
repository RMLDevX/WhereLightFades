using UnityEngine;
using System.Collections;

public class MaterialChanger : MonoBehaviour
{
    [Header("Drag Materials Here")]
    public Material normalMaterial;
    public Material parallelMaterial;

    [Header("Transition Delay")]
    public float materialChangeDelay = 0.5f;

    private Renderer objectRenderer;
    private ParallelWorldManager worldManager;
    private bool isChanging = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        worldManager = FindObjectOfType<ParallelWorldManager>();
    }

    void Update()
    {
        if (!isChanging)
        {
            // Switch material with delay
            if (worldManager.isParallelWorldActive && objectRenderer.material != parallelMaterial)
            {
                StartCoroutine(ChangeMaterialWithDelay(parallelMaterial));
            }
            else if (!worldManager.isParallelWorldActive && objectRenderer.material != normalMaterial)
            {
                StartCoroutine(ChangeMaterialWithDelay(normalMaterial));
            }
        }
    }

    IEnumerator ChangeMaterialWithDelay(Material newMaterial)
    {
        isChanging = true;
        yield return new WaitForSeconds(materialChangeDelay);
        objectRenderer.material = newMaterial;
        isChanging = false;
    }
}