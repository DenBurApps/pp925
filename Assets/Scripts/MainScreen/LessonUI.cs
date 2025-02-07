using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LessonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    
    public void Initialize(Lesson lesson)
    {
        nameText.text = lesson.name;
        durationText.text = $"{lesson.duration.Hours:D2}:{lesson.duration.Minutes:D2}";
        
        editButton.onClick.AddListener(() => EditLesson(lesson));
        deleteButton.onClick.AddListener(() => DeleteLesson(lesson));
    }
    
    private void EditLesson(Lesson lesson)
    {
        // Implement edit functionality
    }
    
    private void DeleteLesson(Lesson lesson)
    {
        // Implement delete functionality
    }
}
