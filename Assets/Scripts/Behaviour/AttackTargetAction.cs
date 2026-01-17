using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack Target", story: "[Self] attacks [Target] untill it dies .", category: "Action", id: "acf36909137d3a9cbc99c4f0efa73c67")]
public partial class AttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<AttackConfigSO> AttackConfig;
    [SerializeReference] public BlackboardVariable<List<GameObject>> NearbyEnemies;

    private NavMeshAgent navMeshAgent;
    private AbstractUnit unit;
    private Transform selfTransform;
    private Animator animator;

    private IDamageable targetDamageable;
    private Transform targetTransform;
    private Collider[] enemyColliders;

    private float lastAttackTime;

    protected override Status OnStart()
    {
        if (!HasValidInputs()) return Status.Failure;

        selfTransform = Self.Value.transform;
        navMeshAgent = selfTransform.GetComponent<NavMeshAgent>();
        animator = selfTransform.GetComponent<Animator>();
        unit = selfTransform.GetComponent<AbstractUnit>();

        targetTransform = Target.Value.transform;
        targetDamageable = Target.Value.GetComponent<IDamageable>();
        if(AttackConfig.Value.IsAreaEffect)
        {
            enemyColliders = new Collider[AttackConfig.Value.MaxEnemiesHitPerAttack];
        }

        if (!NearbyEnemies.Value.Contains(Target.Value))
        {
            navMeshAgent.SetDestination(targetTransform.position);
            navMeshAgent.isStopped = false;
            if(animator != null)
            {
                animator.SetBool(AnimationConstants.ATTACK, false);
            }
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null || targetDamageable.CurrentHealth == 0) return Status.Success;

        if (animator != null)
        {
            animator.SetFloat(AnimationConstants.SPEED, navMeshAgent.velocity.magnitude);
        }

        if (!NearbyEnemies.Value.Contains(Target.Value))
        {
            return Status.Running;
        }

        navMeshAgent.isStopped = true;
        LookAtTarget();

        if (animator != null)
        {
            animator.SetBool(AnimationConstants.ATTACK, true);
        }

        if (Time.time >= lastAttackTime + AttackConfig.Value.AttackDelay)
        {
            ApplyDamage();
        }

        return Status.Running;
    }

    private void LookAtTarget()
    {
        Quaternion lookRotation = Quaternion.LookRotation(
                    (targetTransform.position - selfTransform.position).normalized,
                    Vector3.up
                );
        selfTransform.rotation = Quaternion.Euler(
            selfTransform.root.eulerAngles.x,
            lookRotation.eulerAngles.y,
            selfTransform.rotation.eulerAngles.z
        );
    }

    protected override void OnEnd()
    {
        if (animator != null)
        {
            animator.SetBool(AnimationConstants.ATTACK, false);
        }
        if(navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
    }

    private void ApplyDamage()
    {
        lastAttackTime = Time.time;
        if (unit.AttackingParticleSystem != null)
        {
            unit.AttackingParticleSystem.Play();
        }
        if (AttackConfig.Value.HasProjectileAttacks) return;
        
        targetDamageable.TakeDamage(AttackConfig.Value.Damage);

        if (!AttackConfig.Value.IsAreaEffect) return;
        
        int hits = Physics.OverlapSphereNonAlloc(
            targetTransform.position,
            AttackConfig.Value.AreaOfEffectRadius,
            enemyColliders,
            AttackConfig.Value.DamageableLayer
        );

        for (int i = 0; i < hits; i++)
        {
            if (enemyColliders[i].TryGetComponent(out IDamageable nearbyDamageable)
                && targetDamageable != nearbyDamageable)
            {
                nearbyDamageable.TakeDamage(
                    AttackConfig.Value.CalculateAreaOfEffectDamage(targetTransform.position, nearbyDamageable.Transform.position)
                );
            }
        }        
    }

    private bool HasValidInputs() => Self.Value != null && Self.Value.TryGetComponent(out NavMeshAgent _)
        && Self.Value.TryGetComponent(out AbstractUnit _)
        && Target.Value != null && Target.Value.TryGetComponent(out IDamageable _)
        && AttackConfig.Value != null
        && NearbyEnemies.Value != null;
}

