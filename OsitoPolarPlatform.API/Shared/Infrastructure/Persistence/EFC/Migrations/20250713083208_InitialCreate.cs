using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    equipment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_identifier = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "longtext", nullable: false),
                    model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    manufacturer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    serial_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    technical_details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    is_powered_on = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    installation_date = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    current_temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    set_temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    optimal_temperature_min = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    optimal_temperature_max = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    location_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    location_address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    location_latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    location_longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    energy_consumption_current = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    energy_consumption_unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    energy_consumption_average = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    owner_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ownership_type = table.Column<string>(type: "longtext", nullable: false),
                    rental_start_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    rental_end_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    rental_monthly_fee = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    rental_provider_id = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment", x => x.equipment_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    first_name = table.Column<string>(type: "longtext", nullable: false),
                    last_name = table.Column<string>(type: "longtext", nullable: false),
                    email_address = table.Column<string>(type: "longtext", nullable: false),
                    address_street = table.Column<string>(type: "longtext", nullable: false),
                    address_number = table.Column<string>(type: "longtext", nullable: false),
                    address_city = table.Column<string>(type: "longtext", nullable: false),
                    address_postal_code = table.Column<string>(type: "longtext", nullable: false),
                    address_country = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_profiles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    plan_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    billing_cycle = table.Column<string>(type: "longtext", nullable: false),
                    max_equipment = table.Column<int>(type: "int", nullable: true),
                    max_clients = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_subscriptions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "technicians",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    specialization = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    availability = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_technicians", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "longtext", nullable: false),
                    password_hash = table.Column<string>(type: "longtext", nullable: false),
                    subscription_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "daily_temperature_averages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    average_temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    min_temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    max_temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_daily_temperature_averages", x => x.id);
                    table.ForeignKey(
                        name: "f_k__daily_avg__equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "energy_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    consumption = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_energy_readings", x => x.id);
                    table.ForeignKey(
                        name: "f_k__energy_reading__equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "equipment_analytics",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_analytics", x => x.id);
                    table.ForeignKey(
                        name: "f_k__analytics__equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "temperature_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_temperature_readings", x => x.id);
                    table.ForeignKey(
                        name: "f_k__temp_reading__equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "service_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    issue_details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    request_time = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    priority = table.Column<string>(type: "longtext", nullable: false),
                    urgency = table.Column<string>(type: "longtext", nullable: false),
                    is_emergency = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    service_type = table.Column<string>(type: "longtext", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    assigned_technician_id = table.Column<int>(type: "int", nullable: true),
                    scheduled_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    time_slot = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    service_address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    desired_completion_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    actual_completion_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    resolution_details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    technician_notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    customer_feedback_rating = table.Column<int>(type: "int", nullable: true),
                    feedback_submission_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_service_requests", x => x.id);
                    table.ForeignKey(
                        name: "f_k_service_requests__technician_assigned_technician_id",
                        column: x => x.assigned_technician_id,
                        principalTable: "technicians",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_service_requests_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    work_order_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    service_request_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    issue_details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    creation_time = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    priority = table.Column<string>(type: "longtext", nullable: false),
                    assigned_technician_id = table.Column<int>(type: "int", nullable: true),
                    scheduled_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    time_slot = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    service_address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    desired_completion_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    actual_completion_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    resolution_details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    technician_notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    customer_feedback_rating = table.Column<int>(type: "int", nullable: true),
                    feedback_submission_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    service_type = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_work_orders", x => x.id);
                    table.ForeignKey(
                        name: "f_k_work_orders__technician_assigned_technician_id",
                        column: x => x.assigned_technician_id,
                        principalTable: "technicians",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_work_orders_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_work_orders_service_requests_service_request_id",
                        column: x => x.service_request_id,
                        principalTable: "service_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "subscriptions",
                columns: new[] { "id", "billing_cycle", "created_at", "max_clients", "max_equipment", "plan_name", "updated_at" },
                values: new object[,]
                {
                    { 1, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), null, 6, "Basic (Polar Bear) - $18.99/month - Up to 6 units", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 2, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), null, 12, "Standard (Snow Bear) - $35.13/month - Up to 12 units", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 3, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), null, 24, "Premium (Glacial Bear) - $67.56/month - Up to 24 units", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 4, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), 10, null, "Small Company - $40.51/month - Up to 10 clients", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 5, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), 30, null, "Medium Company - $81.08/month - Up to 30 clients", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 6, "Monthly", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)), 999999, null, "Enterprise Premium - $162.16/month - Unlimited clients", new DateTimeOffset(new DateTime(2025, 7, 13, 8, 32, 7, 615, DateTimeKind.Unspecified).AddTicks(7641), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "i_x_daily_temperature_averages_equipment_id_date",
                table: "daily_temperature_averages",
                columns: new[] { "equipment_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_energy_readings_equipment_id_timestamp",
                table: "energy_readings",
                columns: new[] { "equipment_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "i_x_equipment_code",
                table: "equipment",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_equipment_serial_number",
                table: "equipment",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_equipment_analytics_equipment_id",
                table: "equipment_analytics",
                column: "equipment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_service_requests_assigned_technician_id",
                table: "service_requests",
                column: "assigned_technician_id");

            migrationBuilder.CreateIndex(
                name: "i_x_service_requests_equipment_id",
                table: "service_requests",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "i_x_temperature_readings_equipment_id_timestamp",
                table: "temperature_readings",
                columns: new[] { "equipment_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "i_x_work_orders_assigned_technician_id",
                table: "work_orders",
                column: "assigned_technician_id");

            migrationBuilder.CreateIndex(
                name: "i_x_work_orders_equipment_id",
                table: "work_orders",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "i_x_work_orders_service_request_id",
                table: "work_orders",
                column: "service_request_id");

            migrationBuilder.CreateIndex(
                name: "i_x_work_orders_work_order_number",
                table: "work_orders",
                column: "work_order_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_temperature_averages");

            migrationBuilder.DropTable(
                name: "energy_readings");

            migrationBuilder.DropTable(
                name: "equipment_analytics");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "temperature_readings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "work_orders");

            migrationBuilder.DropTable(
                name: "service_requests");

            migrationBuilder.DropTable(
                name: "technicians");

            migrationBuilder.DropTable(
                name: "equipment");
        }
    }
}
