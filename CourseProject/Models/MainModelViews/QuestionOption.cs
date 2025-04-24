using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CourseProject.Models.MainModelViews
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        [ValidateNever]
        public Question Question { get; set; }

        [ValidateNever]
        public List<SelectedOption> SelectedOptions { get; set; } = new List<SelectedOption>();
    }
}