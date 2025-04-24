using CourseProject.Infastructure.TagHelpers.PagedListTagHelper.Base;

namespace CourseProject.Infastructure.TagHelpers.PagedListTagHelper
{
    public class PagerPageActive : PagerPageBase
    {
        public PagerPageActive(string title, int value) : base(title, value, true)
        {
        }
    }
}
