using CourseProject.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class View
{
    public int Id { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; }

    public int TemplateId { get; set; }
    [ForeignKey("TemplateId")]
    public Template Template { get; set; }
}