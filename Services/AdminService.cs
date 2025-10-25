using knapsack_app.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class AdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> CreateAccount(AdminCreationReqDto request)
    {
        var existedAcc = _context.Admin.FirstOrDefault(acc => acc.Account == request.Account);
        if (existedAcc != null)
        {
            return (false, "Tài khoản đã tồn tại");
        }

        string avatarStr = await FileHelper.ConvertToBase64Async(request.Avatar);

        var newAcc = new AdminModel(request.Account, request.Passwords, "system", avatarStr);

        _context.Admin.Add(newAcc);
        await _context.SaveChangesAsync();
        return (true, "Tạo tài khoản thành công");
    }

    public async Task<List<AdminModel>> GetAllAdmins()
    {
        return _context.Admin.ToList();
    }

    public async Task<AdminModel> GetAdminById(string id)
    {
        return _context.Admin.FirstOrDefault(a => a.Id == id);
    }

    public async Task<(bool Success, string Message)> UpdateAdmin(string id, AdminUpdateReqDto request)
    {
        var admin = _context.Admin.FirstOrDefault(a => a.Id == id);
        if (admin == null)
        {
            return (false, "Admin không tồn tại");
        }

        admin.Account = request.Account ?? admin.Account;
        admin.Passwords = request.Passwords ?? admin.Passwords;
        admin.Avatar = request.Avatar ?? admin.Avatar;
        admin.Role = request.Role ?? admin.Role;
        admin.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return (true, "Cập nhật thành công");
    }

    public async Task<(bool Success, string Message)> DeleteAdmin(string id)
    {
        var admin = _context.Admin.FirstOrDefault(a => a.Id == id);
        if (admin == null)
        {
            return (false, "Admin không tồn tại");
        }

        _context.Admin.Remove(admin);
        await _context.SaveChangesAsync();
        return (true, "Xóa thành công");
    }

    public async Task<(bool Success, string Message, AdminModel Admin)> Login(LoginRequest request)
    {
        var admin = _context.Admin.FirstOrDefault(a => a.Account == request.Account && a.Passwords == request.Passwords);
        if (admin == null)
        {
            return (false, "Sai tài khoản hoặc mật khẩu", null);
        }
        return (true, "Đăng nhập thành công", admin);
    }
}