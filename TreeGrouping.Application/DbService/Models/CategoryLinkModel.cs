namespace TreeGrouping.Application.DbService.Models;

public class CategoryLinkModel
{
    public int Id { get; set; }
    public int CtCategoryId { get; set; }
    public int LinkCategoryId { get; set; }
    public string LinkTypeName { get; set; } = string.Empty;
    
}
