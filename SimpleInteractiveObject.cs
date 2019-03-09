using UnityEngine;
using System.Collections;
using System;

public class SimpleInteractiveObject : MonoBehaviour, ISimpleInteractive, ISimpleSteam
{


    protected Rigidbody rigidBody;
    protected BreakableObject breakableObject;
    protected bool isBreakable = false;
    public bool hasBeenHit { get { return _hasBeenHit; } }
    protected bool _hasBeenHit = false;

    public bool hasBeenMagnet { get { return _hasBeenMagnet; } }
    protected bool _hasBeenMagnet = false;

    public bool useMagnetBeforeHit = true;
    protected HealthController healthController;
    [SerializeField]
    private float boundingScale = 1;
    [SerializeField]
    public float health = 2f;
    [SerializeField]
    public ObjectMaterialType.type materialType = ObjectMaterialType.type.Wood;
    [SerializeField]
    protected bool rotateDuringPull = true;
    [SerializeField]
    protected float pushingForceMultiplier = 1;
    

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        healthController = new HealthController(health);
        breakableObject = GetComponent<BreakableObject>();
        isBreakable = breakableObject != null;
    }

    public bool StillExcist()
    {
        return rigidBody != null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void MagnetPull(float force, Vector3 magnetPosition, Quaternion magnetRotation)
    {
        if (useMagnetBeforeHit)
        {
            if (rigidBody)
            {
                _hasBeenMagnet = true;
                Vector3 direction = (magnetPosition - transform.position).normalized;
                float distance = Vector3.Distance(magnetPosition, transform.position);
                if (distance < 1)
                {
                    rigidBody.velocity = Vector3.zero;
                    if (rotateDuringPull)
                    {
                        rigidBody.MoveRotation(magnetRotation);
                    }
                    rigidBody.AddForce(direction * force * 1000 * distance, ForceMode.Acceleration);
                }
                else
                {
                    force = force / distance;
                    if (transform.position.y < magnetPosition.y)
                    {
                        rigidBody.AddForce(transform.up * force, ForceMode.Impulse);
                    }

                    rigidBody.AddForce(direction * force * 4, ForceMode.Impulse);
                }
            }
        }
    }

    public void MagnetPush(float force, Vector3 magnetPosition)
    {
        if (useMagnetBeforeHit)
        {
            if (rigidBody)
            {
                Vector3 direction = (magnetPosition - transform.position).normalized;
                float distance = Mathf.Max(1, Vector3.Distance(magnetPosition, transform.position));
                force *= pushingForceMultiplier;
                force = force / (distance);
                rigidBody.AddForce(direction * -force, ForceMode.Impulse);
            }
        }
    }


    public virtual void AddForceFromMeleWeapon(Vector3 force)
    {
        KnockBack(force, 1);
    }

    public virtual void AddForceFromMeleWeapon(Vector3 direction, float hitForce)
    {
        KnockBack(direction, hitForce);
    }

    public void TakeHit(float hit, Vector3 direction, float hitForce)
    {
        KnockBack(direction, hitForce);
        _hasBeenHit = true;
        TakeHit(hit);
    }

    private void KnockBack(Vector3 direction, float hitForce)
    {
        rigidBody.AddForce(direction * hitForce, ForceMode.Impulse);
    }

    public void TakeHit(float hit)
    {
        if (isBreakable)
        {
            healthController.TakeDamage(hit);
            if (healthController.IsDead())
            {
                Die();
            }
        }
    }

    private void Die()
    {
        breakableObject.DestoryObjectWithHit();
    }

    public ObjectMaterialType.type GetMaterial()
    {
        return materialType;
    }

    public void ExplosionHit(float force, Vector3 explosionPosition, float explosionRadius, float hit)
    {
        if (rigidBody)
        {
            force *= pushingForceMultiplier;
            rigidBody.AddExplosionForce(force, explosionPosition, explosionRadius);
            TakeHit(CountHit(hit, explosionPosition));
        }
    }

    private float CountHit(float maxHit, Vector3 explosionPosition)
    {
        return Mathf.Max(0, maxHit - maxHit * CountZeroOneDistance(explosionPosition));
    }

    private float CountZeroOneDistance(Vector3 explosionPosition)
    {
        float distance = Vector3.Distance(transform.position, explosionPosition);
        return distance / DynamiteController.explosionRange;
    }

    public float GetBoundsScale()
    {
        return boundingScale;
    }

    public void TakeFreezeHit(Vector3 direction, float hit, Vector3 weaponPosition)
    {
        if (rigidBody!=null)
        {
            float distanceValue = Vector3.Distance(weaponPosition, transform.position)*.25f;
            float hitForce = 5f / distanceValue;
            rigidBody.AddForce(direction * hitForce, ForceMode.Impulse);
        }
    }
}
