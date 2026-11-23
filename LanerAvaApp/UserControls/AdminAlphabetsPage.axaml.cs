using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LanerAvaApp.UserControls
{
    public partial class AdminAlphabetsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private ObservableCollection<AlphabetRow> _alphabets = new();
        public ObservableCollection<Language> Languages { get; } = new();

        private AlphabetRow? _selectedAlph;

        public AdminAlphabetsPage(IDbContextFactory<AppDbContext> context)
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
                _alphabets.Clear();

                await using var db = await _context.CreateDbContextAsync();

                var source = db.Alphabets.Include(a => a.Lang)
                    .ToList().Select(a => new AlphabetRow(a));

                foreach (var row in source)
                    _alphabets.Add(row);

                AlphabetsDataGrid.ItemsSource = _alphabets;
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
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AddAlphBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var alphRow = new AlphabetRow();
                _alphabets.Add(alphRow);
                AlphabetsDataGrid.SelectedItem = alphRow;
                AlphabetsDataGrid.ScrollIntoView(alphRow, null);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SaveAlphBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                AlphabetsDataGrid.CommitEdit();

                await using var db = await _context.CreateDbContextAsync();

                foreach (var row in _alphabets)
                {
                    var a = row.Model;
                    if (a.IdAlphabet== 0)
                        await db.Alphabets.AddAsync(a);
                    else
                        db.Alphabets.Update(a);
                }
                await db.SaveChangesAsync();

                var success = MessageBoxManager.GetMessageBoxStandard("Success", "Изменения сохранены!", ButtonEnum.Ok);
                await success.ShowAsync();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void DelAlphBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var warning = MessageBoxManager.GetMessageBoxStandard("Warning", "Вы точно хотите удалить эту запись?", ButtonEnum.YesNo);
                var result = await warning.ShowAsync();

                if (warning != null)
                {
                    if (result == ButtonResult.Yes)
                    {
                        if (_selectedAlph is not AlphabetRow alphabetRow) return;

                        var alphModel = alphabetRow.Model;

                        await using var db = await _context.CreateDbContextAsync();

                        _alphabets.Remove(alphabetRow);
                        db.Alphabets.Remove(alphModel);
                        await db.SaveChangesAsync();

                        var success = MessageBoxManager.GetMessageBoxStandard("Success", "Запись удалена!", ButtonEnum.Ok);
                        await success.ShowAsync();
                    }
                    else return;
                    _selectedAlph = null!;
                    DelAlphBtn.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AlphabetsDataGrid_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            try
            {
                var selectedRow = AlphabetsDataGrid.SelectedItem as AlphabetRow;
                if (selectedRow == null) return;
                _selectedAlph = selectedRow;
                DelAlphBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}