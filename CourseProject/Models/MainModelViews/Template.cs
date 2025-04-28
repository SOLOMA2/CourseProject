using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models.MainModelViews;
using CourseProject.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Template
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public TemplateType Type { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(255)]
    public string? ImagePath { get; set; }

    [NotMapped]
    public IFormFile ImageFile { get; set; }

    [ValidateNever]
    public string AuthorId { get; set; }

    [ValidateNever]
    public AppUser Author { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    public int TopicId { get; set; }

    [ValidateNever] 
    public Topic Topic { get; set; }

    public List<Question> Questions { get; set; } = new List<Question>();
    public List<TemplateTag> Tags { get; set; } = new List<TemplateTag>();
    public List<FormResponse> Responses { get; set; } = new List<FormResponse>();

    public List<Like> Likes { get; set; } = new List<Like>();

    public int LikesCount { get; set; }

    [NotMapped]
    public bool IsLikedByCurrentUser { get; set; }

    public List<View> Views { get; set; } = new List<View>();

    public int ViewsCount { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public int CommentsCount { get; set; }
}