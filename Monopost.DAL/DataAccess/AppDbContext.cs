using Microsoft.EntityFrameworkCore;
using Monopost.DAL.Entities;

namespace Monopost.DAL.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        //
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostMedia> PostMedia { get; set; }
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

            modelBuilder.Entity<Template>()
                .HasOne(t => t.Author)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.AuthorId)
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

            modelBuilder.Entity<PostMedia>()
                .HasOne(psm => psm.Post)
                .WithMany(p => p.PostMedia)
                .HasForeignKey(psm => psm.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
