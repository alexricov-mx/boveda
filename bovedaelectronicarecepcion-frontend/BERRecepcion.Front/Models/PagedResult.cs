using System.Collections.Generic;

namespace BERRecepcion.Front.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> items { get; set; }
    public int totalItems { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int totalPages { get; set; }
    public bool hasPreviousPage { get; set; }
    public bool hasNextPage { get; set; }
}