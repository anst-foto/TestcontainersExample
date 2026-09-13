using System;
using System.Collections.Generic;

namespace TestcontainersExample.DataBaseLib;

public class Book
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Title { get; init; }
    public virtual IList<Author> Authors { get; init; } = new List<Author>();
}