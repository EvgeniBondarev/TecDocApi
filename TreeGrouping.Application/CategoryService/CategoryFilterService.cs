using TreeGrouping.Application.DbService.Models;

namespace TreeGrouping.Application.CategoryService;

public class CategoryFilterService
{
    public List<CategoryModel> FilterCategoriesByName(List<CategoryModel> categories, string name)
    {
        // Если фильтр не задан — возвращаем все категории, отсортированные по алфавиту
        if (string.IsNullOrEmpty(name))
        {
            foreach (var category in categories)
            {
                category.IsFiltred = false;
            }

            return categories
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Фильтруем категории по названию
        var filteredCategories = categories
            .Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Собираем всех родительских категорий для найденных элементов
        var parentIds = new HashSet<int?>(filteredCategories.Select(c => c.ParentId));
        var allRelevantCategories = new HashSet<CategoryModel>(filteredCategories);

        while (parentIds.Count > 0)
        {
            var newParents = categories
                .Where(c => parentIds.Contains(c.Id) && !allRelevantCategories.Contains(c))
                .ToList();

            if (!newParents.Any())
            {
                break;
            }

            foreach (var parent in newParents)
            {
                allRelevantCategories.Add(parent);
            }

            parentIds = new HashSet<int?>(newParents.Select(c => c.ParentId));
        }

        foreach (var category in allRelevantCategories)
        {
            category.IsFiltred = true;
        }

        // Возвращаем отсортированный результат
        return allRelevantCategories
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    
    public List<CategoryModel> OrderCategoriesByName(List<CategoryModel> categories)
    {
        // Возвращаем отсортированный результат
        return categories
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

}