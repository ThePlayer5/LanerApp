using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace LanerAvaApp.UserControls
{
    public partial class ChatsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;
        private readonly ChatService _chatService;

        private readonly ObservableCollection<ChatListItem> _filtered = new();
        private List<ChatListItem> _chats = new();

        public ChatsPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService, ChatService chatService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;
            _chatService = chatService;

            LoadUser();
        }

        private async void LoadUser()
        {
            try
            {
                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                if (GeneralVariables.SelectedChat is not null)
                    await _chatService.RemoveChatUser(user.UserId, GeneralVariables.SelectedChat.IdChat);
                GeneralVariables.SelectedChat = null!;

                await LoadChats(user.UserId);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        public async Task LoadChats(int supportUserId)
        {
            try
            {
                await using var db = await _context.CreateDbContextAsync();

                var chats = await db.Chats.Include(u => u.User).Include(u => u.User.Role)
                    .Where(u => u.UserId != supportUserId && string.Equals(u.User.Role.Name, Roles.User))
                    .Select(u => new { u.IdChat, u.UserId, u.User.Fullname })
                    .ToListAsync();

                var chatIds = chats.Select(u => u.IdChat).ToList();

                var lastMessages = await db.Messages
                    .Where(m => chatIds.Contains(m.Chat.IdChat))
                    .GroupBy(m => m.Chat.IdChat)
                    .Select(g => new 
                    {
                        IdChat = g.Key,
                        Text = g.OrderByDescending(m => m.Timing).Select(m => m.MessageText).First(),
                        SentAt = g.Max(m => m.Timing)
                    })
                    .ToListAsync();

                var lastByChat = lastMessages.ToDictionary(x => x.IdChat);

                _chats = chats.Select(u =>
                {
                    lastByChat.TryGetValue(u.IdChat, out var last);
                    return new ChatListItem
                    {
                        IdChat = u.IdChat,
                        UserId = u.UserId,
                        Fullname = u.Fullname,
                        LastMessageText = last?.Text,
                        LastMessageAt = last?.SentAt
                    };
                })
                .OrderByDescending(u => u.LastMessageAt ?? DateTime.MinValue)
                .ThenBy(u => u.Fullname)
                .ToList();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ApplyFilter()
        {
            try
            {
                var search = (SearchBox.Text ?? "").Trim();

                var filtered = string.IsNullOrEmpty(search) ? _chats :
                    _chats.Where(u => u.Fullname
                    .Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _filtered.Clear();
                foreach (var item in filtered)
                    _filtered.Add(item);

                EmptyHint.IsVisible = filtered.Count == 0 && !string.IsNullOrEmpty(search);

                UsersList.ItemsSource = _filtered;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void UsersList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UsersList.SelectedItem is not ChatListItem chatItem) return;
                GeneralVariables.SelectedChat = chatItem;

                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await _chatService.AddChatUser(user.UserId, chatItem.IdChat);

                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<TechSupportPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}