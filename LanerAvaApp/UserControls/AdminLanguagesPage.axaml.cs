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
    public partial class AdminLanguagesPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private ObservableCollection<LanguageRow> _languages = new();

        private LanguageRow? _selectedLang;

        public AdminLanguagesPage(IDbContextFactory<AppDbContext> context)
        {
            InitializeComponent();
            _context = context;

            RefreshData();
        }

        private async void RefreshData()
        {
            try
            {
                _languages.Clear();

                await using var db = await _context.CreateDbContextAsync();

                var source = db.Languages.ToList().Select(l => new LanguageRow(l));

                foreach (var row in source)
                    _languages.Add(row);

                LangsDataGrid.ItemsSource = _languages;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AddLangBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var langRow = new LanguageRow();
                _languages.Add(langRow);
                LangsDataGrid.SelectedItem = langRow;
                LangsDataGrid.ScrollIntoView(langRow, null);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void SaveLangBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                LangsDataGrid.CommitEdit();

                await using var db = await _context.CreateDbContextAsync();

                foreach (var row in _languages)
                {
                    var l = row.Model;
                    if (l.IdLang== 0)
                        await db.Languages.AddAsync(l);
                    else
                        db.Languages.Update(l);
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

        private async void DelLangBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var warning = MessageBoxManager.GetMessageBoxStandard("Warning", "Вы точно хотите удалить эту запись?", ButtonEnum.YesNo);
                var result = await warning.ShowAsync();

                if (warning != null)
                {
                    if (result == ButtonResult.Yes)
                    {
                        if (_selectedLang is not LanguageRow languageRow) return;

                        var langModel = languageRow.Model;

                        await using var db = await _context.CreateDbContextAsync();

                        _languages.Remove(languageRow);
                        db.Languages.Remove(langModel);
                        await db.SaveChangesAsync();

                        var success = MessageBoxManager.GetMessageBoxStandard("Success", "Запись удалена!", ButtonEnum.Ok);
                        await success.ShowAsync();
                    }
                    else return;
                    _selectedLang = null!;
                    DelLangBtn.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LangsDataGrid_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            try
            {
                var selectedRow = LangsDataGrid.SelectedItem as LanguageRow;
                if (selectedRow == null) return;
                _selectedLang = selectedRow;
                DelLangBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}