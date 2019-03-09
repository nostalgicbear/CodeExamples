using UnityEngine;
using System.Collections;
using System;

public class RatApexController : Enemy
{       

    private State prevState;
    private float attackSpeed;
    private float attackMomentInTime;    
    [SerializeField]
    private float playerHitPosMove = .05f;
    //alt kill system
    const float maxAlt = 7f;
    const float maxTimeInAlt = 12f;
    float altTime = 0;
    private RatApexMover ratApexMover;
    [SerializeField]
    protected float restartDistance = 1;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerHitTesterPosition.position + playerHitTesterPosition.forward * playerHitPosMove, playerHitTesterRadius);
    }

    public override float GetRestartDistance()
    {        
        return restartDistance;
    }

   

    public override void MagnetPush(float force, Vector3 magnetPosition)
    {
        switch ((int)scale)
        {
            case 2:
                force *= scale * 4f;
                break;
            case 4:
                force *= scale * 80f;
                break;
            case 8:
                force *= scale * 1600;
                break;

        }
        base.MagnetPush(force, magnetPosition);
    }

    public override void AddForceFromMeleWeapon(Vector3 direction, float hitForce)
    {
        switch ((int)scale)
        {
            case 1:
                hitForce /= 8;
                break;
            case 2:
                hitForce *= scale * 1.5f;
                break;
            case 4:
                hitForce *= scale * 10;

                break;
            case 8:
                hitForce *= scale * 100;
                break;

        }
        base.AddForceFromMeleWeapon(direction, hitForce);
    }

    protected override void CheckIfFreeze()
    {
        if (freezeController.IsFreezed())
        {
            if (curState != State.Freeze)
            {
                prevState = curState;                
                StopKeyRotation();
                SetFreezeForAnimator(true);
                curState = State.Freeze;
                SetWheelsSpeed(0);
                StopMoveSound();
                PlaySound(1, SoundFreeze, false);
                ratApexMover.StopUnit();
            }

        }
        else
        {
            if (curState == State.Freeze)
            {
                SetFreezeForAnimator(false);
                curState = prevState;
                ratApexMover.MoveUnit(currentPreparedPlayerPos);
            }
            else
            {
                PlayMoveSound();
            }

        }
    }

    protected override void UpdateAttackState()
    {
        if (CanDoAction())
        {
            if (!CheckIfCanAttack())
            {
                curState = State.Move;
            }
            SetAttackAnimation();                        
            SetKeyRotation();            
            SetWheelsAnimation();
            SetProperMoveForRatApexMover();
        }
        else
        {
            curState = State.GetsUp;
        }
    }

    protected override void UpdateDiveState()
    {

    }

    protected override void UpdateFreezeState()
    {
        SetWheelsSpeed(0);
    }

    protected override void ActionsBeforeDisabledAfterPlayerWins()
    {        
        StopKeyRotation();
        SetFreezeForAnimator(true);     
        SetWheelsSpeed(0);
        StopMoveSound();
        PlaySound(1, SoundFreeze, false);
        ratApexMover.StopUnit();
    }

    protected override void UpdateDisabledAfterPlayerWin()
    {
        SetWheelsSpeed(0);
    }

    protected override void UpdateMoveState()
    {
        if (CanDoAction())
        {
           if (CheckIfCanAttack())
            {
                curState = State.Attack;
            }
            SetMoveAnimation();
            SetKeyRotation();
            SetWheelsAnimation();
            SetProperMoveForRatApexMover();

        }
        else
        {
            curState = State.GetsUp;
        }
    }

    private void SetProperMoveForRatApexMover()
    {        
        if (Vector3.Distance(currentPreparedPlayerPos, preparedPlayerPosition) > .25f)
        {
           
            preparedPlayerPosition = currentPreparedPlayerPos;           
            ratApexMover.MoveUnit(currentPreparedPlayerPos);
        }
    }

    protected override float DistanceToPlayer
    {
        get
        {            
            currentPreparedPlayerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            return Vector3.Distance(currentPreparedPlayerPos, transform.position);
        }
    }

    protected override void UpdateStandingState()
    {
        if (CanDoAction())
        {
            curState = State.Move;
        }
        else
        {            
            StartStandUpOperation();
            SetKeyRotation();
            SetWheelsSpeed(0);
        }
    }

    override protected void EnemyInitialize()
    {
        SetComponents();
        SetParamsDependsOnScale();
        AddClasses();
        SetStartParams();
    }

    private void SetParamsDependsOnScale()
    {
        if (scale > 1)
        {
            pullingPartScale = scale * .6f;
            rigidBody.mass *= Mathf.Pow(scale, 3);                    
        }
    }

    private void AddClasses()
    {
        base.AddCommonClasses();
    }

    private void SetComponents()
    {
        base.SetCommonComponents();
        ratApexMover = GetComponent<RatApexMover>();
    }

    private void SetStartParams()
    {
        SetOtherParams();
        SetKeyRotation();
        SetAttackSpeed();
    }    

    void SetOtherParams()
    {
        prevState = curState = State.Move;
    }


    void SetAttackSpeed()
    {
        attackSpeed = Mathf.Max(.5f, 2 / scale);
        attackMomentInTime = 0.5f / attackSpeed;
        animator.SetFloat("AttackMultiplier", attackSpeed);
    }

    private void SetWheelsAnimation()
    {
        if (!healthController.IsDead())
        {            
            SetWheelsSpeed(ratApexMover.GetVelocity());
        }
        else
        {
            SetWheelsSpeed(0);
        }
    }

    public Vector3 GetCurrentPreparedPlayerPos()
    {
        return currentPreparedPlayerPos;
    }

    override public void ReturnToPool()
    {
        SetExplosions(scale, countVolumeForScale());
        ratApexMover.StopUnit();
        curState = State.Move;
        SetMoveAnimation();       
        rigidBody.velocity = Vector3.zero;
        animator.SetTrigger("Idle");
        StopAllCoroutines();
        shocked = false;
        tryingToStandUp = false;
        base.ReturnToPool();
    }

    protected override void TurnOnEnemy()
    {
        base.TurnOnEnemy();
        if (ratApexMover != null)
        {
            ratApexMover.MoveUnit(currentPreparedPlayerPos);
        }
    }

    protected override void AttackHandler()
    {
        StartCoroutine(AttackForRat());
    }

    IEnumerator AttackForRat()
    {
        yield return new WaitForSeconds(attackMomentInTime);
        PlaySound(1, SoundAttack, false, countVolumeForScale());
        if (playerHitTester.CheckIfHitPlayer(playerHitTesterPosition.position + playerHitTesterPosition.forward * playerHitPosMove))
        {
            GivePlayerDamage(attackPoints, false, playerHitTester.GetPlayerGameObject());
        }
    }

    protected float countVolumeForScale()
    {
        float volume = .8f;
        if (scale < 8)
        {
            if (scale < 4)
            {
                volume = scale / 4;
            }
            else
            {
                volume = scale / 6;
            }
        }

        return volume;
    }

    private void SetWheelsSpeed(float speed)
    {
        animator.SetFloat("WheelsMultiplier", speed);
    }

    protected virtual void SetAttackAnimation()
    {
        animator.SetBool("CanAttack", true);
    }

    protected void SetMoveAnimation()
    {
        animator.SetBool("CanAttack", false);
    }

    private void SetFreezeForAnimator(bool enabled)
    {
        animator.SetBool("IsFreeze", enabled);
    }

    override protected void MakeCustomActions()
    {
        if (isActive)
        {
            if (!healthController.IsDead())
            {                
                CheckAltitude();
            }
        }
    }

    private void CheckAltitude()
    {
        if (transform.position.y < maxAlt)
        {
            altTime = 0;
        }
        else
        {
            altTime += Time.deltaTime;
            if (altTime >= maxTimeInAlt)
            {
                KillEnemy();
            }
        }
    }


    override protected void MakeShock(float hitPoints)
    {
        if (!shocked)
        {
            if (!IsDead())
            {
                shockHitPoints = hitPoints;
                countToShockTime = 0;
                shocked = true;
                prevState = curState;
                ratApexMover.StopUnit();
                StopKeyRotation();
                SetWheelsSpeed(0);
                StopMoveSound();
                PlaySound(1, SoundPain, false, 1);
                curState = State.Shock;
                animator.SetTrigger("Shock");
            }
        }
    }

    protected override void UpdateShockState()
    {
        if (shocked)
        {
            StopMoveSound();
            countToShockTime += Time.deltaTime;
            if (countToShockTime >= shockTime)
            {
                BackFromShock();
            }
        }
    }

    void BackFromShock()
    {
        countToShockTime = 0;
        animator.SetTrigger("Idle");
        curState = prevState;
        shocked = false;        
        TakeHit(shockHitPoints);
        if (!IsDead())
        {
            ratApexMover.MoveUnit(currentPreparedPlayerPos);
        }
    }

    override  protected float CountExplosionForce(float force)
    {
        float returnForce = force / 4;

        switch ((int)scale)
        {
            case 2:
                returnForce = force * 12;
                break;
            case 4:
                returnForce = force * 250;
                break;
            case 8:
                returnForce = force * 7500;
                break;
        }
        return returnForce;
    }


}
