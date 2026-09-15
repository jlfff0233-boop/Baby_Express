# CHAT HANDOFF

## PURPOSE

2026-08-26 채팅 이동을 위한 임시 인수인계 문서다.

* 프로젝트 공식 상태의 단일 원본은 `GUIDELINE.md` / `ROADMAP.md` / `TODO.md` / `DECISION.md` / `System` / `DONE.md`이며 이 문서는 새 채팅의 빠른 문맥 복구만 담당한다.
* 새 채팅에서는 이 문서만 믿고 바로 수정하지 말고 `GUIDELINE.md`의 `WORK_START` 순서와 실제 코드 / Git 상태를 다시 확인한다.
* 인수인계가 끝난 뒤 계속 관리할 영구 메모가 아니므로 확정 사항은 담당 System 문서와 `DONE.md`에 반영한다.

## CURRENT_STATE

* 작업 트리는 인수인계 문서 생성 전 기준으로 깨끗했다.
* 확인한 최신 커밋은 `773b4ec UI 연출/교체(인벤토리, 정비, 주문), 슬롯 오브젝트 풀링`이다.
* 현재 전체 우선순위는 UI 소스 이미지 교체와 공용 UI 통합 검증이다.
* 상점 / 제작 / 정비 메인 화면에는 `PanelTweenView` 표시·퇴장·즉시 숨김 흐름이 연결되어 있다.
* 상점 / 제작 / 정비의 사용자 닫기 이벤트는 메인 패널 퇴장 완료 후 발행하며, 불러오기 초기화는 `HideInstant()`를 사용한다.
* 사이드 패널의 좌우 슬라이드 연출은 메인 패널 작업 이후 단계로 미뤄 둔 상태다.
* 사용자 요청에 따라 Unity 빌드 / C# 빌드 / `dotnet build`는 직접 실행하지 않는다.

## IMMEDIATE_TASK

상점과 인벤토리의 카테고리 UI를 개편한다.

현재 UI:

* 전체만 `Button`이다.
* 파츠 / 시설 / 소모용품은 각각 별도 `TMP_Dropdown`이다.
* 정렬용 `TMP_Dropdown`이 별도로 있다.

사용자가 원하는 UI:

* 전체 / 파츠 / 시설 / 소모용품을 모두 메인 카테고리 `Button`으로 변경한다.
* 기존 카테고리별 드롭다운 3개를 제거한다.
* 정렬 드롭다운 옆에 공용 `분류 드롭다운` 1개를 둔다.
* 선택한 메인 카테고리에 따라 분류 드롭다운 옵션을 동적으로 교체한다.

아직 이 개편에 대한 스크립트 수정은 하지 않았다. 직전 요청은 수정 계획이었고 계획만 답변했다.

## AGREED_FLOW

메인 카테고리와 분류 옵션은 다음 기준으로 처리한다.

| 메인 버튼 | 분류 드롭다운 옵션 | 버튼 클릭 직후 목록 |
| --- | --- | --- |
| 전체 | 전체 | 전체 아이템 |
| 파츠 | 파츠 전체 / `PartType` 표시명 | 전체 파츠 |
| 시설 | 시설 전체 / 장비 / 가구 / 확장 | 시설 전체 |
| 소모용품 | 소모용품 전체 | 소모용품 전체 |

* 파츠 표시명은 기존 `PartTypeUtility.GetDisplayName()`을 재사용한다.
* 버튼을 누르면 분류 옵션을 바꾸고 선택값을 0번 전체 항목으로 초기화한 뒤 목록을 즉시 갱신한다.
* 같은 메인 버튼을 다시 눌러도 해당 카테고리 전체로 돌아간다.
* 카테고리를 바꿔도 현재 정렬값은 유지한다.
* 전체와 소모용품은 현재 분류가 하나뿐이므로 분류 드롭다운을 표시하되 입력은 비활성화하는 계획이다.
* 상점 재진입은 기존처럼 전체 / 기본 정렬로 초기화한다.
* 인벤토리 재진입은 기존처럼 선택한 카테고리 / 분류 / 정렬을 유지한다.

## IMPLEMENTATION_SCOPE

### ShopView

대상 파일: `Assets/02. Scripts/02. Shop/View/ShopView.cs`

현재 직렬화 필드:

* `_allButton`
* `_partDropdown`
* `_facilityDropdown`
* `_consumableDropdown`
* `_sortDropdown`

변경 예정:

* `_allButton` 유지
* `_partDropdown` -> `_partButton`
* `_facilityDropdown` -> `_facilityButton`
* `_consumableDropdown` -> `_consumableButton`
* 공용 `_filterDropdown` 추가
* `_sortDropdown` 유지

현재의 `ResetCategory(TMP_Dropdown)`과 카테고리별 드롭다운 입력 연결은 제거하고 다음 흐름으로 교체한다.

* 메인 버튼 선택
* 현재 메인 카테고리 저장
* `_filterDropdown.SetOptions()`로 옵션 교체
* 0번 옵션을 이벤트 없이 선택
* 기존 카테고리 이벤트를 이용해 목록 갱신
* 분류 드롭다운 입력은 현재 메인 카테고리에 맞는 기존 이벤트로 전달

기존 이벤트를 유지하면 `ShopListPresenter`의 필터 로직을 그대로 사용할 수 있다.

* 파츠 버튼 / 파츠 분류: `OnPartSelected(int)`
* 시설 버튼 / 시설 분류: `OnFacilitySelected(int)`
* 소모용품 버튼: `OnConsumableSelected()`
* 전체 버튼: `OnAllSelected`
* 정렬: `OnSortSelected(int)`

상점의 기존 `ResetSelection()`은 메인 카테고리를 전체로 바꾸고 분류 옵션 / 분류 값 / 정렬 값을 초기화하도록 수정한다.

### InventoryView

대상 파일: `Assets/02. Scripts/01. Inventory/View/InventoryView.cs`

현재 직렬화 필드:

* `_allButton`
* `_partDropdown`
* `_facilityDropdown`
* `_consumableDropdown`
* `_sortDropdown`

변경 예정:

* `_allButton` 유지
* `_partDropdown` -> `_partButton`
* `_facilityDropdown` -> `_facilityButton`
* `_consumableDropdown` -> `_consumableButton`
* 공용 `_filterDropdown` 추가
* `_sortDropdown` 유지

상점과 동일한 버튼 / 공용 분류 드롭다운 흐름을 적용한다. 기존 이벤트를 유지하면 `InventoryListPresenter`의 필터 로직을 그대로 사용할 수 있다.

* 파츠: `OnPartSelected(int)`
* 시설: `OnFacilitySelected(int)`
* 소모용품: `OnConsumableSelected(int)`이며 현재 유효 값은 0뿐이다.
* 전체: `OnAllSelected`
* 정렬: `OnSortSelected(int)`

### Presenter / Model

다음 로직은 현재 구조를 재사용하는 계획이다.

* `ShopListPresenter`
* `InventoryListPresenter`
* `ShopListBuilder`
* `InventoryListBuilder`
* `ShopModel`
* `InventoryModel`
* 정렬 enum과 정렬 처리

View가 기존 이벤트와 필터 인덱스 규약을 유지하면 Presenter / Model의 실질적인 변경은 필요하지 않다.

## UNITY_INSPECTOR

씬 / 프리팹 / YAML은 Codex가 직접 수정하지 않는다. 사용자가 Unity에서 다음 UI 오브젝트를 구성하고 View 참조를 연결해야 한다.

* 전체 / 파츠 / 시설 / 소모용품 버튼
* 공용 분류 드롭다운
* 정렬 드롭다운

분류와 정렬 옵션은 코드에서 생성하므로 Inspector 옵션은 비워도 된다. 공용 `TMP_Dropdown.SetOptions()` 확장 함수는 이미 존재한다.

## VERIFICATION

사용자가 Play Mode에서 확인할 항목:

* 각 메인 버튼을 누르면 해당 카테고리 전체가 즉시 표시되는지
* 파츠 / 시설 버튼에 따라 분류 옵션이 올바르게 교체되는지
* 분류 변경 시 해당 세부 목록만 표시되는지
* 카테고리를 변경해도 정렬값이 유지되는지
* 같은 버튼을 다시 누르면 분류가 전체로 초기화되는지
* 전체 / 소모용품에서 분류 드롭다운 입력이 비활성화되는지
* 상점 재진입 초기화와 인벤토리 재진입 상태 유지가 기존대로 동작하는지
* 슬롯 풀링 / 선택 강조 / 메인 패널 트윈에 회귀 문제가 없는지

## COLLABORATION_NOTES

* 사용자가 `계획`을 요청하면 설명만 하고 파일을 수정하지 않는다.
* 사용자가 `코드 보여줘`라고 하면 변경 코드를 제시하는 요청이며, 직접 수정 승인이 명시되지 않았다면 파일을 수정하지 않는다.
* 사용자가 `네가 수정`, `진행`, `수정 후 보고`처럼 직접 수정을 명시했을 때만 실제 파일을 변경한다.
* 사용자가 대기를 요청하면 추가 조사 / 수정 / 메모 갱신을 하지 않는다.
* 사용자 말투를 따라 하지 않고 존댓말로 간결하게 답한다.
* 사용자 변경과 관련 없는 dirty 파일은 손대거나 되돌리지 않는다.
* 씬 / 프리팹 / YAML / `.meta`는 직접 수정하지 않는다.
* 빌드와 컴파일은 사용자가 명시적으로 요청한 경우에만 실행하며, 직접 확인하지 않은 결과를 성공으로 보고하지 않는다.

## NEXT_CHAT_START

1. `GUIDELINE.md`의 `WORK_START` 순서대로 공식 메모를 읽는다.
2. `git status --short`와 위 대상 View / Presenter의 현재 코드를 다시 확인한다.
3. 사용자가 Unity UI 오브젝트를 먼저 교체했는지 확인한다.
4. 사용자의 요청이 계획 / 코드 제시 / 직접 수정 중 무엇인지 구분한다.
5. 직접 수정 승인이 있으면 View 중심으로 구현하고 Presenter / Model은 기존 필터 규약을 유지한다.
