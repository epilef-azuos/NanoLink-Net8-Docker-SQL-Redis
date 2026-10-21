using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NanoLink.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClickEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "ClickEvents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "ClickEvents",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "ClickEvents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "ClickEvents",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "ClickEvents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferrerDomain",
                table: "ClickEvents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityName",
                table: "ClickEvents");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "ClickEvents");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "ClickEvents");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "ClickEvents");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "ClickEvents");

            migrationBuilder.DropColumn(
                name: "ReferrerDomain",
                table: "ClickEvents");
        }
    }
}
