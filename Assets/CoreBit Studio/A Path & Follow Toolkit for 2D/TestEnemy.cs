using UnityEngine;
using Spine.Unity;
using System;

public class TestEnemy : MonoBehaviour
{
    [SerializeField] private FollowerPathAgent followerPathAgent;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private AnimationReferenceAsset idleAnimation;
    [SerializeField] private AnimationReferenceAsset attackAnimation;
    [SerializeField] private ParticleSystem fireVFX;
    [SerializeField] private Transform fireVFXPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        followerPathAgent.OnMovedLeft.AddListener(OnMovedLeft);
        followerPathAgent.OnMovedRight.AddListener(OnMovedRight);
        followerPathAgent.OnEndMove.AddListener(OnEndMove);
    }

    private void OnEndMove()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnimation, true,0);
        fireVFX.transform.position = fireVFXPosition.position;
        fireVFX.Play();
    }

    private void OnMovedLeft()
    {
        skeletonAnimation.Skeleton.ScaleX = -1;
    }

    private void OnMovedRight()
    {
        skeletonAnimation.Skeleton.ScaleX = 1;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
