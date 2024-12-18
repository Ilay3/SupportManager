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
        public string DaysLeftDisplay
        {
            get
            {
                var now = DateTime.Now;

                var endOfSupport = SupportEndDate;

                var timeDifference = endOfSupport - now;

                if (timeDifference.TotalDays < 0)
                {
                    return "Поддержка кончилась";
                }
                else if (timeDifference.TotalDays < 1)
                {
                    return "Осталось меньше суток";
                }
                else
                {
                    return $"{(int)Math.Ceiling(timeDifference.TotalDays)} дн. осталось";
                }
            }
        }



        // Новое поле для PDF файла
        public string PdfPath { get; set; } = "";
    }
}
