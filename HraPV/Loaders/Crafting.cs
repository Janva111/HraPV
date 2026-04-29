using System.Text.Json;

namespace HraPV.Loaders;

public class Recipe
{
    public List<string> Ingredients { get; set; } = new();
    public string Result { get; set; } = "";
    public string Description { get; set; } = "";
}

public static class Crafting
{
    private static List<Recipe> _recipes = new();

    public static void LoadRecipes()
    {
        if (File.Exists("recipes.json"))
        {
            string json = File.ReadAllText("recipes.json");
            var data = JsonSerializer.Deserialize<Dictionary<string, Recipe>>(json);
            _recipes = data?.Values.ToList() ?? new();
        }
    }

    public static Recipe FindRecipe(List<string> providedItems)
    {
        return _recipes.FirstOrDefault(r =>
            r.Ingredients.Count == providedItems.Count &&
            !r.Ingredients.Except(providedItems).Any() &&
            !providedItems.Except(r.Ingredients).Any()
        );
    }
}