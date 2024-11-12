﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Monopost.DAL.DataAccess;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Monopost.DAL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241112183651_RenamePostsMediaToPostsSocialMedia")]
    partial class RenamePostsMediaToPostsSocialMedia
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Monopost.DAL.Entities.Credential", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<int>("CredentialType")
                        .HasColumnType("integer");

                    b.Property<string>("CredentialValue")
                        .HasColumnType("text");

                    b.Property<string>("LocalPath")
                        .HasColumnType("text");

                    b.Property<bool>("StoredLocally")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Jar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("LongJarId")
                        .HasColumnType("text");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<string>("ShortJarId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Jars");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PostId"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DatePosted")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("PostId");

                    b.HasIndex("AuthorId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.PostMedia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MessageId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PostId")
                        .HasColumnType("integer");

                    b.Property<int>("SocialMediaName")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("PostMedia");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Restriction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateEnded")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateStarted")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RestrictionType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Restrictions");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.TemplateFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("FileData")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TemplateId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.ToTable("TemplateFiles");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Credential", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.User", "Author")
                        .WithMany("Credentials")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Jar", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.User", "Owner")
                        .WithMany("Jars")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Post", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.User", "Author")
                        .WithMany("Posts")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.PostMedia", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.Post", null)
                        .WithMany("PostMedia")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Restriction", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.User", "User")
                        .WithMany("Restrictions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Template", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.User", null)
                        .WithMany("Templates")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.TemplateFile", b =>
                {
                    b.HasOne("Monopost.DAL.Entities.Template", "Template")
                        .WithMany("TemplateFiles")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Post", b =>
                {
                    b.Navigation("PostMedia");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.Template", b =>
                {
                    b.Navigation("TemplateFiles");
                });

            modelBuilder.Entity("Monopost.DAL.Entities.User", b =>
                {
                    b.Navigation("Credentials");

                    b.Navigation("Jars");

                    b.Navigation("Posts");

                    b.Navigation("Restrictions");

                    b.Navigation("Templates");
                });
#pragma warning restore 612, 618
        }
    }
}
