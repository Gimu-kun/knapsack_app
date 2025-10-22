// /Hubs/RoomHub.cs

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

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

    // 1. JoinRoom: ... (giữ nguyên)
    public async Task JoinRoom(string roomId, string userId, string username, string avatar)
    {
        // ... (Logic JoinRoom giữ nguyên)

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

    // 2. SetDifficulty: ... (giữ nguyên)
    public async Task SetDifficulty(string roomId, string difficulty)
    {
        // ... (Logic SetDifficulty giữ nguyên)
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

    // 3. StartGame: ... (giữ nguyên)
    public async Task StartGame(string roomId, string difficulty, string url)
    {
        // ... (Logic StartGame giữ nguyên)
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var player = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            
            // Chỉ Host mới được phép bắt đầu
            if (player != null && player.IsHost)
            {
                // Logic chuẩn bị trò chơi (tạo challenge, v.v...)
                
                // Thông báo cho TẤT CẢ mọi người trong phòng và chuyển hướng
                await Clients.Group(roomId).SendAsync("GameStarted", roomId, difficulty,url);
                
                // (Tùy chọn) Xóa phòng sau khi game bắt đầu nếu cần
                // Rooms.TryRemove(roomId, out _);
            }
        }
    }

    // ✅ 4. KickPlayer: Host gọi để kick người chơi (Sử dụng ConnectionId hoặc UserId để xác định)
    public async Task KickPlayer(string roomId, string userIdToKick)
    {
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var kicker = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            // 1. Kiểm tra xem người gọi có phải là Host không
            if (kicker != null && kicker.IsHost)
            {
                // 2. Tìm người chơi cần kick (dùng UserId vì nó ổn định hơn ConnectionId)
                var playerToKick = roomState.Players.FirstOrDefault(p => p.UserId == userIdToKick);

                if (playerToKick != null && playerToKick.UserId != kicker.UserId) // Không cho Host tự kick mình
                {
                    // 3. Gửi thông báo "bị kick" đến người chơi đó
                    await Clients.Client(playerToKick.ConnectionId).SendAsync("KickedFromRoom");

                    // 4. Xóa người chơi khỏi nhóm và khỏi trạng thái phòng
                    await Groups.RemoveFromGroupAsync(playerToKick.ConnectionId, roomId);
                    roomState.Players.Remove(playerToKick);

                    // 5. Thông báo cập nhật danh sách người chơi cho cả phòng
                    await Clients.Group(roomId).SendAsync("PlayersUpdated", roomState.Players);
                }
            }
        }
    }
    
    // ✅ PHƯƠNG THỨC MỚI: Đồng bộ trạng thái ô DP
    // Khi một người chơi nhập giá trị vào ô [i, w], gửi cập nhật đến người khác.
    public async Task SendCellUpdate(string roomId, int i, int w, int value, bool isCorrect)
    {
        // Gửi cập nhật đến TẤT CẢ người chơi trong phòng (trừ người gửi - OthersInGroup)
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveCellUpdate", i, w, value, isCorrect);
    }

    // ✅ PHƯƠNG THỨC MỚI: Đồng bộ điểm số
    // API Server nên gọi Hub (thông qua IHubContext) để gửi điểm. 
    // Phương thức này cho phép client cập nhật điểm số chung của đội/phòng.
    public async Task SendScoreDeduct(string roomId, int i, int w, int scoreChange)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveScoreDeduct", i, w, scoreChange);
    }
    
    public async Task SendFindAndRevealCell(string roomId)
    {
        await Clients.Group(roomId).SendAsync("ReceiveFindAndRevealCell");
    }

    // ✅ PHƯƠNG THỨC MỚI: Đồng bộ trạng thái bắt đầu trò chơi (Chỉ Host mới gọi)
    public async Task SignalGameStarted(string roomId)
    {
        // Gửi thông báo đến TẤT CẢ mọi người trong phòng
        await Clients.Group(roomId).SendAsync("GameStarted");
    }


    // 5. OnDisconnectedAsync: Xử lý khi người chơi rời đi/mất kết nối (giữ nguyên)
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

    public async Task SendKnapsackItemAdded(string roomId, string itemId)
    {
        await Clients.Group(roomId).SendAsync("ReceiveKnapsackItemAdded", itemId);
    }

    public async Task SendKnapsackItemRemoved(string roomId, Any Element, string itemId, string itemName, int itemValue, int itemWeight)
    {
        await Clients.Group(roomId).SendAsync("ReceiveKnapsackItemRemoved", Element, itemId, itemName, itemValue, itemWeight);
    }
    
    public async Task SendEndGameKnapsack(string roomId,int totalWeight, int totalValue, bool isFinal)
    {
        await Clients.Group(roomId).SendAsync("ReceiveEndGameKnapsack", totalWeight, totalValue, isFinal);
    }
}