using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamoNET.Database.Migrations
{
    /// <inheritdoc />
    public partial class PropertyRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "DDSDevices",
                newName: "ApiDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApiDeviceId",
                table: "DDSDevices",
                newName: "DeviceId");
        }
    }
}
