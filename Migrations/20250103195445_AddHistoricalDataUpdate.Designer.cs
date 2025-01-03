﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Project.Data;

#nullable disable

namespace Project.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250103195445_AddHistoricalDataUpdate")]
    partial class AddHistoricalDataUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Project.Models.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Rate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Currencies");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "USD",
                            Rate = 4.00m
                        },
                        new
                        {
                            Id = 2,
                            Code = "EUR",
                            Rate = 4.50m
                        },
                        new
                        {
                            Id = 3,
                            Code = "GBP",
                            Rate = 5.20m
                        },
                        new
                        {
                            Id = 4,
                            Code = "PLN",
                            Rate = 1.00m
                        });
                });

            modelBuilder.Entity("Project.Models.HistoricalData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Rate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyCode", "Timestamp");

                    b.ToTable("HistoricalDatas");
                });
#pragma warning restore 612, 618
        }
    }
}