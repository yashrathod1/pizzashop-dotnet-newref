namespace pizzashop_repository.ViewModels;

public class PaginationViewModel
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public string? SearchTerm { get; set; }

}


public class UserPaginationViewModel : PaginationViewModel
{
    public string SortBy { get; set; } = null!;

    public string SortOrder { get; set; } = null!;
}

public class MenuItemPaginationViewModel : PaginationViewModel
{
    public int CategoryId { get; set; }
}

public class ModifierPaginationViewModel : PaginationViewModel
{
    public int ModifierGroupId { get; set; }
}

public class TablePaginationViewModel : PaginationViewModel
{
    public int SectionId { get; set; }
}

public class OrderPaginationViewModel : PaginationViewModel
{
    public string SortBy { get; set; } = null!;

    public string SortOrder { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string DateRange { get; set; } = null!;

    public string? FromDate { get; set; }

    public string? ToDate { get; set; }

}

public class CustomerPaginationViewModel : PaginationViewModel
{
    public string SortBy { get; set; } = null!;

    public string SortOrder { get; set; } = null!;

    public string DateRange { get; set; } = null!;

    public DateTime? CustomStartDate { get; set; }

    public DateTime? CustomEndDate { get; set; }
}
