using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using HarfBuzzSharp;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LanerAvaApp.UserControls
{
    public partial class LevelsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;

        private ObservableCollection<LevelCard> _levels { get; } = new();

        public LevelsPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            LoadLevels();
        }

        private async void LoadLevels()
        {
            try
            {
                _levels.Clear();

                var lang = GeneralVariables.SelectedLang;

                if (lang is null)
                    throw new ArgumentNullException("Не удалось получить уровни сложности!", nameof(lang));

                await using var db = await _context.CreateDbContextAsync();
                var source = await db.DifficultyLevels.OrderBy(dl => dl.Difficulty).ToListAsync();

                if (source is null)
                    throw new ArgumentNullException("Не удалось получить уровни сложности!", nameof(source));

                foreach (var level in source)
                {
                    var words = await db.Words.Include(w => w.Level).Include(w => w.Lang)
                        .Where(w => w.LangId == lang.LangId && w.LevelId == level.IdLevel).ToListAsync();

                    if (words is null)
                        throw new ArgumentNullException("Не удалось получить список слов!", nameof(words));

                    int wordsQty = words.Count;

                    IBrush cardBg = Brushes.White;

                    switch (level.Difficulty)
                    {
                        case 1:
                            cardBg = Brush.Parse("#E1F8EB");
                            break;
                        case 2:
                            cardBg = Brush.Parse("#FDF9CB");
                            break;
                        case 3:
                            cardBg = Brush.Parse("#F9D7D7");
                            break;
                    }

                    _levels.Add(new LevelCard
                    {
                        LevelId = level.IdLevel,
                        LevelName = level.Name,
                        CardBackground = cardBg,
                        WordsQty = wordsQty
                    });
                }

                LevelsLB.ItemsSource = _levels;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LevelScroll_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            try
            {
                if (sender is ScrollViewer sv)
                {
                    sv.Offset = sv.Offset.WithX(sv.Offset.X - e.Delta.Y * 60);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LevelsLB_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (LevelsLB.SelectedItem is not LevelCard levelCard) return;
                GeneralVariables.SelectedLevel = levelCard;

                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<WordsPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}