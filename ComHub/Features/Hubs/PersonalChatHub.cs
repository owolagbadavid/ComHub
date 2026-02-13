using System.Security.Claims;
using Api.Db;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SignalRChat.Hubs;

[Authorize]
public class PersonalChatHub(AppDbContext dbContext) : Hub<IPersonalChatClient>
{
    private readonly AppDbContext _dbContext = dbContext;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var ct = Context.ConnectionAborted;
        var conversationIds = await _dbContext.UsersConversations
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.ConversationId)
            .ToListAsync(ct);

        foreach (var conversationId in conversationIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId), ct);
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(int conversationId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        ct = ResolveToken(ct);
        var isMember = await _dbContext.UsersConversations.AnyAsync(
            uc => uc.ConversationId == conversationId && uc.UserId == userId,
            ct
        );

        if (!isMember)
        {
            throw new HubException("Not a participant in this conversation.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId), ct);
    }

    public async Task<PersonalConversationDto> StartPersonalChat(
        int otherUserId,
        CancellationToken ct = default
    )
    {
        var userId = GetUserId();
        ct = ResolveToken(ct);
        var conversation = await GetOrCreatePersonalConversation(userId, otherUserId, ct);

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversation.Id), ct);

        var dto = new PersonalConversationDto
        {
            ConversationId = conversation.Id,
            ParticipantIds = conversation.UserConversations.Select(uc => uc.UserId).ToArray(),
        };

        await Clients.User(otherUserId.ToString()).PersonalConversationStarted(dto);

        return dto;
    }

    public async Task<PersonalMessageDto> SendPersonalMessage(
        int otherUserId,
        string content,
        CancellationToken ct = default
    )
    {
        var userId = GetUserId();
        ct = ResolveToken(ct);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new HubException("Message content is required.");
        }

        var conversation = await GetOrCreatePersonalConversation(userId, otherUserId, ct);

        var sender =
            await _dbContext.Users.FindAsync([userId], cancellationToken: ct)
            ?? throw new HubException("Sender not found.");
        var recipient =
            await _dbContext.Users.FindAsync([otherUserId], cancellationToken: ct)
            ?? throw new HubException("Recipient not found.");

        var message = new Message
        {
            Content = content.Trim(),
            Sender = sender,
            Conversation = conversation,
        };

        await _dbContext.Messages.AddAsync(message, ct);
        await _dbContext.UsersMessages.AddRangeAsync(
            [
                new UserMessage
                {
                    Message = message,
                    User = sender,
                    Status = MessageStatus.Read,
                },
                new UserMessage
                {
                    Message = message,
                    User = recipient,
                    Status = MessageStatus.Sent,
                },
            ],
            ct
        );

        await _dbContext.SaveChangesAsync(ct);

        var dto = new PersonalMessageDto
        {
            ConversationId = conversation.Id,
            MessageId = message.Id,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
        };

        await Clients.Group(GroupName(conversation.Id)).ReceivePersonalMessage(dto);

        return dto;
    }

    private async Task<Conversation> GetOrCreatePersonalConversation(
        int userId,
        int otherUserId,
        CancellationToken ct
    )
    {
        if (userId == otherUserId)
        {
            throw new HubException("Cannot start a personal chat with yourself.");
        }

        var conversation = await _dbContext.Conversations
            .Include(c => c.UserConversations)
            .Where(c => c.UserConversations.Count == 2)
            .Where(c =>
                c.UserConversations.Any(uc => uc.UserId == userId)
                && c.UserConversations.Any(uc => uc.UserId == otherUserId)
            )
            .FirstOrDefaultAsync(ct);

        if (conversation != null)
        {
            return conversation;
        }

        var user =
            await _dbContext.Users.FindAsync([userId], cancellationToken: ct)
            ?? throw new HubException("User not found.");
        var otherUser =
            await _dbContext.Users.FindAsync([otherUserId], cancellationToken: ct)
            ?? throw new HubException("User not found.");

        conversation = new Conversation();
        conversation.UserConversations =
        [
            new UserConversation { User = user, Conversation = conversation },
            new UserConversation { User = otherUser, Conversation = conversation },
        ];

        await _dbContext.Conversations.AddAsync(conversation, ct);
        await _dbContext.SaveChangesAsync(ct);

        return conversation;
    }

    private CancellationToken ResolveToken(CancellationToken ct) =>
        ct == default ? Context.ConnectionAborted : ct;

    private int GetUserId()
    {
        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            throw new HubException("Unauthorized.");
        }

        return userId;
    }

    private static string GroupName(int conversationId) => $"conversation:{conversationId}";
}

public interface IPersonalChatClient
{
    Task PersonalConversationStarted(PersonalConversationDto conversation);
    Task ReceivePersonalMessage(PersonalMessageDto message);
}

public class PersonalConversationDto
{
    public int ConversationId { get; set; }
    public required int[] ParticipantIds { get; set; }
}

public class PersonalMessageDto
{
    public int ConversationId { get; set; }
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
