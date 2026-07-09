namespace FoodstoreApi.Core.Constants;

public static class CacheKeys
{
    public static class Categories
    {
        public static string All => "categories:all";
        public static string ById(Guid id) => $"categories:{id}";
    }

    public static class MenuItems
    {
        public static string All => "menu_items:all";
        public static string ById(Guid id) => $"menu_items:{id}";
    }

    public static class Addons
    {
        public static string All => "addons:all";
        public static string ById(Guid id) => $"addons:{id}";
    }

    public static class Combos
    {
        public static string All => "combos:all";
        public static string ById(Guid id) => $"combos:{id}";
    }

    public static class Discounts
    {
        public static string All => "discounts:all";
        public static string ById(Guid id) => $"discounts:{id}";
    }

    public static class Sources
    {
        public static string All => "sources:all";
        public static string AllForScope(string scope) => $"sources:all:{scope}";
        public static string ById(Guid id) => $"sources:{id}";
    }

    public static class Dashboard
    {
        public static string Overview => "dashboard:overview";
        public static string Stats(string from, string to, string groupBy) => $"dashboard:stats:{from}:{to}:{groupBy}";
        public static string Forecast(int horizon) => $"dashboard:forecast:{horizon}";
        public static string ComboRecommendations => "dashboard:combo_recommendations";
    }
}
