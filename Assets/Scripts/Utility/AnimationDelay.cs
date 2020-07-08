using UnityEngine;

namespace Phoenix.Utility
{
    [RequireComponent(typeof(Animator))]
    public class AnimationDelay : MonoBehaviour
    {
        private Animator _animator;
        public float FirstDelay;
        public float Delay;
        private static readonly int Play1 = Animator.StringToHash("Play");

        private void Start()
        {
            _animator = GetComponent<Animator>();
            InvokeRepeating(nameof(Play), FirstDelay, Delay);
        }

        private void Play()
        {
            _animator.SetTrigger(Play1);
        }
    }
}
