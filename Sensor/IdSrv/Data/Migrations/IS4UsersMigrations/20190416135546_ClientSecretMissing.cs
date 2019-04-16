using Microsoft.EntityFrameworkCore.Migrations;

namespace IdSrv.Data.Migrations.IS4UsersMigrations
{
    public partial class ClientSecretMissing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "OpenIDConfigs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
