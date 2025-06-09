using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Interaction")]
        [Tooltip("The maximum distance for picking up objects")]
        public float interactionDistance = 3f; // Увеличено для двери
        [Tooltip("The crosshair UI component")]
        public Crosshair crosshair; // UI-точка прицела
        [Tooltip("Distance in front of player for holding objects")]
        public float holdDistance = 1f; // Расстояние для зависания объектов
        [Tooltip("Height offset for holding objects")]
        public float holdHeightOffset = 1f; // Высота зависания (относительно ног персонажа)
        [Tooltip("Rotation speed for held object using mouse scroll (degrees per second)")]
        public float objectRotationSpeed = 90f; // Скорость вращения удерживаемого объекта
        [Tooltip("Force applied when pushing held object")]
        public float pushForce = 10f; // Сила толчка удерживаемого объекта

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // interaction
        private GameObject _heldObject; // Объект, который "зависает" перед персонажем
        private GameObject _targetObject; // Объект, на который смотрит игрок

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private bool _hasAnimator;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            if (crosshair != null)
            {
                crosshair.SetActive(false);
            }
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            CheckForInteractable();
            HandleInteraction();
            UpdateHeldObjectPosition();
            RotateHeldObject();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Поворачиваем тело персонажа в направлении взгляда (курсора)
            transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);

            // Движение относительно направления камеры
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                // Получаем направление камеры (без вертикальной компоненты)
                Vector3 cameraForward = _mainCamera.transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = _mainCamera.transform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                // Вычисляем направление движения
                Vector3 moveDirection = cameraRight * inputDirection.x + cameraForward * inputDirection.z;

                // Двигаем персонажа
                _controller.Move(moveDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            else
            {
                // Если нет ввода, применяем только вертикальную скорость
                _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void UpdateHeldObjectPosition()
        {
            if (_heldObject != null)
            {
                if (_mainCamera == null)
                {
                    Debug.LogError("MainCamera is null! Cannot position held object.");
                    return;
                }

                // Raycast из камеры для определения точки, куда смотрит курсор
                Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
                Vector3 targetPosition = ray.origin + ray.direction * holdDistance; // Позиция в центре экрана на расстоянии holdDistance

                // Используем Rigidbody для физического перемещения
                var rb = _heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Вычисляем вектор к целевой позиции
                    Vector3 direction = targetPosition - _heldObject.transform.position;
                    float distance = direction.magnitude;

                    // Применяем силу для перемещения
                    float forceMagnitude = 50f * distance; // Пропорционально расстоянию
                    rb.AddForce(direction.normalized * forceMagnitude, ForceMode.Acceleration);

                    // Демпфируем скорость для плавности
                    rb.velocity *= 0.9f; // Уменьшаем скорость на 10% за кадр
                    rb.angularVelocity *= 0.9f; // Демпфируем вращение

                    Debug.Log($"Holding: {_heldObject.name}, Position: {_heldObject.transform.position}, Target: {targetPosition}, Velocity: {rb.velocity}");
                }
                else
                {
                    Debug.LogError($"No Rigidbody on {_heldObject.name}!");
                }
            }
        }

        private void RotateHeldObject()
        {
            if (_heldObject == null || _input == null) return;

            // Проверяем ввод колеса мыши
            float scrollInput = _input.mouseScroll; // Предполагается, что mouseScroll — float в StarterAssetsInputs
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                // Определяем ось вращения: положительный scroll — ось X, отрицательный — ось Y
                Vector3 rotationAxis = scrollInput > 0 ? Vector3.right : Vector3.up;
                float rotationAmount = objectRotationSpeed * Time.deltaTime;

                // Вращаем объект в локальном пространстве
                _heldObject.transform.Rotate(rotationAxis, rotationAmount, Space.Self);

                // Сбрасываем угловую скорость Rigidbody, чтобы избежать влияния физики
                var rb = _heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"Rotating {_heldObject.name} around {rotationAxis} by {rotationAmount} degrees");
            }
        }

        private void CheckForInteractable()
        {
            RaycastHit hit;
            bool isInteractable = false;

            // Визуализация луча (красный, длительность 0.1 сек)
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance * 1000, Color.red, 0.1f);

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                GameObject hitObject = hit.collider.gameObject;
                GameObject targetObject = hitObject;

                // Проверяем, есть ли родитель у объекта
                

                float dist = Vector3.Distance(transform.position, targetObject.transform.position);

                // Проверяем тег объекта (родителя или самого объекта)
                if (targetObject.CompareTag("Cup") || targetObject.CompareTag("Lid"))
                {
                    isInteractable = true;
                    CheckPerent("Cup");
                    CheckPerent("Lid");
                }
                else if (targetObject.CompareTag("CoffeeMachine") && dist < 2f)
                {
                    isInteractable = true;
                    CheckPerent("CoffeeMachine");
                }
                else if (targetObject.CompareTag("Door") && dist < 3f)
                {
                    isInteractable = true;
                    CheckPerent("Door");
                }
                else if (targetObject.CompareTag("SwitchLight") && dist < 3f)
                {
                    isInteractable = true;
                    CheckPerent("SwitchLight");
                }
                else
                {
                    Debug.Log($"Raycast hit: {hitObject.name}, Target: {targetObject.name}, Tag: {targetObject.tag}, Not interactable");
                }

                void CheckPerent(string tag)
                {
                    if (targetObject.transform.parent != null)
                    {
                        if(targetObject.transform.parent.CompareTag(tag))
                            _targetObject = targetObject.transform.parent.gameObject;
                        else
                        {
                            _targetObject = targetObject;
                        }
                    }
                }
                
                
            }
            else
            {
                Debug.Log("Raycast missed");
            }

            if (!isInteractable)
            {
                _targetObject = null;
            }

            if (crosshair != null)
            {
                crosshair.SetActive(isInteractable);
            }
        }

        private void HandleInteraction()
        {
            if (_input == null)
            {
                Debug.LogError("StarterAssetsInputs component is null!");
                return;
            }

            // Подбор или отпускание объекта (Interact)
            if (_input.interact)
            {
                if (_heldObject == null && _targetObject != null)
                {
                    if (_targetObject.CompareTag("Cup") || _targetObject.CompareTag("Lid"))
                    {
                        // Подбираем объект
                        Debug.Log($"Attempting to pick up: {_targetObject.name}");
                        _heldObject = _targetObject;

                        
                        // Настраиваем Rigidbody
                        var rb = _heldObject.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            Debug.Log($"Adding Rigidbody to {_targetObject.name}");
                            rb = _heldObject.AddComponent<Rigidbody>();
                        }
                        rb.useGravity = false; // Отключаем гравитацию для "зависания"
                        rb.drag = 5f; // Плавность движения
                        rb.angularDrag = 5f; // Плавность вращения

                        // Вызываем PickUp для Cup
                        if (_heldObject.CompareTag("Cup"))
                        {
                            var cupScript = _heldObject.GetComponent<CoffeeFillSingleParticle>();
                            if (cupScript != null)
                            {
                                Debug.Log("Calling Cup.PickUp");
                                try
                                {
                                    cupScript.PickUp();
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError($"Error in Cup.PickUp: {e.Message}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"No CoffeeFillSingleParticle on {_heldObject.name}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"Cannot pick up {_targetObject.name}: Invalid tag ({_targetObject.tag})");
                    }
                }
                else if (_heldObject != null)
                {
                    // Отпускаем объект
                    Debug.Log($"Dropping: {_heldObject.name}");
                    if (_heldObject.CompareTag("Cup"))
                    {
                        var cupScript = _heldObject.GetComponent<CoffeeFillSingleParticle>();
                        if (cupScript != null)
                        {
                            Debug.Log("Calling Cup.Drop");
                            try
                            {
                                cupScript.Drop();
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"Error in Cup.Drop: {e.Message}");
                            }
                        }
                    }

                    // Восстанавливаем физику
                    var rb = _heldObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.useGravity = true;
                        rb.drag = 0f;
                        rb.angularDrag = 0.05f; // Стандартное значение
                    }
                    var collider = _heldObject.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    _heldObject = null;
                }
                else
                {
                    Debug.Log("No valid target to pick up");
                }
                _input.interact = false; // Сбрасываем ввод
            }

            // Использование объекта (Use)
            if (_input.use && _targetObject != null)
            {
                Debug.Log($"Use action triggered on: {_targetObject.name}");
                if (_heldObject != null && _heldObject.CompareTag("Cup"))
                {
                    // Проверяем, есть ли крышечка перед персонажем
                    GameObject heldLid = null;
                    if (_targetObject.CompareTag("Lid"))
                    {
                        heldLid = _targetObject;
                    }
                    if (heldLid != null)
                    {
                        var cupScript = _heldObject.GetComponent<CoffeeFillSingleParticle>();
                        if (cupScript != null)
                        {
                            Debug.Log("Placing Lid on Cup");
                            try
                            {
                                cupScript.PlaceLid(heldLid);
                                _heldObject = null;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"Error in Cup.PlaceLid: {e.Message}");
                            }
                        }
                    }
                }
                else if (_targetObject.CompareTag("CoffeeMachine"))
                {
                    float dist = Vector3.Distance(transform.position, _targetObject.transform.position);
                    Debug.Log($"CoffeeMachine distance: {dist}");
                    if (dist < 2f)
                    {
                        var machineScript = _targetObject.GetComponent<CoffeeMachine>();
                        if (machineScript != null)
                        {
                            Debug.Log("Calling CoffeeMachine.Use");
                            machineScript.Use();
                        }
                        else
                        {
                            Debug.LogError("CoffeeMachine script not found on target object!");
                        }
                    }
                    else
                    {
                        Debug.Log($"CoffeeMachine too far: {dist} meters");
                    }
                }
                else if (_targetObject.CompareTag("Door"))
                {
                    float dist = Vector3.Distance(transform.position, _targetObject.transform.position);
                    Debug.Log($"Door distance: {dist}");
                    if (dist < 3f)
                    {
                        var doorScript = _targetObject.GetComponent<SojaExiles.OpenCloseDoor>();
                        if (doorScript != null)
                        {
                            Debug.Log("Calling OpenCloseDoor.Use");
                            doorScript.Use();
                        }
                    }
                }
                else if (_targetObject.CompareTag("SwitchLight"))
                {
                    float dist = Vector3.Distance(transform.position, _targetObject.transform.position);
                    Debug.Log($"SwitchLight: {dist}");
                    if (dist < 3f)
                    {
                        var doorScript = _targetObject.GetComponent<LightSwitch>();
                        if (doorScript != null)
                        {
                            Debug.Log("Calling SwitchLight.Use");
                            doorScript.Use();
                        }
                    }
                }
                _input.use = false; // Сбрасываем ввод
            }

            // Толчёк удерживаемого объекта (Push)
            if (_input.push && _heldObject != null)
            {
                var rb = _heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Применяем силу в направлении взгляда камеры
                    Vector3 pushDirection = _mainCamera.transform.forward;
                    rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                    Debug.Log($"Pushing {_heldObject.name} with force {pushForce} in direction {pushDirection}");

                    // Отпускаем объект после толчка
                    if (_heldObject.CompareTag("Cup"))
                    {
                        var cupScript = _heldObject.GetComponent<CoffeeFillSingleParticle>();
                        if (cupScript != null)
                        {
                            Debug.Log("Calling Cup.Drop");
                            try
                            {
                                cupScript.Drop();
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"Error in Cup.Drop: {e.Message}");
                            }
                        }
                    }

                    rb.useGravity = true;
                    rb.drag = 0f;
                    rb.angularDrag = 0.05f;
                    var collider = _heldObject.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    _heldObject = null;
                }
                _input.push = false; // Сбрасываем ввод
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}