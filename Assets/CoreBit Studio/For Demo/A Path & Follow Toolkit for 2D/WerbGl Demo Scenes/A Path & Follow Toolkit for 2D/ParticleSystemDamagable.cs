using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemDamagable : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private ParticleSystem particle;

    private bool canInvoke = true;
    private UnityAction onTrigger;
    private Transform dumbrielTransform;

    public ParticleSystem Particle { get { return particle; }  }
    public UnityAction OnTrigger { get { return onTrigger; } set { onTrigger = value; } }
    public bool CanInvoke { get { return canInvoke; } set { canInvoke = value; } }
    void Start()
    {
        dumbrielTransform = player.transform;
        particle.trigger.AddCollider(dumbrielTransform);
    }

    private void OnParticleTrigger()
    {
        if (canInvoke == true)
        {
            onTrigger?.Invoke();
            canInvoke = false;
        }
    }
}