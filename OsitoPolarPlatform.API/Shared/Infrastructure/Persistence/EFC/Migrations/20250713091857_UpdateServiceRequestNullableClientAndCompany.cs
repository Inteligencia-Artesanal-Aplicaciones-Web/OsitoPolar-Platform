using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequestNullableClientAndCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "company_id",
                table: "service_requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "client_id",
                table: "service_requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 9, 18, 56, 470, DateTimeKind.Unspecified).AddTicks(5404), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "company_id",
                table: "service_requests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "client_id",
                table: "service_requests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
