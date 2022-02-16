using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TalkToApi.Migrations
{
    public partial class MessageDeletedUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {                       
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Message",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateAt",
                table: "Message",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {           
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "UpdateAt",
                table: "Message");
            
        }
    }
}
