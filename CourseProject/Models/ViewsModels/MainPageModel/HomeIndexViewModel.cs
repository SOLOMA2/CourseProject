using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models.MainModelViews;

namespace CourseProject.Models.ViewsModels.MainPageModel
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Template> PopularTemplates { get; set; } = Enumerable.Empty<Template>();
        public IEnumerable<Template> RecentTemplates { get; set; } = Enumerable.Empty<Template>();
        public IEnumerable<TagInfo> PopularTags { get; set; } = Enumerable.Empty<TagInfo>();
        public string SearchQuery { get; set; }
    }
}
