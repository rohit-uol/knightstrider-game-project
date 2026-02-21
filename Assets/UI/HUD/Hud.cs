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
        }

        void UpdateLevelText()
        {
            levelText.SetText($"Lvl {level}");
        }

        void UpdateHealthText()
        {
            healthText.SetText($"Hp {health.Value}");
        }
    }
}