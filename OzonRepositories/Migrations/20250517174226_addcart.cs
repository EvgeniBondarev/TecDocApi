using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OzonRepositories.Migrations
{
    /// <inheritdoc />
    public partial class addcart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartStatuses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStatuses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Provider = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comment = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientOrderNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CartStatusId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCarts_CartStatuses_CartStatusId",
                        column: x => x.CartStatusId,
                        principalTable: "CartStatuses",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "OrderItems",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        ItemId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Quantity = table.Column<int>(type: "int", nullable: false),
                        StudioOrderId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Comment = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        ItemStatusId = table.Column<int>(type: "int", nullable: true),
                        OrderCartId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_OrderItems", x => x.Id);
                        table.ForeignKey(
                            name: "FK_OrderItems_ItemStatuses_ItemStatusId",
                            column: x => x.ItemStatusId,
                            principalTable: "ItemStatuses",
                            principalColumn: "Id");
                        table.ForeignKey(
                            name: "FK_OrderItems_OrderCarts_OrderCartId",
                            column: x => x.OrderCartId,
                            principalTable: "OrderCarts",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.CreateIndex(
                name: "IX_OrderCarts_CartStatusId",
                table: "OrderCarts",
                column: "CartStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ItemStatusId",
                table: "OrderItems",
                column: "ItemStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderCartId",
                table: "OrderItems",
                column: "OrderCartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ItemStatuses");

            migrationBuilder.DropTable(
                name: "OrderCarts");

            migrationBuilder.DropTable(
                name: "CartStatuses");
        }

    }
}
