using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Reflection;

[System.Serializable]
public struct FruitStruct
{
    public FruitType fruitType;
    public GameObject fruitObject;
    public Texture fruitTexture;
}

[System.Serializable]
public enum FruitType
{
    Apple,
    Mango,
    Banana,
    Watermelon,
    Strawberry,
    Grape,
    Orange,
    Lemon,
    DragonFruit,
    Pineapple
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    private int score = 0;
    private int missCount = 5;
    [SerializeField] public List<FruitStruct> fruits;
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] public TextMeshProUGUI missText;
    public FruitStruct[] coreFruits;
    private readonly float zPosition = -0.8f;
    public delegate void OnGameOver();
    public event OnGameOver GameOver;
    public delegate void MissedRecipe();
    public event MissedRecipe OnMissedRecipe;
    [SerializeField] private List<Vector3> spawnPos;
    [SerializeField] private List<FruitStruct> spawnedFruits = new List<FruitStruct>();
    public delegate void OnFruitSliced(FruitType fruit);
    public event OnFruitSliced FruitSliced;

    void Start()
    {
        GetAllStartQuads();
        SpawnFruits();
        Vector3 originalGravity = new(0, -9.81f, 0);
        Physics.gravity = PlayerPrefs.GetString("degree") == Degree.EASY.ToString() ?  0.25f * originalGravity : PlayerPrefs.GetString("degree") == Degree.MEDIUM.ToString() ? 1/2.25f * originalGravity : originalGravity;
    }

    void GetAllStartQuads()
    {
        GameObject[] quads = GameObject.FindGameObjectsWithTag("SRC");
        foreach (GameObject quad in quads)
        {
            Vector3 newPos = new(quad.transform.position.x, quad.transform.position.y, zPosition);
            spawnPos.Add(newPos);
        }
    }

    void Update()
    {
        scoreText.text = "Score: " + score;
        missText.text = "Missed: " + missCount;
    }

    public void SpawnFruits()
    {
        ShufflePositions(spawnPos);

        for (int index = 0; index < spawnPos.Count; index++)
        {
            if (index < coreFruits.Length)
            {
                GameObject fruit = Instantiate(coreFruits[index].fruitObject, spawnPos[index], coreFruits[index].fruitObject.transform.rotation);
                
                spawnedFruits.Add(new FruitStruct
                {
                    fruitType = coreFruits[index].fruitType,
                    fruitObject = fruit
                });
                fruit.GetComponent<Fruit>().OnFruitFallen += FruitFall;
            }
            else
            {
                int randomIndex = Random.Range(0, fruits.Count);
                GameObject fruit = Instantiate(fruits[randomIndex].fruitObject, spawnPos[index], fruits[randomIndex].fruitObject.transform.rotation);
                
                spawnedFruits.Add(new FruitStruct
                {
                    fruitType = fruits[randomIndex].fruitType,
                    fruitObject = fruit
                });

                fruit.GetComponent<Fruit>().OnFruitFallen += FruitFall;
            }
        }
        Invoke(nameof(SpawnFruits), (PlayerPrefs.GetString("degree") == Degree.EASY.ToString()) ? 3f : PlayerPrefs.GetString("degree") == Degree.MEDIUM.ToString() ? 2.25f : 1.5f);
    }

    private void ShufflePositions(List<Vector3> spawnPositions)
    {
        for (int i = spawnPositions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (spawnPositions[j], spawnPositions[i]) = (spawnPositions[i], spawnPositions[j]);
        }
    }

    public void RemoveFromSpawnedFruits(GameObject fruit)
    {
        foreach (FruitStruct f in spawnedFruits)
        {
            if (f.fruitObject == fruit)
            {
                spawnedFruits.Remove(f);
                break;
            }
        }
    }

    void FruitFall(GameObject fruit)
    {
        RemoveFromSpawnedFruits(fruit);
        Destroy(fruit);
    }
    
    public void RemoveFruitAfterSliced(float xPosition)
    {
        for (int i = spawnedFruits.Count - 1; i >= 0; i--)
        {
            FruitStruct fruit = spawnedFruits[i];
            if (Mathf.Approximately(fruit.fruitObject.transform.position.x, xPosition) && fruit.fruitObject.transform.position.y > -3f)
            {
                FruitType slicedFruitType = fruit.fruitType;
                spawnedFruits.RemoveAt(i);
                Destroy(fruit.fruitObject);
                FruitSliced?.Invoke(slicedFruitType);
                break;
            }
        }
    }

    public void AddScore(int v)
    {
        score += v;
    }
    public void ReduceMissCount(int v)
    {
        if (missCount <= 0)
        {
            Time.timeScale = 0f;
            int prevHighScore = SceneAndUserMgt.GetLastHighScore();
            if (score > prevHighScore)
            {
                SceneAndUserMgt.SaveUserHighScore(score);
            }

            prevHighScore = SceneAndUserMgt.GetLastHighScore();

            Color panelColor = PlayerPrefs.GetString("degree") == Degree.EASY.ToString() ? Color.blue : PlayerPrefs.GetString("degree") == Degree.MEDIUM.ToString() ? Color.yellow : Color.red;
            panelColor.a = 0.8f;
            gameOverPanel.GetComponent<Image>().color = panelColor;
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.ToString();
            gameOverPanel.transform.Find("HighScore").GetComponent<TextMeshProUGUI>().text = prevHighScore.ToString();
            GameOver?.Invoke();
            AudioManager.Instance.PlayGameOver();
        }
        missCount -= v;
        OnMissedRecipe?.Invoke();
    }

    

    public void ResetGame()
    {
        score = 0;
        missCount = 5;
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
    }
}