using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Modules.Ticketing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Inint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ticketing");

            migrationBuilder.CreateTable(
                name: "attachment",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    file_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_visible = table.Column<bool>(type: "bit", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operators",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ref_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operators", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_visible = table.Column<bool>(type: "bit", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "severities",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_visible = table.Column<bool>(type: "bit", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_severities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_types",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_visible = table.Column<bool>(type: "bit", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_titles",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    default_severity_id = table.Column<int>(type: "int", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_titles", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_titles_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "ticketing",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    title_id = table.Column<int>(type: "int", nullable: true),
                    type_id = table.Column<int>(type: "int", nullable: false),
                    severity_id = table.Column<int>(type: "int", nullable: true),
                    group_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    other_title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                    table.ForeignKey(
                        name: "fk_tickets_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "ticketing",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_severities_severity_id",
                        column: x => x.severity_id,
                        principalSchema: "ticketing",
                        principalTable: "severities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tickets_ticket_titles_title_id",
                        column: x => x.title_id,
                        principalSchema: "ticketing",
                        principalTable: "ticket_titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_ticket_types_type_id",
                        column: x => x.type_id,
                        principalSchema: "ticketing",
                        principalTable: "ticket_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_comments",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ticket_id = table.Column<int>(type: "int", nullable: false),
                    commenter = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_comments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticketing",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_histories",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ticket_id = table.Column<int>(type: "int", nullable: false),
                    operator_id = table.Column<int>(type: "int", nullable: false),
                    operator_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_histories_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticketing",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_operators",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ticket_id = table.Column<int>(type: "int", nullable: false),
                    operator_id = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_operators", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_operators_operators_operator_id",
                        column: x => x.operator_id,
                        principalSchema: "ticketing",
                        principalTable: "operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ticket_operators_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticketing",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tickets_attachments",
                schema: "ticketing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ticket_id = table.Column<int>(type: "int", nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_by_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_tickets_attachments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "ticketing",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "ticketing",
                table: "categories",
                columns: new[] { "id", "is_visible", "name", "sort" },
                values: new object[,]
                {
                    { 1, true, "Recharge Operations", 1 },
                    { 2, true, "Meter & Customer configuration", 2 },
                    { 3, true, "Meter Operations", 3 },
                    { 4, true, "Payment & Settlement Operations", 4 },
                    { 5, true, "System Configuration Errors", 5 },
                    { 6, true, "Reports & Data Issues", 6 },
                    { 7, true, "System Performance & Unexpected Errors", 7 }
                });

            migrationBuilder.InsertData(
                schema: "ticketing",
                table: "service",
                columns: new[] { "id", "is_visible", "name", "sort" },
                values: new object[,]
                {
                    { 1, true, "Gas", 1 },
                    { 2, true, "Water", 2 },
                    { 3, true, "Electric", 3 }
                });

            migrationBuilder.InsertData(
                schema: "ticketing",
                table: "severities",
                columns: new[] { "id", "is_visible", "name", "sort" },
                values: new object[,]
                {
                    { 1, true, "Low", 1 },
                    { 2, true, "Medium", 2 },
                    { 3, true, "High", 3 },
                    { 4, true, "Critical", 4 }
                });

            migrationBuilder.InsertData(
                schema: "ticketing",
                table: "ticket_types",
                columns: new[] { "id", "is_visible", "name", "sort" },
                values: new object[,]
                {
                    { 1, true, "Complaint", 1 },
                    { 2, true, "Inquiry", 2 }
                });

            migrationBuilder.InsertData(
                schema: "ticketing",
                table: "ticket_titles",
                columns: new[] { "id", "category_id", "created_date", "default_severity_id", "name" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Failed Recharge" },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Wrong Balance After Recharge" },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Wrong Payment Type" },
                    { 4, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Add/Edit (Meter or Customer data )" },
                    { 5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "System Card issue" },
                    { 6, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Customer Card Issue" },
                    { 7, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Link Meter to Customer Failed" },
                    { 8, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Read retrival card issue" },
                    { 9, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Replace Card Issue" },
                    { 10, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Replace Meter Issue" },
                    { 11, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Adjustment Issue" },
                    { 12, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Payment Order Issue" },
                    { 13, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Debit Settlement Issue" },
                    { 14, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Pay Adjustment Issue" },
                    { 15, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Add/Edit(Users, Privileges, Cashier Holiday, Friendly Time, General Settings,Fees, Tariff, Customer Complaints, Activity, Adjustment Type, Banks, Organization, Replace Meter Reasons, Services)" },
                    { 16, 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Depends on Report Type" },
                    { 17, 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "System Crash / Slow / Unexpected Error" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_ticket_id",
                schema: "ticketing",
                table: "ticket_comments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_histories_ticket_id",
                schema: "ticketing",
                table: "ticket_histories",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_operators_operator_id",
                schema: "ticketing",
                table: "ticket_operators",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_operators_ticket_id",
                schema: "ticketing",
                table: "ticket_operators",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_titles_category_id",
                schema: "ticketing",
                table: "ticket_titles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_category_id",
                schema: "ticketing",
                table: "tickets",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_severity_id",
                schema: "ticketing",
                table: "tickets",
                column: "severity_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_title_id",
                schema: "ticketing",
                table: "tickets",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_type_id",
                schema: "ticketing",
                table: "tickets",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_attachments_ticket_id",
                schema: "ticketing",
                table: "tickets_attachments",
                column: "ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachment",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "service",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "ticket_comments",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "ticket_histories",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "ticket_operators",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "tickets_attachments",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "operators",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "tickets",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "severities",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "ticket_titles",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "ticket_types",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "ticketing");
        }
    }
}
