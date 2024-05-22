using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookReadingService.Migrations
{
    public partial class addMemidtoAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipId",
                table: "Accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_MembershipId",
                table: "Accounts",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Memberships_MembershipId",
                table: "Accounts",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "MembershipId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Memberships_MembershipId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_MembershipId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "Accounts");
        }
    }
}
