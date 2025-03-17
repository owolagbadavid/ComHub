using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs
{
    public class TestHub : Hub<IChatClient>
    {
        [HubMethodName("SendMessage")]
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        //the server can request a result from a client.
        public async Task<string> WaitForMessage(string connectionId)
        {
            var message = await Clients.Client(connectionId).GetMessage();
            return message;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                // Log error
                // user disconnected due to error
            }
            await base.OnDisconnectedAsync(exception);
        }

        // RemoveFromGroupAsync doesn't need to be called in
        // OnDisconnectedAsync, it's automatically handled for you.

        public Task ThrowException() =>
            throw new HubException("This error will be sent to the client!");

        // users and groups

        public Task SendPrivateMessage(string user, string message)
        {
            return Clients.User(user).ReceiveMessage(user, message);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients
                .Group(groupName)
                .ReceiveMessage(
                    "Send",
                    $"{Context.ConnectionId} has joined the group {groupName}."
                );
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients
                .Group(groupName)
                .ReceiveMessage("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);

    Task<string> GetMessage();
}
