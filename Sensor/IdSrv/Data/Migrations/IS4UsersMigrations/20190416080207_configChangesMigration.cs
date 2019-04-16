using Microsoft.EntityFrameworkCore.Migrations;

namespace IdSrv.Data.Migrations.IS4UsersMigrations
{
    public partial class configChangesMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignInScheme",
                table: "OpenIDConfigs");

            migrationBuilder.DropColumn(
                name: "SignOutScheme",
                table: "OpenIDConfigs");

            migrationBuilder.DropColumn(
                name: "GetClaimsFromUserInfoEndpoint",
                table: "OpenIDConfigs");

            migrationBuilder.DropColumn(
                name: "LicenseKey",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "Licensee",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "MetadataPath",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "NameIdClaimType",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "SaveTokens",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "SignAuthenticationRequests",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "SignInScheme",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "SpEntityId",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "SpSigningCertificate",
                table: "SamlConfigs");

            migrationBuilder.DropColumn(
                name: "TimeComparisonTolerance",
                table: "SamlConfigs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignInScheme",
                table: "OpenIDConfigs");

            migrationBuilder.AddColumn<string>(
                name: "SignOutScheme",
                table: "OpenIDConfigs");

            migrationBuilder.AddColumn<string>(
                name: "GetClaimsFromUserInfoEndpoint",
                table: "OpenIDConfigs");

            migrationBuilder.AddColumn<string>(
                name: "LicenseKey",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Licensee",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataPath",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameIdClaimType",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaveTokens",
                table: "SamlConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignAuthenticationRequests",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignInScheme",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpEntityId",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpSigningCertificate",
                table: "SamlConfigs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeComparisonTolerance",
                table: "SamlConfigs",
                nullable: false,
                defaultValue: 0);
        }
    }
}
