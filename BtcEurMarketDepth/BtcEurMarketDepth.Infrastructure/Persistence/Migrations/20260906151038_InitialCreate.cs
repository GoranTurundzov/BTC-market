using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BtcEurMarketDepth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_book_snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    AcquiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BestBid = table.Column<decimal>(type: "decimal(20,8)", precision: 20, scale: 8, nullable: true),
                    BestAsk = table.Column<decimal>(type: "decimal(20,8)", precision: 20, scale: 8, nullable: true),
                    BidsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AsksJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_book_snapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_book_snapshots_Symbol_AcquiredAt",
                table: "order_book_snapshots",
                columns: new[] { "Symbol", "AcquiredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_book_snapshots");
        }
    }
}
