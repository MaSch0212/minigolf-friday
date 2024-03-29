﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinigolfFriday.Data;

#nullable disable

namespace MinigolfFriday.Migrations
{
    [DbContext(typeof(MinigolfFridayContext))]
    [Migration("20240204112523_RemovedInvites")]
    partial class RemovedInvites
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("EventInstanceEntityUserEntity", b =>
                {
                    b.Property<Guid>("EventInstancesId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PlayersId")
                        .HasColumnType("TEXT");

                    b.HasKey("EventInstancesId", "PlayersId");

                    b.HasIndex("PlayersId");

                    b.ToTable("EventInstanceEntityUserEntity");
                });

            modelBuilder.Entity("EventInstancePreconfigurationEntityUserEntity", b =>
                {
                    b.Property<Guid>("EventInstancePreconfigurationsId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PlayersId")
                        .HasColumnType("TEXT");

                    b.HasKey("EventInstancePreconfigurationsId", "PlayersId");

                    b.HasIndex("PlayersId");

                    b.ToTable("EventInstancePreconfigurationEntityUserEntity");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("RegistrationDeadline")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventInstanceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EventTimeslotId")
                        .HasColumnType("TEXT");

                    b.Property<string>("GroupCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EventTimeslotId");

                    b.ToTable("EventInstances");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventInstancePreconfigurationEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EventTimeslotId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EventTimeslotId");

                    b.ToTable("EventInstancePreconfigurations");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventPlayerRegistrationEntity", b =>
                {
                    b.Property<Guid>("EventTimeslotId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("FallbackEventTimeslotId")
                        .HasColumnType("TEXT");

                    b.HasKey("EventTimeslotId", "PlayerId");

                    b.HasIndex("FallbackEventTimeslotId");

                    b.HasIndex("PlayerId");

                    b.ToTable("EventPlayerRegistration");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventTimeslotEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("EventId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFallbackAllowed")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("MapId")
                        .HasColumnType("TEXT");

                    b.Property<TimeOnly>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MapId");

                    b.HasIndex("EventId", "Time")
                        .IsUnique();

                    b.ToTable("EventTimeslots");
                });

            modelBuilder.Entity("MinigolfFriday.Data.MinigolfMapEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("MinigolfMaps");
                });

            modelBuilder.Entity("MinigolfFriday.UserEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("FacebookId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FacebookId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UserEntityUserEntity", b =>
                {
                    b.Property<Guid>("AvoidId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AvoidedById")
                        .HasColumnType("TEXT");

                    b.HasKey("AvoidId", "AvoidedById");

                    b.HasIndex("AvoidedById");

                    b.ToTable("UserEntityUserEntity");
                });

            modelBuilder.Entity("UserEntityUserEntity1", b =>
                {
                    b.Property<Guid>("PreferId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PreferredById")
                        .HasColumnType("TEXT");

                    b.HasKey("PreferId", "PreferredById");

                    b.HasIndex("PreferredById");

                    b.ToTable("UserEntityUserEntity1");
                });

            modelBuilder.Entity("EventInstanceEntityUserEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventInstanceEntity", null)
                        .WithMany()
                        .HasForeignKey("EventInstancesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("PlayersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EventInstancePreconfigurationEntityUserEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventInstancePreconfigurationEntity", null)
                        .WithMany()
                        .HasForeignKey("EventInstancePreconfigurationsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("PlayersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventInstanceEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventTimeslotEntity", "EventTimeSlot")
                        .WithMany("Instances")
                        .HasForeignKey("EventTimeslotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeSlot");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventInstancePreconfigurationEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventTimeslotEntity", "EventTimeSlot")
                        .WithMany("Preconfigurations")
                        .HasForeignKey("EventTimeslotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeSlot");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventPlayerRegistrationEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventTimeslotEntity", "EventTimeslot")
                        .WithMany("Registrations")
                        .HasForeignKey("EventTimeslotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.EventTimeslotEntity", "FallbackEventTimeslot")
                        .WithMany()
                        .HasForeignKey("FallbackEventTimeslotId");

                    b.HasOne("MinigolfFriday.UserEntity", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventTimeslot");

                    b.Navigation("FallbackEventTimeslot");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventTimeslotEntity", b =>
                {
                    b.HasOne("MinigolfFriday.Data.EventEntity", "Event")
                        .WithMany("Timeslots")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.Data.MinigolfMapEntity", "Map")
                        .WithMany()
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Map");
                });

            modelBuilder.Entity("UserEntityUserEntity", b =>
                {
                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("AvoidId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("AvoidedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserEntityUserEntity1", b =>
                {
                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("PreferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MinigolfFriday.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("PreferredById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventEntity", b =>
                {
                    b.Navigation("Timeslots");
                });

            modelBuilder.Entity("MinigolfFriday.Data.EventTimeslotEntity", b =>
                {
                    b.Navigation("Instances");

                    b.Navigation("Preconfigurations");

                    b.Navigation("Registrations");
                });
#pragma warning restore 612, 618
        }
    }
}
