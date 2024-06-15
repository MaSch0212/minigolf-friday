﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinigolfFriday.Data;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240615131727_AddUserPushSubscriptions")]
    partial class AddUserPushSubscriptions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<DateTimeOffset>("RegistrationDeadline")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("registration_deadline");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("started_at");

                    b.HasKey("Id");

                    b.ToTable("events", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstanceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("GroupCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("group_code");

                    b.Property<long>("timeslot_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("timeslot_id");

                    b.ToTable("event_instances", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstancePreconfigurationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("event_timeslot_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("event_timeslot_id");

                    b.ToTable("event_instance_preconfigurations", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("EventId")
                        .HasColumnType("bigint")
                        .HasColumnName("event_id");

                    b.Property<bool>("IsFallbackAllowed")
                        .HasColumnType("bit")
                        .HasColumnName("is_fallback_allowed");

                    b.Property<long>("MapId")
                        .HasColumnType("bigint")
                        .HasColumnName("map_id");

                    b.Property<TimeOnly>("Time")
                        .HasColumnType("time")
                        .HasColumnName("time");

                    b.HasKey("Id");

                    b.HasIndex("MapId");

                    b.HasIndex("EventId", "Time")
                        .IsUnique();

                    b.ToTable("event_timeslots", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotRegistrationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("EventTimeslotId")
                        .HasColumnType("bigint")
                        .HasColumnName("event_timeslot_id");

                    b.Property<long?>("FallbackEventTimeslotId")
                        .HasColumnType("bigint")
                        .HasColumnName("fallback_event_timeslot_id");

                    b.Property<long>("PlayerId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("EventTimeslotId");

                    b.HasIndex("FallbackEventTimeslotId");

                    b.HasIndex("PlayerId");

                    b.ToTable("event_timeslot_registration", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.MinigolfMapEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("maps", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.RoleEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("roles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "Player"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Admin"
                        });
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.UserEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Alias")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("alias");

                    b.Property<string>("LoginToken")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("login_token");

                    b.HasKey("Id");

                    b.HasIndex("LoginToken")
                        .IsUnique()
                        .HasFilter("[login_token] IS NOT NULL");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.UserPushSubscriptionEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Auth")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("auth");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)")
                        .HasColumnName("endpoint");

                    b.Property<string>("Lang")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("lang");

                    b.Property<string>("P256DH")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("p256dh");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("Endpoint")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("user_push_subscriptions", (string)null);
                });

            modelBuilder.Entity("event_instances_to_users", b =>
                {
                    b.Property<long>("event_instance_id")
                        .HasColumnType("bigint");

                    b.Property<long>("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("event_instance_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("event_instances_to_users");
                });

            modelBuilder.Entity("users_to_avoided_users", b =>
                {
                    b.Property<long>("avoided_user_id")
                        .HasColumnType("bigint");

                    b.Property<long>("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("avoided_user_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_avoided_users");
                });

            modelBuilder.Entity("users_to_event_instance_preconfigurations", b =>
                {
                    b.Property<long>("event_instance_preconfiguration_id")
                        .HasColumnType("bigint");

                    b.Property<long>("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("event_instance_preconfiguration_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_event_instance_preconfigurations");
                });

            modelBuilder.Entity("users_to_preferred_users", b =>
                {
                    b.Property<long>("preferred_user_id")
                        .HasColumnType("bigint");

                    b.Property<long>("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("preferred_user_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_preferred_users");
                });

            modelBuilder.Entity("users_to_roles", b =>
                {
                    b.Property<int>("role_id")
                        .HasColumnType("int");

                    b.Property<long>("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("role_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_roles");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstanceEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "EventTimeslot")
                        .WithMany("Instances")
                        .HasForeignKey("timeslot_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeslot");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstancePreconfigurationEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "EventTimeSlot")
                        .WithMany("Preconfigurations")
                        .HasForeignKey("event_timeslot_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeSlot");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventEntity", "Event")
                        .WithMany("Timeslots")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.MinigolfMapEntity", "Map")
                        .WithMany("EventTimeslots")
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Map");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotRegistrationEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "EventTimeslot")
                        .WithMany("Registrations")
                        .HasForeignKey("EventTimeslotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "FallbackEventTimeslot")
                        .WithMany()
                        .HasForeignKey("FallbackEventTimeslotId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeslot");

                    b.Navigation("FallbackEventTimeslot");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.UserPushSubscriptionEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", "User")
                        .WithMany("PushSubscriptions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("event_instances_to_users", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventInstanceEntity", null)
                        .WithMany()
                        .HasForeignKey("event_instance_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("users_to_avoided_users", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("avoided_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("users_to_event_instance_preconfigurations", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventInstancePreconfigurationEntity", null)
                        .WithMany()
                        .HasForeignKey("event_instance_preconfiguration_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("users_to_preferred_users", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("preferred_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("users_to_roles", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.RoleEntity", null)
                        .WithMany()
                        .HasForeignKey("role_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventEntity", b =>
                {
                    b.Navigation("Timeslots");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotEntity", b =>
                {
                    b.Navigation("Instances");

                    b.Navigation("Preconfigurations");

                    b.Navigation("Registrations");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.MinigolfMapEntity", b =>
                {
                    b.Navigation("EventTimeslots");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.UserEntity", b =>
                {
                    b.Navigation("PushSubscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}