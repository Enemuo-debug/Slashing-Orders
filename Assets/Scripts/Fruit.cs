using UnityEngine;

public class Fruit : MonoBehaviour
{
    public delegate void FruitFallHandler(GameObject fruit);
    public event FruitFallHandler OnFruitFallen;
    void Update()
    {
        if (transform.position.y < -6f)
        {
            OnFruitFallen?.Invoke(gameObject);
        }
    }
}
