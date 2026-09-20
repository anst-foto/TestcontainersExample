using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestcontainersExample.DataBaseLib.Tests;

public class DataBaseContextTest : IClassFixture<DataBaseFixture>
{
    private readonly DataBaseFixture _fixture;

    public DataBaseContextTest(DataBaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateBook_Test()
    {
        var connectionString = _fixture.ConnectionString;
        await using var context = DataBaseContextFactory.CreateDbContext(connectionString);

        var author1 = new Author { FullName = "Test1" };
        var author2 = new Author { FullName = "Test2" };
        var book = new Book { Title = "Test" };
        book.Authors.Add(author1);
        book.Authors.Add(author2);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        await using var assertContext = DataBaseContextFactory.CreateDbContext(connectionString);
        var savedItem = await assertContext.Books.SingleOrDefaultAsync(i => i.Id == book.Id);
        Assert.NotNull(savedItem);
        Assert.Multiple(
            () => Assert.Equal("Test", savedItem.Title),
            () => Assert.Equal(2, savedItem.Authors.Count));
    }
}