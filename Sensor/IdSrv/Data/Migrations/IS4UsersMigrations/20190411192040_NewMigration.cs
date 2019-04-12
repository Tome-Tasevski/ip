using Microsoft.EntityFrameworkCore.Migrations;

namespace IdSrv.Data.Migrations.IS4UsersMigrations
{
    public partial class NewMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_UserClaims_UserClaimsId",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserClaims_UserClaimsId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserClaimsId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Role_UserClaimsId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "UserClaimsId",
                table: "Role");

            migrationBuilder.RenameColumn(
                name: "UserClaimsId",
                table: "Users",
                newName: "Provider");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalUser",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternalUser",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Provider",
                table: "Users",
                newName: "UserClaimsId");

            migrationBuilder.AlterColumn<string>(
                name: "UserClaimsId",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserClaimsId",
                table: "Role",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    UserClaimsId = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.UserClaimsId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserClaimsId",
                table: "Users",
                column: "UserClaimsId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_UserClaimsId",
                table: "Role",
                column: "UserClaimsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_UserClaims_UserClaimsId",
                table: "Role",
                column: "UserClaimsId",
                principalTable: "UserClaims",
                principalColumn: "UserClaimsId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserClaims_UserClaimsId",
                table: "Users",
                column: "UserClaimsId",
                principalTable: "UserClaims",
                principalColumn: "UserClaimsId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
