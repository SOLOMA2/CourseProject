using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CourseProject.Infastructure.TagHelpers.PagedListTagHelper
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("tag-name")]
    public class PagedListTagHelper : TagHelper
    {
        private const string TextAttributeName = "Text";
        private readonly IPagerTagHelperService _tagHelperService;
        public PagedListTagHelper(IPagerTagHelperService tagHelperService)
        {
            _tagHelperService = tagHelperService;
        }

        //public override void Process(TagHelperContext context, TagHelperOutput output)
        //{
        //    var pagerContext = _tagHelperService.GetPagerContext();
        //    var pages = _tagHelperService.GetPages(pagerContext);
        //}
    }
}
