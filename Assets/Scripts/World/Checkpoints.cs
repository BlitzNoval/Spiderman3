using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    // Public field to assign the material in the inspector
    public Material ringMaterial;

    private void Start()
    {
        if (ringMaterial == null)
        {
            Debug.LogError("Ring material not assigned in the inspector.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Change the ring's color to green
            if (ringMaterial != null)
            {
                ringMaterial.color = Color.green;
            }

            // Output messages to the console
            Debug.Log("Player passed through the ring!");
            Debug.Log("Player Has Passed Through Checkpoint");
        }
    }
}
