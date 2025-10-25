

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

public class Player
{
    public string ConnectionId { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public bool IsHost { get; set; }
    public int Score { get; set; } = 0;
}

public class RoomState
{
    public List<Player> Players { get; set; } = new List<Player>();
    public string Difficulty { get; set; } = "easy";
}

public class GameHub : Hub
{
    private static ConcurrentDictionary<string, RoomState> Rooms = new ConcurrentDictionary<string, RoomState>();

    public async Task JoinRoom(string roomId, string userId, string username, string avatar)
    {

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        var roomState = Rooms.GetOrAdd(roomId, new RoomState());
        var room = roomState.Players;

        var existingPlayer = room.FirstOrDefault(p => p.UserId == userId);
        bool isHost = false;

        if (existingPlayer != null)
        {
            existingPlayer.ConnectionId = Context.ConnectionId;
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

        await Clients.Caller.SendAsync("DifficultyUpdated", roomState.Difficulty);
        
        await Clients.Caller.SendAsync("RoleAssigned", isHost);
        await Clients.Group(roomId).SendAsync("PlayersUpdated", room);
    }

    public async Task SetDifficulty(string roomId, string difficulty)
    {
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var player = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            
            if (player != null && player.IsHost)
            {
                roomState.Difficulty = difficulty;
                await Clients.Group(roomId).SendAsync("DifficultyUpdated", difficulty);
            }
        }
    }

    public async Task StartGame(string roomId, string difficulty, string url)
    {
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var player = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            
            if (player != null && player.IsHost)
            {
                await Clients.Group(roomId).SendAsync("GameStarted", roomId, difficulty,url);
                
            }
        }
    }

    public async Task KickPlayer(string roomId, string userIdToKick)
    {
        if (Rooms.TryGetValue(roomId, out var roomState))
        {
            var kicker = roomState.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            if (kicker != null && kicker.IsHost)
            {
                var playerToKick = roomState.Players.FirstOrDefault(p => p.UserId == userIdToKick);

                if (playerToKick != null && playerToKick.UserId != kicker.UserId)
                {
                    await Clients.Client(playerToKick.ConnectionId).SendAsync("KickedFromRoom");

                    await Groups.RemoveFromGroupAsync(playerToKick.ConnectionId, roomId);
                    roomState.Players.Remove(playerToKick);

                    await Clients.Group(roomId).SendAsync("PlayersUpdated", roomState.Players);
                }
            }
        }
    }
    
    public async Task SendCellUpdate(string roomId, int i, int w, int value, bool isCorrect)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveCellUpdate", i, w, value, isCorrect);
    }

    public async Task SendScoreDeduct(string roomId, int i, int w, int scoreChange)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveScoreDeduct", i, w, scoreChange);
    }
    
    public async Task SendFindAndRevealCell(string roomId)
    {
        await Clients.Group(roomId).SendAsync("ReceiveFindAndRevealCell");
    }

    public async Task SignalGameStarted(string roomId)
    {
        await Clients.Group(roomId).SendAsync("GameStarted");
    }


    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string connectionId = Context.ConnectionId;
        string roomIdToRemove = null;

        foreach (var pair in Rooms)
        {
            var roomState = pair.Value;
            var playerToRemove = roomState.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

            if (playerToRemove != null)
            {
                roomState.Players.Remove(playerToRemove);

                if (roomState.Players.Count == 0)
                {
                    roomIdToRemove = pair.Key;
                }
                else if (playerToRemove.IsHost)
                {
                    var newHost = roomState.Players.First();
                    newHost.IsHost = true;
                    await Clients.Client(newHost.ConnectionId).SendAsync("RoleAssigned", true);
                }

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