using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HarfBuzzSharp;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LanerAvaApp.UserControls
{
    public partial class RatingPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;

        private readonly ObservableCollection<RatingItem> _users = new();

        public RatingPage(IDbContextFactory<AppDbContext> context)
        {
            InitializeComponent();
            _context = context;

            LoadAsync();
            RatingList.ItemsSource = _users;

            SortSelector.SelectionChanged += async (_, _) => await ReloadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task ReloadAsync()
        {
            try
            {
                _users.Clear();
                EmptyHint.IsVisible = false;

                var rows = SortSelector.SelectedIndex switch
                {
                    0 => await LoadByTotalWordsAsync(),
                    1 => await LoadByAdvancedWordsAsync(),
                    2 => await LoadByProgressAsync(),
                    _ => new List<(string, string)>()
                };

                int rank = 1;
                foreach (var (fullname, valueText) in rows)
                {
                    _users.Add(new RatingItem
                    {
                        Rank = rank++,
                        Fullname = fullname,
                        ValueText = valueText
                    });
                }

                EmptyHint.IsVisible = _users.Count == 0;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task<List<(string, string)>> LoadByTotalWordsAsync()
        {
            try
            {
                await using var db = await _context.CreateDbContextAsync();

                var data = await db!.Users
                    .Include(x => x.Role)
                    .Where(x => string.Equals(x.Role.Name, Roles.User))
                    .Select(x => new
                    {
                        x.Fullname,
                        Total = db.LearningStats
                            .Where(s => s.Login == x.Ulogin)
                            .Sum(s => (int?)s.LearnedQuantity) ?? 0
                    })
                    .OrderByDescending(x => x.Total)
                    .ThenBy(x => x.Fullname)
                    .ToListAsync();

                return data.Select(x => (x.Fullname, $"{x.Total} слов")).ToList();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
                return new List<(string, string)>();
            }
        }

        private async Task<List<(string, string)>> LoadByAdvancedWordsAsync()
        {
            try
            {
                await using var db = await _context.CreateDbContextAsync();

                const string advancedLevel = "Продвинутый";

                var data = await db.Users
                    .Include(x => x.Role)
                    .Where(x => string.Equals(x.Role.Name, Roles.User))
                    .Select(u => new
                    {
                        u.Fullname,
                        Advanced = db.LearningStats
                            .Where(s => s.Login == u.Ulogin && s.DiffLevel == advancedLevel)
                            .Sum(s => (int?)s.LearnedQuantity) ?? 0
                    })
                    .OrderByDescending(x => x.Advanced)
                    .ThenBy(x => x.Fullname)
                    .ToListAsync();

                return data.Select(x => (x.Fullname, $"{x.Advanced} слов")).ToList();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
                return new List<(string, string)>();
            }
        }

        private async Task<List<(string, string)>> LoadByProgressAsync()
        {
            try
            {
                await using var db = await _context.CreateDbContextAsync();

                var data = await db!.Users
                    .Include(x => x.Role)
                    .Where(x => string.Equals(x.Role.Name, Roles.User))
                    .Select(u => new
                    {
                        u.Fullname,
                        Percent = db.LearningProgresses
                            .Where(p => p.UserId == u.IdUser)
                            .Average(p => (double?)p.Percent) ?? 0.0
                    })
                    .OrderByDescending(x => x.Percent)
                    .ThenBy(x => x.Fullname)
                    .ToListAsync();

                return data.Select(x => (x.Fullname, $"{x.Percent:F1}%")).ToList();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
                return new List<(string, string)>();
            }
        }
    }
}