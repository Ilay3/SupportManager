// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SupportManager.Helpers;
using SupportManager.Models;
using SupportManager.Services;
using SupportManager.Views;
using Xceed.Document.NET;
using Xceed.Words.NET;


namespace SupportManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ISupportRecordService _service;

        public ObservableCollection<SupportRecord> Records { get; set; }
        public ObservableCollection<SupportRecord> FilteredRecords { get; set; }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string? _selectedExecutor;
        public string? SelectedExecutor
        {
            get => _selectedExecutor;
            set { _selectedExecutor = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private bool? _isActiveSupport = null;
        public bool? IsActiveSupport
        {
            get => _isActiveSupport;
            set { _isActiveSupport = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ObservableCollection<string> Executors { get; set; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ExportToWordCommand { get; }

        private SupportRecord? _selectedRecord;
        public SupportRecord? SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }

        private const int PageSize = 5;
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); UpdatePagedRecords(); }
        }

        public int TotalPages => FilteredRecords.Count == 0 ? 1 : (int)Math.Ceiling((double)FilteredRecords.Count / PageSize);

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

            Executors = new ObservableCollection<string>(Records.Select(r => r.Executor).Distinct().OrderBy(e => e));
            Executors.Insert(0, "Все");
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
            var window = new AddEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
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
            var window = new AddEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var index = Records.IndexOf(SelectedRecord);
                Records[index] = vm.Record;
                UpdateExecutors();
                ApplyFilter();
            }
        }

        private void DeleteRecord()
        {
            if (SelectedRecord == null) return;
            var result = MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _service.Delete(SelectedRecord.Id);
                Records.Remove(SelectedRecord);
                UpdateExecutors();
                ApplyFilter();
            }
        }

        private void ResetFilters()
        {
            SearchText = "";
            SelectedExecutor = "Все";
            IsActiveSupport = null;
        }

        private void ApplyFilter()
        {
            var filtered = Records.AsEnumerable();

            // Фильтрация по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                filtered = filtered.Where(r => r.NameDesignation.ToLower().Contains(lower) ||
                                               r.Executor.ToLower().Contains(lower));
            }

            // Фильтрация по исполнителю
            if (!string.IsNullOrEmpty(SelectedExecutor) && SelectedExecutor != "Все")
            {
                filtered = filtered.Where(r => r.Executor == SelectedExecutor);
            }

            // Фильтрация по статусу поддержки
            if (IsActiveSupport.HasValue)
            {
                var now = DateTime.Now;

                if (IsActiveSupport.Value) // Активная поддержка
                {
                    filtered = filtered.Where(r => r.SupportEndDate >= now);
                }
                else // Неактивная поддержка
                {
                    filtered = filtered.Where(r => r.SupportEndDate < now);
                }
            }

            // Обновляем коллекцию
            FilteredRecords.Clear();
            foreach (var record in filtered)
            {
                FilteredRecords.Add(record);
            }

            // Сбрасываем пагинацию на первую страницу
            CurrentPage = 1;
            UpdatePagedRecords();
        }


        private void UpdatePagedRecords()
        {
            PagedRecords.Clear();
            var items = FilteredRecords.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            foreach (var i in items)
                PagedRecords.Add(i);

            OnPropertyChanged(nameof(TotalPages));
            CommandManager.InvalidateRequerySuggested();
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }

        private void UpdateExecutors()
        {
            var curr = SelectedExecutor;
            Executors.Clear();
            Executors.Add("Все");
            foreach (var ex in Records.Select(r => r.Executor).Distinct().OrderBy(e => e))
                Executors.Add(ex);
            if (curr != null && Executors.Contains(curr))
                SelectedExecutor = curr;
            else
                SelectedExecutor = "Все";
        }

        private void ExportToWord()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx",
                    FileName = "SupportRecords"
                };

                if (dlg.ShowDialog() == true)
                {
                    using (var doc = DocX.Create(dlg.FileName))
                    {
                       
                        doc.InsertParagraph($"Дата экспорта: {DateTime.Now:d}")
                            .FontSize(10)
                            .Italic()
                            .Alignment = Alignment.right;

                        if (FilteredRecords.Count > 0)
                        {
                            var table = doc.AddTable(FilteredRecords.Count + 1, 6);

                            table.Rows[0].Cells[0].Paragraphs[0].Append("№ п.п").Bold();
                            table.Rows[0].Cells[1].Paragraphs[0].Append("Дата оформ.").Bold();
                            table.Rows[0].Cells[2].Paragraphs[0].Append("Наименование").Bold();
                            table.Rows[0].Cells[3].Paragraphs[0].Append("Исполнитель").Bold();
                            table.Rows[0].Cells[4].Paragraphs[0].Append("Оконч. поддержки").Bold();
                            table.Rows[0].Cells[5].Paragraphs[0].Append("Статус").Bold();

                            int row = 1;
                            foreach (var rec in FilteredRecords)
                            {
                                table.Rows[row].Cells[0].Paragraphs[0].Append(rec.Id.ToString());
                                table.Rows[row].Cells[1].Paragraphs[0].Append(rec.DateOfCreation.ToShortDateString());
                                table.Rows[row].Cells[2].Paragraphs[0].Append(rec.NameDesignation);
                                table.Rows[row].Cells[3].Paragraphs[0].Append(rec.Executor);
                                table.Rows[row].Cells[4].Paragraphs[0].Append(rec.SupportEndDate.ToShortDateString());
                                table.Rows[row].Cells[5].Paragraphs[0].Append(rec.DaysLeftDisplay);
                                row++;
                            }

                            doc.InsertParagraph();
                            doc.InsertTable(table);
                        }
                        else
                        {
                            doc.InsertParagraph("Нет данных для экспорта.").FontSize(12);
                        }

                        doc.Save();
                        MessageBox.Show("Экспорт завершен успешно.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
