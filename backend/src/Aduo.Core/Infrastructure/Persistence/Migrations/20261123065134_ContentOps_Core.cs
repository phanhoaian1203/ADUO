using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aduo.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContentOps_Core : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "app_settings",
                schema: "content",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "content_revisions",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    change_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_revisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "licenses",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    allows_commercial = table.Column<bool>(type: "boolean", nullable: false),
                    requires_attribution = table.Column<bool>(type: "boolean", nullable: false),
                    share_alike = table.Column<bool>(type: "boolean", nullable: false),
                    allows_modification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "review_checklists",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    items = table.Column<string>(type: "jsonb", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_review_checklists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sources",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    provider = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    default_license_id = table.Column<short>(type: "smallint", nullable: false),
                    license_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sources", x => x.id);
                    table.ForeignKey(
                        name: "fk_sources_licenses_default_license_id",
                        column: x => x.default_license_id,
                        principalSchema: "content",
                        principalTable: "licenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "media",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    bucket = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    duration_ms = table.Column<int>(type: "integer", nullable: true),
                    checksum_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    original_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attribution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_id = table.Column<short>(type: "smallint", nullable: false),
                    original_ref = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    origin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ai_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    prompt_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    import_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_licenses_license_id",
                        column: x => x.license_id,
                        principalSchema: "content",
                        principalTable: "licenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_media_sources_source_id",
                        column: x => x.source_id,
                        principalSchema: "content",
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    report = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    committed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_batches", x => x.id);
                    table.ForeignKey(
                        name: "fk_import_batches_media_assets_file_media_id",
                        column: x => x.file_media_id,
                        principalSchema: "content",
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_import_batches_sources_source_id",
                        column: x => x.source_id,
                        principalSchema: "content",
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "app_settings",
                columns: new[] { "key", "updated_at", "value" },
                values: new object[,]
                {
                    { "budget.monthlyUsd", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "10" },
                    { "exam.graceSeconds", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "30" },
                    { "infohub.reviewMonths.Ausbildung", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "3" },
                    { "infohub.reviewMonths.default", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "6" },
                    { "lesson.passThresholdPct", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "60" },
                    { "reports.maxPerDay", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "10" }
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "licenses",
                columns: new[] { "id", "allows_commercial", "allows_modification", "code", "name", "requires_attribution", "share_alike", "url" },
                values: new object[,]
                {
                    { (short)1, true, true, "CC-BY-SA-4.0", "Creative Commons Attribution-ShareAlike 4.0", true, true, "https://creativecommons.org/licenses/by-sa/4.0/" },
                    { (short)2, true, true, "CC-BY-2.0-FR", "Creative Commons Attribution 2.0 France", true, false, "https://creativecommons.org/licenses/by/2.0/fr/" },
                    { (short)3, true, true, "CC0-1.0", "Creative Commons Zero 1.0 (public domain)", false, false, "https://creativecommons.org/publicdomain/zero/1.0/" },
                    { (short)4, true, true, "Proprietary", "Độc quyền (nội dung của ADUO hoặc theo điều khoản nhà cung cấp)", false, false, null },
                    { (short)5, false, true, "CC-BY-NC-4.0", "Creative Commons Attribution-NonCommercial 4.0 (KHÔNG dùng thương mại)", true, false, "https://creativecommons.org/licenses/by-nc/4.0/" }
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "sources",
                columns: new[] { "id", "created_at", "default_license_id", "is_active", "license_verified_at", "name", "notes", "provider", "updated_at", "url" },
                values: new object[,]
                {
                    { new Guid("0193b000-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, true, null, "Wiktionary (kaikki.org)", "Dữ liệu CC BY-SA: bản dịch và biến đổi vẫn là CC BY-SA, phải ghi công trên UI (BR-CNT-04).", "Wiktionary / kaikki.org", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://kaikki.org" },
                    { new Guid("0193b000-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)2, true, null, "Tatoeba", "Phần lớn câu CC BY 2.0 FR; một số câu CC0. Kiểm tra từng câu khi import.", "Tatoeba", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://tatoeba.org" },
                    { new Guid("0193b000-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)4, true, null, "ADUO (tự biên soạn)", null, "ADUO", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("0193b000-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)4, true, null, "Azure TTS", "⚖️ Chưa xác minh điều khoản lưu và phát lại audio tổng hợp. Xác minh trước khi dùng audio thật.", "Microsoft Azure", new DateTimeOffset(new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_revisions_entity",
                schema: "content",
                table: "content_revisions",
                columns: new[] { "entity_type", "entity_id", "version" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_import_batches_created",
                schema: "content",
                table: "import_batches",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_import_batches_file_media_id",
                schema: "content",
                table: "import_batches",
                column: "file_media_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_batches_source_id",
                schema: "content",
                table: "import_batches",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ux_licenses_code",
                schema: "content",
                table: "licenses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_checksum",
                schema: "content",
                table: "media",
                column: "checksum_sha256");

            migrationBuilder.CreateIndex(
                name: "ix_media_kind_created",
                schema: "content",
                table: "media",
                columns: new[] { "kind", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_media_license_id",
                schema: "content",
                table: "media",
                column: "license_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_source_id",
                schema: "content",
                table: "media",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_mediaasset_status",
                schema: "content",
                table: "media",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_media_storage_key",
                schema: "content",
                table: "media",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_review_checklists_entity",
                schema: "content",
                table: "review_checklists",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sources_default_license_id",
                schema: "content",
                table: "sources",
                column: "default_license_id");

            migrationBuilder.CreateIndex(
                name: "ux_sources_name",
                schema: "content",
                table: "sources",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_revisions",
                schema: "content");

            migrationBuilder.DropTable(
                name: "import_batches",
                schema: "content");

            migrationBuilder.DropTable(
                name: "review_checklists",
                schema: "content");

            migrationBuilder.DropTable(
                name: "media",
                schema: "content");

            migrationBuilder.DropTable(
                name: "sources",
                schema: "content");

            migrationBuilder.DropTable(
                name: "licenses",
                schema: "content");
        }
    }
}
