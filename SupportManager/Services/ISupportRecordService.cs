// Services/ISupportRecordService.cs
using System.Collections.Generic;
using SupportManager.Models;

namespace SupportManager.Services
{
    public interface ISupportRecordService
    {
        IEnumerable<SupportRecord> GetAll();
        void Add(SupportRecord record);
        void Update(SupportRecord record);
        void Delete(int id);
        SupportRecord GetById(int id);
        void SaveChanges();
        void LoadData();
    }
}
