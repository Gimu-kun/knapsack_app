using knapsack_app.Models;
using Microsoft.EntityFrameworkCore;

public class SessionService
{
    private readonly AppDbContext _context;

    public SessionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserSessionLogModel?> SetLogin(string userId)
    {
        var activeSession = await _context.UserSessionLog
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LogoutTime == null);

        if (activeSession != null)
        {
            return activeSession;
        }

        var newSession = new UserSessionLogModel
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            LoginTime = DateTime.Now,
            LogoutTime = null
        };

        _context.UserSessionLog.Add(newSession);
        await _context.SaveChangesAsync();

        return newSession;
    }

    public async Task<bool> SetLogout(string userId)
    {
        var activeSession = await _context.UserSessionLog
            .Where(x => x.UserId == userId && x.LogoutTime == null)
            .OrderByDescending(x => x.LoginTime)
            .FirstOrDefaultAsync();

        if (activeSession == null)
            return false;

        activeSession.LogoutTime = DateTime.Now;
        _context.UserSessionLog.Update(activeSession);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<UserSessionLogModel>> GetSessionHistory(string userId)
    {
        return await _context.UserSessionLog
             .Where(x => x.UserId == userId)
             .OrderByDescending(x => x.LoginTime)
             .ToListAsync();
    }
}
