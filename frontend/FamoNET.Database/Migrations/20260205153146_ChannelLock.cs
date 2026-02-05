using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamoNET.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChannelLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DDSChannel_DDSDevices_DeviceId",
                table: "DDSChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DDSChannel",
                table: "DDSChannel");

            migrationBuilder.RenameTable(
                name: "DDSChannel",
                newName: "DDSChannels");

            migrationBuilder.RenameIndex(
                name: "IX_DDSChannel_DeviceId",
                table: "DDSChannels",
                newName: "IX_DDSChannels_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DDSChannels",
                table: "DDSChannels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DDSChannels_DDSDevices_DeviceId",
                table: "DDSChannels",
                column: "DeviceId",
                principalTable: "DDSDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DDSChannels_DDSDevices_DeviceId",
                table: "DDSChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DDSChannels",
                table: "DDSChannels");

            migrationBuilder.RenameTable(
                name: "DDSChannels",
                newName: "DDSChannel");

            migrationBuilder.RenameIndex(
                name: "IX_DDSChannels_DeviceId",
                table: "DDSChannel",
                newName: "IX_DDSChannel_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DDSChannel",
                table: "DDSChannel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DDSChannel_DDSDevices_DeviceId",
                table: "DDSChannel",
                column: "DeviceId",
                principalTable: "DDSDevices",
                principalColumn: "Id");
        }
    }
}
