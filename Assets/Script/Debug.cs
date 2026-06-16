using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var results = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };
            EventSystem.current.RaycastAll(eventData, results);

            Debug.Log($"クリック位置にあるUI: {results.Count}件");
            foreach (var r in results)
                Debug.Log($"  → {r.gameObject.name} (depth:{r.depth}, sortOrder:{r.sortingOrder})");
        }
    }
}