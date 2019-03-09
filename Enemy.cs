using UnityEngine;
using System.Collections;
using System;

public abstract class Enemy : MonoBehaviour, IEnemy
{

    public enum State
    {
        Move,
        HeadingToAttack,
        Attack,
        Freeze,
        GetsUp,
        Shock,
        DisabledAfterPlayerWin
    }
    [SerializeField]
    protected float scale = 1;
    [SerializeField]
    protected float healthPoints = 10;
    [SerializeField]
    protected float attackPoints = 5;
    [SerializeField]
    protected float freezePoints = 10;
    [SerializeField]
    protected float freezeTime = 10;
    [SerializeField]
    protected float shockTime = 10;
    #region SOUNDS
    [SerializeField]
    protected AudioClip SoundExplosion;
    [SerializeField]
    protected AudioClip SoundHitGround;
    [SerializeField]
    protected AudioClip SoundFreeze;
    [SerializeField]
    protected AudioClip SoundAttack;
    [SerializeField]
    protected AudioClip SoundMove;
    [SerializeField]
    protected AudioClip SoundPain;
    #endregion

    protected AudioSource[] audioSources;
    protected GroundController groundController;
    protected HealthController healthController;
    protected FreezeController freezeController;
    protected PlayerHitTester playerHitTester;
    protected Rigidbody rigidBody;
    protected Transform player;
    protected Animator animator;
    [SerializeField]
    Explosion explosionPrefab;
    [SerializeField]
    GameObject dustPuffPrefab;
    [SerializeField]
    protected float rayCheckingGroundLength = 0.6f;
    protected float quaterCheckGround = 0.1f;
    protected float halfCheckGround = 0.1f;
    [SerializeField]
    protected float attackCheckDistance = 0.5f;
    [SerializeField]
    protected ParticleSystem damageSmoke;
    protected bool tryingToStandUp = false;
    protected State curState;
    protected int maxParticlesForDamageSmoke;
    protected int startParticlesForDamageSmoke;
    [HideInInspector]
    public EnemyPool enemyPool { get; set; }
    [HideInInspector]
    public bool backgroundEnemy = false;
    [SerializeField]
    protected uint type = 0; //type for enemy pool!!!
    private AttackClipEventHandler attackClipEventHandler;
    [SerializeField]
    protected Transform playerHitTesterPosition;
    [SerializeField]
    protected float playerHitTesterRadius = 0.09f;
    //
    protected float playHitDurability = 8f;
    [SerializeField]
    protected float durability = 11.0f;
    protected float pullingPartScale = 1;
    protected float timeOfCollision = 0;
    protected bool isActive = true;
    protected int layerMask;
    protected const float baseRadius = .22f;
    protected bool shocked = false;
    protected float countToShockTime = 0;
    protected float shockHitPoints;
    [SerializeField]
    internal Vector3 gridPosition;
    protected Vector3 preparedPlayerPosition;
    protected Vector3 currentPreparedPlayerPos;
    protected ExplosionPool explosionPool;
    protected float standUpTime;
    [SerializeField]
    protected float downRayCheckGroundMulti = 1;
    [SerializeField]
    protected float standUpRaysMulti = 1;
    protected AdditionalTargetController additionalTarget;




    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Vector3 down = transform.TransformDirection(Vector3.down) * downRayCheckGroundMulti;
        Gizmos.DrawWireCube(transform.position + down * rayCheckingGroundLength / 2, new Vector3(halfCheckGround, rayCheckingGroundLength, quaterCheckGround));
    }

    public virtual void SetActive(bool enabled)
    {

        gameObject.SetActive(false);
        isActive = false;
        if (enabled)
        {
            CheckForProperTarget();
            WaitForEmptySpace();
        }
    }

    private void CheckForProperTarget()
    {
        if (additionalTarget != null)
        {
            player = additionalTarget.transform;
        }
        else
        {
            player = PlayerBodyController.Instance.transform;
        }
    }

    private void WaitForEmptySpace()
    {
        if (CheckIfNotHitEnemy())
        {
            TurnOnEnemy();
        }
        else
        {

            Invoke("WaitForEmptySpace", .5f);
        }
    }

    protected virtual void TurnOnEnemy()
    {
        isActive = true;
        gameObject.SetActive(true);
    }


    protected void SetCommonComponents()
    {
        layerMask = (1 << LayerMask.NameToLayer("Enemies"));
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        audioSources = GetComponents<AudioSource>();
        explosionPool = StarterController.Instance.GetExplosionPool();
        startParticlesForDamageSmoke = damageSmoke.main.maxParticles;
        maxParticlesForDamageSmoke = damageSmoke.main.maxParticles * 2;
        durability *= durability;
        quaterCheckGround = rayCheckingGroundLength / 4;
        standUpTime = .2f * (Mathf.Max(scale * .5f, 2f));
    }



    protected void AddCommonClasses()
    {
        freezeController = new FreezeController(freezePoints, freezeTime);
        playerHitTester = new PlayerHitTester(playerHitTesterRadius);
        groundController = new GroundController(transform, rayCheckingGroundLength, downRayCheckGroundMulti, standUpRaysMulti);
        healthController = new HealthController(healthPoints);
        attackClipEventHandler = GetComponentInChildren<Animator>().gameObject.AddComponent<AttackClipEventHandler>();
        attackClipEventHandler.OnAttack += AttackHandler;
    }
    protected abstract void AttackHandler();

    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetBackgroundEnemy(bool isBackground)
    {
        backgroundEnemy = isBackground;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetGridPosition(Vector3 position)
    {
        gridPosition = position;
    }

    public virtual void SetAdditionalTarget(AdditionalTargetController target)
    {
        this.additionalTarget = target;
    }

    public virtual void AddForceFromMeleWeapon(Vector3 force)
    {
        KnockBack(force, 1);
    }

    public virtual void AddForceFromMeleWeapon(Vector3 direction, float hitForce)
    {
        KnockBack(direction, hitForce);
    }



    protected virtual void KnockBack(Vector3 direction, float hitForce)
    {
        if (!healthController.IsDead())
        {
            rigidBody.AddForce(direction * hitForce, ForceMode.Impulse);
        }
    }

    public virtual void MagnetPull(float force, Vector3 magnetPosition, Quaternion magnetRotation)
    {
        Vector3 direction = (magnetPosition - transform.position).normalized;
        float distance = Vector3.Distance(magnetPosition, transform.position);
        force /= pullingPartScale;
        if (distance < 1)
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.AddForce(direction * force * ((1000 / pullingPartScale) * distance), ForceMode.Acceleration);
        }
        else
        {
            if (transform.position.y < magnetPosition.y)
            {
                rigidBody.AddForce(transform.up * force * 2, ForceMode.Acceleration);
            }
            force = force / distance;
            rigidBody.AddForce(direction * force, ForceMode.VelocityChange);
        }
    }

    public virtual float GetRestartDistance()
    {
        return 0;
    }

    public virtual void MagnetPush(float force, Vector3 magnetPosition)
    {
        Vector3 direction = (transform.position - magnetPosition).normalized;
        float distanceValue = Vector3.Distance(magnetPosition, transform.position) * .5f;
        force = force / (distanceValue);
        rigidBody.AddForce(direction * force, ForceMode.Impulse);
    }

    public virtual void TakeFreezeHit(Vector3 direction, float hit, Vector3 weaponPosition)
    {
        float startForce = 10;
        if (!healthController.IsDead())
        {
            float distanceValue = Vector3.Distance(weaponPosition, transform.position) * .25f;
            float hitForce = startForce / distanceValue;
            rigidBody.AddForce(direction * hitForce, ForceMode.Impulse);

            if (!freezeController.IsFreezed())
            {
                freezeController.TakeDamage(hit);
            }
        }

    }



    public virtual void TakeHit(float hit)
    {
        healthController.TakeDamage(hit);
        if (healthController.IsDead())
        {
            ReturnToPool();
        }
    }

    public virtual void KillEnemy()
    {
        if (!healthController.IsDead())
        {
            healthController.Kill();
            ReturnToPool();
        }
    }


    public virtual void TakeHit(float hit, Vector3 direction, float hitForce)
    {
        KnockBack(direction, hitForce);
        TakeHit(hit);
    }

    public virtual void ReturnToPool()
    {
        if (enemyPool)
        {
            freezeController.Restore();
            healthController.Restore();
            ResetDamageSmoke();
            StopAllSounds();
            enemyPool.AddToPool(this, type);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void StopAllSounds()
    {
        int i;
        int length = audioSources.Length;
        for (i = 0; i < length; i++)
        {
            audioSources[i].Stop();
        }
    }

    protected void PlaySound(int num, AudioClip clip, Boolean loop, float volume = 1, float delay = 0)
    {
        if (transform.gameObject.activeSelf)
        {
            if (clip != null)
            {
                audioSources[num].Stop();
                audioSources[num].PlayDelayed(delay);
                audioSources[num].volume = volume;
                audioSources[num].loop = loop;
                audioSources[num].clip = clip;
                audioSources[num].Play();
            }
        }
    }

    protected void StopSound(int num)
    {
        audioSources[num].Stop();
    }

    protected void ResetDamageSmoke()
    {

        damageSmoke.Stop();
        damageSmoke.Clear();
        var ma = damageSmoke.main;
        ma.maxParticles = startParticlesForDamageSmoke;
    }

    protected void SetExplosions(float explosionScale, float explosionVolume)
    {
        float explosionRate = Mathf.Max(1, explosionScale / 4);
        Explosion boom = explosionPool.GetPooledExplosion(explosionPrefab.explosionType);
        boom.MakeExplosion(transform.position, Quaternion.identity);
        boom.PlayExplosionSound(SoundExplosion, explosionVolume, explosionRate);
    }

    protected void StartStandUpOperation()
    {
        if (groundController.CheckIfCanStartToStandUp())
        {
            if (!tryingToStandUp)
            {
                StartCoroutine(MakeStandUpAfterTime());
                tryingToStandUp = true;
            }

        }
    }

    IEnumerator MakeStandUpAfterTime()
    {
        yield return new WaitForSeconds(.25f);
        StandingOperation();
    }

    protected virtual void StandingOperation()
    {
        Vector3 endRotation = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
        LeanTween.rotate(gameObject, endRotation, standUpTime).setOnComplete(CheckIfStandUpComplete);
        rigidBody.constraints = RigidbodyConstraints.FreezePositionX;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ;
    }

    public virtual bool CanDoAction()
    {
        return (!tryingToStandUp && groundController.CheckIfOnGround());
    }


    protected void CheckIfStandUpComplete()
    {

        if (RightZeroAngle(transform.rotation.eulerAngles.x) && RightZeroAngle(transform.rotation.eulerAngles.z))
        {
            StopCoroutine(MakeStandUpAfterTime());
            rigidBody.constraints = RigidbodyConstraints.None;
            LeanTween.cancel(gameObject);
            tryingToStandUp = false;
        }
    }

    private bool RightZeroAngle(float angle)
    {
        return angle < 1 || angle > 359;
    }

    /*** FSM Actions **/

    void Start()
    {
        EnemyInitialize();
    }

    protected virtual void EnemyInitialize()
    {
    }

    void Update()
    {
        EnemyStateUpdate();
    }


    protected virtual void EnemyStateUpdate()
    {
        if (isActive)
        {
            if (healthController != null)
            {
                if (!healthController.IsDead())
                {
                    CheckIfGameWinByPlayer();
                    MakeCustomActions();
                    CheckToShowDamage();
                    FreezeActions();
                    SetProperState();
                }
            }
        }
    }

    private void CheckIfGameWinByPlayer()
    {
        if (enemyPool.levelWin)
        {
            if (curState != State.DisabledAfterPlayerWin)
            {
                ActionsBeforeDisabledAfterPlayerWins();
                curState = State.DisabledAfterPlayerWin;
            }
        }
    }

    protected virtual void ActionsBeforeDisabledAfterPlayerWins()
    {

    }

    protected virtual void MakeCustomActions()
    {

    }

    protected virtual float DistanceToPlayer
    {
        get
        {
            currentPreparedPlayerPos = new Vector3(player.position.x, player.position.y, player.position.z);
            return Vector3.Distance(currentPreparedPlayerPos, transform.position);
        }
    }

    private void CheckToShowDamage()
    {
        if (healthController.GetPercentage() < 50)
        {
            var ma = damageSmoke.main;
            if (!damageSmoke.isPlaying)
            {
                ma.startSize = new ParticleSystem.MinMaxCurve(ma.startSize.constant * scale);
                damageSmoke.Play();
            }
            if (healthController.GetPercentage() <= 25)
            {
                ma.maxParticles = maxParticlesForDamageSmoke;
            }
        }

    }

    private void FreezeActions()
    {
        CheckIfFreeze();
        freezeController.Recover();
    }
    protected abstract void CheckIfFreeze();



    protected void StartDustAnimation()
    {
        GameObject dust = Instantiate(dustPuffPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity) as GameObject;
        var ma = dust.GetComponent<ParticleSystem>().main;
        ma.startSize = new ParticleSystem.MinMaxCurve(ma.startSize.constant * scale);
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (Time.time - timeOfCollision > .1f)
        {
            if (healthController != null)
            {
                if (!healthController.IsDead())
                {
                    CheckToImpact(collision);
                }
            }
        }

        timeOfCollision = Time.time;
    }

    protected virtual void CheckToImpact(Collision collision)
    {
        if (LayerMask.LayerToName(collision.collider.gameObject.layer) == "DeadZone")
        {
            KillEnemy();
        }
        else
        {
            if (LayerMask.LayerToName(collision.collider.gameObject.layer) != "Ammo" && LayerMask.LayerToName(collision.collider.gameObject.layer) != "Weapons" && LayerMask.LayerToName(collision.collider.gameObject.layer) != "PlayerBody" && LayerMask.LayerToName(collision.collider.gameObject.layer) != "EnemyParts")
            {
                if (collision.relativeVelocity.sqrMagnitude > playHitDurability)
                {
                    if (!healthController.IsDead())
                    {
                        if (collision.relativeVelocity.sqrMagnitude > durability)
                        {
                            KillEnemy();
                        }
                        else
                        {
                            StartDustAnimation();
                            PlaySound(1, SoundHitGround, false);
                        }
                    }
                }
            }
        }
    }

    protected virtual void PlayMoveSound()
    {
        if (!audioSources[0].isPlaying)
        {
            PlaySound(0, SoundMove, true);
        }
    }

    protected void StopMoveSound()
    {
        if (audioSources[0].isPlaying)
        {
            audioSources[0].Stop();
        }
    }

    protected void PlayPainSound()
    {
        PlaySound(1, SoundPain, false);
    }

    private void SetProperState()
    {
        switch (curState)
        {
            case State.Move:
                UpdateMoveState();
                break;
            case State.Attack:
                UpdateAttackState();
                break;
            case State.HeadingToAttack:
                UpdateDiveState();
                break;
            case State.Freeze:
                UpdateFreezeState();
                break;
            case State.GetsUp:
                UpdateStandingState();
                break;
            case State.Shock:
                UpdateShockState();
                break;
            case State.DisabledAfterPlayerWin:
                UpdateDisabledAfterPlayerWin();
                break;

        }
    }

    protected abstract void UpdateStandingState();
    protected abstract void UpdateFreezeState();
    protected abstract void UpdateDiveState();
    protected abstract void UpdateAttackState();
    protected abstract void UpdateMoveState();
    protected abstract void UpdateShockState();
    protected virtual void UpdateDisabledAfterPlayerWin() { }




    public bool CheckIfNotHitEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, baseRadius * scale, layerMask);
        return colliders.Length == 0;
    }

    protected bool CheckIfCanAttack()
    {
        return (DistanceToPlayer < attackCheckDistance);
    }

    protected void GivePlayerDamage(float hit, bool headHit, GameObject playerBody)
    {
        if (additionalTarget != null)
        {
            AdditionalTargetController at = playerBody.GetComponentInChildren<AdditionalTargetController>();
            if (at != null)
            {
                at.TakeHit(hit);
            }
        }
        else
        {
            PlayerBodyController pb = playerBody.GetComponentInChildren<PlayerBodyController>();
            if (pb != null)
            {
                pb.PlayerTakeHit(hit, headHit);
            }
        }
    }

    public bool StillExcist()
    {
        return isActive && !healthController.IsDead();
    }

    public ObjectMaterialType.type GetMaterial()
    {
        return ObjectMaterialType.type.Metal;
    }

    public void TakeShock(float hit)
    {
        if (freezeController.IsFreezed())
        {
            KillEnemy();
        }
        else
        {
            MakeShock(hit);
        }
    }

    protected virtual void MakeShock(float hitPoints)
    {

    }

    public virtual void ExplosionHit(float force, Vector3 explosionPosition, float explosionRadius, float hit)
    {
        if (rigidBody)
        {
            rigidBody.AddExplosionForce(CountExplosionForce(force), explosionPosition, explosionRadius);
            TakeHit(CountHit(hit, explosionPosition));
        }
    }

    protected virtual float CountExplosionForce(float force)
    {
        return force;
    }

    private float CountHit(float maxHit, Vector3 explosionPosition)
    {
        return Mathf.Max(0, (maxHit - maxHit * CountZeroOneDistance(explosionPosition)) * 2.8f);
    }

    private float CountZeroOneDistance(Vector3 explosionPosition)
    {
        float distance = Vector3.Distance(transform.position, explosionPosition);
        return distance / DynamiteController.explosionRange;
    }

    public virtual float GetBoundsScale()
    {
        float boundsScale = 1;

        switch ((int)scale)
        {
            case 2:
                boundsScale = 1.3f;
                break;
            case 4:
                boundsScale = 1.85f;
                break;
            case 8:
                boundsScale = 3.6f;
                break;
        }
        return boundsScale;
    }

    public float GetScale()
    {
        return scale;
    }

    public float GetFreezePercent()
    {
        if (freezeController != null)
        {
            return freezeController.GetFreezePercent();
        }
        else
        {
            return 1;
        }
    }

    public bool IsDead()
    {
        return healthController.IsDead();
    }

    public bool IsFreezed()
    {
        return freezeController.IsFreezed();
    }

    public bool GetShocked()
    {
        return shocked;
    }

    public virtual void TakeBulletHit(float hit)
    {
        TakeHit(hit);
    }

    public float CurrentDistanceToPlayer
    {
        get
        {
            return DistanceToPlayer;
        }
    }

    protected void SetKeyRotation()
    {
        float speed = (1 / scale) * freezeController.GetFreezePercent();
        animator.SetFloat("KeyMultiplier", speed);
    }

    protected void StopKeyRotation()
    {
        animator.SetFloat("KeyMultiplier", 0);
    }
}
