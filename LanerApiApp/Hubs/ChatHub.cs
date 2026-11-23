using LanerAvaApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace LanerApiApp.Hubs
{
    public class ChatHub : Hub
    {
        public ChatHub() { }

        public static Dictionary<int, string> ActiveUsers = new();
        public static Dictionary<int, string> ChatUsers = new();

        public async Task AddActiveUser(int userId)
        {
            try
            {
                if (ActiveUsers.ContainsKey(userId)) return;

                ActiveUsers[userId] = Context.ConnectionId;
                await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.Keys.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddChatUser(int userId, int chatId)
        {
            try
            {
                if (ChatUsers.ContainsKey(userId)) return;

                ChatUsers[userId] = Context.ConnectionId;

                string chatGroup = chatId.ToString();

                await Groups.AddToGroupAsync(ChatUsers[userId], chatGroup);
                await Clients.Group(chatGroup).SendAsync("UpdateChatUsers", ChatUsers.Keys.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task SendMessage(MessageRow message)
        {
            try
            {
                if (message is null)
                    throw new ArgumentNullException("Не удалось получить сообщение!", nameof(message));

                string chatGroup = message.ChatId.ToString();

                message.Timing = DateTime.Now;

                await Clients.Group(chatGroup).SendAsync("NewMessage", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task RemoveChatUser(int userId, int chatId)
        {
            try
            {
                if (!ChatUsers.ContainsKey(userId)) return;

                string chatGroup = chatId.ToString();

                await Groups.RemoveFromGroupAsync(ChatUsers[userId], chatGroup);
                await Clients.Group(chatGroup).SendAsync("UpdateChatUsers", ChatUsers.Keys.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            try
            {
                var activeUser = ActiveUsers.FirstOrDefault(au => au.Value == Context.ConnectionId);

                if (activeUser.Key != 0)
                {
                    ActiveUsers.Remove(activeUser.Key);
                    await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.Keys.ToList());
                }

                var chatUser = ChatUsers.FirstOrDefault(cu => cu.Value == Context.ConnectionId);

                if (chatUser.Key != 0)
                {
                    ChatUsers.Remove(chatUser.Key);
                    await Clients.All.SendAsync("UpdateChatUsers", ChatUsers.Keys.ToList());
                }

                await base.OnDisconnectedAsync(ex);
            }
            catch
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
