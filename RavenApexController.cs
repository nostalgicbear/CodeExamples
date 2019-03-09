using UnityEngine;
using System.Collections;
using System;

public class RavenApexController : Enemy
{
    [SerializeField]
    protected AudioClip SoundDive;
    [SerializeField]
    protected AudioClip SoundVoiceAggresive;
    [SerializeField]
    protected AudioClip SoundVoiceNormal;
    protected bool normalVoicePlayed = false;
    protected bool initialized = false;
    protected GameObject waypointsContainer;
    protected RavenApexMover ravenApexMover;
    protected int moveState;
    protected int diveState;
    protected int attackState;
    protected int walkState;
    protected int shockState;

    protected bool flyMode = true;
    Transform[] waypoints;
    protected int waypointsLength;

    protected float attackStopDistance = 3f;
    protected float diveArriveDistance = 9f;
    protected float diveCheckDistance = 11f;
    protected const float waypointArriveDistance = .2f;
    protected bool playerIsTarget = true;
    protected int currentState = -1;
    protected int normalSoundDelay = 5;

    override protected void EnemyInitialize()
    {
        moveState = Animator.StringToHash("Base.Flying");
        diveState = Animator.StringToHash("Base.Dive");
        attackState = Animator.StringToHash("Base.Attack");
        walkState = Animator.StringToHash("Base.Walking");
        shockState = Animator.StringToHash("Base.Shock");
    }



    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerHitTesterPosition.position + playerHitTesterPosition.right * -.09f, playerHitTesterRadius);
    }

    public void InitRavenForFirstTime(GameObject waypointsContainer)
    {
        this.waypointsContainer = waypointsContainer;
        if (!initialized)
        {
            SetComponents();
            SetWayPoints();
            SetParams();
            AddClasses();
            curState = State.Move;
            initialized = true;
        }
    }

    protected virtual void SetWayPoints()
    {
        Transform[] waypointsFromContainer = waypointsContainer.GetComponentsInChildren<Transform>();
        int i;
        int length = waypointsFromContainer.Length;
        waypoints = new Transform[length + 1];
        waypoints[0] = player;
        for (i = 0; i < length; i++)
        {
            waypoints[i + 1] = waypointsFromContainer[i];
        }
        waypointsLength = waypoints.Length;
    }

    protected void AddClasses()
    {
        base.AddCommonClasses();
    }

    protected virtual void SetComponents()
    {
        ravenApexMover = GetComponent<RavenApexMover>();
        base.SetCommonComponents();
    }

    public void OnDestroy()
    {
    }

    protected virtual void SetParams()
    {
        rigidBody.drag = ravenApexMover.GetSpeed();
        rigidBody.angularDrag = ravenApexMover.GetSpeed();
    }


    protected override void TurnOnEnemy()
    {
        base.TurnOnEnemy();
        if (ravenApexMover != null)
        {
            CountCurrentPlayerPos();
            playerIsTarget = true;
            MoveRaven(currentPreparedPlayerPos, diveArriveDistance);
        }
    }

    protected virtual void MoveRaven(Vector3 position, float arriveDistance)
    {
        ravenApexMover.MoveUnit(position, arriveDistance);
    }

    override public void ReturnToPool()
    {
        SetExplosions(2, .75f);
        ravenApexMover.StopUnit();
        curState = State.Move;
        rigidBody.velocity = Vector3.zero;
        StopAllCoroutines();
        shocked = false;
        flyMode = true;
        rigidBody.drag = ravenApexMover.GetSpeed();
        rigidBody.angularDrag = ravenApexMover.GetSpeed();
        rigidBody.useGravity = false;
        tryingToStandUp = false;
        base.ReturnToPool();
    }

    internal virtual void PathReached()
    {
        if (DistanceToPlayer < diveArriveDistance * 1.1f)
        {
            StartDive();
        }
        else
        {
            playerIsTarget = true;
            CountCurrentPlayerPos();
            MoveRaven(currentPreparedPlayerPos, diveArriveDistance);
        }
    }

    protected void StartDive()
    {
        PlaySound(1, SoundVoiceAggresive, false, 1f);
        curState = State.HeadingToAttack;
    }


    protected void CountCurrentPlayerPos()
    {
        currentPreparedPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
    }

    internal Vector3 RealPlayerPosition()
    {
        return player.position;
    }

    protected override void UpdateMoveState()
    {
        SetMoveAnimation();
        SetProperMoveForRavenApexMover();
    }

    protected virtual void SetProperMoveForRavenApexMover()
    {
        if (playerIsTarget)
        {
            CountCurrentPlayerPos();
            if (Vector3.Distance(currentPreparedPlayerPos, preparedPlayerPosition) > .25f)
            {
                preparedPlayerPosition = currentPreparedPlayerPos;
                playerIsTarget = true;
                MoveRaven(currentPreparedPlayerPos, diveArriveDistance);
            }
        }
    }

    protected override void UpdateDiveState()
    {        
        CheckToStopDive();
        CheckToAttack();
        CountCurrentPlayerPos();
        SetDiveAnimation();
        ravenApexMover.GoToPlayer();
    }

    protected virtual void CheckToStopDive()
    {
        if (DistanceToPlayer > diveCheckDistance)
        {
            ForceFly();
        }
    }

    protected void ForceFly()
    {
        MoveRaven(currentPreparedPlayerPos, diveArriveDistance);
        curState = State.Move;
    }

    protected void CheckToAttack()
    {
        if (CheckIfCanAttack())
        {
            StartAttack();
        }
    }

    protected virtual void StartAttack()
    {
        curState = State.Attack;
        StartCoroutine(WaitRestartToPath(GetRandomAttackTime()));
    }

    IEnumerator WaitRestartToPath(float delay) //After attack
    {
        yield return new WaitForSeconds(delay);
        FlyToWaipoint();
    }

    protected void FlyToWaipoint()
    {
        if (!freezeController.IsFreezed())
        {
            if (!shocked)
            {
                TakeRandomWaypoint();
                playerIsTarget = false;
                MoveRaven(waypoints[TakeRandomWaypoint()].position, waypointArriveDistance);
                curState = State.Move;
            }
        }
    }

    protected int TakeRandomWaypoint()
    {
        return UnityEngine.Random.Range(2, waypointsLength);
    }

    protected float GetRandomAttackTime()
    {
        return UnityEngine.Random.Range(1.5f, 2f);
    }

    protected override void UpdateAttackState()
    {
        CheckToStopAttack();
        CountCurrentPlayerPos();
        SetAttackAnimation();
        ravenApexMover.GoToPlayer();
    }


    protected virtual void CheckToStopAttack()
    {
        if (DistanceToPlayer > attackStopDistance)
        {
            if (DistanceToPlayer > diveCheckDistance)
            {
                ForceFly();
            }
            else
            {
                StartDive();
            }
        }
    }

    protected override float DistanceToPlayer
    {
        get
        {
            CountCurrentPlayerPos();
            return Vector3.Distance(currentPreparedPlayerPos, transform.position);
        }
    }

    public float DistanceToPlayerForDive
    {
        get
        {
            return DistanceToPlayer;
        }
    }

    protected void SetMoveAnimation()
    {
        ChangeState(moveState, "Fly");
    }

    public void SetDiveAnimation()
    {
        ChangeState(diveState, "Dive");
    }

    public void SetAttackAnimation()
    {
        ChangeState(attackState, "Attack");
    }

    protected void SetWalkAnimation()
    {
        ChangeState(walkState, "Walk");
    }

    protected void SetShockAnimation()
    {
        ChangeState(shockState, "Shock");
    }

    protected void ChangeState(int state, string triggerName)
    {
        if ((currentState != state))
        {
            currentState = state;
            animator.SetTrigger(triggerName);
        }
    }

    protected override void UpdateFreezeState()
    {
        if (CanDoAction())
        {
            CountCurrentPlayerPos();
            ravenApexMover.EscapeFromPlayer();
        }
        else
        {
            curState = State.GetsUp;
        }
    }

    protected override void UpdateStandingState()
    {
        if (CanDoAction())
        {
            curState = State.Freeze;
        }
        else
        {
            StartStandUpOperation();
        }
    }


    override protected void MakeShock(float hitPoints)
    {
        if (curState != State.Attack)
        {

            if (!shocked)
            {
                if (!IsDead())
                {
                    shockHitPoints = hitPoints;
                    ravenApexMover.StopUnit();
                    StopMoveSound();
                    countToShockTime = 0;
                    PlaySound(1, SoundPain, false, 1);
                    rigidBody.useGravity = true;
                    rigidBody.drag = 0.5f;
                    rigidBody.angularDrag = 0.05f;
                    animator.SetTrigger("Shock");
                    curState = State.Shock;
                    shocked = true;
                }
            }
        }
    }


    protected override void UpdateShockState()
    {
        if (shocked)
        {
            countToShockTime += Time.deltaTime;
            if (countToShockTime >= shockTime)
            {
                BackFromShock();
            }
        }
    }

    protected void BackFromShock()
    {
        shocked = false;
        flyMode = false;
        countToShockTime = 0;
        TakeHit(shockHitPoints);
        TurnFlyMode(true);
    }


    protected override void AttackHandler()
    {
        PlaySound(1, SoundAttack, false, .4f);
        if (playerHitTester.CheckIfHitPlayer(playerHitTesterPosition.position + playerHitTesterPosition.right * -.09f))
        {
            GivePlayerDamage(attackPoints, true, playerHitTester.GetPlayerGameObject());
        }
    }


    protected override void CheckIfFreeze()
    {
        if (freezeController.IsFreezed())
        {
            if (curState != State.Freeze)
            {
                if (curState != State.GetsUp)
                {
                    StopMoveSound();
                    ravenApexMover.StopUnit();
                    PlaySound(1, SoundFreeze, false);
                    SetFreeze(true);
                    SetWalkAnimation();
                    curState = State.Freeze;
                }
            }
        }
        else
        {
            SetFreeze(false);
            PlayMoveSound();
        }
    }

    protected override void ActionsBeforeDisabledAfterPlayerWins()
    {
        StopKeyRotation();
        StopMoveSound();
        ravenApexMover.StopUnit();
        PlaySound(1, SoundFreeze, false);
        SetFreeze(true);
    }


    protected void SetFreeze(bool activateFreeze)
    {
        TurnFlyMode(!activateFreeze);
    }

    protected virtual void TurnFlyMode(bool active)
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
                    FlyToWaipoint();
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

    override protected void PlayMoveSound()
    {
        if (curState == State.Move || curState == State.HeadingToAttack)
        {
            if (curState == State.HeadingToAttack)
            {
                if (audioSources[0].clip != SoundDive)
                    audioSources[0].clip = SoundDive;
            }
            else
            {
                if (audioSources[0].clip != SoundMove)
                    audioSources[0].clip = SoundMove;
            }

            if (!audioSources[0].isPlaying)
            {
                PlaySound(0, audioSources[0].clip, true);
            }
        }
        else
        {
            StopMoveSound();
        }
    }

    override protected void MakeCustomActions()
    {
        if (curState == State.Move)
        {
            if (!audioSources[1].isPlaying)
            {
                if (!normalVoicePlayed)
                {
                    Invoke("PlayRavenNormalSound", UnityEngine.Random.Range(2, 8));
                    normalVoicePlayed = true;
                }
            }
        }
    }

    protected void PlayRavenNormalSound()
    {
        if (!IsDead())
        {
            if (gameObject.activeSelf)
            {
                if (!audioSources[1].isPlaying && audioSources[1].enabled)
                {
                    PlaySound(1, SoundVoiceNormal, false);
                    StartCoroutine(WaitToSetNormalPlayedToFalse());
                }
                else
                {
                    SetNormalPlayedToFalse();
                }
            }
        }
    }

    IEnumerator WaitToSetNormalPlayedToFalse()
    {
        yield return new WaitForSeconds(normalSoundDelay);
        SetNormalPlayedToFalse();
    }

    private void SetNormalPlayedToFalse()
    {
        normalVoicePlayed = false;
    }


    protected override void CheckToImpact(Collision collision)
    {
        if (curState == State.Attack || curState == State.HeadingToAttack)
        {
            if (collision.collider.gameObject.GetComponent<RavenApexController>() == null)
            {
                base.CheckToImpact(collision);
            }
        }
        else
        {
            base.CheckToImpact(collision);
        }
    }

    public override void AddForceFromMeleWeapon(Vector3 direction, float hitForce)
    {
        hitForce /= 4;
        base.AddForceFromMeleWeapon(direction, hitForce);
    }

    public override void TakeBulletHit(float hit)
    {
        hit *= 2.5f;
        TakeHit(hit);
    }


}
