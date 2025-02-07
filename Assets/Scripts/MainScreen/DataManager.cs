using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class DataManager : MonoBehaviour
{
    private readonly List<Project> _projects = new List<Project>();
    private readonly List<Lesson> _lessons = new List<Lesson>();
    private readonly List<HomeTask> _homeTasks = new List<HomeTask>();
    
    private void Awake()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        // Load data from PlayerPrefs or other storage
        // This is a placeholder for actual data loading implementation
    }
    
    public (List<Project> projects, List<Lesson> lessons, List<HomeTask> homeTasks) GetTodayData()
    {
        var today = DateTime.Today;
        return (
            _projects.Where(p => p.creationDate.Date == today).ToList(),
            _lessons.Where(l => l.date.Date == today).ToList(),
            _homeTasks.Where(h => h.time.Date == today).ToList()
        );
    }
    
    public void AddProject(Project project)
    {
        _projects.Add(project);
        SaveData();
    }
    
    public void AddLesson(Lesson lesson)
    {
        _lessons.Add(lesson);
        SaveData();
    }
    
    public void AddHomeTask(HomeTask homeTask)
    {
        _homeTasks.Add(homeTask);
        SaveData();
    }
    
    private void SaveData()
    {
        // Save data to PlayerPrefs or other storage
        // This is a placeholder for actual data saving implementation
    }
}