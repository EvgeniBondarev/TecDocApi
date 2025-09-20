

namespace TreeGrouping.Application.DbService.Models;

public class CategoryModel
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
    public string CatChar  { get; set; }
    public string Translation  { get; set; }
    public List<CategoryModel> Children { get; set; } = new();
    public string? ParentName { get; set; }
    public bool IsFiltred { get; set; } = false;
    public int LinkId { get; set; } 
    public List<Link> Links { get; set; } = new List<Link>();
    public string DisplayId => CatChar != null ? CatChar  : Id.ToString();
}