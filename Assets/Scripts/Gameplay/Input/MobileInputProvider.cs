using UnityEngine;
using VContainer;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Gameplay.Input
{
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Zones")]
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private float swipeUpThreshold = 50f;
        [SerializeField] private float swipeDownThreshold = 30f;
        [SerializeField] private float dynamicJoystickRange = 80f;

        [Header("Visual")]
        [SerializeField] private RectTransform joystickBase;
        [SerializeField] private RectTransform joystickThumb;

        public Vector2 MovementInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool DropPressed { get; private set; }

        private int _moveTouchId = -1;
        private Vector2 _joystickOrigin;
        private Vector2 _joystickCurrent;

        private int _jumpTouchId = -1;
        private Vector2 _jumpTouchStart;
        private bool _jumpConsumed;
        private bool _dropConsumed;

        private float _screenMidX;

        private GameOverState _gameOverState;

        [Inject]
        public void Construct(GameOverState gameOverState)
        {
            _gameOverState = gameOverState;
            _gameOverState.OnEntered += ResetVisuals;
        }

        private void Awake()
        {
            _screenMidX = Screen.width * 0.5f;
        }

        private void OnDestroy()
        {
            if (_gameOverState != null)
                _gameOverState.OnEntered -= ResetVisuals;
        }

        private void OnEnable()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        }

        public void Poll()
        {
            JumpPressed = false;
            JumpReleased = false;
            DropPressed = false;

            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (touch.screenPosition.x < _screenMidX)
                    HandleMoveTouch(touch);
                else
                    HandleJumpTouch(touch);
            }

            UpdateMovementInput();
            CleanupEndedTouches();
        }

        private void HandleMoveTouch(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _moveTouchId = touch.touchId;
                _joystickOrigin = touch.screenPosition;
                _joystickCurrent = touch.screenPosition;
            }
            else if (touch.touchId == _moveTouchId)
            {
                _joystickCurrent = touch.screenPosition;
            }
        }

        private void HandleJumpTouch(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _jumpTouchId = touch.touchId;
                _jumpTouchStart = touch.screenPosition;
                _jumpConsumed = false;
                _dropConsumed = false;
            }

            if (touch.touchId != _jumpTouchId) return;

            float swipeDeltaY = touch.screenPosition.y - _jumpTouchStart.y;

            if (!_jumpConsumed && !_dropConsumed)
            {
                if (swipeDeltaY > swipeUpThreshold)
                {
                    JumpPressed = true;
                    _jumpConsumed = true;
                }
                else if (swipeDeltaY < -swipeDownThreshold)
                {
                    _dropConsumed = true;
                }
            }

            if (_dropConsumed && touch.phase != UnityEngine.InputSystem.TouchPhase.Ended)
                DropPressed = true;

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (!_jumpConsumed && !_dropConsumed)
                    JumpPressed = true;

                JumpReleased = true;
                DropPressed = false;
                _jumpTouchId = -1;
                _jumpConsumed = false;
                _dropConsumed = false;
            }
        }

        private void UpdateMovementInput()
        {
            if (_moveTouchId == -1)
            {
                MovementInput = Vector2.zero;
                if (joystickBase != null)
                    joystickBase.gameObject.SetActive(false);
                return;
            }

            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(true);
                joystickBase.position = _joystickOrigin;
            }

            Vector2 delta = _joystickCurrent - _joystickOrigin;
            Vector2 normalized = new Vector2(
                Mathf.Clamp(delta.x / dynamicJoystickRange, -1f, 1f),
                Mathf.Clamp(delta.y / dynamicJoystickRange, -1f, 1f));

            if (joystickThumb != null)
                joystickThumb.anchoredPosition = new Vector2(
                    normalized.x * dynamicJoystickRange,
                    normalized.y * dynamicJoystickRange);

            MovementInput = Mathf.Abs(normalized.x) > deadZone
                ? new Vector2(normalized.x, 0f)
                : Vector2.zero;
        }

        private void CleanupEndedTouches()
        {
            bool moveActive = false;

            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (touch.touchId == _moveTouchId)
                    moveActive = true;
            }

            if (!moveActive)
            {
                _moveTouchId = -1;
                MovementInput = Vector2.zero;
            }
        }

        private void ResetVisuals()
        {
            _moveTouchId = -1;
            _jumpTouchId = -1;
            _jumpConsumed = false;
            _dropConsumed = false;

            MovementInput = Vector2.zero;
            JumpPressed = false;
            JumpReleased = false;
            DropPressed = false;

            if (joystickThumb != null)
                joystickThumb.anchoredPosition = Vector2.zero;

            if (joystickBase != null)
                joystickBase.gameObject.SetActive(false);
        }
    }
}