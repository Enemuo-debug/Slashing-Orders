using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


enum ErrorType
{
    Information, Warning
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public RawImage? recipePanelImage;
    public TextMeshProUGUI? recipeCountDown;
    public GameObject? alertPanel;
    [SerializeField] private GameObject recipeItems;
    [SerializeField] private GameObject? RecipeItemsHolder;
    [SerializeField] private GameManager? gameManager;
    private float recipeTime;
    private CanvasGroup alertPanelCanvasGroup;
    private Coroutine alertCoroutine;
    private float alertMaxAlpha = 0.75f;

    void Awake()
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
    }

    public void UpdateRecipeUI(Recipe recipe, int time)
    {
        recipeTime = time;
        // Add UI update logic here
        if (recipePanelImage != null && recipe != null && recipeCountDown != null)
        {
            recipePanelImage.texture = recipe.recipeImage;
            recipeCountDown.text = time.ToString();
            if (RecipeItemsHolder == null) RecipeItemsHolder = GameObject.FindWithTag("recipeItems");
            foreach (Transform child in RecipeItemsHolder.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (var ingredient in recipe.ingredients)
            {
                var ingredientItem = Instantiate(recipeItems, RecipeItemsHolder.transform);
                ingredientItem.name = ingredient.ToString();
                if (gameManager == null)
                {
                    gameManager = FindFirstObjectByType<GameManager>();
                }
                ingredientItem.GetComponent<RawImage>().texture = gameManager.fruits.Find(f => f.fruitType == ingredient).fruitTexture;
            }
        }
        else
        {
            try
            {
                recipePanelImage = GameObject.FindWithTag("Order").GetComponent<RawImage>();
                recipeCountDown = GameObject.FindWithTag("cDown").GetComponent<TextMeshProUGUI>();
                recipePanelImage.texture = recipe.recipeImage;
                recipeCountDown.text = time.ToString();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error updating UI: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (recipeCountDown != null)
        {
            if (recipeTime > 0)
            {
                recipeTime -= Time.deltaTime;
                recipeCountDown.text = Math.Round(recipeTime).ToString();
            }
        }
    }

    public void RemoveItemFromRecipe(FruitType fruitType)
    {
        try
        {
            // Fruit was sliced, remove it from the recipe UI
            Destroy(RecipeItemsHolder.transform.Find(fruitType.ToString()).gameObject);
        }
        catch (System.Exception e)
        {
            if (PlayerPrefs.GetString("degree") == Degree.HARD.ToString())
                gameManager.AddScore(-10);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += (scene, mode) => AfterSceneLoad();
    }

    void AfterSceneLoad()
    {
        JuiceKitchen.OnRecipeChange += UpdateRecipeUI;
    
        RecipeItemsHolder = GameObject.FindWithTag("recipeItems");
        recipePanelImage = GameObject.FindWithTag("Order")?.GetComponent<RawImage>();
        recipeCountDown = GameObject.FindWithTag("cDown")?.GetComponent<TextMeshProUGUI>();
        
        gameManager = FindFirstObjectByType<GameManager>(); 
        if (gameManager != null)
        {
            gameManager.FruitSliced += RemoveItemFromRecipe;
            gameManager.OnMissedRecipe += () => Alert("Oops! Missed an order!");
        }
        
        alertPanel = GameObject.FindWithTag("AlertPanel");
        if (alertPanel != null)
        {
            alertPanelCanvasGroup = alertPanel.GetComponent<CanvasGroup>() ?? alertPanel.AddComponent<CanvasGroup>();
            alertPanelCanvasGroup.alpha = 0f;  // Start invisible
        }
    }

    private void Alert(string v, ErrorType errorType = ErrorType.Warning)
    {
        if (alertPanel == null) return;
        
        var alertText = alertPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (alertText != null)
        {
            alertText.text = v;
            alertPanel.GetComponent<Image>().color = errorType == ErrorType.Warning ? Color.red : Color.blue;
            if (alertCoroutine != null)
            {
                StopCoroutine(alertCoroutine);
                alertCoroutine = null;
            }
            alertCoroutine = StartCoroutine(FadeAlert());
        }
    }

    private IEnumerator FadeAlert()
    {
        float fadeInDuration = 0.12f;
        float fadeOutDuration = 0.12f;
        float displayDuration = 0.9f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            alertPanelCanvasGroup.alpha = Mathf.Clamp01((elapsed / fadeInDuration) * alertMaxAlpha);
            yield return null;
        }
        alertPanelCanvasGroup.alpha = alertMaxAlpha;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            alertPanelCanvasGroup.alpha = Mathf.Clamp01(alertMaxAlpha * (1f - (elapsed / fadeOutDuration)));
            yield return null;
        }
        alertPanelCanvasGroup.alpha = 0f;
        alertCoroutine = null;
    }

    void OnDisable()
    {
        JuiceKitchen.OnRecipeChange -= UpdateRecipeUI;

        if (gameManager != null)
        {
            gameManager.FruitSliced -= RemoveItemFromRecipe;
        }
    }
}
