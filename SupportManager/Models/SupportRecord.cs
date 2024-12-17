// Models/SupportRecord.cs
using System;

namespace SupportManager.Models
{
    public class SupportRecord
    {
        public int Id { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string NameDesignation { get; set; } = "";
        public string Executor { get; set; } = "";
        public DateTime SupportEndDate { get; set; }

        public int DaysLeft => (SupportEndDate - DateTime.Now).Days;
        public string DaysLeftDisplay => DaysLeft > 0 ? $"{DaysLeft} дн. осталось" : "Поддержка кончилась";

        // Новое поле для PDF файла
        public string PdfPath { get; set; } = "";
    }
}
