using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Monopost.DAL.Entities;

namespace Monopost.DAL.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Users = Set<User>();
            Posts = Set<Post>();
            PostsSocialMedia = Set<PostMedia>();
            Templates = Set<Template>();
            TemplateFiles = Set<TemplateFile>();
            Jars = Set<Jar>();
            Credentials = Set<Credential>();
            Restrictions = Set<Restriction>();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostMedia> PostsSocialMedia { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateFile> TemplateFiles { get; set; }
        public DbSet<Jar> Jars { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Restriction> Restrictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TemplateFile>()
                .HasOne(tf => tf.Template)
                .WithMany(t => t.TemplateFiles)
                .HasForeignKey(tf => tf.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Jar>()
                .HasOne(j => j.Owner)
                .WithMany(u => u.Jars)
                .HasForeignKey(j => j.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Credential>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Credentials)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Restriction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Restrictions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    }
                }
            }
        }
    }
}