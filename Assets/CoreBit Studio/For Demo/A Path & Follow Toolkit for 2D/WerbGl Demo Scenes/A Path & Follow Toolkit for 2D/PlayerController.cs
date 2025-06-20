using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float health;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private Transform groundCheckLeft;
    [SerializeField] private Transform groundCheckRight;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private UnityEvent onDeath;

    private Rigidbody2D rb;
    public bool isGrounded;
    public bool isDeath;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // მოძრაობა მარცხნივ-მარჯვნივ
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // მიწაზე ვდგავართ?
        isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, 0.1f, groundLayer);
        if (!isGrounded)
        isGrounded = Physics2D.OverlapCircle(groundCheckRight.position, 0.1f, groundLayer);

        // ხტომა
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    public void GetDamage()
    {
        if (isDeath)
        {
            return;
        }
        //health -= 50;
        healthSlider.value = health;
        if (health <= 0)
        {
            isDeath = true;
            onDeath?.Invoke();
        }
    }
}
