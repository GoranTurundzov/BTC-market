using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BtcEurMarketDepth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcquiredAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_order_book_snapshots_AcquiredAt",
                table: "order_book_snapshots",
                column: "AcquiredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_book_snapshots_AcquiredAt",
                table: "order_book_snapshots");
        }
    }
}
