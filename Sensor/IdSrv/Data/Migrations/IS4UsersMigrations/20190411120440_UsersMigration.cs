using Microsoft.EntityFrameworkCore.Migrations;

namespace IdSrv.Data.Migrations.IS4UsersMigrations
{
    public partial class UsersMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    LoginType = table.Column<string>(nullable: true),
                    Protocol = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    UserClaimsId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.UserClaimsId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UserClaimsId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Role_UserClaims_UserClaimsId",
                        column: x => x.UserClaimsId,
                        principalTable: "UserClaims",
                        principalColumn: "UserClaimsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    UserClaimsId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_UserClaims_UserClaimsId",
                        column: x => x.UserClaimsId,
                        principalTable: "UserClaims",
                        principalColumn: "UserClaimsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserRoleId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_UserClaimsId",
                table: "Role",
                column: "UserClaimsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserClaimsId",
                table: "Users",
                column: "UserClaimsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "UserClaims");
        }
    }
}
