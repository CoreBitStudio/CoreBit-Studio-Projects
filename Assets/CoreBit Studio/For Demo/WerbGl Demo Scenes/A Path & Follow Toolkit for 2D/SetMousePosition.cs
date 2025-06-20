using UnityEngine;

public class SetMousePosition : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn; // ობიექტი, რომელსაც გავაჩენთ

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // მაუსის მარცხენა კლიკი
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f; // 2D სივრცეში Z-ღერძზე არ გვჭირდება მოძრაობა

            objectToSpawn.transform.position = mouseWorldPos;
        }
    }
}
