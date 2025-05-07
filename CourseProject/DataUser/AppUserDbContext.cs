using CourseProject.Models;
using CourseProject.Models.MainModelViews;
using CourseProject.Models.MainModelViews.HelpModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.DataUser
{
    public class AppUserDbContext : IdentityDbContext<AppUser>
    {
        public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options)
        {
        }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<FormResponse> FormResponses { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<SelectedOption> SelectedOptions { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TemplateAccess> TemplateAccess { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Templates)
                .WithOne(t => t.Author)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Responses)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.Author)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.AllowedTemplates)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Templates)
                .WithOne(tpl => tpl.Topic)
                .HasForeignKey(tpl => tpl.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateTag>()
                .HasKey(tt => new { tt.TemplateId, tt.TagId });

            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Template)
                .WithMany(t => t.Tags)
                .HasForeignKey(tt => tt.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Questions)
                .WithOne(q => q.Template)
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Responses)
                .WithOne(r => r.Template)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Likes)
                .WithOne(l => l.Template)
                .HasForeignKey(l => l.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Views)
                .WithOne(v => v.Template)
                .HasForeignKey(v => v.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.Template)
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.AllowedUsers)
                .WithOne(a => a.Template)
                .HasForeignKey(a => a.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionOption>()
                .HasMany(o => o.SelectedOptions)
                .WithOne(so => so.QuestionOption)
                .HasForeignKey(so => so.QuestionOptionId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<FormResponse>()
                .HasMany(r => r.Answers)
                .WithOne(a => a.UserResponse)
                .HasForeignKey(a => a.UserResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasMany(a => a.SelectedOptions)
                .WithOne(so => so.Answer)
                .HasForeignKey(so => so.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}