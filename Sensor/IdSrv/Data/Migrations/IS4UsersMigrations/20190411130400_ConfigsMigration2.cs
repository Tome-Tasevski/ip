using Microsoft.EntityFrameworkCore.Migrations;

namespace IdSrv.Data.Migrations.IS4UsersMigrations
{
    public partial class ConfigsMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenIDConfigs",
                columns: table => new
                {
                    OpenId = table.Column<string>(nullable: false),
                    SignInScheme = table.Column<string>(nullable: true),
                    SignOutScheme = table.Column<string>(nullable: true),
                    Authority = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    GetClaimsFromUserInfoEndpoint = table.Column<string>(nullable: true),
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
                    Licensee = table.Column<string>(nullable: true),
                    LicenseKey = table.Column<string>(nullable: true),
                    IdpEntityId = table.Column<string>(nullable: true),
                    IdpSigningCertificate = table.Column<string>(nullable: true),
                    SingleSignOnEndpoint = table.Column<string>(nullable: true),
                    SingleLogoutEndpoint = table.Column<string>(nullable: true),
                    SpEntityId = table.Column<string>(nullable: true),
                    MetadataPath = table.Column<string>(nullable: true),
                    SignAuthenticationRequests = table.Column<string>(nullable: true),
                    SpSigningCertificate = table.Column<string>(nullable: true),
                    SaveTokens = table.Column<bool>(nullable: false),
                    NameIdClaimType = table.Column<string>(nullable: true),
                    SignInScheme = table.Column<string>(nullable: true),
                    TimeComparisonTolerance = table.Column<int>(nullable: false),
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
                name: "IX_OpenIDConfigs_TenantId",
                table: "OpenIDConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlConfigs_TenantId",
                table: "SamlConfigs",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenIDConfigs");

            migrationBuilder.DropTable(
                name: "SamlConfigs");
        }
    }
}
