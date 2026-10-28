using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Arena.Presentation.Accessibility
{
    public sealed class NotificationPresenter : MonoBehaviour
    {
        // This is test class
        [SerializeField] private ArenaRunAnnouncer _announcer;
        [SerializeField] private Notification _template;
        [SerializeField] private Transform _root;
        [SerializeField] private NotificationAnimation _appearAnimation;

        private readonly Queue<NotificationAnimationState> _notificationAnimationStates = new(16);

        private IObjectPool<Notification> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<Notification>(
                createFunc: () =>
                {
                    Notification instance = Instantiate(_template);
                    instance.gameObject.SetActive(false);
                    return instance;
                });
        }

        private void OnEnable()
        {
            _announcer.Announced += OnAnnonced;
        }

        private void OnDisable()
        {
            _announcer.Announced -= OnAnnonced;
        }

        private void FixedUpdate()
        {
            int count = _notificationAnimationStates.Count;
            float deltaTime = Time.fixedDeltaTime;

            for (int i = 0; i < count; i++)
            {
                NotificationAnimationState animationState = _notificationAnimationStates.Dequeue();
                Notification notification = animationState.Notification;
                float t = 0f;
                bool isHide = false;

                if (animationState.ElapsedTransitionSecondsToAppear < _appearAnimation.TransitionSecondsToAppear)
                {
                    animationState.ElapsedTransitionSecondsToAppear = Mathf.Min(_appearAnimation.TransitionSecondsToAppear, animationState.ElapsedTransitionSecondsToAppear + deltaTime);
                    t = animationState.ElapsedTransitionSecondsToAppear / _appearAnimation.TransitionSecondsToAppear;

                    notification.SetAlpha(Mathf.Lerp(0, animationState.OriginalAlpha, t));
                    notification.transform.localScale = Vector3.Lerp(Vector3.zero, animationState.OriginalScale, t);
                }
                else if (animationState.ElapsedSecondsToDisappear < _appearAnimation.SecondsToDisappear)
                {
                    animationState.ElapsedSecondsToDisappear = Mathf.Min(_appearAnimation.SecondsToDisappear, animationState.ElapsedSecondsToDisappear + deltaTime);
                }
                else if (animationState.ElapsedTransitionSecondsToDisappear < _appearAnimation.TransitionSecondsToDisappear)
                {
                    animationState.ElapsedTransitionSecondsToDisappear = Mathf.Min(_appearAnimation.TransitionSecondsToDisappear, animationState.ElapsedTransitionSecondsToDisappear + deltaTime);
                    t = animationState.ElapsedTransitionSecondsToDisappear / _appearAnimation.TransitionSecondsToDisappear;

                    notification.SetAlpha(Mathf.Lerp(animationState.OriginalAlpha, 0f, t));
                    notification.transform.localScale = Vector3.Lerp(animationState.OriginalScale, Vector3.zero, t);
                }
                else
                {
                    isHide = true;
                }

                if (isHide)
                {
                    notification.Hide();
                    _pool.Release(notification);
                }
                else
                {
                    _notificationAnimationStates.Enqueue(animationState);
                }
            }
        }

        private void OnAnnonced(string obj)
        {
            Notification notification = _pool.Get();
            notification.Initialize(obj);
            notification.transform.SetParent(_root, false);
            notification.transform.SetAsLastSibling();
            notification.Show();

            _notificationAnimationStates.Enqueue(new()
            {
                Notification = notification,
                OriginalAlpha = notification.OriginalAlpha,
                OriginalScale = notification.OriginalScale,
            });
        }

        [Serializable]
        private struct NotificationAnimation
        {
            public float TransitionSecondsToAppear;
            public float TransitionSecondsToDisappear;
            public float SecondsToDisappear;
        }

        private struct NotificationAnimationState
        {
            public Notification Notification;
            public float ElapsedTransitionSecondsToAppear;
            public float ElapsedTransitionSecondsToDisappear;
            public float ElapsedSecondsToDisappear;
            public float OriginalAlpha;
            public Vector3 OriginalScale;
        }
    }
}
