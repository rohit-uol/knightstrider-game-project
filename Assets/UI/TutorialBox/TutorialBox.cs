using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMasterPath
{
    public class TutorialBox : MonoBehaviour
    {
        [SerializeField] GameObject ui;
        [SerializeField] bool showInEditor = false;
        [SerializeField] InputActionAsset inputActions;

        const string HELP_ACTION_NAME = "UI/Help";

        public bool IsOpen => ui.activeSelf;

        void Start()
        {
            inputActions.FindAction(HELP_ACTION_NAME).Enable();
            inputActions.FindAction(HELP_ACTION_NAME).performed += OnHelp;
        }

        void OnDestroy()
        {
            inputActions.FindAction(HELP_ACTION_NAME).performed -= OnHelp;
        }

        public void OnHelp(InputAction.CallbackContext ctx)
        {
            Toggle();
        }

        public void Toggle()
        {
            ui.SetActive(!ui.activeSelf);
        }

        public void OnPlay()
        {
            ui.SetActive(false);
        }
    }
}