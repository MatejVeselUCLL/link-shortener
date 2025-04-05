using Microsoft.EntityFrameworkCore;
using PizzaStore.Data;
using Microsoft.OpenApi.Models;
using PizzaStore.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("pizzas") ?? "Data Source=pizzas.db";
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pizzas API", Description = "Pizza pizza", Version = "v1" });
});

// 1) define a unique string
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 2) define allowed domains, in this case "http://example.com" and "*" = all
//    domains, for testing purposes only.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins(
            "http://example.com", "*");
      });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pizza API V1");
});

app.UseRateLimiter();

// 3) use the capability
app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello World!");

app.MapGet("/links", async (PizzaDb db) => await db.Pizzas.ToListAsync());

app.MapGet("/links/{id}", async (PizzaDb db,  int id) => await db.Pizzas.FindAsync(id));

// app.MapGet("/links/{name}", async (PizzaDb db,  string name) => await db.Pizzas.FindAsync(name));

app.MapPost("/links", async (PizzaDb db, Pizza pizza) =>
{
  // # SERVICE


  // ## save
  await db.Pizzas.AddAsync(pizza);
  await db.SaveChangesAsync();

  // ## get
  Pizza createdPizza = await db.Pizzas.Where(x => x.Id == pizza.Id).FirstAsync();
  createdPizza.Id = createdPizza.Id + 1;
  createdPizza.Description = "http://localhost:5100/links/" + createdPizza.Id;

  // ## save
  await db.Pizzas.AddAsync(createdPizza);
  await db.SaveChangesAsync();

  
  return Results.Created($"/links/{pizza.Id}", pizza);
  // return Results.Created($"/links/{pizza.Id}", "http://localhost:5100/" + pizza.Id);
});

app.MapPut("/links/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
  var pizzaItem = await db.Pizzas.FindAsync(id);
  if (pizzaItem is null) return Results.NotFound();
  pizzaItem.Name = updatePizza.Name;
  pizzaItem.Description = updatePizza.Description;
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.MapDelete("/links/{id}", async (PizzaDb db, int id) =>
{
  var todo = await db.Pizzas.FindAsync(id);
  if (todo is null)
  {
    return Results.NotFound();
  }
  db.Pizzas.Remove(todo);
  await db.SaveChangesAsync();
  return Results.Ok();
});
app.Run();
