using System.Collections;
using System.Collections.Generic;
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

    public enum CharacterState { Normal, Attacking };

    public CharacterState currentState;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _cc = GetComponent<CharacterController>();

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
        if (Vector3.Distance(TargetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);
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
                    if (_cc.isGrounded == false)
                        _verticalVelocity = _gravity;
                    else
                        _verticalVelocity = _gravity * 0.3f;

                    _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;
                    _cc.Move(_movementVelocity);

                }
                else
                {
                    CalculateEnemyMovement();
                }
                break;
            case CharacterState.Attacking:
                break;
        }

    }

    public void SwitchState(CharacterState newState)
    {
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
                break;
        }

        currentState = newState;
        Debug.Log("SwitchState to + " + currentState);
    }

}
