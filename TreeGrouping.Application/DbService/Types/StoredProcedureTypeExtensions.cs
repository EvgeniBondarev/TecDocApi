namespace ConsoleApp1;

public static class StoredProcedureTypeExtensions
{
    public static string GetProcedureName(this StoredProcedureType type)
    {
        return type switch
        {
            StoredProcedureType.GetVolnaCategories => "GetVolnaCategories",
            StoredProcedureType.GetOzonCategories => "GetOzonCategories",
            StoredProcedureType.GetCtCategories => "GetCtCategories",
            StoredProcedureType.GetICGroups => "GetICGroups",
            StoredProcedureType.SearchVolnaCategoryByName => "SearchVolnaCategoryByName",
            StoredProcedureType.SearchOzonCategoryByName => "SearchOzonCategoryByName",
            StoredProcedureType.SearchCtCategoryByName => "SearchCtCategoryByName",
            StoredProcedureType.SearchVolnaCategoryById => "SearchVolnaCategoryById",
            StoredProcedureType.SearchOzonCategoryById => "SearchOzonCategoryById",
            StoredProcedureType.SearchCtCategoryById => "SearchCtCategoryById",
            StoredProcedureType.AddCategoryLink => "AddCategoryLink",
            StoredProcedureType.DeleteCategoryLink => "DeleteCategoryLinkById",
            StoredProcedureType.GetCatTreeCategories => "GetCatTreeCategories",  
            StoredProcedureType.SearchCatTreeCategoryById => "SearchCatTreeCategoryById",
            StoredProcedureType.SearchICGroupById => "SearchICGroupById",
            StoredProcedureType.InsertUnifiedCategoryIfNotExists => "sp_InsertUnifiedCategoryIfNotExists", 
            StoredProcedureType.DeleteUnifiedCategoryByIcId => "DeleteUnifiedCategoryByIcId",
            StoredProcedureType.AddCategoryTranslations => "AddCategoryTranslations", 
            _ => throw new ArgumentException("Unknown procedure type")
        };
    }
    public static Dictionary<string, object> GetParameter(this StoredProcedureType type, object parameter)
    {
        var paramDict = new Dictionary<string, object>();

        if (parameter is not null)
        {
            switch (type)
            {
                case StoredProcedureType.SearchVolnaCategoryByName:
                case StoredProcedureType.SearchOzonCategoryByName:
                case StoredProcedureType.SearchCtCategoryByName:
                    if (parameter is string searchName)
                    {
                        paramDict["searchName"] = searchName;
                    }
                    break;

                case StoredProcedureType.SearchCtCategoryById:
                case StoredProcedureType.SearchVolnaCategoryById:
                case StoredProcedureType.SearchOzonCategoryById:
                    if (parameter is int searchId)
                    {
                        paramDict["categoryId"] = searchId;
                    }
                    break;
                
                case StoredProcedureType.AddCategoryLink:
                    if (parameter is (int ctCategoryId, int linkCategoryId, string linkTypeName))
                    {
                        paramDict["ct_category_id"] = ctCategoryId;
                        paramDict["link_category_id"] = linkCategoryId;
                        paramDict["link_type_name"] = linkTypeName;
                    }
                    break;
                case StoredProcedureType.DeleteCategoryLink: 
                    if (parameter is int categoryLinkId)
                    {
                        paramDict["CategoryLinkId"] = categoryLinkId;
                    }
                    break;
                case StoredProcedureType.SearchCatTreeCategoryById:
                    if (parameter is int catTreeId)
                    {
                        paramDict["categoryId"] = catTreeId;
                    }
                    break;
                case StoredProcedureType.SearchICGroupById:
                    if (parameter is int groupId)
                    {
                        paramDict["groupId"] = groupId; 
                    }
                    break;
                case StoredProcedureType.InsertUnifiedCategoryIfNotExists:
                    if (parameter is ValueTuple<int?, int?, int?> tuple)
                    {
                        paramDict["ozon_id"] = tuple.Item1;
                        paramDict["volna_id"] = tuple.Item2;
                        paramDict["ic_id"] = tuple.Item3;
                    }
                    break;
                case StoredProcedureType.DeleteUnifiedCategoryByIcId: 
                    if (parameter is int icId)
                    {
                        paramDict["IcId"] = icId;
                    }
                    break;
                case StoredProcedureType.AddCategoryTranslations:
                    if (parameter is (int categoryId, string linkType, string translation))
                    {
                        paramDict["category_id"] = categoryId;
                        paramDict["link_type_name"] = linkType;
                        paramDict["translation"] = translation;
                    }
                    break;

            }
        }

        return paramDict;
    }
}