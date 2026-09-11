using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SigurnaDob.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAuthDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "app_roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Coordinator" },
                    { 3, "Caregiver" },
                    { 4, "FamilyMember" }
                });

            migrationBuilder.InsertData(
                table: "app_users",
                columns: new[] { "id", "created_at", "family_contact_id", "is_active", "password_hash", "staff_id", "username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "AQAAAAIAAYagAAAAEFLutaNEinL3QdP2YNWoaL0Qr/tPTAdVIGGkQWYoYdtWxHmHiebIUxFM3ohoxjos1Q==", null, "admin" },
                    { 2, new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "AQAAAAIAAYagAAAAEBJLKQlZj3dSLD7NuQ99SlhqMhe0HpLJdNuq/Hs8gY92z7XYk8OA1irGdOWBAkaUzA==", null, "coordinator" },
                    { 4, new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "AQAAAAIAAYagAAAAELVhZzQ0bK3F1Lz15ptwP/gNojjzJzaj4tTXcuEKlAVQwhHq3Crgsn/2yTToOi4zmQ==", null, "familymember" }
                });

            migrationBuilder.InsertData(
                table: "staff",
                columns: new[] { "id", "email", "full_name", "is_active", "phone", "position" },
                values: new object[] { 1, null, "Ivana Njegovateljica", true, null, "Njegovateljica" });

            migrationBuilder.InsertData(
                table: "app_user_roles",
                columns: new[] { "app_role_id", "app_user_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 4, 4 }
                });

            migrationBuilder.InsertData(
                table: "app_users",
                columns: new[] { "id", "created_at", "family_contact_id", "is_active", "password_hash", "staff_id", "username" },
                values: new object[] { 3, new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "AQAAAAIAAYagAAAAEOwSz/Mzqr6ENjI97rKG0mwoE4sk8cTvIxWnKGBdhQ3Gil76gFMpwule2AkdoIRFPA==", 1, "caregiver" });

            migrationBuilder.InsertData(
                table: "app_user_roles",
                columns: new[] { "app_role_id", "app_user_id" },
                values: new object[] { 3, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "app_user_roles",
                keyColumns: new[] { "app_role_id", "app_user_id" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "app_user_roles",
                keyColumns: new[] { "app_role_id", "app_user_id" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "app_user_roles",
                keyColumns: new[] { "app_role_id", "app_user_id" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "app_user_roles",
                keyColumns: new[] { "app_role_id", "app_user_id" },
                keyValues: new object[] { 4, 4 });

            migrationBuilder.DeleteData(
                table: "app_roles",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "app_roles",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "app_roles",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "app_roles",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "app_users",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "app_users",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "app_users",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "app_users",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "staff",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
