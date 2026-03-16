using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concert.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "venues",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "instagram_url",
                table: "members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_url",
                table: "groups",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "customers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "concerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "venues");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "members");

            migrationBuilder.DropColumn(
                name: "instagram_url",
                table: "members");

            migrationBuilder.DropColumn(
                name: "video_url",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "email",
                table: "customers");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "concerts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
