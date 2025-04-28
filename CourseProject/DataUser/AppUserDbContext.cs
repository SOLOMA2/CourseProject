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

            // Глобальное отключение каскадного удаления
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Конфигурация Template
            modelBuilder.Entity<Template>(entity =>
            {
                // Индексы
                entity.HasIndex(t => t.AuthorId)
                    .IncludeProperties(t => new { t.Title, t.CreatedAt })
                    .HasDatabaseName("IX_Templates_AuthorId")
                    .HasFillFactor(90);

                entity.HasIndex(t => t.TopicId)
                    .HasDatabaseName("IX_Templates_TopicId");

                entity.HasIndex(t => t.CreatedAt)
                    .HasDatabaseName("IX_Templates_CreatedAt")
                    .IsDescending();

                // Связи с явным каскадным удалением
                entity.HasMany(t => t.Questions)
                    .WithOne(q => q.Template)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Tags)
                    .WithOne(tt => tt.Template)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Likes)
           .WithOne(l => l.Template)
           .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Views)
                    .WithOne(v => v.Template)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Comments)
                    .WithOne(c => c.Template)
                    .OnDelete(DeleteBehavior.Cascade);

                // Денормализованные счетчики
                entity.Property(t => t.LikesCount)
                    .HasDefaultValue(0)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(t => t.ViewsCount)
                    .HasDefaultValue(0)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(t => t.CommentsCount)
                    .HasDefaultValue(0)
                    .ValueGeneratedOnAddOrUpdate();
            });

            // Конфигурация Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasIndex(q => q.TemplateId)
                    .HasDatabaseName("IX_Questions_TemplateId");

                entity.HasIndex(q => q.Order)
                    .HasDatabaseName("IX_Questions_Order");

                entity.HasMany(q => q.Options)
                    .WithOne(o => o.Question)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Answer
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasIndex(a => a.UserResponseId)
                    .HasDatabaseName("IX_Answers_UserResponseId");

                entity.HasIndex(a => a.QuestionId)
                    .HasDatabaseName("IX_Answers_QuestionId");

                entity.HasMany(a => a.SelectedOptions)
                    .WithOne(so => so.Answer)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация FormResponse
            modelBuilder.Entity<FormResponse>(entity =>
            {
                entity.HasIndex(fr => fr.TemplateId)
                    .HasDatabaseName("IX_FormResponses_TemplateId")
                    .IncludeProperties(fr => new { fr.UserId, fr.CreatedAt });

                entity.HasIndex(fr => fr.CreatedAt)
                    .HasDatabaseName("IX_FormResponses_CreatedAt")
                    .IsDescending();
            });

            // Конфигурация Like
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasIndex(l => new { l.TemplateId, l.UserId })
                    .HasDatabaseName("IX_Likes_TemplateUser")
                    .IsUnique();

                entity.HasIndex(l => l.UserId)
                    .HasDatabaseName("IX_Likes_UserId");
            });

            // Конфигурация View
            modelBuilder.Entity<View>(entity =>
            {
                entity.HasIndex(v => new { v.IPAddress, v.TemplateId })
                    .HasDatabaseName("IX_Views_IPTemplate");

                entity.HasIndex(v => v.TemplateId)
                    .HasDatabaseName("IX_Views_TemplateId");
            });

            // Конфигурация Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(c => c.TemplateId)
                    .HasDatabaseName("IX_Comments_TemplateId");

                entity.HasIndex(c => c.AuthorId)
                    .HasDatabaseName("IX_Comments_AuthorId");

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Конфигурация TemplateTag
            modelBuilder.Entity<TemplateTag>(entity =>
            {
                entity.HasKey(tt => new { tt.TemplateId, tt.TagId });

                entity.HasIndex(tt => tt.TagId)
                    .HasDatabaseName("IX_TemplateTags_TagId");
            });

            // Конфигурация SelectedOption
            modelBuilder.Entity<SelectedOption>(entity =>
            {
                entity.HasIndex(so => so.QuestionOptionId)
                    .HasDatabaseName("IX_SelectedOptions_QuestionOptionId");

                entity.HasOne(so => so.QuestionOption)
                    .WithMany(qo => qo.SelectedOptions)
                    .OnDelete(DeleteBehavior.Restrict); // Исправление для циклической зависимости
            });

            // Оптимизация для Identity
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(u => u.NormalizedUserName)
                    .HasDatabaseName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("[NormalizedUserName] IS NOT NULL");

                entity.HasIndex(u => u.NormalizedEmail)
                    .HasDatabaseName("EmailIndex");
            });

            // Дополнительные оптимизации
            modelBuilder.Entity<QuestionOption>(entity =>
            {
                entity.HasIndex(qo => qo.QuestionId)
                    .HasDatabaseName("IX_QuestionOptions_QuestionId");
            });
        }
    }
}