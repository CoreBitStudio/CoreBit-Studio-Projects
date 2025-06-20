using System;
using UnityEngine;
using Spine.Unity;
public class PoiintMoverScript : MonoBehaviour
{
    [SerializeField] private FollowerPathAgent followerPathAgent;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    void Start()
    {
        followerPathAgent.OnMovedLeft.AddListener(OnMovedLeft);
        followerPathAgent.OnMovedRight.AddListener(OnMovedRight);
    }

    private void OnMovedLeft()
    {
        skeletonAnimation.Skeleton.ScaleX = -1;
    }
    private void OnMovedRight()
    {
        skeletonAnimation.Skeleton.ScaleX = 1;
    }

}
