// ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SupportManager.Models;
using SupportManager.Services;
using SupportManager.Views;
using Xceed.Document.NET;
using Xceed.Words.NET; // Добавьте это пространство имен

namespace SupportManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ISupportRecordService _service;

        public ObservableCollection<SupportRecord> Records { get; set; }
        public ObservableCollection<SupportRecord> FilteredRecords { get; set; }

        // Фильтры
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private string? _selectedExecutor;
        public string? SelectedExecutor
        {
            get => _selectedExecutor;
            set
            {
                _selectedExecutor = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private bool? _isActiveSupport = null;
        public bool? IsActiveSupport
        {
            get => _isActiveSupport;
            set
            {
                _isActiveSupport = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        // Список исполнителей для фильтра
        public ObservableCollection<string> Executors { get; set; }

        // Команды
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ExportToWordCommand { get; } // Новая команда

        private SupportRecord? _selectedRecord;
        public SupportRecord? SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }

        // Постраничный вывод
        private const int PageSize = 20;
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); UpdatePagedRecords(); }
        }

        public int TotalPages
        {
            get
            {
                if (FilteredRecords.Count == 0) return 1;
                return (int)Math.Ceiling((double)FilteredRecords.Count / PageSize);
            }
        }

        private ObservableCollection<SupportRecord> _pagedRecords = new();
        public ObservableCollection<SupportRecord> PagedRecords
        {
            get => _pagedRecords;
            set { _pagedRecords = value; OnPropertyChanged(); }
        }

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public MainViewModel(ISupportRecordService service)
        {
            _service = service;

            Records = new ObservableCollection<SupportRecord>(_service.GetAll());
            FilteredRecords = new ObservableCollection<SupportRecord>(Records);

            // Инициализация списка исполнителей
            Executors = new ObservableCollection<string>(_service.GetAll()
                                        .Select(r => r.Executor)
                                        .Distinct()
                                        .OrderBy(e => e));
            Executors.Insert(0, "Все"); // Опция для выбора всех исполнителей
            SelectedExecutor = "Все";

            AddCommand = new RelayCommand(_ => AddRecord());
            EditCommand = new RelayCommand(_ => EditRecord(), _ => SelectedRecord != null);
            DeleteCommand = new RelayCommand(_ => DeleteRecord(), _ => SelectedRecord != null);
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
            ExportToWordCommand = new RelayCommand(_ => ExportToWord());

            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);

            UpdatePagedRecords();
        }

        private void AddRecord()
        {
            var vm = new AddEditViewModel(_service);
            var window = new AddEditWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            if (window.ShowDialog() == true)
            {
                Records.Add(vm.Record);
                UpdateExecutors();
                ApplyFilter();
            }
        }

        private void EditRecord()
        {
            if (SelectedRecord == null) return;

            var vm = new AddEditViewModel(_service, SelectedRecord);
            var window = new AddEditWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            if (window.ShowDialog() == true)
            {
                // Обновление списка
                var index = Records.IndexOf(SelectedRecord);
                Records[index] = vm.Record;
                UpdateExecutors();
                ApplyFilter();
            }
        }

        private void DeleteRecord()
        {
            if (SelectedRecord == null) return;

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _service.Delete(SelectedRecord.Id);
                Records.Remove(SelectedRecord);
                UpdateExecutors();
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var filtered = Records.AsEnumerable();

            // Фильтрация по поисковому тексту
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                filtered = filtered.Where(r => r.NameDesignation.ToLower().Contains(lower) || r.Executor.ToLower().Contains(lower));
            }

            // Фильтрация по исполнителю
            if (!string.IsNullOrWhiteSpace(SelectedExecutor) && SelectedExecutor != "Все")
            {
                filtered = filtered.Where(r => r.Executor == SelectedExecutor);
            }

            // Фильтрация по статусу поддержки
            if (IsActiveSupport.HasValue)
            {
                if (IsActiveSupport.Value)
                {
                    filtered = filtered.Where(r => r.DaysLeft > 0);
                }
                else
                {
                    filtered = filtered.Where(r => r.DaysLeft <= 0);
                }
            }

            FilteredRecords.Clear();
            foreach (var record in filtered)
                FilteredRecords.Add(record);

            // Обновление постраничного вывода
            CurrentPage = 1;
            UpdatePagedRecords();
        }

        private void UpdatePagedRecords()
        {
            PagedRecords.Clear();
            var items = FilteredRecords.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            foreach (var item in items)
                PagedRecords.Add(item);

            OnPropertyChanged(nameof(TotalPages));
            CommandManager.InvalidateRequerySuggested();
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        private void UpdateExecutors()
        {
            var currentSelected = SelectedExecutor;
            Executors.Clear();
            Executors.Add("Все");
            foreach (var executor in Records.Select(r => r.Executor).Distinct().OrderBy(e => e))
                Executors.Add(executor);

            if (!string.IsNullOrEmpty(currentSelected) && Executors.Contains(currentSelected))
                SelectedExecutor = currentSelected;
            else
                SelectedExecutor = "Все";
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedExecutor = "Все";
            IsActiveSupport = null;
        }

        // Новая функция экспорта в Word
        private void ExportToWord()
        {
            try
            {
                // Откройте диалог сохранения файла
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx",
                    FileName = "SupportRecords",
                    DefaultExt = ".docx"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var doc = DocX.Create(filePath))
                    {
                        // Добавляем заголовок
                        doc.InsertParagraph("Список записей о поддержке")
                           .FontSize(20)
                           .Bold()
                           .Alignment = Alignment.center;
                        doc.InsertParagraph($"Дата экспорта: {DateTime.Now.ToShortDateString()}")
                           .FontSize(10)
                           .Italic()
                           .Alignment = Alignment.right;
                        doc.InsertParagraph(Environment.NewLine);

                        // Определяем количество столбцов
                        int columnCount = 6;

                        // Создаем таблицу с заголовками
                        var table = doc.AddTable(FilteredRecords.Count + 1, columnCount);
                        table.Alignment = Alignment.center;
                        table.Design = TableDesign.TableGrid;

                        // Заполняем заголовки
                        table.Rows[0].Cells[0].Paragraphs[0].Append("№ п.п").Bold();
                        table.Rows[0].Cells[1].Paragraphs[0].Append("Дата оформления").Bold();
                        table.Rows[0].Cells[2].Paragraphs[0].Append("Наименование, обозначение угла").Bold();
                        table.Rows[0].Cells[3].Paragraphs[0].Append("Исполнитель").Bold();
                        table.Rows[0].Cells[4].Paragraphs[0].Append("Дата окончания поддержки").Bold();
                        table.Rows[0].Cells[5].Paragraphs[0].Append("Статус поддержки").Bold();

                        // Заполняем строки таблицы
                        for (int i = 0; i < FilteredRecords.Count; i++)
                        {
                            var record = FilteredRecords[i];
                            table.Rows[i + 1].Cells[0].Paragraphs[0].Append(record.Id.ToString());
                            table.Rows[i + 1].Cells[1].Paragraphs[0].Append(record.DateOfCreation.ToShortDateString());
                            table.Rows[i + 1].Cells[2].Paragraphs[0].Append(record.NameDesignation);
                            table.Rows[i + 1].Cells[3].Paragraphs[0].Append(record.Executor);
                            table.Rows[i + 1].Cells[4].Paragraphs[0].Append(record.SupportEndDate.ToShortDateString());
                            table.Rows[i + 1].Cells[5].Paragraphs[0].Append(record.DaysLeftDisplay);
                        }

                        // Вставляем таблицу в документ
                        doc.InsertTable(table);

                        // Сохраняем документ
                        doc.Save();

                        MessageBox.Show("Данные успешно экспортированы в Word.", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
