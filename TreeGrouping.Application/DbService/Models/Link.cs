using ConsoleApp1;

namespace TreeGrouping.Application.DbService.Models;

public class Link
{
    public int Id { get; set; }
    public CategoryLinkType CategoryLinkType { get; set; }
    public int CategoryId { get; set; }
}