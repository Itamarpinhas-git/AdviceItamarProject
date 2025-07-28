using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElevatorSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Building",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumberOfFloors = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Building", x => x.id);
                    table.ForeignKey(
                        name: "FK_Building_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Elevator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingID = table.Column<int>(type: "int", nullable: false),
                    CurrentFloor = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoorStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elevator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elevator_Building",
                        column: x => x.BuildingID,
                        principalTable: "Building",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ElevatorCall",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    RequestedFloor = table.Column<int>(type: "int", nullable: false),
                    DestinationFloor = table.Column<int>(type: "int", nullable: true),
                    CallTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorCall", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorCall_Building",
                        column: x => x.BuildingId,
                        principalTable: "Building",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ElevatorCallAssignment",
                columns: table => new
                {
                    CallID = table.Column<int>(type: "int", nullable: false),
                    ElevatorId = table.Column<int>(type: "int", nullable: false),
                    ElevatorCallId = table.Column<int>(type: "int", nullable: false),
                    AssignmentTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorCallAssignment", x => x.CallID);
                    table.ForeignKey(
                        name: "FK_ElevatorCallAssignment_Elevator",
                        column: x => x.ElevatorId,
                        principalTable: "Elevator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElevatorCallAssignment_ElevatorCall",
                        column: x => x.CallID,
                        principalTable: "ElevatorCall",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ElevatorCallAssignment_ElevatorCall_ElevatorCallId",
                        column: x => x.ElevatorCallId,
                        principalTable: "ElevatorCall",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Building_UserId",
                table: "Building",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "FK_Building",
                table: "Elevator",
                column: "BuildingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorCall_BuildingId",
                table: "ElevatorCall",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorCallAssignment_ElevatorCallId",
                table: "ElevatorCallAssignment",
                column: "ElevatorCallId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorCallAssignment_ElevatorId",
                table: "ElevatorCallAssignment",
                column: "ElevatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElevatorCallAssignment");

            migrationBuilder.DropTable(
                name: "Elevator");

            migrationBuilder.DropTable(
                name: "ElevatorCall");

            migrationBuilder.DropTable(
                name: "Building");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
