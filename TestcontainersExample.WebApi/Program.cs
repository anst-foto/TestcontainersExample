using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TestcontainersExample.DataBaseLib;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataBaseContext>(options => options.UseLazyLoadingProxies()
    .UseNpgsql(connectionString));

builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataBaseContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var detail = app.Environment.IsDevelopment()
            ? exception?.Message
            : "При обработке запроса произошла непредвиденная ошибка.";

        var problem = Results.Problem(
            statusCode: statusCode,
            title: "При обработке запроса произошла ошибка.",
            detail: detail,
            instance: context.Request.Path);

        await problem.ExecuteAsync(context);
    });
});

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

const string apiPrefix = "/api/v1";

var apiAuthors = app.MapGroup($"{apiPrefix}/authors");
apiAuthors.MapGet("/", async (DataBaseContext context) =>
{
    var authors = await context.Authors.ToListAsync();
    return authors.Count  > 0 
        ? Results.Ok(authors) 
        : Results.NotFound();
});
apiAuthors.MapGet("/{id:guid}", async (Guid id, DataBaseContext context) =>
{
    var author = await context.Authors.SingleOrDefaultAsync(a => a.Id == id);
    return author != null 
        ? Results.Ok(author) 
        : Results.NotFound();
});
apiAuthors.MapPost("/", async (DataBaseContext context, Author author) => 
{
    try
    {
        await context.Authors.AddAsync(author);
        await context.SaveChangesAsync();
        
        return Results.Ok(author);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

var apiBooks = app.MapGroup($"{apiPrefix}/books");
apiBooks.MapGet("/", async (DataBaseContext context) =>
{
    var books = await context.Books.ToListAsync();
    return books.Count > 0
        ? Results.Ok(books)
        : Results.NotFound();
});
apiBooks.MapGet("/{id:guid}", async (Guid id, DataBaseContext context) =>
{
    var book = await context.Books.SingleOrDefaultAsync(b => b.Id == id);
    return book != null
        ? Results.Ok(book)
        : Results.NotFound();
});
apiBooks.MapPost("/", async (DataBaseContext context, Book book) =>
{
    try
    {
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        return Results.Ok(book);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.Run();