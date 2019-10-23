﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rambler.Data;

namespace Rambler.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20191101015716_AddedBotEntities")]
    partial class AddedBotEntities
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099");

            modelBuilder.Entity("Rambler.Models.AccessToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApiSource");

                    b.Property<DateTime>("ExpirationDate");

                    b.Property<int>("UserId");

                    b.Property<string>("access_token");

                    b.Property<int>("expires_in");

                    b.Property<string>("refresh_token");

                    b.Property<string>("token_type");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AccessTokens");
                });

            modelBuilder.Entity("Rambler.Models.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("Points");

                    b.Property<int>("Score");

                    b.Property<string>("Source");

                    b.Property<string>("SourceAuthorId");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("Rambler.Models.AuthorFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthorId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("FilterType");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("AuthorFilters");
                });

            modelBuilder.Entity("Rambler.Models.AuthorScoreHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthorId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Reason");

                    b.Property<int>("Value");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("AuthorScoreHistories");
                });

            modelBuilder.Entity("Rambler.Models.BotAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<string>("Command");

                    b.Property<string>("Parameters");

                    b.HasKey("Id");

                    b.ToTable("BotActions");
                });

            modelBuilder.Entity("Rambler.Models.Channel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<int?>("MaximumReputation");

                    b.Property<int?>("MinimumReputation");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Channels");

                    b.HasData(
                        new { Id = 1, Name = "All" },
                        new { Id = 2, Name = "Reader" },
                        new { Id = 3, Name = "OBS" },
                        new { Id = 4, Name = "TTS" }
                    );
                });

            modelBuilder.Entity("Rambler.Models.ChatMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthorId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Message");

                    b.Property<string>("Source");

                    b.Property<string>("SourceMessageId");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Rambler.Models.ConfigurationSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Key");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("ConfigurationSettings");

                    b.HasData(
                        new { Id = 1, Key = "Authentication:Google:ClientId", Name = "Youtube client id" },
                        new { Id = 2, Key = "Authentication:Google:ClientSecret", Name = "Youtube client secret" },
                        new { Id = 3, Key = "Authentication:Twitch:ClientId", Name = "Twitch client id" },
                        new { Id = 4, Key = "Authentication:Twitch:ClientSecret", Name = "Twitch client secret" }
                    );
                });

            modelBuilder.Entity("Rambler.Models.Integration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsEnabled");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Integrations");

                    b.HasData(
                        new { Id = 1, IsEnabled = false, Name = "Youtube" },
                        new { Id = 2, IsEnabled = false, Name = "Twitch" }
                    );
                });

            modelBuilder.Entity("Rambler.Models.Twitch.TwitchNotifications", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("email");

                    b.Property<bool>("push");

                    b.HasKey("id");

                    b.ToTable("TwitchNotifications");
                });

            modelBuilder.Entity("Rambler.Models.Twitch.TwitchUser", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("_id");

                    b.Property<string>("bio");

                    b.Property<DateTime>("created_at");

                    b.Property<string>("display_name");

                    b.Property<string>("email");

                    b.Property<bool>("email_verified");

                    b.Property<string>("logo");

                    b.Property<string>("name");

                    b.Property<int?>("notificationsid");

                    b.Property<bool>("partnered");

                    b.Property<bool>("twitter_connected");

                    b.Property<string>("type");

                    b.Property<DateTime?>("updated_at");

                    b.HasKey("id");

                    b.HasIndex("notificationsid");

                    b.ToTable("TwitchUsers");
                });

            modelBuilder.Entity("Rambler.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<int>("FailedLogins");

                    b.Property<bool>("IsLocked");

                    b.Property<DateTime?>("LastLoginDate");

                    b.Property<bool>("MustChangePassword");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new { Id = 1, FailedLogins = 0, IsLocked = false, MustChangePassword = true, UserName = "Admin" }
                    );
                });

            modelBuilder.Entity("Rambler.Models.WordFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Word");

                    b.HasKey("Id");

                    b.ToTable("WordFilters");
                });

            modelBuilder.Entity("Rambler.Models.AccessToken", b =>
                {
                    b.HasOne("Rambler.Models.User", "User")
                        .WithMany("AccessTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rambler.Models.AuthorFilter", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("AuthorFilters")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rambler.Models.AuthorScoreHistory", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("AuthorScoreHistories")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rambler.Models.ChatMessage", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("ChatMessages")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rambler.Models.Twitch.TwitchUser", b =>
                {
                    b.HasOne("Rambler.Models.Twitch.TwitchNotifications", "notifications")
                        .WithMany()
                        .HasForeignKey("notificationsid");
                });
#pragma warning restore 612, 618
        }
    }
}
