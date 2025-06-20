using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private AnimationReferenceAsset walkAnimation;
    [SerializeField] private AnimationReferenceAsset idleAnimation;

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
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (moveInput != 0)
        {
            if (moveInput > 0)
            {
                skeletonAnimation.Skeleton.ScaleX = 1;
            }
            else if(moveInput < 0)
            {
                skeletonAnimation.Skeleton.ScaleX = -1;
            }
            if (isGrounded)
            {
                if (!skeletonAnimation.AnimationState.GetCurrent(0).Animation.Name.Equals(walkAnimation.name))
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, walkAnimation, true);
                    skeletonAnimation.AnimationState.TimeScale = 2;
                }
            }
        }
        else
        {
            if (!skeletonAnimation.AnimationState.GetCurrent(0).Animation.Name.Equals(idleAnimation.name))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);
                skeletonAnimation.AnimationState.TimeScale = 1;
            }
        }
        isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, 0.1f, groundLayer);
        if (!isGrounded)
        isGrounded = Physics2D.OverlapCircle(groundCheckRight.position, 0.1f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation,true);
            skeletonAnimation.AnimationState.TimeScale = 1;
        }
    }
    public void GetDamage()
    {
        if (isDeath)
        {
            return;
        }
        health -= 50;
        healthSlider.value = health;
        if (health <= 0)
        {
            isDeath = true;
            onDeath?.Invoke();
        }
    }
}
