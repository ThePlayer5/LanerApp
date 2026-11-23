using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanerAvaApp.UserControls
{
    public partial class ProfilePage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;
        private User _user;

        public ProfilePage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            LoadUser();
        }

        private async void LoadUser()
        {
            try
            {
                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();
                var findUser = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.IdUser == user.UserId);
                if (findUser is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(findUser));

                _user = findUser;

                LoadProfile();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadProfile()
        {
            try
            {
                ViewFullname.Text = _user.Fullname;
                ViewLogin.Text = _user.Ulogin;
                ViewEmail.Text = _user.Email;

                HeaderName.Text = _user.Fullname;
                HeaderRole.Text = _user.Role?.Name ?? "—";
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void EditBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                EditFullname.Text = _user.Fullname;
                EditLogin.Text = _user.Ulogin;
                EditEmail.Text = _user.Email;
                EditPassword.Text = EditPasswordConfirm.Text = string.Empty;

                ViewModePanel.IsVisible = false;
                EditModePanel.IsVisible = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                ExitEditMode();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ExitEditMode()
        {
            try
            {
                EditModePanel.IsVisible = false;
                ViewModePanel.IsVisible = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SaveBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditFullname.Text))
                    throw new ArgumentException("Введите ФИО");
                if (string.IsNullOrWhiteSpace(EditLogin.Text))
                    throw new ArgumentException("Введите логин");
                if (EditLogin.Text == _user.Password)
                    throw new ArgumentException("Логин не должен совпадать с паролем");
                if (string.IsNullOrWhiteSpace(EditEmail.Text) || !EditEmail.Text.Contains('@'))
                    throw new ArgumentException("Введите корректный email");

                var pwd = EditPassword.Text ?? "";
                var pwdConf = EditPasswordConfirm.Text ?? "";
                bool changePassword = !string.IsNullOrEmpty(pwd);

                if (changePassword && pwd != pwdConf)
                    throw new InvalidOperationException("Пароли не совпадают");
                if (changePassword && pwd.Length < 6)
                    throw new InvalidOperationException("Пароль должен содержать минимум 6 символов");

                _user.Fullname = EditFullname.Text!.Trim();
                _user.Ulogin = EditLogin.Text!.Trim();
                _user.Email = EditEmail.Text!.Trim();

                await using var db = await _context.CreateDbContextAsync();

                db.Users.Update(_user);
                await db.SaveChangesAsync();

                LoadProfile();
                ExitEditMode();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ExitBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                _sessionService.Clear();
                GeneralVariables.SelectedAlph = null!;
                GeneralVariables.SelectedChat = null!;
                GeneralVariables.SelectedLang = null!;
                GeneralVariables.SelectedLevel = null!;

                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                    throw new ArgumentException("Не видит MainWindow");

                await mainWindow.RefreshProfile();
                mainWindow.NavigateTo<AuthPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task ExportCSV()
        {
            try
            {
                var user = _sessionService.Current;
                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();
                var rows = await db.LearningStats.AsNoTracking().Where(ls => ls.Login == user.Login).ToListAsync();

                if (rows.Count == 0)
                {
                    return;
                }

                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is null) return;

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить статистику",
                    SuggestedFileName = $"LearningStats_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv",
                    DefaultExtension = "csv",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } }
                    }
                });

                if (file is null) return;

                var sb = new StringBuilder();
                sb.AppendLine("Fullname;Login;Email;Language;DiffLevel;LearnedQuantity");

                foreach (var r in rows)
                {
                    sb.AppendLine(string.Join(";",
                        r.Fullname,
                        r.Login,
                        r.Email,
                        r.Language,
                        r.DiffLevel,
                        r.LearnedQuantity));
                }

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, new UTF8Encoding(true));
                await writer.WriteAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }

        }

        private async void ExportBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                await ExportCSV();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}