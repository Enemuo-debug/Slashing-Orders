using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Recipe
{
    public string name;
    public FruitType[] ingredients;
    public Texture recipeImage;
}

public class JuiceKitchen : MonoBehaviour
{
    [SerializeField] private List<Recipe> Recipes;
    [SerializeField] private GameManager gameManager;
    public delegate void RecipeChangeDelegate(Recipe RecipeName, int time);
    public static RecipeChangeDelegate OnRecipeChange;
    public Recipe currentRecipe = null;
    private Coroutine nextRecipeCoroutine;

    IEnumerator NextRecipe (int time)
    {
        yield return new WaitForSeconds(time);
        nextRecipeCoroutine = null;
        ChooseRecipe();
    }

    private void ChooseRecipe()
    {
        if (currentRecipe != null && currentRecipe.ingredients != null)
        {
            if (currentRecipe.ingredients.Length > 0)
            {
                gameManager.ReduceMissCount(1);
            }
            else
            {
                gameManager.AddScore(10);
            }
        }

        var template = Recipes[Random.Range(0, Recipes.Count)];

        // Create a copy of the chosen recipe so we don't mutate the template in Recipes
        currentRecipe = new Recipe
        {
            name = template.name,
            ingredients = template.ingredients != null ? template.ingredients.ToArray() : new FruitType[0],
            recipeImage = template.recipeImage,
        };

        gameManager.coreFruits = currentRecipe.ingredients.Select(u => 
            gameManager.fruits.Find(f => f.fruitType == u)).ToArray();

        int PrepTime = PlayerPrefs.GetString("degree", Degree.EASY.ToString()) == Degree.EASY.ToString() ? 3 * currentRecipe.ingredients.Length : 2 * currentRecipe.ingredients.Length;

        OnRecipeChange?.Invoke(currentRecipe, PrepTime);

        if (nextRecipeCoroutine != null)
        {
            StopCoroutine(nextRecipeCoroutine);
            nextRecipeCoroutine = null;
        }
        nextRecipeCoroutine = StartCoroutine(NextRecipe(PrepTime));
    }

    public void UpdateCoreFruits()
    {
        gameManager.coreFruits = currentRecipe.ingredients.Select(u => 
            gameManager.fruits.Find(f => f.fruitType == u)).ToArray();
    }

    public FruitType GetTopIngridient()
    {
        return currentRecipe.ingredients[0];
    }

    void Start()
    {
        ChooseRecipe();
        gameManager.FruitSliced += RemoveIngredient;
    }

    private void RemoveIngredient(FruitType fruit)
    {
        try 
        {
            if (currentRecipe == null || currentRecipe.ingredients == null) return;

            var ingredientList = currentRecipe.ingredients.ToList();
            // Remove a single instance of the ingredient (safer for recipes with duplicates)
            bool removed = ingredientList.Remove(fruit);
            if (removed)
            {
                currentRecipe.ingredients = ingredientList.ToArray();
                UpdateCoreFruits();
                if (currentRecipe.ingredients.Length == 0)
                {
                    ChooseRecipe();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error removing ingredient: {e.Message}");
        }
    }
}
