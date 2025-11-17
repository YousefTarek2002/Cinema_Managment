using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeCreatedToActorsAndCinemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Movies",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cinemas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Cinemas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Actors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Actors",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Movies",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.InsertData(
                table: "Actors",
                columns: new[] { "Id", "Bio", "FullName", "ImgUrl" },
                values: new object[,]
                {
                    { 1, "Known for Inception and Titanic", "Leonardo DiCaprio", "leo.jpg" },
                    { 2, "Starred in Joker (2019)", "Joaquin Phoenix", "joaquin.jpg" },
                    { 3, "Avengers and Lucy", "Scarlett Johansson", "scarlett.jpg" },
                    { 4, "Venom and Mad Max", "Tom Hardy", "hardy.jpg" },
                    { 5, "A Quiet Place and Edge of Tomorrow", "Emily Blunt", "emily.jpg" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 18, 12, 33, 24, 17, DateTimeKind.Local).AddTicks(5799), "High energy and stunts", 0, null, "Action", null },
                    { 2, new DateTime(2025, 10, 18, 12, 33, 24, 18, DateTimeKind.Local).AddTicks(6176), "Emotional and deep storytelling", 0, null, "Drama", null },
                    { 3, new DateTime(2025, 10, 18, 12, 33, 24, 18, DateTimeKind.Local).AddTicks(6187), "Funny and entertaining", 0, null, "Comedy", null },
                    { 4, new DateTime(2025, 10, 18, 12, 33, 24, 18, DateTimeKind.Local).AddTicks(6188), "Scary and thrilling", 0, null, "Horror", null },
                    { 5, new DateTime(2025, 10, 18, 12, 33, 24, 18, DateTimeKind.Local).AddTicks(6189), "Love and relationships", 0, null, "Romance", null }
                });

            migrationBuilder.InsertData(
                table: "Cinemas",
                columns: new[] { "Id", "Description", "ImgUrl", "Location", "Logo", "Name" },
                values: new object[,]
                {
                    { 1, "Top cinema in Cairo", "galaxy.jpg", "Cairo", null, "Galaxy Cinema" },
                    { 2, "Luxury screens in Alexandria", "vox.jpg", "Alexandria", null, "Vox Cinema" },
                    { 3, "Massive experience in Giza", "imax.jpg", "Giza", null, "IMAX" },
                    { 4, "Modern cinema in Mansoura", "cinemacity.jpg", "Mansoura", null, "Cinema City" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "CategoryId", "CinemaId", "DateTime", "Description", "MainImg", "Name", "Price", "Status", "SubImages" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2020, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dream-sharing heist movie", "inception.jpg", "Inception", 120m, true, null },
                    { 2, 2, 2, new DateTime(2019, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "A mentally troubled comedian turns criminal", "joker.jpg", "Joker", 100m, true, null },
                    { 3, 1, 3, new DateTime(2019, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Superheroes unite to reverse the snap", "endgame.jpg", "Avengers: Endgame", 150m, true, null },
                    { 4, 4, 4, new DateTime(2018, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "A family must live in silence", "quietplace.jpg", "A Quiet Place", 90m, true, null },
                    { 5, 5, 1, new DateTime(1997, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Love story aboard the doomed ship", "titanic.jpg", "Titanic", 110m, true, null }
                });

            migrationBuilder.InsertData(
                table: "MovieActors",
                columns: new[] { "ActorId", "MovieId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 3 },
                    { 5, 4 },
                    { 1, 5 },
                    { 3, 5 }
                });
        }
    }
}
