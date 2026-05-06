using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Needed for callback context

public class InputManager : MonoBehaviour
{
    public delegate void InputReceivedHandler(int direction);
    public event InputReceivedHandler OnInputReceived;
    public static InputManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float minimumDistance = 0.2f;
    [SerializeField] private float maximumTime = 1f;

    private PlayerControls controls;
    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Touch.Enable();
        controls.Touch.TouchInput.started += ctx => StartTouch(ctx);
        controls.Touch.TouchInput.canceled += ctx => EndTouch(ctx);
    }

    void OnDisable()
    {
        if (controls != null)
        {
            controls.Touch.TouchInput.started -= ctx => StartTouch(ctx);
            controls.Touch.TouchInput.canceled -= ctx => EndTouch(ctx);
            controls.Touch.Disable();
        }
    }

    private void StartTouch(InputAction.CallbackContext ctx)
    {
        startPosition = controls.Touch.TouchPosition.ReadValue<Vector2>();
        startTime = (float)ctx.time;
    }

    private void EndTouch(InputAction.CallbackContext ctx)
    {
        float endTime = (float)ctx.time;
        
        endPosition = controls.Touch.TouchPosition.ReadValue<Vector2>();

        float distance = Vector2.Distance(startPosition, endPosition);
        float timeDelta = endTime - startTime;

        if (distance > minimumDistance && timeDelta < maximumTime)
        {
            int direction = SwipeDirection(endPosition - startPosition);
            OnInputReceived?.Invoke(direction);
        }
    }

    int SwipeDirection(Vector2 direction)
    {
        Vector2 dir = direction.normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                if (SceneManager.GetActiveScene().buildIndex == 0)
                {
                    SceneManager.LoadScene(1);
                    Debug.Log("Open new scene");
                    return 12;
                }
                return 1; 
            }
            else
            {
                if (SceneManager.GetActiveScene().buildIndex == 0)
                {
                    Application.Quit();
                    Debug.Log("Close Game");
                    return 12;
                }
                return -1; 
            }
        } else {
            return 0;
        }
    }
}