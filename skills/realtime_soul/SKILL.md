---
name: Realtime Soul (Communication)
description: Master real-time communication in .NET + Next.js using SignalR and WebRTC, focusing on chat, notifications, and video coordination.
---

# Realtime Soul Skill: Seamless Communication

Enable instant interaction between patients and therapists, building a bridge of immediate support.

## 1. SignalR Setup (.NET Backend)
Configure SignalR to handle persistent connections and real-time messaging.

```csharp
// Program.cs
builder.Services.AddSignalR();

// Hubs/ChatHub.cs
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

// Map the hub
app.MapHub<ChatHub>("/chatHub");
```

## 2. Next.js Integration (@microsoft/signalr)
Connect your Next.js frontend to the SignalR hub.

```typescript
// hooks/useChat.ts
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export function useChat() {
    const [connection, setConnection] = useState<HubConnection | null>(null);

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/chatHub`)
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);
    
    // Logic for joining rooms and sending messages
}
```

## 3. Real-time Notifications
Use SignalR to push notifications for:
- New appointment requests.
- Incoming messages.
- Reminders for upcoming sessions.

## 4. WebRTC Coordination
For video calls, use SignalR as a signaling server to exchange session descriptions and ICE candidates between peers.

## 5. Persistence & History
Always store chat history in the SQL database before broadcasting to clients to ensure reliability and auditability.

```csharp
public async Task JoinRoom(string roomId)
{
    var history = await _db.Messages.Where(m => m.RoomId == roomId).ToListAsync();
    await Clients.Caller.SendAsync("LoadHistory", history);
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
}
```

## Best Practices
- **Security**: Authenticate SignalR connections using JWT.
- **Scalability**: For high traffic, use Redis Backplane for SignalR.
- **Handling Disconnections**: Implement robust client-side logic to handle network drops gracefully.
- **Privacy (Encryption)**: Ensure chat messages are encrypted at rest.
