using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Website.Models;

namespace Website.TagHelpers
{
	public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageViewModel PageModel { get; set; }
        public string PageAction { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
			output.TagName = "div";

			// набор ссылок будет представлять список ul
			TagBuilder tag = new TagBuilder("ul");
			tag.AddCssClass("pagination");
			tag.Attributes.Add("style", "margin: auto;");
			// создаем ссылку на предыдущую страницу
			TagBuilder prevItem = CreateTag("«", PageModel.PageNumber - 1, PageModel.HasPreviousPage, urlHelper);
			tag.InnerHtml.AppendHtml(prevItem);

			// создаем ссылку на следующую страницу
			TagBuilder nextItem = CreateTag("»", PageModel.PageNumber + 1, PageModel.HasNextPage, urlHelper);
			tag.InnerHtml.AppendHtml(nextItem);

			output.Content.AppendHtml(tag);
		}

		TagBuilder CreateTag(string text, int pageNumber, bool active, IUrlHelper urlHelper)
		{
			TagBuilder item = new TagBuilder("li");
			TagBuilder link = new TagBuilder("a");
			item.AddCssClass("page-item");
			link.AddCssClass("page-link");
			if (active)
			{
				link.Attributes["href"] = urlHelper.Action(PageAction, new { page = pageNumber });
			}
			else
			{
				item.AddCssClass("disabled");
			}
			link.InnerHtml.Append(text);
			item.InnerHtml.AppendHtml(link);
			return item;
		}
	}
}