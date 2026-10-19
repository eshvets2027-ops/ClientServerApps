using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610190002_AddOrganizerToEvent")]
public sealed class AddOrganizerToEvent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Organizer",
            table: "Events",
            type: "character varying(120)",
            maxLength: 120,
            nullable: false,
            defaultValue: "Організатор не вказаний");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Organizer", table: "Events");
    }
}
