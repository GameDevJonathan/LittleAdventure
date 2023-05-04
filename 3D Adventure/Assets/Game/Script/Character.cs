using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController _cc;
    public float MoveSpeed = 5f;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private float _verticalVelocity;
    public float _gravity = -9.8f;
    private Animator _animator;

    //enemy
    public bool isPlayer = true;
    private NavMeshAgent _navMeshAgent;
    private Transform TargetPlayer;

    //player
    private float attackStartTime;
    public float AttackSlideDuration = 0.4f;
    public float AttackSlideSpeed = 0.06f;

    private Health _health;
    private DamageCaster _damageCaster;

    public enum CharacterState { Normal, Attacking };

    public CharacterState currentState;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        if (!isPlayer)
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            TargetPlayer = GameObject.FindWithTag("Player").transform;
            _navMeshAgent.speed = MoveSpeed;
        }
        else
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }

    public void CalculateEnemyMovement()
    {
        if (Vector3.Distance(TargetPlayer.position, transform.position) > _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);

            SwitchState(CharacterState.Attacking);
        }

    }

    public void CalculatePlayerMovement()
    {
        if(_playerInput.MouseButtonDown && _cc.isGrounded)
        {
            SwitchState(CharacterState.Attacking);
            return;
        }

        _movementVelocity.Set(_playerInput.HorizontalInput, 0f, _playerInput.VerticalInput);
        _movementVelocity.Normalize();
        _movementVelocity = Quaternion.Euler(0, -45f, 0) * _movementVelocity;

        _animator.SetFloat("Speed", _movementVelocity.magnitude);
        _movementVelocity *= MoveSpeed * Time.deltaTime;

        if (_movementVelocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_movementVelocity);

        _animator.SetBool("Airborne", !_cc.isGrounded);
    }

    public void FixedUpdate()
    {
        switch (currentState)
        {
            case CharacterState.Normal:
                if (isPlayer)
                {
                    CalculatePlayerMovement();
                }
                else
                {
                    CalculateEnemyMovement();
                }
                break;
            case CharacterState.Attacking:
                if (isPlayer)
                {
                    _movementVelocity = Vector3.zero;

                    if(Time.time < attackStartTime + AttackSlideDuration)
                    {
                        float timePassed = Time.time - attackStartTime;
                        float lerpTime = timePassed / AttackSlideDuration;
                        _movementVelocity = Vector3.Lerp(transform.forward * AttackSlideDuration, Vector3.zero, lerpTime);
                    }
                }
                break;
        }

        if (isPlayer)
        {
            if (_cc.isGrounded == false)
                _verticalVelocity = _gravity;
            else
                _verticalVelocity = _gravity * 0.3f;

            _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;
            _cc.Move(_movementVelocity);
        }

    }

    public void SwitchState(CharacterState newState)
    {
        //clear Cache
        if(isPlayer)
        _playerInput.MouseButtonDown = false;

        //exiting state
        switch (currentState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                break;
        }

        //entering state
        switch (newState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if (!isPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(TargetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }


                _animator.SetTrigger("Attack");

                if (isPlayer)
                {
                    attackStartTime = Time.time;
                }
                break;
        }

        currentState = newState;
        Debug.Log("SwitchState to + " + currentState);
    }

    public void AttackAnimationEnds()
    {
        SwitchState(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if(_health != null)
        {
            _health.ApplyDamage(damage);
        }
    }

    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();

    }

    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }

}
