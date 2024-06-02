﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinigolfFriday.Data;

#nullable disable

namespace MinigolfFriday.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240515171610_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.5");

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT")
                        .HasColumnName("date");

                    b.Property<DateTimeOffset>("RegistrationDeadline")
                        .HasColumnType("TEXT")
                        .HasColumnName("registration_deadline");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("TEXT")
                        .HasColumnName("started_at");

                    b.HasKey("Id");

                    b.ToTable("events", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstanceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("GroupCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT")
                        .HasColumnName("group_code");

                    b.Property<long>("timeslot_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("timeslot_id");

                    b.ToTable("event_instances", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstancePreconfigurationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("event_timeslot_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("event_timeslot_id");

                    b.ToTable("event_instance_preconfigurations", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<bool>("IsFallbackAllowed")
                        .HasColumnType("INTEGER")
                        .HasColumnName("is_fallback_allowed");

                    b.Property<TimeOnly>("Time")
                        .HasColumnType("TEXT")
                        .HasColumnName("time");

                    b.Property<long>("event_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("map_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Time")
                        .IsUnique();

                    b.HasIndex("event_id");

                    b.HasIndex("map_id");

                    b.ToTable("event_timeslots", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotRegistrationEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("event_timeslot_id")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("fallback_event_timeslot_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("event_timeslot_id");

                    b.HasIndex("fallback_event_timeslot_id");

                    b.HasIndex("user_id");

                    b.ToTable("event_timeslot_registration", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.MinigolfMapEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("maps", (string)null);
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.RoleEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT")
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
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("TEXT")
                        .HasColumnName("alias");

                    b.Property<string>("LoginToken")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT")
                        .HasColumnName("login_token");

                    b.HasKey("Id");

                    b.HasIndex("LoginToken")
                        .IsUnique();

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("event_instances_to_users", b =>
                {
                    b.Property<long>("event_instance_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("event_instance_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("event_instances_to_users");
                });

            modelBuilder.Entity("users_to_avoided_users", b =>
                {
                    b.Property<long>("avoided_user_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("avoided_user_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_avoided_users");
                });

            modelBuilder.Entity("users_to_event_instance_preconfigurations", b =>
                {
                    b.Property<long>("event_instance_preconfiguration_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("event_instance_preconfiguration_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_event_instance_preconfigurations");
                });

            modelBuilder.Entity("users_to_preferred_users", b =>
                {
                    b.Property<long>("preferred_user_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("preferred_user_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_preferred_users");
                });

            modelBuilder.Entity("users_to_roles", b =>
                {
                    b.Property<int>("role_id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("user_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("role_id", "user_id");

                    b.HasIndex("user_id");

                    b.ToTable("users_to_roles");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventInstanceEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "EventTimeSlot")
                        .WithMany("Instances")
                        .HasForeignKey("timeslot_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeSlot");
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
                        .HasForeignKey("event_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.MinigolfMapEntity", "Map")
                        .WithMany()
                        .HasForeignKey("map_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Map");
                });

            modelBuilder.Entity("MinigolfFriday.Data.Entities.EventTimeslotRegistrationEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "EventTimeslot")
                        .WithMany("Registrations")
                        .HasForeignKey("event_timeslot_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.Entities.EventTimeslotEntity", "FallbackEventTimeslot")
                        .WithMany()
                        .HasForeignKey("fallback_event_timeslot_id");

                    b.HasOne("MinigolfFriday.Data.Entities.UserEntity", "Player")
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeslot");

                    b.Navigation("FallbackEventTimeslot");

                    b.Navigation("Player");
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
                        .OnDelete(DeleteBehavior.Cascade)
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
                        .OnDelete(DeleteBehavior.Cascade)
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
#pragma warning restore 612, 618
        }
    }
}
