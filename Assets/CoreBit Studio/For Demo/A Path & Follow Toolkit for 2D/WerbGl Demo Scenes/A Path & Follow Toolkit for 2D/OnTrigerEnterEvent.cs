using UnityEngine;
using UnityEngine.Events;

public class OnTrigerEnterEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent<Collider2D> onTrigerEnter = new UnityEvent<Collider2D>();
    [SerializeField] private LayerMask layer;
    [SerializeField] private bool onlyIsTrigerCollider;

    public UnityEvent<Collider2D> OnTrigerEnter { get { return onTrigerEnter; }set { onTrigerEnter = value; } }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((layer & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
        {
            if (onlyIsTrigerCollider == true)
            {
                if (collision.isTrigger == true)
                {
                    onTrigerEnter?.Invoke(collision);
                }
            }
            else
            {
                onTrigerEnter?.Invoke(collision);
            }
        }
    }
}