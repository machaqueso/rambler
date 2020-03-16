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
    [Migration("20200316031557_DiscordSeedData")]
    partial class DiscordSeedData
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2");

            modelBuilder.Entity("Rambler.Models.AccessToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApiSource")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("access_token")
                        .HasColumnType("TEXT");

                    b.Property<int>("expires_in")
                        .HasColumnType("INTEGER");

                    b.Property<string>("refresh_token")
                        .HasColumnType("TEXT");

                    b.Property<string>("token_type")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AccessTokens");
                });

            modelBuilder.Entity("Rambler.Models.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Points")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceAuthorId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("Rambler.Models.AuthorFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilterType")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("AuthorFilters");
                });

            modelBuilder.Entity("Rambler.Models.AuthorScoreHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("AuthorScoreHistories");
                });

            modelBuilder.Entity("Rambler.Models.BotAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .HasColumnType("TEXT");

                    b.Property<string>("Command")
                        .HasColumnType("TEXT");

                    b.Property<string>("Parameters")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("BotActions");
                });

            modelBuilder.Entity("Rambler.Models.Channel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MaximumReputation")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MinimumReputation")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Channels");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "All"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Reader"
                        },
                        new
                        {
                            Id = 3,
                            Name = "OBS"
                        },
                        new
                        {
                            Id = 4,
                            Name = "TTS"
                        });
                });

            modelBuilder.Entity("Rambler.Models.ChatMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceMessageId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Rambler.Models.ConfigurationSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ConfigurationSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Key = "Authentication:Google:ClientId",
                            Name = "Youtube client id"
                        },
                        new
                        {
                            Id = 2,
                            Key = "Authentication:Google:ClientSecret",
                            Name = "Youtube client secret"
                        },
                        new
                        {
                            Id = 3,
                            Key = "Authentication:Twitch:ClientId",
                            Name = "Twitch client id"
                        },
                        new
                        {
                            Id = 4,
                            Key = "Authentication:Twitch:ClientSecret",
                            Name = "Twitch client secret"
                        });
                });

            modelBuilder.Entity("Rambler.Models.Integration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Integrations");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DisplayOrder = 30,
                            IsEnabled = false,
                            IsVisible = false,
                            Name = "Youtube"
                        },
                        new
                        {
                            Id = 2,
                            DisplayOrder = 10,
                            IsEnabled = false,
                            IsVisible = true,
                            Name = "Twitch"
                        },
                        new
                        {
                            Id = 3,
                            DisplayOrder = 20,
                            IsEnabled = false,
                            IsVisible = true,
                            Name = "Discord"
                        });
                });

            modelBuilder.Entity("Rambler.Models.MessageInfraction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("InfractionType")
                        .HasColumnType("TEXT");

                    b.Property<int>("MessageId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.ToTable("MessageInfractions");
                });

            modelBuilder.Entity("Rambler.Models.Twitch.TwitchNotifications", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("email")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("push")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("TwitchNotifications");
                });

            modelBuilder.Entity("Rambler.Models.Twitch.TwitchUser", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("_id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("bio")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("created_at")
                        .HasColumnType("TEXT");

                    b.Property<string>("display_name")
                        .HasColumnType("TEXT");

                    b.Property<string>("email")
                        .HasColumnType("TEXT");

                    b.Property<bool>("email_verified")
                        .HasColumnType("INTEGER");

                    b.Property<string>("logo")
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .HasColumnType("TEXT");

                    b.Property<int?>("notificationsid")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("partnered")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("twitter_connected")
                        .HasColumnType("INTEGER");

                    b.Property<string>("type")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("updated_at")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.HasIndex("notificationsid");

                    b.ToTable("TwitchUsers");
                });

            modelBuilder.Entity("Rambler.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<int>("FailedLogins")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("MustChangePassword")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            FailedLogins = 0,
                            IsLocked = false,
                            MustChangePassword = true,
                            UserName = "admin"
                        });
                });

            modelBuilder.Entity("Rambler.Models.WordFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Word")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WordFilters");
                });

            modelBuilder.Entity("Rambler.Models.AccessToken", b =>
                {
                    b.HasOne("Rambler.Models.User", "User")
                        .WithMany("AccessTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rambler.Models.AuthorFilter", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("AuthorFilters")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rambler.Models.AuthorScoreHistory", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("AuthorScoreHistories")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rambler.Models.ChatMessage", b =>
                {
                    b.HasOne("Rambler.Models.Author", "Author")
                        .WithMany("ChatMessages")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rambler.Models.MessageInfraction", b =>
                {
                    b.HasOne("Rambler.Models.ChatMessage", "Message")
                        .WithMany("Infractions")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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
