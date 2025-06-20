using UnityEngine;
using UnityEngine.Events;

public class SetMousePosition : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn; // ობიექტი, რომელსაც გავაჩენთ
    [SerializeField] private UnityEvent onClick;
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) // მაუსის მარცხენა კლიკი
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f; // 2D სივრცეში Z-ღერძზე არ გვჭირდება მოძრაობა

            objectToSpawn.transform.position = mouseWorldPos;
            onClick?.Invoke();
        }
    }
}
