using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610190001_AddEventCoordinates")]
public sealed class AddEventCoordinates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Location",
            table: "Events",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<double>(
            name: "Latitude",
            table: "Events",
            type: "double precision",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "Longitude",
            table: "Events",
            type: "double precision",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Location", table: "Events");
        migrationBuilder.DropColumn(name: "Latitude", table: "Events");
        migrationBuilder.DropColumn(name: "Longitude", table: "Events");
    }
}
