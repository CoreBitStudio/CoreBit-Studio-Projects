using UnityEngine;
using Spine.Unity;
using System;
using System.Collections;

public class DragonEnemyScript : MonoBehaviour
{
    [SerializeField] private FollowerPathAgent followerPathAgent;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private AnimationReferenceAsset flyAnimation;
    [SerializeField] private AnimationReferenceAsset attackAnimation;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ParticleSystemDamagable flameVFXLeft;
    [SerializeField] private ParticleSystemDamagable flameVFXRight;
    [SerializeField] private Transform[] targetEnemyPositions;
    [SerializeField] private float cenAttackDistanceMin;
    [SerializeField] private float cenAttackDistanceMax;
    [SerializeField] private LayerMask layer;

    public float distance = 0;
    private bool attackIsStart = false;

    void Start()
    {
        followerPathAgent.OnMovedLeft.AddListener(OnMovedLeft);
        followerPathAgent.OnMovedRight.AddListener(OnMovedRight);

        cenAttackDistanceMin = cenAttackDistanceMin * cenAttackDistanceMin;
        cenAttackDistanceMax = cenAttackDistanceMax * cenAttackDistanceMax;

        StartCoroutine(CheckDistance());

        flameVFXLeft.OnTrigger += OnTrigger;
        flameVFXRight.OnTrigger += OnTrigger;
    }
    private void OnDisable()
    {
        flameVFXLeft.OnTrigger -= OnTrigger;
        flameVFXRight.OnTrigger -= OnTrigger;

        followerPathAgent.OnMovedLeft.RemoveListener(OnMovedLeft);
        followerPathAgent.OnMovedRight.RemoveListener(OnMovedRight);
    }
    private IEnumerator CheckDistance()
    {
        while (true)
        {
            distance = (transform.position - playerController.transform.position).sqrMagnitude;
            if (distance <= cenAttackDistanceMax && distance >= cenAttackDistanceMin && 
                transform.position.y < gridManager.TargetTransform.position.y + 1.5f &&
                transform.position.y > gridManager.TargetTransform.position.y - 1.5f)
            {
                if (attackIsStart == false)
                {
                    StartCoroutine(Attack());
                    attackIsStart = true;
                }
            }
            else
            {
                if (distance < cenAttackDistanceMin)
                {
                    SetEndPosition();
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }
    private IEnumerator Attack()
    {
        followerPathAgent.CanFollow = false;
        skeletonAnimation.AnimationState.SetAnimation(0,attackAnimation,false);
        if (transform.position.x > playerController.transform.position.x)
        {
            flameVFXLeft.Particle.Play();
            OnMovedLeft();
        }
        else
        {
            flameVFXRight.Particle.Play();
            OnMovedRight();
        }
        yield return new WaitForSeconds(attackAnimation.Animation.Duration);
        followerPathAgent.CanFollow = true;
        skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, true);
        attackIsStart = false;
        flameVFXLeft.CanInvoke = true;
        flameVFXRight.CanInvoke = true;
    }
    private void OnTrigger()
    {
        playerController.GetDamage();
    }
    private void OnMovedLeft()
    {
        skeletonAnimation.Skeleton.ScaleX = -1;
    }
    private void OnMovedRight()
    {
        skeletonAnimation.Skeleton.ScaleX = 1;
    }
    private void SetEndPosition()
    {
        Transform currentTransform = null;

        foreach (var item in targetEnemyPositions)
        {
            if (Physics2D.Linecast(playerController.transform.position, item.position, layer).collider == null)
            {
                currentTransform = item;
                break;
            }
        }

        if (currentTransform != null)
        {
            gridManager.TargetTransform = currentTransform;
        }
    }

}
