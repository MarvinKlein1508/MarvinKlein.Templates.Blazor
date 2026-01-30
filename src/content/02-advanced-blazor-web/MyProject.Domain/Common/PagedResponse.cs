namespace MyProject.Domain.Common;

/// <summary>
/// Represents a paged response container for returning a subset of items along with
/// pagination metadata (page number, page size and total item count).
/// </summary>
/// <typeparam name="TResponse">The item type contained in the response.</typeparam>
public sealed class PagedResponse<TResponse>
{
    /// <summary>
    /// The items included in the current page.
    /// </summary>
    /// <value>An <see cref="IEnumerable{TResponse}"/> containing the items for the requested page.</value>
    public required IEnumerable<TResponse> Items { get; init; } = [];

    /// <summary>
    /// The maximum number of items per page (page size).
    /// </summary>
    /// <value>An integer representing the page size used when paging the result set.</value>
    public required int PageSize { get; init; }

    /// <summary>
    /// The one-based page index for the returned items.
    /// </summary>
    /// <value>An integer representing the current page number (1 = first page).</value>
    public required int Page { get; init; }

    /// <summary>
    /// The total number of items across all pages (not just the current page).
    /// </summary>
    /// <value>An integer representing the full result set size.</value>
    public required int Total { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is at least one more page after the current one.
    /// </summary>
    /// <remarks>
    /// Computed as <c>Total &gt; Page * PageSize</c>. For example, if <see cref="Total"/> is 50,
    /// <see cref="PageSize"/> is 10 and <see cref="Page"/> is 5, this property is <c>false</c>.
    /// </remarks>
    public bool HasNextpage => Total > Page * PageSize;
}
