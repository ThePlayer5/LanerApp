using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LanerAvaApp.UserControls
{
    public partial class TechSupportPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;
        private readonly ChatService _chatService;

        private readonly ObservableCollection<MessageRow> _messages = new();

        private bool _isSupport = false;

        public TechSupportPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService, ChatService chatService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;
            _chatService = chatService;

            DiffRights();
            LoadMessages();
            Task.Delay(500);
            LoadInitialMessages();
        }

        private async void ApplyUIChanges()
        {
            try
            {
                ChatUserAvatar.Source = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/profile-icon.png")));

                await using var db = await _context.CreateDbContextAsync();
                var chat = CreateUserChat();
                var user = await db.Users.FirstOrDefaultAsync(x => x.IdUser == chat.UserId);
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                ChatUserName.Text = user.Fullname;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void DiffRights()
        {
            try
            {
                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                if (user.Role == Roles.Support)
                {
                    _isSupport = true;
                    if (_isSupport)
                        ApplyUIChanges();
                }
                if (user.Role == Roles.User)
                {
                    var chat = CreateUserChat();
                    await _chatService.AddChatUser(user.UserId, chat.IdChat);
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadMessages()
        {
            try
            {
                _chatService.Messages.Clear();

                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                var chat = CreateUserChat();

                await using var db = await _context.CreateDbContextAsync();
                var source = await db.Messages
                    .Where(m => m.ChatId == chat.IdChat)
                    .Select(m => new MessageRow
                    {
                        IdMessage = m.IdMessage,
                        MessageText = m.MessageText,
                        Timing = m.Timing,
                        UserId = m.UserId,
                        ChatId = m.ChatId,
                        CurrentUserId = _chatService.ActiveUserId,
                    }).ToListAsync();

                foreach (var row in source)
                    _chatService.Messages.Add(row);

                MessagesList.ItemsSource = _chatService.Messages;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private Chat CreateUserChat()
        {
            try
            {
                if (_isSupport)
                {
                    var chatItem = GeneralVariables.SelectedChat;
                    if (chatItem is null)
                        throw new ArgumentNullException("Не удалось найти пользователя!", nameof(chatItem));

                    return new Chat { IdChat = chatItem.IdChat, UserId = chatItem.UserId };
                }

                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                using var db = _context.CreateDbContext();
                var findChat = db.Chats.FirstOrDefault(c => c.UserId == user.UserId);

                if (findChat is not null) return findChat;

                var newChat = new Chat
                {
                    UserId = user.UserId
                };

                db.Chats.Add(newChat);
                db.SaveChanges();

                return null!;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                error.ShowAsync();
                return null!;
            }
        }

        private async void LoadInitialMessages()
        {
            try
            {
                var userId = _isSupport ? 0 : -1;
                _chatService.Messages.Add(new MessageRow
                {
                    MessageText = "Здравствуйте! Чем можем помочь?",
                    CurrentUserId = userId,
                    UserId = 0,
                    Timing = DateTime.Now.AddMinutes(-5)
                });

                ScrollToEnd();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SendMessage()
        {
            try
            {
                var text = MessageInput.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                    throw new ArgumentException("Нельзя отправить пустое сообшение!");

                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();
                var chat = CreateUserChat();

                if (chat is null)
                    throw new ArgumentNullException("Не удалось найти чат!", nameof(chat));

                var newMessage = new Message
                {
                    Timing = DateTime.Now,
                    MessageText = text,
                    UserId = user.UserId,
                    ChatId = chat.IdChat
                };

                var message = new MessageRow
                {
                    MessageText = newMessage.MessageText,
                    Timing = newMessage.Timing,
                    UserId = newMessage.UserId,
                    ChatId = newMessage.ChatId,
                    CurrentUserId = _chatService.ActiveUserId,
                };

                await _chatService.SendMessage(message);

                await db.Messages.AddAsync(newMessage);
                await db.SaveChangesAsync();

                MessageInput.Clear();
                ScrollToEnd();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.InnerException}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ScrollToEnd()
        {
            try
            {
                Dispatcher.UIThread.Post(() => MessagesScroll.ScrollToEnd());
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SendButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                SendMessage();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}