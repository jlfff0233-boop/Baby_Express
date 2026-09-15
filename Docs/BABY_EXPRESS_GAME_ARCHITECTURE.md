# Baby Express 전체 게임 아키텍처 도식 명세

## 1. 문서 목적

이 문서는 Unity 프로그래머 포트폴리오에 사용할 **Baby Express 전체 게임 아키텍처 도식**의 편집 원본이다.

도식의 목적은 시스템 목록을 많이 보여주는 것이 아니라 다음 구조를 한눈에 전달하는 것이다.

> 고객 주문을 받고, 필요한 자원을 확보해 아기를 제작·배송하며, 그 결과가 기록·결산·성장으로 이어져 다음 영업일의 조건을 변화시킨다.

이 문서는 실제 프로젝트 코드의 클래스와 연결 관계만 사용한다. FigJam에서는 아래 명세를 기준으로 박스와 화살표를 배치하고, 세부 클래스 관계는 필요할 때 보조 도식으로 분리한다.

---

## 2. 전체 도식 편집 원칙

### 2.1 메인 도식의 정보량

- 메인 도식은 약 10~12개 박스로 제한한다.
- 핵심 플레이 루프는 좌에서 우로 읽히게 배치한다.
- 하나의 박스에는 책임 2개와 대표 클래스 2~4개만 표시한다.
- 같은 시스템 내부의 세부 Presenter / View / Builder는 메인 도식에서 펼치지 않는다.
- 시스템 간 연결은 메인 플레이 진행 / 결과 이벤트 / 지원 관계의 세 종류만 사용한다.
- 선이 교차하면 개별 연결을 더 추가하지 않고 공용 Bus 또는 보조 도식으로 분리한다.

### 2.2 화살표 표현

| 표현 | 의미 | 사용 예시 |
| --- | --- | --- |
| 굵은 실선 | 메인 플레이 진행 또는 핵심 Runtime Data 전달 | 주문 → 제작 / 제작 → 배송 |
| 점선 | 성공 결과 또는 상태 변경 이벤트 | 구매·제작·배송 → 일일 기록 |
| 얇은 회색선 | 설정 데이터, 저장, UI 같은 지원 관계 | ScriptableObject → Model |

### 2.3 색상 그룹

| 그룹 | 권장 색상 역할 |
| --- | --- |
| Application / Shared State | 중립색 |
| Core Gameplay | 메인 강조색 |
| Business Feedback | 보조 강조색 |
| Progression | 성장 계열 색상 |
| Presentation / Support | 낮은 채도의 보조색 |

색만으로 의미를 구분하지 않고 그룹 제목과 화살표 라벨을 함께 표시한다.

---

## 3. 메인 도식 레이아웃

```text
┌────────────────────────────────────────────────────────────────────────────┐
│ Application / Shared Foundation                                            │
│ GameManager ── PlayScene Composition Root ── ScriptableObject / PlayState │
└────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────── Core Gameplay Loop ────────────────────────────────┐
│                                                                                     │
│ [Customer Order] → [Procurement] → [Craft] → [Delivery & Evaluation]                │
│                         │                                      │                    │
│                         └─────────── Runtime Result ────────────┘                    │
└─────────────────────────────────────────────────────────────────────────────────────┘
                                      │ 성공 이벤트
                                      ▼
┌────────────────────────────── Business Feedback ────────────────────────────┐
│ [Daily Record] → [Daily / Weekly Settlement] → [Next Week Adjustment]       │
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌──────────────────────────── Progression / Next Day ─────────────────────────┐
│ [Maintenance · Employee · Achievement] → [BusinessDay / Next Day]          │
│                         └──────── 다음 주문과 운영 조건 변경 ────────┘        │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────── Support Layer ──────────────────────────────┐
│ MVP Presentation / Tutorial · Dialogue / Save · Restore / Object Pool       │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 읽는 순서

1. 상단에서 게임의 전역 수명과 런타임 조립 방식을 확인한다.
2. 중앙에서 주문 이행 파이프라인을 좌에서 우로 읽는다.
3. 핵심 시스템의 성공 결과가 기록과 결산으로 내려가는 것을 확인한다.
4. 결산과 성장 결과가 다음 영업일의 주문과 운영 조건으로 돌아가는 순환을 확인한다.
5. 하단 지원 레이어에서 UI, 튜토리얼, 저장, 풀링이 전체 구조를 횡단한다는 것을 확인한다.

---

## 4. 메인 도식 박스 명세

## A. Application / Shared Foundation

### A-1. Global Services

**박스 제목**

```text
GameManager / Global Services
```

**박스 본문**

```text
씬 전환 후에도 유지되는 공용 서비스
Play 시작 요청과 Audio · Settings · Save · Pool 제공
```

**대표 클래스**

- `Singleton<GameManager>`
- `GameManager`
- `AudioManager`
- `SettingsManager`
- `SaveManager`
- `PoolManager`

**아키텍처 책임**

- 씬을 넘어 유지되는 공용 서비스의 수명을 관리한다.
- Title에서 만든 `PlayStartRequest`를 Play 씬으로 전달한다.
- 실제 플레이 규칙과 플레이 Runtime Model은 직접 소유하지 않는다.

### A-2. Play Scene Composition Root

**박스 제목**

```text
PlayScene Composition Root
```

**박스 본문**

```text
Runtime Model 생성 · Presenter 의존성 주입
화면 이동 · 기록 · 영업일 · 저장 Handler 조립
```

**대표 클래스**

- `PlayScene`
- `PlaySceneStartHandler`
- `PlaySceneNavHandler`
- `PlaySceneRecordHandler`
- `PlaySceneDayHandler`
- `PlaySceneSaveHandler`

**아키텍처 책임**

- 공용 Model을 일반 C# 객체로 생성한다.
- Scene의 Presenter에 담당 Model을 전달한다.
- 서로 다른 시스템을 연결하는 화면 이동, 기록, 영업일, 저장 흐름을 Handler로 분리한다.
- 게임 규칙 자체는 담당 Model에 위임한다.

### A-3. Data & Shared Runtime State

**박스 제목**

```text
Authoring Data / Shared Runtime State
```

**박스 본문**

```text
ScriptableObject: 변하지 않는 설계 데이터
PlayStateModel: 날짜 · 자금 · 해금 · 평가 누적
```

**대표 클래스**

- `PurchasableDataMap`
- `OrderGenerationSettingsData`
- `CraftScoreSettingsData`
- `DeliverySettingData`
- `PlayStateModel`

**아키텍처 책임**

- ScriptableObject는 가격, 기본 재고, 주문 난이도, 점수, 배송 배율, 정비 단계 같은 원본 데이터를 제공한다.
- `PlayStateModel`은 현재 자금, 누적 영업일, 상품 해금, 신규 해금, 고등급 평가 누적을 관리한다.
- Runtime 변경값을 ScriptableObject에 저장하지 않는다.

---

## B. Core Gameplay Loop

### B-1. Customer Order

**박스 제목**

```text
Customer Order
```

**박스 본문**

```text
해금 파츠 기반 주문 생성 · 조건 · 기한
Waiting → Production → Crafted → Shipping → Closed
```

**대표 클래스**

- `CustomerOrderModel`
- `CustomerOrder`
- `OrderGeneratorModel`
- `OrderGenerationCoordinator`

**입력**

- `PlayStateModel.TotalDay`
- 해금된 파츠 목록
- 고등급 평가 누적
- 주간 주문 생성 보정
- 주문량 페널티

**출력**

- 제작 대상 `Production` 주문
- 제작·배송 상태 전이
- 거절·취소·실패 결과 이벤트

### B-2. Procurement

**박스 제목**

```text
Procurement
Shop · Cart · Purchase · Inventory
```

**박스 본문**

```text
필요 파츠 선택과 구매 사전 검증
상점 재고 · 인벤토리 · 자금을 하나의 결과로 변경
```

**대표 클래스**

- `ShopModel`
- `CartModel`
- `PurchaseModel`
- `InventoryModel`

**입력**

- 주문에 필요한 파츠 판단
- `PurchasableData` 원본
- 상점 현재 가격·재고
- 현재 자금과 인벤토리 용량

**출력**

- 구매 후 인벤토리 파츠 증가
- 상점 재고 감소
- 자금 감소
- 구매 성공 이벤트

**내부 처리 핵심**

```text
전체 조건 사전 검증
→ 인벤토리 추가
→ 상점 재고 차감
→ 자금 차감
→ 장바구니 초기화
→ 구매 성공 이벤트
```

중간 실패 시 앞에서 변경한 상태를 복구한다.

### B-3. Craft

**박스 제목**

```text
Craft
```

**박스 본문**

```text
보유 파츠 배치와 주문 조건 · 테마 · 점수 판정
파츠 소비 후 확정 CraftResult 저장
```

**대표 클래스**

- `CraftModel`
- `CraftReviewModel`
- `CraftCompleteModel`
- `CraftResult`

**입력**

- `Production` 주문
- 인벤토리 보유 파츠
- 주문 최대 제작 코스트와 파츠 수
- 제작 점수 설정과 연구 보너스

**출력**

- 확정 `CraftResult`
- 인벤토리 사용 파츠 감소
- 주문 `Production → Crafted`
- 제작 성공 이벤트
- `BusinessDayModel` 제작 완료 수 증가

**내부 처리 핵심**

```text
배치 편집
→ 최소 조건과 코스트 검사
→ 주요 요구 · 희망 · 테마 판정
→ 인벤토리 소비 사전 검증
→ CraftResult 복사 저장
→ 주문 Crafted 전환
```

### B-4. Delivery & Evaluation

**박스 제목**

```text
Delivery & Evaluation
```

**박스 본문**

```text
직접 / 직원 배송 일정 확정
제작 결과와 도착 시점으로 등급 · 주문 대금 계산
```

**대표 클래스**

- `DeliveryModel`
- `DeliverySchedule`
- `DeliveryResult`
- `EmployeeModel`

**입력**

- `Crafted` 주문
- 확정 `CraftResult.ReviewData`
- 배송 설정과 배송 기간 보정
- 현재 날짜와 납품 기한
- 직접 배송 슬롯 또는 배송 직원 상태

**출력**

- 주문 `Crafted → Shipping → Closed`
- 직접 배송비 지출
- 확정 배송 등급과 주문 대금
- 자금 증가와 고등급 평가 누적
- 배송 출발·도착 이벤트
- 배송 직원 배치·해제

**책임 경계**

- Craft가 확정한 주문 조건과 테마 판정을 다시 계산하지 않는다.
- Delivery는 제작 점수에 배송 시점·난이도·등급·특수 주문 보정을 적용한다.

---

## C. Business Feedback

### C-1. Daily Record

**박스 제목**

```text
Daily Record / Event Collector
```

**박스 본문**

```text
구매 · 판매 · 제작 · 배송 · 주문 · 직원 결과 수집
결산과 업적 판정의 단일 기록 원본
```

**대표 클래스**

- `PlaySceneRecordHandler`
- `DailyRecordModel`
- `DailyRecord`

**구독 이벤트**

- `PurchaseModel.OnPurchased`
- `InventoryItemActionModel.OnSold`
- `QuickRestockModel.OnRestocked`
- `CraftCompleteModel.OnCraftCompleted`
- `DeliveryModel.OnDeliveryStarted`
- `DeliveryModel.OnDeliveryCompleted`
- `DeliveryModel.OnDeliveryCostPaid`
- `CustomerOrderModel.OnOrderClosed`
- `EmployeeModel.OnHireCostPaid`
- `EmployeeModel.OnEmployeeHired`
- `EmployeeModel.OnWeeklyWagePaid`

**출력**

- 현재 영업일 `DailyRecord`
- 완료된 과거 일일 기록 목록
- 일일·주간 결산 입력
- 업적 판정 입력

### C-2. Daily / Weekly Settlement

**박스 제목**

```text
Daily / Weekly Settlement
```

**박스 본문**

```text
일일 기록을 운영 · 주문 · 평가 · 경제 결과로 집계
주간 평가와 다음 주 주문 생성 보정 생성
```

**대표 클래스**

- `SettlementModel`
- `DailySettlementData`
- `WeeklySettlementData`
- `WeeklyOrderAdjustment`

**입력**

- 확정된 `DailyRecord`
- 현재 주문 상태
- 주간 종료 시 직원 고용 수

**출력**

- 일일 결산
- 과거 주간 결산
- 주간 평가
- 다음 주 주문 수와 난이도 가중치 보정

**피드백 루프**

```text
DailyRecord 목록
→ SettlementModel.CreateWeekly
→ WeeklySettlementData.NextWeekAdjustment
→ OrderGeneratorModel.ApplyWeeklyAdjustment
→ 다음 주 주문 생성 변화
```

---

## D. Progression / Next Day

### D-1. Progression

**박스 제목**

```text
Progression
Maintenance · Employee · Achievement
```

**박스 본문**

```text
영구 성장 단계 · 직원 고용 · 업적 보상
기존 Gameplay Model의 계산 조건을 변경
```

**대표 클래스**

- `MaintenanceModel`
- `MaintenanceEffectModel`
- `MaintenanceUpgradeModel`
- `EmployeeModel`
- `AchvModel`
- `AchvRewardProcessor`

**책임**

- 정비 구매 단계와 적용 단계를 관리한다.
- 시설·연구·편의는 별도 Model이 아니라 `MaintenanceType`으로 분류한다.
- 직원 고용·해고·배송 배치·주급·직원 효과를 관리한다.
- 일일 기록과 결산을 이용해 업적을 판정하고 수동 보상을 지급한다.

### D-2. Gameplay Modifier Bus

메인 도식에서는 정비와 직원에서 모든 시스템으로 개별 화살표를 펼치지 않고 아래 공용 박스를 사용한다.

**박스 제목**

```text
Gameplay Modifiers
```

**박스 본문**

```text
용량 · 재고 · 재입고 · 주문 수
제작량 · 연구 점수 · 배송 · 비용
```

**실제 적용 매핑**

| 대상 시스템 | 정비·직원 효과 |
| --- | --- |
| Inventory | 최대 용량 / 판매율 |
| Shop | 파츠 최대 재고 / 일반 재입고 간격 / 빠른 재입고 비용 |
| Order | 수락 대기 한도 / 일일 최소 주문 수 |
| Craft / BusinessDay | 다음 날 제작 할당량 / 연구 테마 점수 |
| Delivery | 배송 기간 단축 / 직원 배송 슬롯과 배치 |
| Maintenance | 시설·연구 업그레이드 비용 |
| PlayState | 파츠·정보·상품 해금 |
| Employee | 전체 주급 배율 |

### D-3. BusinessDay / Next Day

**박스 제목**

```text
BusinessDay / Next Day Transition
```

**박스 본문**

```text
제작 할당량과 영업 종료 관리
결산 확인 후 다음 날 초기화 순서 통제
```

**대표 클래스**

- `BusinessDayModel`
- `DayPresenter`
- `PlaySceneDayHandler`
- `BankruptcyModel`

**영업 종료 순서**

```text
영업 종료
→ 당일 배송 도착 처리
→ 남은 주문 기한 판정
→ 직원 근무일 기록
→ 7일째 주급 지급
→ 업적 판정
→ DailyRecord 확정
→ 일일 결산
→ 필요 시 주간 결산
```

**다음 영업일 순서**

```text
다음 날 적용 정비 효과 반영
→ TotalDay 증가
→ 주문량 페널티 하루 경과
→ 일반 재입고
→ 제작 · 빠른 재입고 일일 상태 초기화
→ 새 DailyRecord 시작
→ 신규 주문 생성
→ 자동 저장
```

---

## E. Support Layer

### E-1. MVP Presentation

**보조 도식**

```text
Player Input
     │
     ▼
   View ── Input Event ──→ Presenter ── Method Call ──→ Model
     ▲                           │                         │
     └────── ViewData / Result ──┴─────────────────────────┘
```

**책임**

- Model: 데이터, 검증, 계산, 게임 규칙, 실패 처리
- View: 표시, 사용자 입력, 입력 이벤트 전달
- Presenter: View 이벤트 구독, Model 호출 순서, ViewData 변환, 담당 View 갱신
- `PlayScene`: Model 생성과 Presenter 의존성 연결

**적용 시스템**

- Shop
- Inventory
- Order
- Craft
- Delivery
- Maintenance
- Settlement
- Achievement
- Catalog
- Settings

Craft 내부의 `CraftOrderListPresenter`, `CraftPartPresenter`, `CraftPartListPresenter`, `CraftInfoPresenter`, `CraftCompletePresenter`는 별도 상세 MVP 도식에서만 펼친다.

### E-2. Tutorial / Dialogue

**박스 제목**

```text
Tutorial / Dialogue
```

**박스 본문**

```text
기존 Presenter 성공 이벤트와 Model 상태 관찰
대화 · 강조 · 입력 제한만 담당
```

**대표 클래스**

- `TutorialModel`
- `TutorialPresenter`
- `TutorialGuideCoordinator`
- `DialoguePresenter`
- `TutorialView`

**관계**

```text
기존 시스템 Presenter 이벤트
→ 기능별 Tutorial Presenter
→ TutorialPresenter
→ TutorialModel 단계 완료
→ OnProgressChanged
→ 자동 저장
```

튜토리얼은 구매, 제작, 배송, 결산 규칙을 재구현하지 않는다.

### E-3. Save / Restore

**보조 도식**

```text
Runtime Models
     │ snapshot
     ▼
PlaySceneSaveDataBuilder
     │ SaveFileData
     ▼
SaveManager → SaveFileHandler → JSON
     │
     ▼
PlaySceneSaveRestorer / Validator
     │ validated restore
     ▼
Runtime Models
```

**대표 클래스**

- `PlaySceneSaveDataBuilder`
- `PlaySceneSaveRestorer`
- `PlaySceneSaveValidator`
- `PlaySceneSaveHandler`
- `SaveManager`
- `SaveFileHandler`
- `SaveFileData`

**복구 순서**

```text
PlayState
→ Shop
→ Inventory
→ CustomerOrder
→ OrderGenerator
→ CraftResult
→ Employee
→ BusinessDay
→ Delivery
→ DailyRecord
→ Settlement
→ Maintenance
→ Achievement
→ Tutorial
```

**저장하지 않는 일시 상태**

- 장바구니 `CartModel`
- 제작 중 편집 상태 `CraftModel`
- Catalog 검색·필터·페이지 상태
- 결산 확인 대기 화면 상태
- Dialogue 현재 대사 줄
- 현재 코드의 `SettingsValue`

### E-4. Object Pool

**박스 제목**

```text
Shared Object Pool
```

**박스 본문**

```text
프리팹별 Unity ObjectPool
동적 UI 슬롯 대여 · 초기화 · 반환
```

**대표 클래스**

- `PoolManager`
- `Poolable`

**주요 사용처**

- `ShopView`
- `InventoryView`
- `CustomerOrderView`
- `CraftListView`
- `MaintenanceView`
- `SettlementView`
- `LedgerView`

메인 도식에서는 개별 View 연결을 그리지 않고 `Shared Object Pool → Dynamic UI Views` 한 개의 화살표만 사용한다.

---

## 5. 메인 도식 화살표 명세

메인 도식에는 아래 연결만 표시한다.

| 번호 | 출발 → 도착 | 표현 | 화살표 라벨 |
| ---: | --- | --- | --- |
| 1 | GameManager → PlayScene | 얇은 실선 | PlayStartRequest / Global Services |
| 2 | ScriptableObject Data → PlayScene / Models | 회색선 | Authoring Data |
| 3 | PlayScene → Runtime Systems | 얇은 실선 | Model 생성 · Presenter 주입 · Handler 조립 |
| 4 | Customer Order → Procurement | 굵은 실선 | 주문 조건에 필요한 파츠 확보 |
| 5 | Procurement → Craft | 굵은 실선 | 보유 파츠 / Inventory Runtime Data |
| 6 | Customer Order → Craft | 굵은 실선 | Production 주문 · 조건 · 제작 제한 |
| 7 | Craft → Delivery | 굵은 실선 | 확정 CraftResult / 고정 판정 결과 |
| 8 | Delivery → PlayState | 굵은 실선 | 주문 대금 / 고등급 평가 누적 |
| 9 | Order · Purchase · Craft · Delivery · Employee → Daily Record | 점선 Bus | 성공 결과 이벤트 기록 |
| 10 | Daily Record → Settlement | 굵은 실선 | 일일·주간 기록 집계 |
| 11 | Settlement → Customer Order | 굵은 되돌림선 | 다음 주 주문 수 · 난이도 보정 |
| 12 | Progression → Gameplay Modifiers | 굵은 실선 | 정비 단계 · 직원 효과 · 업적 보상 |
| 13 | Gameplay Modifiers → Core Gameplay | 한 개의 Bus | 용량 · 재고 · 주문 · 제작 · 배송 조건 변경 |
| 14 | Settlement → BusinessDay | 굵은 실선 | 결산 확인 후 다음 날 진행 |
| 15 | BusinessDay → Customer Order | 굵은 되돌림선 | 날짜 증가 · 재입고 · 신규 주문 생성 |
| 16 | Runtime Models → Save / Restore | 회색 양방향선 | Snapshot / Validated Restore |
| 17 | System Presenters → Tutorial | 점선 | 성공 행동과 화면 상태 관찰 |
| 18 | PoolManager → Dynamic UI Views | 회색선 | Slot Rent / Return |

---

## 6. 실제 게임 플레이 흐름 문구

FigJam 중앙 또는 포트폴리오 설명에 사용할 흐름이다.

```text
새 게임 초기 자금 · 날짜 · 해금 생성
→ 오늘 주문 생성
→ 주문 확인과 수락
→ 필요한 파츠 구매
→ 인벤토리 기반 파츠 배치
→ 주문 조건 · 테마 · 점수 판정
→ 제작 확정과 파츠 소비
→ 직접 또는 직원 배송 출발
→ 도착 시 등급 · 주문 대금 확정
→ 성공 결과를 DailyRecord에 기록
→ 일일 / 주간 결산
→ 정비 · 직원 · 업적을 통한 성장
→ 다음 영업일 정비 효과 · 재입고 · 신규 주문 반영
```

---

## 7. 메인 도식에서 반드시 강조할 관계

### 1. PlayScene Composition Root

`PlayScene`이 전역 Singleton에 플레이 상태를 몰아넣지 않고 Runtime Model을 생성하고 명시적으로 Presenter와 Handler에 연결한다.

### 2. 원자적 구매 처리

`PurchaseModel`이 Cart / Shop / Inventory / PlayState를 함께 검증하고, 구매 중 실패하면 부분 변경을 복구한다.

### 3. 주문 상태 머신

`CustomerOrderModel`의 `Waiting → Production → Crafted → Shipping → Closed` 전이가 Order / Craft / Delivery를 연결한다.

### 4. 제작 판정과 배송 평가 분리

Craft는 주문 조건·테마·제작 점수를 고정하고, Delivery는 고정 결과에 배송 시점과 보정 배율을 적용한다.

### 5. 이벤트 기반 일일 기록

구매·판매·제작·배송·주문 종료·직원 비용의 성공 이벤트가 `DailyRecordModel`에 모인다.

### 6. 결산 피드백 루프

`DailyRecord → WeeklySettlement → WeeklyOrderAdjustment → OrderGeneratorModel` 흐름이 다음 주 주문 난이도와 수량을 바꾼다.

### 7. 성장 효과의 기존 시스템 재사용

Maintenance와 Employee가 새로운 재고·주문·제작 규칙을 중복 소유하지 않고 기존 담당 Model의 계산값을 변경한다.

### 8. 참조 순서를 보장하는 저장 복구

Order를 복구한 뒤 CraftResult와 Delivery를 복구하고, Employee 상태를 준비한 뒤 배송 직원 참조를 검증한다.

### 9. 기존 시스템을 관찰하는 튜토리얼

튜토리얼은 게임 규칙을 대신 실행하지 않고 기존 Presenter 이벤트와 Model 상태를 이용해 진행 단계만 관리한다.

---

## 8. 메인 도식에서 제외할 세부 내용

아래 내용은 실제 코드에 존재하지만 전체 도식의 가독성을 위해 보조 도식이나 설명문으로 이동한다.

- 모든 View 클래스와 UI 입력 이벤트
- `ShopListPresenter`, `OrderDetailPresenter` 같은 하위 Presenter
- Craft의 세부 Presenter 구조
- ViewData / ViewDataBuilder 전체 목록
- SaveData / SaveRestorer / RestoreState 전체 목록
- 직원 8종의 개별 효과 화살표
- 각 정비 단계별 효과값과 선행 조건
- Tutorial 기능별 Presenter 전체 목록
- 모든 PoolManager 사용 View의 개별 화살표
- 실패 결과 enum과 Rollback 세부 순서
- Catalog 검색·필터·정렬 세부 구조
- Settings 화면의 편집·미리 적용·확정 흐름

---

## 9. FigJam 제작 순서

1. `Customer Order → Procurement → Craft → Delivery`를 중앙 가로축에 배치한다.
2. 중앙 아래에 `Daily Record → Settlement`를 배치하고 핵심 시스템의 이벤트를 하나의 Bus로 모은다.
3. 오른쪽 아래에 `Progression → Gameplay Modifiers → BusinessDay`를 배치한다.
4. `BusinessDay → Customer Order` 되돌림 화살표로 하루 순환을 완성한다.
5. 중앙 위에 `GameManager → PlayScene → Data / Shared State`를 배치한다.
6. 하단에 MVP / Tutorial / Save / Pool을 낮은 강조도의 지원 레이어로 배치한다.
7. 화살표 라벨을 먼저 작성하고, 라벨이 없는 선은 삭제한다.
8. 교차하는 선은 Modifier Bus 또는 Event Bus로 합친다.
9. 박스 안 클래스 이름은 마지막 줄에 작은 텍스트로 표시한다.
10. 축소 화면에서 핵심 루프와 결산 피드백만 읽히는지 최종 확인한다.

---

## 10. 포트폴리오 설명문 초안

> Baby Express는 PlayScene을 Composition Root로 사용해 씬 단위 Runtime Model과 MVP Presentation을 명시적으로 조립합니다. 고객 주문은 구매·재고·제작·배송의 상태 전이를 따라 처리되며, 각 시스템의 성공 결과는 이벤트를 통해 DailyRecord에 모입니다. 누적 기록은 일일·주간 결산과 다음 주 주문 보정으로 이어지고, 정비·직원·업적 시스템은 기존 담당 Model의 계산 조건을 변경해 성장 결과를 다음 영업일의 플레이에 반영합니다. 저장 시스템은 시스템 간 ID 참조를 먼저 검증한 뒤 의존 순서에 맞춰 Runtime 상태를 복구합니다.

---

## 11. 코드 기준 대표 근거 파일

- `Assets/02. Scripts/!_Manager/GameManager.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlayScene.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlaySceneNavHandler.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlaySceneRecordHandler.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlaySceneDayHandler.cs`
- `Assets/02. Scripts/02. Shop/Model/PurchaseModel.cs`
- `Assets/02. Scripts/03. Order/_Model/CustomerOrderModel.cs`
- `Assets/02. Scripts/03. Order/OrderGenerationCoordinator.cs`
- `Assets/02. Scripts/04. Craft/Model/CraftCompleteModel.cs`
- `Assets/02. Scripts/03. Order/Delivery/Model/DeliveryModel.cs`
- `Assets/02. Scripts/05. Day/Model/DailyRecordModel.cs`
- `Assets/02. Scripts/06. Settlement/Model/SettlementModel.cs`
- `Assets/02. Scripts/07. Maintenance/Effect/MaintenanceEffectModel.cs`
- `Assets/02. Scripts/08. Eemployment/Model/EmployeeModel.cs`
- `Assets/02. Scripts/09. SideAction/Achievement/Model/AchvModel.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlaySceneSaveDataBuilder.cs`
- `Assets/02. Scripts/00. Scene/1Play/PlaySceneSaveRestorer.cs`
- `Assets/02. Scripts/00. Scene/Tutorial/_Presenter/TutorialPresenter.cs`
