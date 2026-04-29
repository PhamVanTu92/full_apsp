using Gridify;

namespace ePortal.Models.Common;

public class ParamQuery: IGridifyQuery
{
    public ParamQuery()
    {
    }
    public ParamQuery(int page, int pageSize, string filter, string? orderBy = null)
    {
        Page = page;
        PageSize = pageSize;
        OrderBy = orderBy;
        Filter = filter;
    }

    /// <summary>
    /// Trang cần lấy
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Số lượng kết quả trong một trang
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Sắp xếp theo
    /// </summary>
    public string? OrderBy { get; set; }
    /// <summary>
    /// Bộ lọc theo tên trường của thực thể
    /// </summary>
    public string? Filter { get; set; }
}