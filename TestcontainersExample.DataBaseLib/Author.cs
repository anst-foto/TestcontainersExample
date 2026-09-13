using System;
using System.Collections.Generic;

namespace TestcontainersExample.DataBaseLib;

public class Author
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string FullName { get; init; }
    
    public virtual IList<Book> Books { get; init; } = new List<Book>();
}