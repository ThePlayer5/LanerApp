using Avalonia.Controls;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.UserControls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace LanerAvaApp.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<AppDbContext> _context;
    private readonly IUserSessionProvider _sessionService;
    private readonly IAuthorizationService _authService;
    private readonly ChatService _chatService;

    public MainWindow(
        IServiceProvider serviceProvider,
        IDbContextFactory<AppDbContext> context,
        IUserSessionProvider sessionService,
        IAuthorizationService authService,
        ChatService chatService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _context = context;
        _sessionService = sessionService;
        _authService = authService;
        _chatService = chatService;

        LoadUser();

        _sessionService.SessionChanged += OnSessionChanged;
        Loaded += (_, _) => RefreshProfile();
    }

    private async void LoadUser()
    {
        try
        {
            var loadedData = SaveUserModel.LoadData();

            if (loadedData is null)
            {
                NavigateTo<AuthPage>();
                throw new ArgumentNullException("Не удалось загрузить данные пользователя!", nameof(loadedData));
            }

            if (!loadedData.RememberMe || string.IsNullOrEmpty(loadedData.Login) || string.IsNullOrEmpty(loadedData.Password))
            {
                NavigateTo<AuthPage>();
                throw new ArgumentNullException("Отсутствуют данные пользователя!", nameof(loadedData));
            }

            await using var db = await _context.CreateDbContextAsync();
            var findUser = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                string.Equals(u.Ulogin, loadedData.Login)
                && string.Equals(u.Password, loadedData.Password));

            if (findUser is null)
                throw new ArgumentNullException("Не удалось получить пользователя!!", nameof(loadedData));

            if (findUser.Role is null)
                throw new ArgumentNullException("Не удалось определить роль пользователя!", nameof(findUser.Role));

            var session = new UserSession(findUser.IdUser, findUser.Ulogin, findUser.Role.Name);
            _sessionService.Set(session);

            var user = _sessionService.Current;

            if (user is null)
                throw new ArgumentNullException("Не удалось определить текущую сессию!", nameof(user));

            await _chatService.Connect();
            await _chatService.AddActiveUser(user.UserId);

            NavigateTo<HomePage>();
        }
        catch (Exception ex)
        {
            var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
            await error.ShowAsync();
        }
    }

    public async void NavigateTo<TPage>() where TPage : UserControl
    {
        try
        {
            MainControl.Content = _serviceProvider.GetRequiredService<TPage>();
        }
        catch (Exception ex)
        {
            var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
            await error.ShowAsync();
        }
    }

    private async void OnSessionChanged(object? sender, EventArgs e)
    {
        try
        {
            await RefreshProfile();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void DiffRights()
    {
        try
        {
            bool isAdmin = _authService.IsAdmin;
            string role = _sessionService.Current?.Role!;

            HomePageBtn.IsVisible = true;
            SupportPageBtn.IsVisible = true;
            ChatsPageBtn.IsVisible = true;
            AlphPageBtn.IsVisible = true;
            ProfilePageBtn.IsVisible = true;
            AdminPanelBtn.IsVisible = true;
            GamePageBtn.IsVisible = true;
            CompareGameBtn.IsVisible = true;
            RatingPageBtn.IsVisible = true;

            if (!isAdmin && string.Equals(role, Roles.User))
            {
                ChatsPageBtn.IsVisible = false;
                AdminPanelBtn.IsVisible = false;
            }
            else if (!isAdmin && string.Equals(role, Roles.Support))
            {
                SupportPageBtn.IsVisible = false;
                AdminPanelBtn.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
            await error.ShowAsync();
        }
    }

    public async Task RefreshProfile()
    {
        try
        {
            var user = _sessionService.Current;
            if (user is null)
            {
                NavPanel.IsVisible = false;
                return;
            }

            await using var db = await _context.CreateDbContextAsync();
            var findUser = await db.Users.FirstOrDefaultAsync(u => u.IdUser == user.UserId);

            if (findUser is null)
                throw new ArgumentNullException("Не удалось найти пользователя!", nameof(findUser));

            NavPanel.IsVisible = true;
            FullnameText.Text = findUser.Fullname;
            EmailText.Text = findUser.Email;

            DiffRights();
        }
        catch (Exception ex)
        {
            var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
            await error.ShowAsync();
        }
    }

    public async void ShowNavPanel()
    {
        try
        {
            NavPanel.IsVisible = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void HomePageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<HomePage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ProfilePageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<ProfilePage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void SupportPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            GeneralVariables.SelectedChat = null!;
            NavigateTo<TechSupportPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ChatsPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<ChatsPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void AlphPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<AlphabetsPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void AdminPanelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<AdminPanelPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void GamePageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            await using var db = await _context.CreateDbContextAsync();
            var learnedWords = await db.LearnedWords.ToListAsync();
            if (learnedWords.Count <= 0)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Вы выучили недостаточно слов!\nЧтобы разблокировать данную функцию\nвыучите более 8 слов!", ButtonEnum.Ok);
                await error.ShowAsync();
                return;
            }

            NavigateTo<PairsGamePage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CompareGameBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<CompareWordsPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void RatingPageBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            NavigateTo<RatingPage>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void CloseBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void FullScreenBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void MinimizeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            WindowState = WindowState.Minimized;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}