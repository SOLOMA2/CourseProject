using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews
{
    public enum AccessType
    {
        [Display(Name = "Публичный")]
        Public,       // Доступен всем авторизованным пользователям
        [Display(Name = "Приватный")]
        Private,      // Только автор и админы
        [Display(Name = "Выбранные пользователи")]
        SelectedUsers // Автор + указанные пользователи
    }
}
