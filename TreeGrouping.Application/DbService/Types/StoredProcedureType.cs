namespace ConsoleApp1;

public enum StoredProcedureType
{
    GetVolnaCategories,
    GetOzonCategories,
    GetCtCategories,
    GetICGroups,
    SearchVolnaCategoryByName,
    SearchOzonCategoryByName,
    SearchCtCategoryByName,
    SearchVolnaCategoryById,
    SearchOzonCategoryById,
    SearchCtCategoryById,
    AddCategoryLink,
    DeleteCategoryLink,
    GetCatTreeCategories,
    SearchCatTreeCategoryById,
    SearchICGroupById,
    InsertUnifiedCategoryIfNotExists,
    DeleteUnifiedCategoryByIcId,
    AddCategoryTranslations
}
