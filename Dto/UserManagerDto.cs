namespace knapsack_app.ViewModels
{
    public class UserManagementDto
    {
        public string Id { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public bool Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public int TotalChallengesTaken { get; set; }
        public int TotalLogins { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class UserQueryRequestDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int TotalChallengesTaken { get; set; }
        public int TotalLogins { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}