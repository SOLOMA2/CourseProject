using CourseProject.Infastructure.TagHelpers.PagedListTagHelper.Base;

namespace CourseProject.Infastructure.TagHelpers.PagedListTagHelper
{
    public interface IPagerTagHelperService
    {
        PagerContext GetPagerContext(int pageIndex, int PageSize, int totlaPages, int pagesInGroup);
        List<PagerPageBase> GetPages(PagerContext pagerContext);
    }

    public class PagerTagHelperService : IPagerTagHelperService
    {
        public PagerContext GetPagerContext(int pageIndex, int PageSize, int totlaPages, int pagesInGroup)
        {
            throw new NotImplementedException();
        }

        public List<PagerPageBase> GetPages(PagerContext pagerContext)
        {
            throw new NotImplementedException();
        }
    }
}
