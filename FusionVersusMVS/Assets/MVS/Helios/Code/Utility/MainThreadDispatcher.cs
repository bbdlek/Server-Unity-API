namespace MVS.Helios.Utility
{
    using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using MTD = MainThreadDispatcher;

public class MainThreadDispatcher : MonoBehaviour
{
    /***********************************************************************
    *                               Singleton
    ***********************************************************************/
    #region .

    private static MTD _instance;
    public static MTD Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MTD>();

                if (_instance == null)
                {
                    GameObject container = new GameObject($"Main Thread Dispatcher");
                    _instance = container.AddComponent<MTD>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);
        }
        else
        {
            if (_instance != this)
            {
                if (GetComponents<Component>().Length <= 2)
                    Destroy(gameObject);
                else
                    Destroy(this);
            }
        }
    }

    #endregion

    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary> 메인 스레드에 작업 요청(코루틴) </summary>
    public void Request(IEnumerator coroutine)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() =>
            {
                StartCoroutine(coroutine);
            });
        }
    }

    /// <summary> 메인 스레드에 작업 요청(메소드) </summary>
    public void Request(Action action)
    {
        Request(ActionWrapper(action));
    }

    /// <summary> 메인 스레드에 작업 요청 및 대기(await) </summary>
    public Task RequestAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();

        void WrappedAction()
        {
            try
            {
                action();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        Request(ActionWrapper(WrappedAction));
        return tcs.Task;
    }

    /// <summary> 메인 스레드에 작업 요청 및 대기(await) + 값 받아오기 </summary>
    public Task<T> RequestAsync<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();

        void WrappedAction()
        {
            try
            {
                var result = action();
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        Request(ActionWrapper(WrappedAction));
        return tcs.Task;
    }

    /// <summary> Action을 코루틴으로 래핑 </summary>
    private IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
}
}