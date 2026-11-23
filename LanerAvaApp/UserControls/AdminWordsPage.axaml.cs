using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LanerAvaApp.UserControls
{
    public partial class AdminWordsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;

        private ObservableCollection<WordRow> _words = new();
        public ObservableCollection<Language> Languages { get; } = new();
        public ObservableCollection<DifficultyLevel> Levels { get; } = new();

        private WordRow? _selectedWord;

        public AdminWordsPage(IDbContextFactory<AppDbContext> context)
        {
            InitializeComponent();
            _context = context;

            RefreshData();
            LoadComboBoxes();
        }

        private async void RefreshData()
        {
            try
            {
                _words.Clear();

                await using var db = await _context.CreateDbContextAsync();

                var source = db.Words.Include(w => w.Lang).Include(w => w.Level)
                    .ToList().Select(w => new WordRow(w));

                foreach (var row in source)
                    _words.Add(row);

                WordsDataGrid.ItemsSource = _words;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadComboBoxes()
        {
            try
            {
                await using var db = await _context.CreateDbContextAsync();

                Languages.Clear();
                foreach (var lang in db.Languages.ToList())
                    Languages.Add(lang);

                Levels.Clear();
                foreach (var level in db.DifficultyLevels.ToList())
                    Levels.Add(level);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AddWordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var wordRow = new WordRow();
                _words.Add(wordRow);
                WordsDataGrid.SelectedItem = wordRow;
                WordsDataGrid.ScrollIntoView(wordRow, null);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SaveWordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                WordsDataGrid.CommitEdit();

                await using var db = await _context.CreateDbContextAsync();

                foreach (var row in _words)
                {
                    var w = row.Model;
                    if (w.IdWord == 0)
                        await db.Words.AddAsync(w);
                    else
                        db.Words.Update(w);
                }
                await db.SaveChangesAsync();

                var success = MessageBoxManager.GetMessageBoxStandard("Success", "Изменения сохранены!", ButtonEnum.Ok);
                await success.ShowAsync();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.InnerException}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void DelWordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var warning = MessageBoxManager.GetMessageBoxStandard("Warning", "Вы точно хотите удалить эту запись?", ButtonEnum.YesNo);
                var result = await warning.ShowAsync();

                if (warning != null)
                {
                    if (result == ButtonResult.Yes)
                    {
                        if (_selectedWord is not WordRow wordRow) return;

                        var wordModel = wordRow.Model;

                        await using var db = await _context.CreateDbContextAsync();

                        _words.Remove(wordRow);
                        db.Words.Remove(wordModel);
                        await db.SaveChangesAsync();

                        var success = MessageBoxManager.GetMessageBoxStandard("Success", "Запись удалена!", ButtonEnum.Ok);
                        await success.ShowAsync();
                    }
                    else return;
                    _selectedWord = null!;
                    DelWordBtn.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void WordsDataGrid_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            try
            {
                var selectedRow = WordsDataGrid.SelectedItem as WordRow;
                if (selectedRow == null) return;
                _selectedWord = selectedRow;
                DelWordBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}