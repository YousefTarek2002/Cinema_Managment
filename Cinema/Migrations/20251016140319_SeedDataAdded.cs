using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "High energy and stunts", "Action" },
                    { 2, "Emotional and deep storytelling", "Drama" },
                    { 3, "Funny and entertaining", "Comedy" },
                    { 4, "Scary and thrilling", "Horror" },
                    { 5, "Love and relationships", "Romance" }
                });

            migrationBuilder.InsertData(
                table: "Cinemas",
                columns: new[] { "Id", "Description", "ImgUrl", "Location", "Name" },
                values: new object[,]
                {
                    { 1, "Top cinema in Cairo", "galaxy.jpg", "Cairo", "Galaxy Cinema" },
                    { 2, "Luxury screens in Alexandria", "vox.jpg", "Alexandria", "Vox Cinema" },
                    { 3, "Massive experience in Giza", "imax.jpg", "Giza", "IMAX" },
                    { 4, "Modern cinema in Mansoura", "cinemacity.jpg", "Mansoura", "Cinema City" }
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 5, 4 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorId", "MovieId" },
                keyValues: new object[] { 3, 5 });

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
