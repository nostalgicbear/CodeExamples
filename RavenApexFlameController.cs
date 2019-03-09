using UnityEngine;
using System.Collections;
using Apex.Steering.Behaviours;
using System;

public class RavenApexFlameController : RavenApexController
{
    protected RavenApexFlameMover ravenApexFlameMover;
    protected PatrolPointsComponent route;
    protected const float distanceForBeDown = 15f;
    protected bool canShoot = true;
    [HideInInspector]
    public BulletPool bulletPool { get; set; }
    protected const float minTimeBetweenAttacks = 7f;
    protected const float maxTimeBetweenAttacks = 14f;
    protected float lastTimeAttack = 0f;
    protected const float waitToShootTime = 1.2f;
    protected float minDistanceToDive;



    internal override void PathReached()
    {

    }

    protected override void TurnOnEnemy()
    {
        base.TurnOnEnemy();
        lastTimeAttack = 0;
        canShoot = true;        
    }


    protected override void SetParams()
    {
        base.SetParams();
        normalSoundDelay = 8;
        minDistanceToDive = diveArriveDistance * .5f;
    }

    protected override void SetComponents()
    {
        base.SetComponents();
        ravenApexFlameMover = (RavenApexFlameMover)ravenApexMover;
    }


    protected override void SetWayPoints()
    {

    }

    public void SetDirectlyRoute(PatrolPointsComponent patrolPointComponent)
    {
        route = patrolPointComponent;
    }

    override protected void MoveRaven(Vector3 position, float arriveDistance)
    {
        playerIsTarget = false;
        SetAnimSpeed();
        ravenApexFlameMover.StartPatrol(route, false, false, 0, waypointArriveDistance);
    }

    protected virtual void SetAnimSpeed()
    {
        animator.SetFloat("FlyingMultiplier", 1.5f);
    }

    protected override void SetProperMoveForRavenApexMover()
    {

        if (DistanceToPlayer < diveArriveDistance)
        {
            if (TimePassedSinceLastAttack())
            {
                if (DistanceToPlayer > minDistanceToDive)
                {
                    SwitchToDive();
                }
            }
        }
        else
        {
            SwitchToFly();
        }
    }

    protected bool TimePassedSinceLastAttack()
    {
        return Time.time - lastTimeAttack > GetMinTimeBetweenAttacks();
    }

    protected float GetMinTimeBetweenAttacks()
    {
        return UnityEngine.Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks);
    }

    private void SwitchToDive()
    {
        if (curState != State.HeadingToAttack)
        {
            canShoot = true;
            PlaySound(1, SoundVoiceAggresive, false, 1);
            curState = State.HeadingToAttack;
        }
    }

    private void SwitchToFly()
    {
        if (curState != State.Move)
        {
            curState = State.Move;
        }
    }

    public bool IsInDiveState()
    {
        return curState == State.HeadingToAttack;
    }

    protected override void UpdateDiveState()
    {
        SetDiveAnimation();
        MakeShoot();
        SetProperMoveForRavenApexMover();
    }

    private void MakeShoot()
    {
        if (ravenApexFlameMover.NearLowValue())
        {
            if (canShoot)
            {
                canShoot = false;
                ReadyToShoot();
            }
        }
    }

    private void ReadyToShoot()
    {        
        if (!freezeController.IsFreezed())
        {
            if (!shocked)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                Vector3 position = transform.position - (transform.up * .75f);
                bulletPool.GetPooledBullet(direction, position, Quaternion.identity);
                lastTimeAttack = Time.time;                
                SwitchToFly();
            }
        }
    }

    protected void BackToFlying()
    {
        MoveRaven(Vector3.zero, waypointArriveDistance);
        curState = State.Move;
    }

    protected override void TurnFlyMode(bool active)
    {
        if (!IsDead())
        {
            if (active)
            {
                if (!flyMode)
                {
                    rigidBody.drag = ravenApexMover.GetSpeed();
                    rigidBody.angularDrag = ravenApexMover.GetSpeed();
                    rigidBody.useGravity = false;
                    BackToFlying();
                    flyMode = true;
                }
            }
            else
            {
                if (flyMode)
                {
                    rigidBody.useGravity = true;
                    rigidBody.drag = 0.5f;
                    rigidBody.angularDrag = 0.05f;
                    flyMode = false;
                }
            }
        }
    }



}
