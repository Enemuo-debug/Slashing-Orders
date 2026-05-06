using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private float[] xPos = new float[4];
    private Animator animatorController;
    [SerializeField] private GameManager gameManager;
    
    void Start()
    {
        animatorController = GetComponent<Animator>();
        GameObject[] pathObjects = GameObject.FindGameObjectsWithTag("Path");
        
        for (int i = 0; i < pathObjects.Length && i < xPos.Length; i++)
        {
            xPos[i] = pathObjects[i].transform.position.x;
        }
        
        Array.Sort(xPos);
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInputReceived += MovtoLane;
        }
        else
        {
            Debug.LogError("InputManager.Instance is null! Make sure InputManager is in the scene.");
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInputReceived -= MovtoLane;
        }
    }

    public void MovtoLane(int direction)
    {
        if (direction == 0){
            Slice();
            return;
        }
        
        Vector3 newPosition = transform.position;
        int currentLane = Array.IndexOf(xPos, transform.position.x);
        int targetLane = currentLane + direction;
        targetLane += xPos.Length; // Ensure non-negative before modulo
        targetLane %= xPos.Length;
        Debug.Log($"Current Lane: {currentLane}, Target Lane: {targetLane}, Direction: {direction}");
        
        newPosition.x = xPos[targetLane];
        transform.position = newPosition;
    }

    private void Slice()
    {
        if (animatorController != null)
        {
            StartCoroutine(SliceAnimation());
            gameManager.RemoveFruitAfterSliced(transform.position.x);
            AudioManager.Instance.PlaySlash();
        }
        else
        {
            Debug.LogError("Animator is null! Cannot trigger Cut animation.");
        }
    }

    IEnumerator SliceAnimation()
    {
        if (animatorController != null)
        {
            animatorController.SetBool("Cut", true);
            yield return new WaitForSeconds(0.01f);
            animatorController.SetBool("Cut", false);
        }
        else
        {
            Debug.LogError("Animator is null! Cannot trigger Cut animation.");
        }
    }
}