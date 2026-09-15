using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 프리팹별 유니티 ObjectPool 생성과 오브젝트 대여 및 반환 관리
/// </summary>
public class PoolManager : MonoBehaviour
{
    Dictionary<GameObject, ObjectPool<GameObject>> _poolMap =
        new Dictionary<GameObject, ObjectPool<GameObject>>( );       //프리팹별 오브젝트 풀

    HashSet<GameObject> _activeObjects =
        new HashSet<GameObject>( );       //현재 풀에서 대여 중인 오브젝트

    /// <summary>
    /// 지정 프리팹의 풀에서 게임오브젝트 대여
    /// </summary>
    /// <param name="prefab">생성 기준 프리팹</param>
    /// <param name="parent">대여 후 사용할 부모</param>
    /// <returns>대여한 게임오브젝트</returns>
    public GameObject GetFromPool (
        GameObject prefab, Transform parent )
    {
        ObjectPool<GameObject> pool = GetPool( prefab );
        GameObject instance = pool.Get( );

        //UI 레이아웃 기준을 유지하도록 부모 설정 후 활성화
        instance.transform.SetParent( parent, false );
        instance.SetActive( true );

        return instance;
    }

    /// <summary>
    /// 현재 대여 중인 모든 게임오브젝트를 원래 풀로 반환
    /// </summary>
    public void ReturnAll ()
    {
        //반환 중 원본 컬렉션이 변경되므로 복사본 순회
        var activeObjects = new List<GameObject>( _activeObjects );

        for ( int i = 0; i < activeObjects.Count; i++ )
        {
            Poolable poolable =
                activeObjects [ i ].GetComponent<Poolable>( );

            poolable.ReturnToPool( );
        }
    }

    /// <summary>
    /// 실행 종료 전 활성 오브젝트 전체 반환
    /// </summary>
    void OnApplicationQuit ()
    {
        ReturnAll( );
    }

    /// <summary>
    /// 지정 프리팹의 기존 풀 조회 또는 신규 풀 생성
    /// </summary>
    /// <param name="prefab">생성 기준 프리팹</param>
    /// <returns>프리팹 전용 오브젝트 풀</returns>
    ObjectPool<GameObject> GetPool ( GameObject prefab )
    {
        if ( _poolMap.TryGetValue(
            prefab, out ObjectPool<GameObject> currentPool ) )
            return currentPool;

        //프리팹별 비활성 오브젝트 보관 부모 생성
        GameObject poolParent =
            new GameObject( $"Pool_{prefab.name}" );
        poolParent.transform.SetParent( transform, false );

        ObjectPool<GameObject> pool = null;

        pool = new ObjectPool<GameObject>(
            createFunc: ( ) => CreateInstance(
                prefab, poolParent.transform, pool ),
            actionOnGet: GetInstance,
            actionOnRelease: ( instance ) => ReleaseInstance(
                instance, poolParent.transform ),
            actionOnDestroy: DestroyInstance,
            collectionCheck: true );

        _poolMap.Add( prefab, pool );
        return pool;
    }

    /// <summary>
    /// 프리팹 전용 풀에 사용할 신규 오브젝트 생성
    /// </summary>
    /// <param name="prefab">생성 기준 프리팹</param>
    /// <param name="poolParent">비활성 상태 보관 부모</param>
    /// <param name="pool">생성된 오브젝트를 반환할 풀</param>
    /// <returns>신규 풀 오브젝트</returns>
    GameObject CreateInstance (
        GameObject prefab, Transform poolParent,
        IObjectPool<GameObject> pool )
    {
        GameObject instance =
            Instantiate( prefab, poolParent );

        //반환 책임을 오브젝트 자체에 연결한 뒤 비활성 보관
        Poolable poolable =
            instance.GetOrAddComponent<Poolable>( );
        poolable.Init( pool );
        instance.SetActive( false );

        return instance;
    }

    /// <summary>
    /// 풀에서 대여한 오브젝트 추적 시작
    /// </summary>
    /// <param name="instance">대여한 오브젝트</param>
    void GetInstance ( GameObject instance )
    {
        //중복 반환을 구분하도록 대여 상태 설정
        instance.GetComponent<Poolable>( ).Rent( );

        _activeObjects.Add( instance );
    }

    /// <summary>
    /// 반환된 오브젝트 초기화와 비활성 보관
    /// </summary>
    /// <param name="instance">반환된 오브젝트</param>
    /// <param name="poolParent">비활성 상태 보관 부모</param>
    void ReleaseInstance (
        GameObject instance, Transform poolParent )
    {
        instance.SetActive( false );
        instance.transform.SetParent( poolParent, false );
        _activeObjects.Remove( instance );
    }

    /// <summary>
    /// 풀의 최대 보관 범위를 넘은 오브젝트 제거
    /// </summary>
    /// <param name="instance">제거할 오브젝트</param>
    void DestroyInstance ( GameObject instance )
    {
        _activeObjects.Remove( instance );
        Destroy( instance );
    }
}
