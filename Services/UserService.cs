using knapsack_app.Models;
using knapsack_app.ViewModels;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;
    public UserService(AppDbContext context, IWebHostEnvironment env, JwtService jwtService, PasswordService passwordService)
    {
        _context = context;
        _env = env;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    // Tạo user mới - Cập nhật logic xử lý file
    public async Task<(bool Success, string Message)> CreateUser(UserCreationReqDto request)
    {
        var existedAcc = _context.User.FirstOrDefault(u => u.Account == request.Account);
        if (existedAcc != null)
            return (false, "Tài khoản đã tồn tại");

        string? avatarPath = null;
        if (request.Avatar != null)
        {
            // 1. Tạo tên file duy nhất
            string fileExtension = Path.GetExtension(request.Avatar.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

            // 2. Xác định thư mục lưu trữ (ví dụ: wwwroot/avatars)
            string uploadFolder = Path.Combine(_env.WebRootPath, "avatars");

            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // 3. Tạo đường dẫn file đầy đủ
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            // 4. Lưu file vật lý
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.Avatar.CopyToAsync(fileStream);
            }

            // 5. Lưu đường dẫn tương đối (để truy cập qua URL) vào DB
            avatarPath = "/avatars/" + uniqueFileName;
        }

        // Nếu không có file được upload, avatarStr sẽ là null và UserModel sẽ dùng giá trị mặc định.
        // Nếu có file, avatarStr là đường dẫn tương đối.
        var newUser = new UserModel(request.Account, _passwordService.hashPassword(request.Passwords), avatarPath);

        _context.User.Add(newUser);
        await _context.SaveChangesAsync();
        return (true, "Tạo tài khoản thành công");
    }

    // Lấy danh sách tất cả user
    public async Task<PaginatedResponse<UserManagementDto>> GetPaginatedUsersAsync(UserQueryRequestDto query)
    {
        // 1. Áp dụng LỌC (Filtering)
        var usersQuery = _context.User.AsQueryable();

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            usersQuery = usersQuery.Where(u =>
                u.Account.Contains(query.SearchTerm) ||
                u.Id.Contains(query.SearchTerm));
        }

        // 2. Tính TỔNG SỐ DÒNG và TỔNG SỐ TRANG
        var totalItems = await usersQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

        // Đảm bảo PageIndex hợp lệ
        query.PageIndex = Math.Max(1, Math.Min(query.PageIndex, totalPages > 0 ? totalPages : 1));

        // 3. Áp dụng PHÂN TRANG (Paging)
        var pagedUsers = usersQuery
            .OrderBy(u => u.Account) // Sắp xếp mặc định
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize);

        // 4. THỰC HIỆN TRUY VẤN VÀ TỔNG HỢP (Aggregation)
        var userIds = await pagedUsers.Select(u => u.Id).ToListAsync();

        // 4.1. Tổng hợp Total Challenges Taken (COUNT từ bảng Taken)
        var takenCounts = await _context.Taken
            .Where(t => userIds.Contains(t.UserId))
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        // 4.2. Tổng hợp Total Logins (COUNT từ bảng UserSessionLog)
        var loginCounts = await _context.UserSessionLog
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);


        // 5. MAP DỮ LIỆU SANG DTO
        var usersDto = await pagedUsers.Select(u => new UserManagementDto
        {
            Id = u.Id,
            Account = u.Account,
            Status = u.Status, // Giả định UserModel.Status là bool/TINYINT
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin, // Lấy LastLogin từ bảng User

            // Sẽ được điền sau
            TotalChallengesTaken = 0,
            TotalLogins = 0
        }).ToListAsync();

        // Điền dữ liệu tổng hợp
        foreach (var dto in usersDto)
        {
            dto.TotalChallengesTaken = takenCounts.GetValueOrDefault(dto.Id, 0);
            dto.TotalLogins = loginCounts.GetValueOrDefault(dto.Id, 0);
        }

        // 6. Trả về response phân trang
        return new PaginatedResponse<UserManagementDto>
        {
            Data = usersDto,
            TotalItems = totalItems,
            TotalPages = totalPages,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    // Lấy thông tin user theo id
    public async Task<UserModel> GetUserById(string id)
    {
        return _context.User.FirstOrDefault(u => u.Id == id);
    }

    // Cập nhật thông tin user
    public async Task<(bool Success, string Message)> UpdateUser(string id, UserUpdateReqDto request)
    {
        var user = _context.User.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return (false, "User không tồn tại");

        user.Account = request.Account ?? user.Account;
        user.Passwords = request.Passwords ?? user.Passwords;
        user.Avatar = request.Avatar ?? user.Avatar;
        user.Status = request.Status ?? user.Status;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, "Cập nhật thành công");
    }

    // Xóa user
    public async Task<(bool Success, string Message)> DeleteUser(string id)
    {
        var user = _context.User.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return (false, "User không tồn tại");

        _context.User.Remove(user);
        await _context.SaveChangesAsync();
        return (true, "Xóa thành công");
    }

        public async Task<AuthResult> Login(LoginReqDto request)
    {
        var user = _context.User.FirstOrDefault(u => u.Account == request.Account);
            if (user == null)
            {
                return new AuthResult { Success = true, Message = "Tài khoản không tồn tại" };
            }

            if (!_passwordService.verifyPassword(user.Passwords, request.Password))
            {
                return new AuthResult { Success = false, Message = "Mật khẩu không đúng." };
            }
            
            return new AuthResult { Success = true, Message = "Đăng nhập thành công.", Token = _jwtService.GenerateToken(user.Id,user.Account,false) };
        }
}