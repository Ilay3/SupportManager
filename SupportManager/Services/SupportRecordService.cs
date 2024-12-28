using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SupportManager.Models;

namespace SupportManager.Services
{
    public class SupportRecordService : ISupportRecordService
    {
        private readonly List<SupportRecord> _records = new List<SupportRecord>();
        private int _nextId = 1;
        private readonly string _dataFilePath;

        public SupportRecordService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SupportManager");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _dataFilePath = Path.Combine(appDataPath, "data.json");
            LoadData();
        }

        public IEnumerable<SupportRecord> GetAll()
        {
            return _records;
        }

        public void Add(SupportRecord record)
        {
            record.Id = _nextId++;
            _records.Add(record);
            SaveChanges();
        }

        public void Update(SupportRecord record)
        {
            var existing = _records.FirstOrDefault(r => r.Id == record.Id);
            if (existing != null)
            {
                existing.DateOfCreation = record.DateOfCreation;
                existing.NameDesignation = record.NameDesignation;
                existing.Executor = record.Executor;
                existing.SupportEndDate = record.SupportEndDate;
                SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var record = _records.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                _records.Remove(record);
                SaveChanges();
            }
        }

        public SupportRecord GetById(int id)
        {
            return _records.FirstOrDefault(r => r.Id == id);
        }

        public void SaveChanges()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_records, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var records = JsonSerializer.Deserialize<List<SupportRecord>>(json);
                    if (records != null)
                    {
                        _records.AddRange(records);
                        if (_records.Any())
                            _nextId = _records.Max(r => r.Id) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void CreateBackup()
        {
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var backupDir = Path.Combine(appDir, "Backups");

                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                var backupFileName = $"data_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var backupFilePath = Path.Combine(backupDir, backupFileName);

                File.Copy(_dataFilePath, backupFilePath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании резервной копии: {ex.Message}");
            }
        }
    }
}
