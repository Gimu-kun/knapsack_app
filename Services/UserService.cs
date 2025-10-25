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

    public async Task<(bool Success, string Message)> CreateUser(UserCreationReqDto request)
    {
        var existedAcc = _context.User.FirstOrDefault(u => u.Account == request.Account);
        if (existedAcc != null)
            return (false, "Tài khoản đã tồn tại");

        string? avatarPath = null;
        if (request.Avatar != null)
        {
            string fileExtension = Path.GetExtension(request.Avatar.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

            string uploadFolder = Path.Combine(_env.WebRootPath, "avatars");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.Avatar.CopyToAsync(fileStream);
            }

            avatarPath = "/avatars/" + uniqueFileName;
        }

        var newUser = new UserModel(request.Account, _passwordService.hashPassword(request.Passwords), avatarPath);

        _context.User.Add(newUser);
        await _context.SaveChangesAsync();
        return (true, "Tạo tài khoản thành công");
    }

    public async Task<PaginatedResponse<UserManagementDto>> GetPaginatedUsersAsync(UserQueryRequestDto query)
    {
        var usersQuery = _context.User.AsQueryable();

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            usersQuery = usersQuery.Where(u =>
                u.Account.Contains(query.SearchTerm) ||
                u.Id.Contains(query.SearchTerm));
        }

        var totalItems = await usersQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

        query.PageIndex = Math.Max(1, Math.Min(query.PageIndex, totalPages > 0 ? totalPages : 1));

        var pagedUsers = usersQuery
            .OrderBy(u => u.Account)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize);

        var userIds = await pagedUsers.Select(u => u.Id).ToListAsync();

        var takenCounts = await _context.Taken
            .Where(t => userIds.Contains(t.UserId))
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var loginCounts = await _context.UserSessionLog
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);


        var usersDto = await pagedUsers.Select(u => new UserManagementDto
        {
            Id = u.Id,
            Account = u.Account,
            Status = u.Status,
            CreatedAt = u.CreatedAt,
            LastLogin = u.LastLogin,

            TotalChallengesTaken = 0,
            TotalLogins = 0
        }).ToListAsync();

        foreach (var dto in usersDto)
        {
            dto.TotalChallengesTaken = takenCounts.GetValueOrDefault(dto.Id, 0);
            dto.TotalLogins = loginCounts.GetValueOrDefault(dto.Id, 0);
        }

        return new PaginatedResponse<UserManagementDto>
        {
            Data = usersDto,
            TotalItems = totalItems,
            TotalPages = totalPages,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<UserModel> GetUserById(string id)
    {
        return _context.User.FirstOrDefault(u => u.Id == id);
    }

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

        return new AuthResult { Success = true, Message = "Đăng nhập thành công.", Token = _jwtService.GenerateToken(user.Id, user.Account, false), UserId = user.Id };
    }
    
    public async Task<(bool Success, string Message, bool NewStatus)> ToggleUserStatus(string id)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return (false, "User không tồn tại.", false);
        }

        bool newStatus = !user.Status;
        
        user.Status = newStatus;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        string statusMessage = newStatus ? "kích hoạt" : "hủy kích hoạt";
        return (true, $"Cập nhật trạng thái thành công. Tài khoản đã được {statusMessage}.", newStatus);
    }
}