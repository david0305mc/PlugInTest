using UnityEngine;


public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }

            return _instance;
        }
    }    

    public static bool HasInstance => _instance != null;


    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        if(_instance == (T)this)
        {
            OnAwake();
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    protected virtual void OnAwake() { }    
}

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    protected static T _instance;
    public static T Instance => _instance ?? (_instance = new T());
    public static bool HasInstance => _instance != null;
}

