// /Hubs/RoomHub.cs

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

// Định nghĩa trạng thái Player (giữ nguyên hoặc thêm Score)
public class Player
{
    public string ConnectionId { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public bool IsHost { get; set; }
    public int Score { get; set; } = 0; // Giả định
}

// ✅ THÊM: Định nghĩa trạng thái Phòng (để lưu độ khó)
public class RoomState
{
    public List<Player> Players { get; set; } = new List<Player>();
    public string Difficulty { get; set; } = "easy"; // Mặc định là Dễ
}

// Sử dụng ConcurrentDictionary để lưu trữ trạng thái các phòng
public class GameHub : Hub
{
    // Key: RoomId (string), Value: RoomState
    private static ConcurrentDictionary<string, RoomState> Rooms = new ConcurrentDictionary<string, RoomState>();

    // 1. JoinRoom: Cập nhật để khởi tạo RoomState và gửi độ khó
    public async Task JoinRoom(string roomId, string userId, string username, string avatar)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        var roomState = Rooms.GetOrAdd(roomId, new RoomState());
        var room = roomState.Players;

        var existingPlayer = room.FirstOrDefault(p => p.UserId == userId);
        bool isHost = false;

        if (existingPlayer != null)
        {
            existingPlayer.ConnectionId = Context.ConnectionId; // Cập nhật ConnectionId
            isHost = existingPlayer.IsHost;
        }
        else
        {
            isHost = room.Count == 0;
            var newPlayer = new Player 
            { 
                ConnectionId = Context.ConnectionId, 
                UserId = userId, 
                Username = username, 
                Avatar = avatar,
                IsHost = isHost 
            };
            room.Add(newPlayer);
        }

        // ✅ Gửi thông báo độ khó hiện tại ngay khi Join
        await Clients.Caller.SendAsync("DifficultyUpdated", roomState.Difficulty);
        
        await Clients.Caller.SendAsync("RoleAssigned", isHost);
        await Clients.Group(roomId).SendAsync("PlayersUpdated", room);
    }

    // ✅ 2. SetDifficulty: Host gọi để cập nhật độ khó
    public async Task SetDifficulty(string roomId, string difficulty)
    {
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var player = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            
            // Chỉ Host mới được phép thay đổi
            if (player != null && player.IsHost)
            {
                roomState.Difficulty = difficulty;
                // Thông báo cho TẤT CẢ mọi người trong phòng
                await Clients.Group(roomId).SendAsync("DifficultyUpdated", difficulty);
            }
        }
    }

    // ✅ 3. StartGame: Host gọi để bắt đầu game
    public async Task StartGame(string roomId, string difficulty)
    {
         if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var player = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            
            // Chỉ Host mới được phép bắt đầu
            if (player != null && player.IsHost)
            {
                // Logic chuẩn bị trò chơi (tạo challenge, v.v...)
                
                // Thông báo cho TẤT CẢ mọi người trong phòng và chuyển hướng
                await Clients.Group(roomId).SendAsync("GameStarted", roomId, difficulty);
                
                // (Tùy chọn) Xóa phòng sau khi game bắt đầu nếu cần
                // Rooms.TryRemove(roomId, out _);
            }
        }
    }

    // 4. OnDisconnectedAsync: Xử lý khi người chơi rời đi/mất kết nối
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string connectionId = Context.ConnectionId;
        string roomIdToRemove = null;

        // Tìm phòng mà người chơi đã tham gia
        foreach (var pair in Rooms)
        {
            var roomState = pair.Value;
            var playerToRemove = roomState.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

            if (playerToRemove != null)
            {
                roomState.Players.Remove(playerToRemove);

                if (roomState.Players.Count == 0)
                {
                    // Nếu phòng trống, đánh dấu để xóa
                    roomIdToRemove = pair.Key;
                }
                else if (playerToRemove.IsHost)
                {
                    // Nếu Host rời đi, chỉ định người đầu tiên còn lại làm Host mới
                    var newHost = roomState.Players.First();
                    newHost.IsHost = true;
                    // Thông báo vai trò mới
                    await Clients.Client(newHost.ConnectionId).SendAsync("RoleAssigned", true);
                }

                // Gửi cập nhật danh sách người chơi
                await Clients.Group(pair.Key).SendAsync("PlayersUpdated", roomState.Players);
                break;
            }
        }
        
        if (roomIdToRemove != null)
        {
            Rooms.TryRemove(roomIdToRemove, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }
}