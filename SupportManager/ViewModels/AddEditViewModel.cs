// ViewModels/AddEditViewModel.cs
using System;
using System.Windows;
using System.Windows.Input;
using SupportManager.Models;
using SupportManager.Services;
using SupportManager.Helpers;
using Microsoft.Win32;

namespace SupportManager.ViewModels
{
    public class AddEditViewModel : BaseViewModel
    {
        private readonly ISupportRecordService _service;
        public SupportRecord Record { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectPdfCommand { get; } // Новая команда

        public AddEditViewModel(ISupportRecordService service, SupportRecord? record = null)
        {
            _service = service;
            Record = record != null
                ? new SupportRecord
                {
                    Id = record.Id,
                    DateOfCreation = record.DateOfCreation,
                    NameDesignation = record.NameDesignation,
                    Executor = record.Executor,
                    SupportEndDate = record.SupportEndDate,
                    PdfPath = record.PdfPath
                }
                : new SupportRecord
                {
                    DateOfCreation = DateTime.Now,
                    SupportEndDate = DateTime.Now.AddMonths(2)
                };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            SelectPdfCommand = new RelayCommand(SelectPdf);
        }

        private void Save(object? obj)
        {
            if (string.IsNullOrWhiteSpace(Record.NameDesignation) || string.IsNullOrWhiteSpace(Record.Executor))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Record.SupportEndDate < Record.DateOfCreation)
            {
                MessageBox.Show("Дата окончания поддержки не может быть раньше даты оформления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Record.Id == 0)
                _service.Add(Record);
            else
                _service.Update(Record);

            if (obj is Window w)
            {
                w.DialogResult = true;
                w.Close();
            }
        }

        private void Cancel(object? obj)
        {
            if (obj is Window w)
            {
                w.DialogResult = false;
                w.Close();
            }
        }

        private void SelectPdf(object? obj)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Выберите PDF-файл"
            };

            if (dlg.ShowDialog() == true)
            {
                Record.PdfPath = dlg.FileName;
                OnPropertyChanged(nameof(Record));
            }
        }
    }
}
