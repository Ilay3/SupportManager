// Models/SupportRecord.cs
using System;

namespace SupportManager.Models
{
    public class SupportRecord
    {
        public int Id { get; set; } // Уникальный идентификатор
        public DateTime DateOfCreation { get; set; }
        public string NameDesignation { get; set; }
        public string Executor { get; set; }
        public DateTime SupportEndDate { get; set; }

        // Вычисляемое свойство для дней оставшихся до окончания поддержки
        public int DaysLeft
        {
            get
            {
                return (SupportEndDate - DateTime.Now).Days;
            }
        }

        // Вычисляемое свойство для отображения дней или сообщения
        public string DaysLeftDisplay
        {
            get
            {
                int days = DaysLeft;
                return days > 0 ? $"{days} день(дней) осталось" : "Поддержка кончилась";
            }
        }
    }
}
