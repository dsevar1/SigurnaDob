using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SigurnaDob.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "care_task_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_task_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "care_task_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_task_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resident_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resident_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "room_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_room_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    full_name = table.Column<string>(type: "TEXT", nullable: false),
                    position = table.Column<string>(type: "TEXT", nullable: true),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visit_request_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_request_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activity_type_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_activities_activity_types_activity_type_id",
                        column: x => x.activity_type_id,
                        principalTable: "activity_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    room_number = table.Column<string>(type: "TEXT", nullable: false),
                    capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    room_status_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_rooms_room_statuses_room_status_id",
                        column: x => x.room_status_id,
                        principalTable: "room_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "residents",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    full_name = table.Column<string>(type: "TEXT", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    resident_status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    room_id = table.Column<int>(type: "INTEGER", nullable: true),
                    admission_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_residents", x => x.id);
                    table.ForeignKey(
                        name: "fk_residents_resident_statuses_resident_status_id",
                        column: x => x.resident_status_id,
                        principalTable: "resident_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_residents_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_participants",
                columns: table => new
                {
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
                    resident_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_participants", x => new { x.activity_id, x.resident_id });
                    table.ForeignKey(
                        name: "fk_activity_participants_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_participants_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "care_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    resident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    care_task_type_id = table.Column<int>(type: "INTEGER", nullable: false),
                    care_task_status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    assigned_staff_id = table.Column<int>(type: "INTEGER", nullable: true),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    completed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    completion_note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_care_tasks_care_task_statuses_care_task_status_id",
                        column: x => x.care_task_status_id,
                        principalTable: "care_task_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_tasks_care_task_types_care_task_type_id",
                        column: x => x.care_task_type_id,
                        principalTable: "care_task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_tasks_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_tasks_staff_assigned_staff_id",
                        column: x => x.assigned_staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_contacts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    resident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    full_name = table.Column<string>(type: "TEXT", nullable: false),
                    relationship = table.Column<string>(type: "TEXT", nullable: true),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_contacts_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resident_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    resident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    original_file_name = table.Column<string>(type: "TEXT", nullable: false),
                    stored_file_name = table.Column<string>(type: "TEXT", nullable: false),
                    content_type = table.Column<string>(type: "TEXT", nullable: false),
                    file_size_bytes = table.Column<long>(type: "INTEGER", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resident_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_resident_documents_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    staff_id = table.Column<int>(type: "INTEGER", nullable: true),
                    family_contact_id = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_users_family_contacts_family_contact_id",
                        column: x => x.family_contact_id,
                        principalTable: "family_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_app_users_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "visit_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    resident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    family_contact_id = table.Column<int>(type: "INTEGER", nullable: false),
                    visit_request_status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    requested_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    decided_by_staff_id = table.Column<int>(type: "INTEGER", nullable: true),
                    decision_note = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_visit_requests_family_contacts_family_contact_id",
                        column: x => x.family_contact_id,
                        principalTable: "family_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_visit_requests_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_visit_requests_staff_decided_by_staff_id",
                        column: x => x.decided_by_staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_visit_requests_visit_request_statuses_visit_request_status_id",
                        column: x => x.visit_request_status_id,
                        principalTable: "visit_request_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_user_roles",
                columns: table => new
                {
                    app_user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    app_role_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user_roles", x => new { x.app_user_id, x.app_role_id });
                    table.ForeignKey(
                        name: "fk_app_user_roles_app_roles_app_role_id",
                        column: x => x.app_role_id,
                        principalTable: "app_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_app_user_roles_app_users_app_user_id",
                        column: x => x.app_user_id,
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "activity_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Društvena" },
                    { 2, "Kreativna" },
                    { 3, "Tjelovježba" },
                    { 4, "Edukativna" },
                    { 5, "Izlet" }
                });

            migrationBuilder.InsertData(
                table: "care_task_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Novo" },
                    { 2, "Dodijeljeno" },
                    { 3, "U tijeku" },
                    { 4, "Izvršeno" },
                    { 5, "Otkazano" }
                });

            migrationBuilder.InsertData(
                table: "care_task_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Terapija" },
                    { 2, "Prehrana" },
                    { 3, "Higijena" },
                    { 4, "Pratnja" },
                    { 5, "Administrativno" }
                });

            migrationBuilder.InsertData(
                table: "resident_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "U pripremi za prijem" },
                    { 2, "Aktivan" },
                    { 3, "Privremeno odsutan" },
                    { 4, "Premješten iz doma" },
                    { 5, "Arhiviran" }
                });

            migrationBuilder.InsertData(
                table: "room_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "U uporabi" },
                    { 2, "Održavanje" },
                    { 3, "Izvan uporabe" }
                });

            migrationBuilder.InsertData(
                table: "visit_request_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Zaprimljeno" },
                    { 2, "Odobreno" },
                    { 3, "Odbijeno" },
                    { 4, "Održano" },
                    { 5, "Otkazano" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_activities_activity_type_id",
                table: "activities",
                column: "activity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_participants_resident_id",
                table: "activity_participants",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_types_name",
                table: "activity_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_roles_name",
                table: "app_roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_user_roles_app_role_id",
                table: "app_user_roles",
                column: "app_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_users_family_contact_id",
                table: "app_users",
                column: "family_contact_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_users_staff_id",
                table: "app_users",
                column: "staff_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_users_username",
                table: "app_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_care_task_statuses_name",
                table: "care_task_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_care_task_types_name",
                table: "care_task_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_care_tasks_assigned_staff_id",
                table: "care_tasks",
                column: "assigned_staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_tasks_care_task_status_id",
                table: "care_tasks",
                column: "care_task_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_tasks_care_task_type_id",
                table: "care_tasks",
                column: "care_task_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_tasks_resident_id",
                table: "care_tasks",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_contacts_resident_id",
                table: "family_contacts",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "ix_resident_documents_resident_id",
                table: "resident_documents",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "ix_resident_statuses_name",
                table: "resident_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_residents_resident_status_id",
                table: "residents",
                column: "resident_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_residents_room_id",
                table: "residents",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_statuses_name",
                table: "room_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_room_number",
                table: "rooms",
                column: "room_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_room_status_id",
                table: "rooms",
                column: "room_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_visit_request_statuses_name",
                table: "visit_request_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visit_requests_decided_by_staff_id",
                table: "visit_requests",
                column: "decided_by_staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_visit_requests_family_contact_id",
                table: "visit_requests",
                column: "family_contact_id");

            migrationBuilder.CreateIndex(
                name: "ix_visit_requests_resident_id",
                table: "visit_requests",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "ix_visit_requests_visit_request_status_id",
                table: "visit_requests",
                column: "visit_request_status_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_participants");

            migrationBuilder.DropTable(
                name: "app_user_roles");

            migrationBuilder.DropTable(
                name: "care_tasks");

            migrationBuilder.DropTable(
                name: "resident_documents");

            migrationBuilder.DropTable(
                name: "visit_requests");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "app_roles");

            migrationBuilder.DropTable(
                name: "app_users");

            migrationBuilder.DropTable(
                name: "care_task_statuses");

            migrationBuilder.DropTable(
                name: "care_task_types");

            migrationBuilder.DropTable(
                name: "visit_request_statuses");

            migrationBuilder.DropTable(
                name: "activity_types");

            migrationBuilder.DropTable(
                name: "family_contacts");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "residents");

            migrationBuilder.DropTable(
                name: "resident_statuses");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "room_statuses");
        }
    }
}
