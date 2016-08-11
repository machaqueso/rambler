using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rambler.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("Rambler.Models.GoogleToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ExpirationDate");

                    b.Property<int>("UserId");

                    b.Property<string>("access_token");

                    b.Property<int>("expires_in");

                    b.Property<string>("refresh_token");

                    b.Property<string>("token_type");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("GoogleTokens");
                });

            modelBuilder.Entity("Rambler.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("GoogleTokenId");

                    b.HasKey("Id");

                    b.HasIndex("GoogleTokenId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Rambler.Models.GoogleToken", b =>
                {
                    b.HasOne("Rambler.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rambler.Models.User", b =>
                {
                    b.HasOne("Rambler.Models.GoogleToken", "GoogleToken")
                        .WithOne()
                        .HasForeignKey("Rambler.Models.User", "GoogleTokenId");
                });
        }
    }
}
