using UnityEngine;

namespace TheMasterPath
{
    public class Timer : MonoBehaviour
    {
        public float PlayTime { get; private set; }

        [SerializeField]
        Movement movement;

        public void Update()
        {
            if (movement.EnableInput)
            {
                PlayTime += Time.deltaTime;
            }
        }
    }
}