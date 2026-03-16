using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concert.Migrations
{
    public partial class AddImageUrlToConcert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "concerts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "concerts");
        }
    }
}