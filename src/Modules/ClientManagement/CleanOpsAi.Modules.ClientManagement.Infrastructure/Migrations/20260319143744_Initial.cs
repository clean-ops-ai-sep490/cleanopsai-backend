using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "client_management");

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url_file = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contracts", x => x.id);
                    table.ForeignKey(
                        name: "fk_contracts_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "client_management",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    street = table.Column<string>(type: "text", nullable: true),
                    commune = table.Column<string>(type: "text", nullable: true),
                    province = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_locations_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "client_management",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zones",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_zones_locations_location_id",
                        column: x => x.location_id,
                        principalSchema: "client_management",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_areas",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    zone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_areas", x => x.id);
                    table.ForeignKey(
                        name: "fk_work_areas_zones_zone_id",
                        column: x => x.zone_id,
                        principalSchema: "client_management",
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contract_shifts",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    shift_end = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    break_minutes = table.Column<int>(type: "integer", nullable: false),
                    required_workers = table.Column<int>(type: "integer", nullable: false),
                    day_type = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_shifts", x => x.id);
                    table.ForeignKey(
                        name: "fk_contract_shifts_contracts_contract_id",
                        column: x => x.contract_id,
                        principalSchema: "client_management",
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contract_shifts_work_areas_work_area_id",
                        column: x => x.work_area_id,
                        principalSchema: "client_management",
                        principalTable: "work_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sla",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    environment_type = table.Column<int>(type: "integer", nullable: false),
                    service_type = table.Column<int>(type: "integer", nullable: false),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sla", x => x.id);
                    table.ForeignKey(
                        name: "fk_sla_contracts_contract_id",
                        column: x => x.contract_id,
                        principalSchema: "client_management",
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sla_work_areas_work_area_id",
                        column: x => x.work_area_id,
                        principalSchema: "client_management",
                        principalTable: "work_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_area_details",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    area = table.Column<double>(type: "double precision", nullable: false),
                    total_area = table.Column<double>(type: "double precision", nullable: false),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_work_area_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_work_area_details_work_areas_work_area_id",
                        column: x => x.work_area_id,
                        principalSchema: "client_management",
                        principalTable: "work_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sla_shifts",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sla_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    required_worker = table.Column<int>(type: "integer", nullable: false),
                    break_time = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sla_shifts", x => x.id);
                    table.ForeignKey(
                        name: "fk_sla_shifts_sla_sla_id",
                        column: x => x.sla_id,
                        principalSchema: "client_management",
                        principalTable: "sla",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sla_tasks",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sla_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurrence_type = table.Column<string>(type: "text", nullable: false),
                    recurrence_config = table.Column<string>(type: "jsonb", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sla_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_sla_tasks_sla_sla_id",
                        column: x => x.sla_id,
                        principalSchema: "client_management",
                        principalTable: "sla",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contract_shifts_contract_id",
                schema: "client_management",
                table: "contract_shifts",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_contract_shifts_work_area_id",
                schema: "client_management",
                table: "contract_shifts",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_client_id",
                schema: "client_management",
                table: "contracts",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_client_id",
                schema: "client_management",
                table: "locations",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_sla_contract_id",
                schema: "client_management",
                table: "sla",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "ix_sla_work_area_id",
                schema: "client_management",
                table: "sla",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_sla_shifts_sla_id",
                schema: "client_management",
                table: "sla_shifts",
                column: "sla_id");

            migrationBuilder.CreateIndex(
                name: "ix_sla_tasks_sla_id",
                schema: "client_management",
                table: "sla_tasks",
                column: "sla_id");

            migrationBuilder.CreateIndex(
                name: "ix_work_area_details_work_area_id",
                schema: "client_management",
                table: "work_area_details",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_work_areas_zone_id",
                schema: "client_management",
                table: "work_areas",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_zones_location_id",
                schema: "client_management",
                table: "zones",
                column: "location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_shifts",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "sla_shifts",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "sla_tasks",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "work_area_details",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "sla",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "contracts",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "work_areas",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "zones",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "locations",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "client_management");
        }
    }
}
