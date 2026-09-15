using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장바구니 모델 - 상품 조회, 추가, 수량 변경, 제거, 딕셔너리 비우기
/// </summary>
public class CartModel
{
    /// <summary>
    /// 장바구니 딕셔너리(아이디, 장바구니 상품)
    /// </summary>
    Dictionary<string, CartItem> _items = new Dictionary<string, CartItem>( );

    /// <summary>
    /// 장바구니 상품 목록(읽기 전용)
    /// </summary>
    public IReadOnlyCollection<CartItem> Items => _items.Values;
    public int Count => _items.Count;
    public bool IsCartEmpty => _items.Count == 0;

    /// <summary>
    /// 장바구니 변경 이벤트
    /// </summary>
    public event Action OnCartChanged;

    /// <summary>
    /// 상품 데이터 확인
    /// </summary>
    /// <param name="data">추가할 상품 데이터</param>
    /// <param name="quantity">추가 수량</param>
    bool IsValidData ( PurchasableData data, int quantity )
    {
        //상품 데이터와 아이디 확인
        //없거나 공백이면
        if ( data == null || string.IsNullOrWhiteSpace( data.Id ) )
        {
            return false;
        }

        //추가 수량 확인
        if ( quantity <= 0 )
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 장바구니 변경 이벤트 발행
    /// </summary>
    void ChangeNotify ()
    {
        OnCartChanged?.Invoke( );
    }

    /// <summary>
    /// 장바구니 상품 조회
    /// </summary>
    /// <param name="id">선택한 상품 아이디</param>
    /// <param name="item">조회한 장바구니 상품</param>
    public bool GetItem ( string id, out CartItem item )
    {
        item = null;

        //아이디 확인
        //공백이면
        if ( string.IsNullOrWhiteSpace( id ) )
        {
            return false;
        }

        //성공 여부와 데이터 반환
        return _items.TryGetValue( id, out item );
    }

    /// <summary>
    /// 장바구니에 선택한 상품 추가
    /// </summary>
    /// <param name="data">추가할 상품 데이터</param>
    /// <param name="quantity">추가 수량</param>
    public bool AddItem ( PurchasableData data, int quantity )
    {
        //추가할 상품 데이터 확인
        if ( IsValidData( data, quantity ) == false )
        {
            return false;
        }

        //이미 있으면 기존 수량에 합산
        if ( GetItem( data.Id, out var item ) )
        {
            //수량 최댓값 초과 방지
            if ( item.Quantity > int.MaxValue - quantity )
            {
                return false;
            }

            //추가 수량만큼 수량 변경
            item.ChangeQuantity( quantity );
        }
        else
        {
            //아니면 추가
            _items.Add( data.Id, new CartItem( data, quantity ) );
        }

        //이벤트 발행
        ChangeNotify( );
        return true;
    }

    /// <summary>
    /// 수량 설정(직접 지정)
    /// </summary>
    /// <param name="id">선택한 상품 아이디</param>
    /// <param name="quantity">지정 수량</param>
    public bool SetQuantity ( string id, int quantity )
    {
        //없는 상품, 음수 수량 차단
        if ( GetItem( id, out var item ) == false || quantity < 0 )
        {
            return false;
        }

        //0이면 장바구니에서 제거
        if ( quantity == 0 )
        {
            _items.Remove( id );
            ChangeNotify( );

            return true;
        }

        //현재 수량과 같으면 변경하지 않음
        if ( item.Quantity == quantity )
        {
            return false;
        }

        item.SetQuantity( quantity );
        ChangeNotify( );

        return true;
    }

    /// <summary>
    /// 수량 변경(버튼)
    /// </summary>
    /// <param name="id">선택한 상품 아이디</param>
    /// <param name="amount">변경 수량</param>
    public bool ChangeQuantity ( string id, int amount )
    {
        //없는 상품, 수량 0이면 종료
        if ( GetItem( id, out var item ) == false || amount == 0 )
        {
            return false;
        }

        //계산 중 정수 범위 초과 방지
        long changedQuantity = ( long ) item.Quantity + amount;

        if ( changedQuantity > int.MaxValue )
        {
            return false;
        }

        //변경 결과가 0 이하면
        if ( changedQuantity <= 0 )
        {
            //장바구니에서 제거
            _items.Remove( id );

            //이벤트 발행
            ChangeNotify( );

            return true;
        }

        //수량 변경
        item.ChangeQuantity( amount );

        //이벤트 발행
        ChangeNotify( );

        return true;
    }

    /// <summary>
    /// 장바구니에서 선택한 상품 제거
    /// </summary>
    /// <param name="id">선택한 상품 아이디</param>
    public bool RemoveItem ( string id )
    {
        //상품이 없으면 종료
        if ( string.IsNullOrWhiteSpace( id ) || _items.Remove( id ) == false )
        {
            return false;
        }

        //이벤트 발행
        ChangeNotify( );

        return true;
    }

    /// <summary>
    /// 장바구니 전체 비우기
    /// </summary>
    public bool Clear ()
    {
        //이미 비어 있으면 종료
        if ( _items.Count == 0 )
        {
            return false;
        }

        //전체 비우기
        _items.Clear( );

        //이벤트 발행
        ChangeNotify( );

        return true;
    }
}
