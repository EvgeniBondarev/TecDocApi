using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OServcies.FiltersServcies.FilterModels;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;


public static class HtmlHelpers
{
    public static IHtmlContent RenderTableHeader(IHtmlHelper htmlHelper, string columnName, string columnHeader, string sortState, string sortOrder, string cssClass = "text-dark")
    {
        if (htmlHelper.ViewData.Model is OrderPageViewModel<Order, OrderFilterModel> model && model.User.UserAccess.AvailableOrderColumns.Contains(columnName))
        {
            var th = new TagBuilder("th");
            th.Attributes["id"] = $"th{columnName}";

            var a = new TagBuilder("a");
            a.AddCssClass(cssClass);
            a.Attributes["asp-action"] = "Index";
            a.Attributes["asp-route-sortOrder"] = sortOrder;

            var small = new TagBuilder("small");
            small.InnerHtml.AppendHtml(sortState == $"{sortOrder}Asc" ? $"{columnHeader}↑" : $"{columnHeader}↓");

            a.InnerHtml.AppendHtml(small);
            th.InnerHtml.AppendHtml(a);

            return th;
        }

        return HtmlString.Empty;
    }
}
