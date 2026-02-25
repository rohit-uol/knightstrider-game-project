using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheMasterPath
{
    public class Hud : MonoBehaviour
    {
        [Header("Settings")]
        
        [SerializeField]
        int level;

        [Header("References")]
        
        [SerializeField]
        TextMeshProUGUI levelText;

        [SerializeField]
        TextMeshProUGUI healthText;

        [SerializeField]
        TextMeshProUGUI timeText;

        [SerializeField]
        TutorialBox tutorialBox;

        [SerializeField]
        Health health;

        void Start()
        {
            UpdateLevelText();
        }

        public void OnHelpButtonPressed()
        {
            tutorialBox.Toggle();
        }

        public void OnResetButtonPressed()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void Update()
        {
            UpdateHealthText();
            UpdateTimeText();
        }

        void UpdateLevelText()
        {
            levelText.SetText(level.ToString("00"));
        }

        void UpdateHealthText()
        {
            healthText.SetText(health.Value.ToString("00"));
        }

        void UpdateTimeText()
        {
            timeText.SetText(Time.realtimeSinceStartup.ToString("00"));
        }
    }
}