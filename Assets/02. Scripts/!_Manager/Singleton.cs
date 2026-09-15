using UnityEngine;

/// <summary>
/// 씬 전환 후에도 하나의 인스턴스를 유지하는 제네릭 싱글톤
/// </summary>
/// <typeparam name="T">싱글톤으로 사용할 컴포넌트 타입</typeparam>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    static T _instance;       //현재 싱글톤 인스턴스

    /// <summary>
    /// 현재 싱글톤 인스턴스
    /// </summary>
    public static T Instance
    {
        get
        {
            if ( _instance == null )
            {
                _instance = FindAnyObjectByType<T>( );

                if ( _instance == null )
                {
                    GameObject singletonObject =
                        new GameObject( $"{typeof( T ).Name} (Singleton)" );

                    _instance = singletonObject.AddComponent<T>( );
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// 현재 컴포넌트가 유지할 싱글톤 인스턴스인지 여부
    /// </summary>
    protected bool IsPrimaryInstance => _instance == this;

    /// <summary>
    /// 싱글톤 인스턴스 등록 또는 중복 제거
    /// </summary>
    protected virtual void Awake ()
    {
        if ( _instance != null && _instance != this )
        {
            Destroy( gameObject );
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad( gameObject );
    }
}