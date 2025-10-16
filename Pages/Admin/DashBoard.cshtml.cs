using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace knapsack_app.Pages.Dashboard
{
    // Đặt tên lớp theo cấu trúc Dashboard/Index -> IndexModel
    public class DashBoardModel : PageModel
    {
        // ----------------------------------------------------
        // 1. Thuộc tính để lưu trữ dữ liệu hiển thị trên Dashboard
        // ----------------------------------------------------

        // Thuộc tính để lưu trữ dữ liệu Card (Ví dụ: Doanh thu)
        public DashboardMetrics Metrics { get; set; } = new DashboardMetrics();

        // Thuộc tính để lưu trữ danh sách các Đơn hàng gần đây
        public List<Order> RecentOrders { get; set; } = new List<Order>();

        // ----------------------------------------------------
        // 2. Phương thức xử lý logic (OnGet)
        // ----------------------------------------------------
        
        // Phương thức này được gọi khi trang được tải lần đầu (HTTP GET)
        public void OnGet()
        {
            // TẠM THỜI: Khởi tạo dữ liệu giả lập để hiển thị
            LoadFakeData();
        }

        private void LoadFakeData()
        {
            // Dữ liệu cho các Card Metrics (giá trị cứng)
            Metrics = new DashboardMetrics
            {
                TotalRevenue = 12099m,
                AffiliateRevenue = 12099m,
                Refunds = 0.00m,
                AvgRevenuePerUser = 28000m
            };

            // Dữ liệu cho Bảng Đơn hàng Gần đây
            RecentOrders = new List<Order>
            {
                new Order { Id = 1, Product = "Product #1", ProductId = "id0000001", Quantity = 20, Price = 80.00m, Time = "27-08-2019 01:22:12", Customer = "Patricia J. King", Status = "In Transit" },
                new Order { Id = 2, Product = "Product #2", ProductId = "id0000002", Quantity = 12, Price = 180.00m, Time = "25-08-2019 21:12:56", Customer = "Rachel J. Wicker", Status = "Delivered" },
                new Order { Id = 3, Product = "Product #3", ProductId = "id0000003", Quantity = 23, Price = 820.00m, Time = "24-08-2019 14:12:37", Customer = "Michael K. Ledford", Status = "Delivered" },
                new Order { Id = 4, Product = "Product #4", ProductId = "id0000004", Quantity = 34, Price = 340.00m, Time = "23-08-2019 09:12:35", Customer = "Michael K. Ledford", Status = "Delivered" }
            };
            
            // THỰC TẾ: Ở đây bạn sẽ gọi Service/Repository để lấy dữ liệu
            // Ví dụ: RecentOrders = _orderService.GetRecentOrders(10);
        }
    }

    // ----------------------------------------------------
    // 3. Các lớp DTO/ViewModel để ánh xạ dữ liệu
    // ----------------------------------------------------

    // ViewModel cho các số liệu thống kê (Metrics Cards)
    public class DashboardMetrics
    {
        public decimal TotalRevenue { get; set; }
        public decimal AffiliateRevenue { get; set; }
        public decimal Refunds { get; set; }
        public decimal AvgRevenuePerUser { get; set; }
    }

    // ViewModel cho mỗi đơn hàng
    public class Order
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Time { get; set; }
        public string Customer { get; set; }
        public string Status { get; set; }
        // Thêm ImageUrl nếu bạn có hình ảnh sản phẩm
        // public string ImageUrl { get; set; } 
    }
}