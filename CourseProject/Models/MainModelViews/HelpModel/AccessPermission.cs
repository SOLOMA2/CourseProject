using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public enum AccessPermission
    {
        [Display(Name = "Просмотр")]
        View,

        [Display(Name = "Заполнение")]
        Submit,

        [Display(Name = "Редактирование")]
        Edit
    }
}
