using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace RealTime
{
    public class RealTimeNotifier : MonoBehaviour
    {
        public static event Action OnTick; // Подписка через Action
        public static UnityEvent TickEvent = new UnityEvent(); // Подписка через UnityEvent
        
        [SerializeField] private float tickInterval = 2f;

        private void Start()
        {
            StartCoroutine(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(tickInterval);
                Notify();
            }
        }

        private void Notify()
        {
            OnTick?.Invoke();
            TickEvent.Invoke();
        }
    }
}
