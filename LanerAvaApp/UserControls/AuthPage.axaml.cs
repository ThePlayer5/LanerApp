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

namespace LanerAvaApp.UserControls
{
    public partial class AuthPage : UserControl
    {
        private readonly IUserSessionProvider _sessionService;
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly ChatService _chatService;

        public AuthPage(IUserSessionProvider sessionService, IDbContextFactory<AppDbContext> context, ChatService chatService)
        {
            InitializeComponent();
            _sessionService = sessionService;
            _context = context;
            _chatService = chatService;
        }

        private async void SignInBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LoginField.Text)
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
                var findUser = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                    string.Equals(u.Ulogin, LoginField.Text.ToString())
                    && string.Equals(u.Password, PasswordField.Text.ToString()));

                if (findUser is null)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", "Не удалось найти пользователя!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }

                if (findUser.Role is null)
                    throw new ArgumentNullException("Не удалось определить роль пользователя!", nameof(findUser.Role));

                var session = new UserSession(findUser.IdUser, findUser.Ulogin, findUser.Role.Name);
                _sessionService.Set(session);

                var user = _sessionService.Current;

                if (user is null)
                    throw new ArgumentNullException("Не удалось определить текущую сессию!", nameof(user));

                await _chatService.Connect();
                await _chatService.AddActiveUser(user.UserId);

                if (RememberBox.IsChecked is true)
                {
                    var userData = new UserData
                    {
                        Login = findUser.Ulogin,
                        Password = findUser.Password,
                        RememberMe = true
                    };
                    SaveUserModel.SaveData(userData);
                }

                LoginField.Clear();
                PasswordField.Clear();
                ConfirmPassField.Clear();
                RememberBox.IsChecked = false;

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

        private async void RegNavLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<RegPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}