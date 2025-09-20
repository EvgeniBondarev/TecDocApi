namespace ConsoleApp1;

public static class CategoryLinkTypeExtensions
{
    public static StoredProcedureType GetProcedureType(this CategoryLinkType type)
    {
        return type switch
        {
            CategoryLinkType.Ozon => StoredProcedureType.SearchOzonCategoryById,
            CategoryLinkType.Volna => StoredProcedureType.SearchVolnaCategoryById,
            CategoryLinkType.CatTree => StoredProcedureType.SearchCatTreeCategoryById,
            CategoryLinkType.IC => StoredProcedureType.SearchICGroupById,
            _ => throw new ArgumentException("Unknown category link type")
        };
    }
}
