using ConsoleApp1;
using TreeGrouping.Application.DbService;
using TreeGrouping.Application.DbService.Models;

namespace TreeGrouping.Application.CategoryService;

public class CategoryTreeService
{
    public List<CategoryModel> BuildTree(List<CategoryModel> categories)
    {
        var lookup = categories.ToLookup(c => c.ParentId);

        List<CategoryModel> BuildBranch(int? parentId) =>
            lookup[parentId]
                .Select(c => new CategoryModel
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    Name = c.Name,
                    ParentName = c.ParentName,
                    Children = c.Children.Any() ? c.Children : BuildBranch(c.Id),
                    IsFiltred = c.IsFiltred,
                    LinkId = c.LinkId,
                    Links = c.Links,
                    CatChar  = c.CatChar,
                    Translation = c.Translation,
                }).ToList();

        return BuildBranch(null).Concat(BuildBranch(0)).ToList();
    }


    public async Task<List<CategoryModel>> CategoryLinkToModel(IDatabaseService dbService, List<CategoryModel> tree, List<CategoryLinkModel> links)
    {
        foreach (var link in links)
        {
            if (!Enum.TryParse<CategoryLinkType>(link.LinkTypeName, true, out var linkType))
                continue;

            var linkTreeTask = await dbService.ExecuteStoredProcedureAsync(linkType.GetProcedureType(), link.LinkCategoryId);
            var linkCategory = linkTreeTask.FirstOrDefault(lc => lc.Id == link.LinkCategoryId);

            var treeItem = tree.FirstOrDefault(tr => tr.Id == link.CtCategoryId);
            if (treeItem != null && linkCategory != null)
            {
                bool linkExists = treeItem.Links.Any(l => l.Id == link.Id);
                if (!linkExists)
                {
                    treeItem.Links.Add(new Link
                    {
                        CategoryLinkType = linkType,
                        CategoryId = linkCategory.Id,
                        Id = link.Id
                    });
                }

                treeItem.ParentName ??= link.LinkTypeName; // только если ещё не установлено
            }
        }

        return tree;
    }
}