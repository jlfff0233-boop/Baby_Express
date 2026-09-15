using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 생성된 유니티 ObjectPool로 게임오브젝트를 반환하는 컴포넌트
/// </summary>
public class Poolable : MonoBehaviour
{
    IObjectPool<GameObject> _pool;       //현재 오브젝트를 생성한 풀
    bool _isRented;       //현재 풀에서 대여 중인지 여부

    /// <summary>
    /// 반환할 유니티 ObjectPool 연결
    /// </summary>
    /// <param name="pool">현재 오브젝트를 생성한 풀</param>
    public void Init ( IObjectPool<GameObject> pool )
    {
        _pool = pool;
    }

    /// <summary>
    /// 풀에서 대여된 상태로 설정
    /// </summary>
    public void Rent ()
    {
        _isRented = true;
    }

    /// <summary>
    /// 현재 게임오브젝트를 생성한 풀로 반환
    /// </summary>
    public void ReturnToPool ()
    {
        //중앙 반환과 뷰 반환이 겹치면 한 번만 처리
        if ( _isRented == false ) return;

        _isRented = false;
        _pool.Release( gameObject );
    }
}
