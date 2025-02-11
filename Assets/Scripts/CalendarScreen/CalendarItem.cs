using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CalendarScreen
{
    public class CalendarItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _selectedImage;
        [SerializeField] private TMP_Text _dateText;
        
        [Header("Color Settings")]
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _previousDateColor = Color.gray;
        [SerializeField] private Color _nextDateColor = Color.white;
        [SerializeField] private Color _currentDateColor = Color.white;
        
        [Header("Optional Settings")]
        [SerializeField] private bool _hideSelectedImageOnStart = true;
        
        // Public callback for click events
        public Action OnItemClicked { get; set; }
        
        // Private variables
        private Button _button;
        private DateTime _currentDate;
        private bool _isSelected;

        private void Awake()
        {
            // Get or add required button component
            _button = GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
            }
            
            // Setup click listener
            _button.onClick.AddListener(HandleClick);
            
            // Initialize selected image state
            if (_selectedImage != null && _hideSelectedImageOnStart)
            {
                _selectedImage.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
            // Clear callback to prevent memory leaks
            OnItemClicked = null;
        }

        private void HandleClick()
        {
            OnItemClicked?.Invoke();
        }

        /// <summary>
        /// Updates the calendar item's display with the specified date and state
        /// </summary>
        /// <param name="date">The date to display</param>
        /// <param name="isPastDate">Whether this date is in the past</param>
        /// <param name="isFutureDate">Whether this date is in the future</param>
        public void UpdateDisplay(DateTime date, bool isPastDate, bool isFutureDate)
        {
            _currentDate = date;
            
            if (_dateText != null)
            {
                // Update date number
                _dateText.text = date.Day.ToString();
                
                // Update text color based on date status
                if (isPastDate)
                {
                    _dateText.color = _previousDateColor;
                }
                else if (isFutureDate)
                {
                    _dateText.color = _nextDateColor;
                }
                else // Current date
                {
                    _dateText.color = _currentDateColor;
                }
            }
            
            // Update interactability
            if (_button != null)
            {
                _button.interactable = true; // You can add logic here if you want to disable certain dates
            }
        }

        /// <summary>
        /// Sets the selected state of the calendar item
        /// </summary>
        /// <param name="selected">Whether the item should be selected</param>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            
            // Update selection indicator
            if (_selectedImage != null)
            {
                _selectedImage.enabled = selected;
            }
            
            // Update text color
            if (_dateText != null)
            {
                _dateText.color = selected ? _selectedColor : GetDefaultTextColor();
            }
        }

        /// <summary>
        /// Gets the appropriate text color based on the date status
        /// </summary>
        private Color GetDefaultTextColor()
        {
            if (_currentDate.Date == DateTime.Today)
            {
                return _currentDateColor;
            }
            else if (_currentDate.Date < DateTime.Today)
            {
                return _previousDateColor;
            }
            else
            {
                return _nextDateColor;
            }
        }

        /// <summary>
        /// Gets the current date assigned to this calendar item
        /// </summary>
        public DateTime GetCurrentDate()
        {
            return _currentDate;
        }

        /// <summary>
        /// Checks if the calendar item is currently selected
        /// </summary>
        public bool IsSelected()
        {
            return _isSelected;
        }
    }
}