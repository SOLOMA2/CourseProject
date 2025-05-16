using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.ViewsModels
{
    public class SupportTicket
    {
        public string ReportedBy { get; set; }
        public int TemplateId { get; set; }
        public string TemplateTitle { get; set; }

        public string Link { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; }

        [Required(ErrorMessage = "Summary is required")]
        public string Summary { get; set; }
        //public List<string> Admins { get; set; } 

    }
}
