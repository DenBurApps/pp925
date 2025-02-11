using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using AddData;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class SerializableData
{
    public List<Project> Projects = new List<Project>();
    public List<Lesson> Lessons = new List<Lesson>();
    public List<HomeTask> HomeTasks = new List<HomeTask>();
}

public class DataManager : MonoBehaviour
{
    [SerializeField] private CreateTaskScreenController _screenController;
    
    private SerializableData _data;
    private string _saveFilePath;
    private JsonSerializerSettings _jsonSettings;
    
    public event Action DataChanged;
    
    private void Awake()
    {
        
        _data = new SerializableData();
        _saveFilePath = Path.Combine(Application.persistentDataPath, "taskData.json");
        
        // Configure JSON serializer settings
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };
        
        // Add DateTime converter
        _jsonSettings.Converters.Add(new IsoDateTimeConverter());
        
        LoadData();
    }

    private void OnEnable()
    {
        _screenController.OnHomeTaskCreated += AddHomeTask;
        _screenController.OnLessonCreated += AddLesson;
        _screenController.OnProjectCreated += AddProject;
    }

    private void OnDisable()
    {
        _screenController.OnHomeTaskCreated -= AddHomeTask;
        _screenController.OnLessonCreated -= AddLesson;
        _screenController.OnProjectCreated -= AddProject;
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                string jsonData = File.ReadAllText(_saveFilePath);
                _data = JsonConvert.DeserializeObject<SerializableData>(jsonData, _jsonSettings) 
                        ?? new SerializableData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
            _data = new SerializableData();
        }
    }

    public (List<Project> projects, List<Lesson> lessons, List<HomeTask> homeTasks) GetDateData(DateTime date)
    {
        return (
            _data.Projects.Where(p => p.Date.Date == date.Date).ToList(),
            _data.Lessons.Where(l => l.DateTime.Date == date.Date).ToList(),
            _data.HomeTasks.Where(h => h.DateTime.Date == date.Date).ToList()
        );
    }
    
    public (List<Project> projects, List<Lesson> lessons, List<HomeTask> homeTasks) GetTodayData()
    {
        return GetDateData(DateTime.Today);
    }

    public bool HasDataForDate(DateTime date)
    {
        return _data.Projects.Any(p => p.Date.Date == date.Date) ||
               _data.Lessons.Any(l => l.DateTime.Date == date.Date) ||
               _data.HomeTasks.Any(h => h.DateTime.Date == date.Date);
    }
    
    public void AddProject(Project project)
    {
        _data.Projects.Add(project);
        SaveData();
        DataChanged?.Invoke();
    }
    
    public void EditLesson(Lesson oldLesson, Lesson newLesson)
    {
        int index = _data.Lessons.IndexOf(oldLesson);
        if (index != -1)
        {
            _data.Lessons[index] = newLesson;
            SaveData();
            DataChanged?.Invoke();
        }
    }
    
    public void AddLesson(Lesson lesson)
    {
        _data.Lessons.Add(lesson);
        SaveData();
        DataChanged?.Invoke();
    }
    
    public void AddHomeTask(HomeTask homeTask)
    {
        _data.HomeTasks.Add(homeTask);
        SaveData();
        DataChanged?.Invoke();
    }

    public void RemoveProject(Project project)
    {
        if (_data.Projects.Remove(project))
        {
            SaveData();
            DataChanged?.Invoke();
        }
    }
    
    public void EditHomeTask(HomeTask oldTask, HomeTask newTask)
    {
        int index = _data.HomeTasks.IndexOf(oldTask);
        if (index != -1)
        {
            _data.HomeTasks[index] = newTask;
            SaveData();
            DataChanged?.Invoke();
        }
    }

    public void RemoveLesson(Lesson lesson)
    {
        if (_data.Lessons.Remove(lesson))
        {
            SaveData();
            DataChanged?.Invoke();
        }
    }

    public void RemoveHomeTask(HomeTask homeTask)
    {
        if (_data.HomeTasks.Remove(homeTask))
        {
            SaveData();
            DataChanged?.Invoke();
        }
    }
    
    private void SaveData()
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(_data, _jsonSettings);
            File.WriteAllText(_saveFilePath, jsonData);
            Debug.Log($"Data saved successfully to {_saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
    }
}