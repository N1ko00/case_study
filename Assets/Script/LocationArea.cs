using UnityEngine;

public class LocationArea : MonoBehaviour
{
    [SerializeField] private string locationName;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        if (LocationUIController.Instance != null)
        {
            LocationUIController.Instance.SetLocation(locationName);
        }
    }
}
