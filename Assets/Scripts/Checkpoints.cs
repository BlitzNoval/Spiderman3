using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player passing through the ring
            Debug.Log("Player passed through the ring!");
        }
    }
}
