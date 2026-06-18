using UnityEngine;

public class LocationArea : MonoBehaviour
{
    [SerializeField] private string locationName;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.transform.root.CompareTag("Player")) return;

        Debug.Log("playerに触れた");
        if (LocationUIController.Instance != null)
        {
            LocationUIController.Instance.SetLocation(locationName);
        }
    }
}
