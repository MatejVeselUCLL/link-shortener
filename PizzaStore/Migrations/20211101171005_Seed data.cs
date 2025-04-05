using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaStore.Migrations
{
    public partial class Seeddata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Pizzas",
                columns: new[] { "Id", "Description", "Name" , "AccessedTimes", "CreatedOn"},
                values: new object[] { 1, "Pepperoni Pizza", "Pepperoni", 0, DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt")});
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Pizzas",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
