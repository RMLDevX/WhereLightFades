using UnityEngine;

public class DeleteWhenTargetDeleted : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target object to monitor. When this object is deleted, this object will also be deleted.")]
    public GameObject targetObject;

    [Header("Additional Options")]
    [Tooltip("If enabled, will search for target by name if targetObject is null")]
    public bool findTargetByName = false;

    [Tooltip("Name of the target object to search for (if findTargetByName is enabled)")]
    public string targetObjectName = "";

    [Tooltip("Check for target deletion every X seconds (0 = every frame)")]
    public float checkInterval = 0.1f;

    private float timeSinceLastCheck = 0f;
    private bool targetFoundInitially = false;

    void Start()
    {
        // If target object is not assigned but we should find by name
        if (targetObject == null && findTargetByName && !string.IsNullOrEmpty(targetObjectName))
        {
            targetObject = GameObject.Find(targetObjectName);
        }

        // Log warning if no target is set
        if (targetObject == null)
        {
            Debug.LogWarning("No target object assigned for DeleteWhenTargetDeleted script on " + gameObject.name, this);
            return;
        }

        targetFoundInitially = true;
        Debug.Log("Monitoring target: " + targetObject.name + ". This object will be deleted when target is deleted.", this);
    }

    void Update()
    {
        // If no target was found initially, do nothing
        if (!targetFoundInitially) return;

        // Update timer
        timeSinceLastCheck += Time.deltaTime;

        // Check if it's time to verify target existence
        if (timeSinceLastCheck >= checkInterval)
        {
            CheckTargetExistence();
            timeSinceLastCheck = 0f;
        }
    }

    void CheckTargetExistence()
    {
        // Check if target object has been destroyed
        if (targetObject == null)
        {
            Debug.Log("Target object has been deleted. Deleting this object: " + gameObject.name, this);
            Destroy(gameObject);
        }
    }

    // Optional: Also check in OnValidate for editor-time changes
#if UNITY_EDITOR
    void OnValidate()
    {
        // Ensure check interval is not negative
        checkInterval = Mathf.Max(0, checkInterval);
    }
#endif
}