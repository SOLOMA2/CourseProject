using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models.MainModelViews;

namespace CourseProject.Models.ViewsModels.MainPageModel
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Template> PopularTemplates { get; set; }
        public IEnumerable<Template> RecentTemplates { get; set; }
        public IEnumerable<Template> SearchResults { get; set; }
        public IEnumerable<TagInfo> PopularTags { get; set; }
        public string SearchQuery { get; set; }
        public string Tag { get; set; }
    }
}
