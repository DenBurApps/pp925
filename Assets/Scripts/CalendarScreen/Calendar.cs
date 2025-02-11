using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CalendarScreen
{
    public class Calendar : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _dayTexts;
        [SerializeField] private CalendarItem[] _calendarItems;
        [SerializeField] private Button _nextWeekButton;
        [SerializeField] private Button _previousWeekButton;
        [SerializeField] private TMP_Text _currentMonthYearText;

        public event Action<DateTime> DateSelected;

        private DateTime _currentWeekStart;
        private CalendarItem _selectedItem;

        private void Awake()
        {
            // Initialize with current week
            _currentWeekStart = GetStartOfWeek(DateTime.Today);
            
            // Set up navigation buttons
            _nextWeekButton.onClick.AddListener(NextWeek);
            _previousWeekButton.onClick.AddListener(PreviousWeek);
            
            // Initial population
            PopulateCalendar();
        }

        private void OnDestroy()
        {
            if (_nextWeekButton != null) _nextWeekButton.onClick.RemoveListener(NextWeek);
            if (_previousWeekButton != null) _previousWeekButton.onClick.RemoveListener(PreviousWeek);
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void NextWeek()
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            PopulateCalendar();
        }

        private void PreviousWeek()
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            PopulateCalendar();
        }

        private void PopulateCalendar()
        {
            // Update month/year text
            UpdateMonthYearText();

            // Populate each day
            for (int i = 0; i < 7; i++)
            {
                if (i >= _calendarItems.Length) break;

                DateTime currentDate = _currentWeekStart.AddDays(i);
                
                // Update day text (Sun, Mon, etc.)
                if (i < _dayTexts.Length)
                {
                    _dayTexts[i].text = currentDate.ToString("ddd");
                }

                // Update calendar item
                bool isPastDate = currentDate.Date < DateTime.Today;
                bool isFutureDate = currentDate.Date > DateTime.Today;
                
                _calendarItems[i].UpdateDisplay(currentDate, isPastDate, isFutureDate);
                
                // Set up click handler
                int index = i; // Capture for lambda
                _calendarItems[i].OnItemClicked = () => HandleDateSelection(index);
            }
        }

        private void HandleDateSelection(int index)
        {
            // Deselect previous selection
            if (_selectedItem != null)
            {
                _selectedItem.SetSelected(false);
            }

            // Select new item
            _selectedItem = _calendarItems[index];
            _selectedItem.SetSelected(true);

            // Trigger date selected event
            DateTime selectedDate = _currentWeekStart.AddDays(index);
            DateSelected?.Invoke(selectedDate);
        }

        private void UpdateMonthYearText()
        {
            DateTime weekEnd = _currentWeekStart.AddDays(6);
            
            if (_currentWeekStart.Month == weekEnd.Month)
            {
                _currentMonthYearText.text = _currentWeekStart.ToString("MMMM yyyy");
            }
            else if (_currentWeekStart.Year == weekEnd.Year)
            {
                _currentMonthYearText.text = $"{_currentWeekStart.ToString("MMMM")} - {weekEnd.ToString("MMMM")} {weekEnd.Year}";
            }
            else
            {
                _currentMonthYearText.text = $"{_currentWeekStart.ToString("MMMM yyyy")} - {weekEnd.ToString("MMMM yyyy")}";
            }
        }

        // Optional: Public method to jump to a specific date
        public void JumpToDate(DateTime date)
        {
            _currentWeekStart = GetStartOfWeek(date);
            PopulateCalendar();
        }
    }
}