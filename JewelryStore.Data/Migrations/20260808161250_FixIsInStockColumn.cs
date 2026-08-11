using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelryStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixIsInStockColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ 1️⃣ حذف ایندکس‌های وابسته
            migrationBuilder.Sql(@"
        -- حذف ایندکس‌هایی که به ستون IsInStock وابسته هستند
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Active_Stock_Featured' AND object_id = OBJECT_ID('Products'))
        BEGIN
            DROP INDEX [IX_Products_Active_Stock_Featured] ON [Products];
        END

        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsInStock' AND object_id = OBJECT_ID('Products'))
        BEGIN
            DROP INDEX [IX_Products_IsInStock] ON [Products];
        END
    ");

            // ✅ 2️⃣ حذف ستون محاسباتی
            migrationBuilder.Sql(@"
        ALTER TABLE Products DROP COLUMN IsInStock;
    ");

            // ✅ 3️⃣ ایجاد مجدد ستون محاسباتی با نوع داده صحیح
            migrationBuilder.Sql(@"
        ALTER TABLE Products ADD IsInStock AS (CASE WHEN Quantity > 0 THEN 1 ELSE 0 END) PERSISTED;
    ");

            // ✅ 4️⃣ ایجاد مجدد ایندکس‌ها (اختیاری)
            migrationBuilder.Sql(@"
        -- ایجاد مجدد ایندکس‌ها (اگر نیاز دارید)
        CREATE INDEX [IX_Products_Active_Stock_Featured] ON [Products] ([IsActive], [IsInStock], [IsFeatured]);
        CREATE INDEX [IX_Products_IsInStock] ON [Products] ([IsInStock]);
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // برگرداندن به حالت قبل (اگر نیاز باشد)
            migrationBuilder.Sql(@"
        DROP INDEX [IX_Products_Active_Stock_Featured] ON [Products];
        DROP INDEX [IX_Products_IsInStock] ON [Products];
        ALTER TABLE Products DROP COLUMN IsInStock;
        ALTER TABLE Products ADD IsInStock AS (CASE WHEN Quantity > 0 THEN 1 ELSE 0 END) PERSISTED;
        CREATE INDEX [IX_Products_Active_Stock_Featured] ON [Products] ([IsActive], [IsInStock], [IsFeatured]);
        CREATE INDEX [IX_Products_IsInStock] ON [Products] ([IsInStock]);
    ");
        }
    }
}
