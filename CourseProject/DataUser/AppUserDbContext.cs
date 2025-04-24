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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comment>(entity => {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Template)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<View>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasIndex(v => v.TemplateId);
                entity.HasIndex(v => v.IPAddress);

                entity.HasOne(v => v.Template)
                    .WithMany(t => t.Views)
                    .HasForeignKey(v => v.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasOne(l => l.Template)
                    .WithMany(t => t.Likes)
                    .HasForeignKey(l => l.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade); // Оставляем для шаблонов

                entity.HasOne(l => l.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Меняем на Restrict для пользователей
            });
            // Настройка Template
            modelBuilder.Entity<Template>()
                .HasOne(t => t.Author)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Template>()
                .HasOne(t => t.Topic)
                .WithMany(t => t.Templates)
                .HasForeignKey(t => t.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Template → FormResponse
            modelBuilder.Entity<Template>()
                .HasMany(t => t.Responses)
                .WithOne(fr => fr.Template)
                .HasForeignKey(fr => fr.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка Question
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Template)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка TemplateTag (многие-ко-многим)
            modelBuilder.Entity<TemplateTag>()
                .HasKey(tt => new { tt.TemplateId, tt.TagId });

            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Template)
                .WithMany(t => t.Tags)
                .HasForeignKey(tt => tt.TemplateId);

            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TagId);

            // Настройка Answer
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Answer>()
                .HasMany(a => a.SelectedOptions)
                .WithOne(so => so.Answer)
                .HasForeignKey(so => so.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка SelectedOption
            modelBuilder.Entity<SelectedOption>()
                .HasOne(so => so.QuestionOption)
                .WithMany(qo => qo.SelectedOptions)
                .HasForeignKey(so => so.QuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}