using LanerAvaApp.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LanerAvaApp.Services
{
    public class ChatService
    {
        private HubConnection _hubConnection;

        public List<int> ActiveUsers { get; set; } = new();
        public List<int> ChatUsers { get; set; } = new();

        public ObservableCollection<MessageRow> Messages { get; set; } = new();

        public Action? OnActiveUsersUpdate { get; set; }
        public Action? OnChatUsersUpdate { get; set; }
        public Action? OnMessageReceived { get; set; }

        public async Task Connect()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7171/chat")
                .WithAutomaticReconnect()
                .Build();

                _hubConnection.On<List<int>>("UpdateActiveUsers", (activeUsers) =>
                {
                    ActiveUsers = activeUsers;
                    OnActiveUsersUpdate?.Invoke();
                });

                _hubConnection.On<List<int>>("UpdateChatUsers", (chatUsers) =>
                {
                    ChatUsers = chatUsers;
                    OnChatUsersUpdate?.Invoke();
                });

                _hubConnection.On<MessageRow>("NewMessage", (message) =>
                {
                    Messages.Add(new MessageRow
                    {
                        MessageText = message.MessageText,
                        Timing = message.Timing,
                        UserId = message.UserId,
                        ChatId = message.ChatId,
                        CurrentUserId = ActiveUserId,
                    });
                    OnMessageReceived?.Invoke();
                });

                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddActiveUser(int userId)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("AddActiveUser", userId);
                    IsUserActive = true;
                    ActiveUserId = userId;
                }
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
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("AddChatUser", userId, chatId);
                }
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
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendMessage", message);
                }
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
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("RemoveChatUser", userId, chatId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public bool IsUserActive { get; set; }
        public int ActiveUserId { get; set; }
    }
}
