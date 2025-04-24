using CourseProject.Infastructure.TagHelpers.PagedListTagHelper.Base;

namespace CourseProject.Infastructure.TagHelpers.PagedListTagHelper
{
    public class PagerPageDisabled: PagerPageBase
    {
        public PagerPageDisabled(string title, int value) : base(title, value, false)
        {
        }
    }
}
