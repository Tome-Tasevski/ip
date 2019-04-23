using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServerAspNetIdentity.Data.Migrations.Users
{
    public partial class ConfigsDbMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUsers",
                nullable: true);

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
                name: "OpenIDConfigs",
                columns: table => new
                {
                    OpenId = table.Column<string>(nullable: false),
                    Authority = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIDConfigs", x => x.OpenId);
                    table.ForeignKey(
                        name: "FK_OpenIDConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SamlConfigs",
                columns: table => new
                {
                    SamlId = table.Column<string>(nullable: false),
                    IdpEntityId = table.Column<string>(nullable: true),
                    IdpSigningCertificate = table.Column<string>(nullable: true),
                    SingleSignOnEndpoint = table.Column<string>(nullable: true),
                    SingleLogoutEndpoint = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlConfigs", x => x.SamlId);
                    table.ForeignKey(
                        name: "FK_SamlConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIDConfigs_TenantId",
                table: "OpenIDConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlConfigs_TenantId",
                table: "SamlConfigs",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OpenIDConfigs");

            migrationBuilder.DropTable(
                name: "SamlConfigs");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");
        }
    }
}
