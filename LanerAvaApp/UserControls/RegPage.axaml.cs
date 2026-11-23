using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace LanerAvaApp.UserControls
{
    public partial class RegPage : UserControl
    {
        private readonly IUserSessionProvider _sessionService;
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly ChatService _chatService;

        public RegPage(IUserSessionProvider sessionService, IDbContextFactory<AppDbContext> context, ChatService chatService)
        {
            InitializeComponent();

            _sessionService = sessionService;
            _context = context;
            _chatService = chatService;

            DataContext = new User();
        }

        private async void SignUpBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (DataContext is null || DataContext is not User user)
                    throw new ArgumentNullException("DataContext пустой!", nameof(DataContext));
                if (string.IsNullOrWhiteSpace(FullnameField.Text)
                    || string.IsNullOrWhiteSpace(EmailField.Text)
                    || string.IsNullOrWhiteSpace(LoginField.Text)
                    || string.IsNullOrWhiteSpace(PasswordField.Text)
                    || string.IsNullOrWhiteSpace(ConfirmPassField.Text))
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", "Все поля должны быть заполнены!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                if (!string.Equals(PasswordField.Text, ConfirmPassField.Text))
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", "Пароли не совпадают!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }

                await using var db = await _context.CreateDbContextAsync();
                var existUser = await db.Users.FirstOrDefaultAsync(u => string.Equals(u.Ulogin, LoginField.Text.ToString()));

                if (existUser is not null)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", "Пользователь с таким логином уже существует!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }

                foreach (var role in db.Roles.ToList())
                {
                    if (string.Equals(role.Name, Roles.User))
                        user.RoleId = role.IdRole;
                }

                if (user.RoleId == 0)
                    throw new ArgumentNullException("Не удалось определить роль пользователя!", nameof(user.RoleId));

                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();

                var session = new UserSession(user.IdUser, user.Ulogin, Roles.User);
                _sessionService.Set(session);

                var userSession = _sessionService.Current;

                if (userSession is null)
                    throw new ArgumentNullException("Не удалось создать сессию!", nameof(userSession));

                await _chatService.Connect();
                await _chatService.AddActiveUser(userSession.UserId);

                DataContext = new User();
                ConfirmPassField.Clear();
                
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<HomePage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AuthNavLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<AuthPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}