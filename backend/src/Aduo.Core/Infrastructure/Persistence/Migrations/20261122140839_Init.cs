using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aduo.Core.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration đầu tiên: bật extension (pg_trgm, unaccent) và tạo sẵn 7 schema của 7 module.
    /// Phần tạo schema được viết tay, vì EF chỉ tự tạo schema khi đã có bảng trong schema đó.
    /// </summary>
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            foreach (var schema in Schemas.All)
            {
                migrationBuilder.EnsureSchema(schema);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var schema in Schemas.All.Reverse())
            {
                migrationBuilder.Sql($"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE;");
            }

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
