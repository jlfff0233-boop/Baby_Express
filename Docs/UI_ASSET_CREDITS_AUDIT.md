# Baby Express UI 에셋 출처 점검

작성일: 2026-09-02

## 점검 범위

`Play.unity`, `Title.unity`에서 시작해 참조된 Prefab, ScriptableObject, Material의 GUID를 재귀적으로 추적했다.

- 단순히 `Assets` 폴더에 보관된 파일은 제외했다.
- 실제 씬 의존성에 포함된 UI 이미지 94개를 확인했다.
- 캐릭터, 몬스터 파츠, 배경 일러스트, 직원 초상화는 이번 UI 목록에서 제외했다.
- 폴더명과 로컬 라이선스로 확정할 수 있는 항목과, 파일명·이미지 스타일로 추정한 항목을 분리했다.

## 바로 사용할 크레딧 초안

```text
UI Assets
- Paper UI Asset Pack for Games — Lynda Mc Donald (LoudEyes)
- Simple Vector UI Pack — PlayPug
- Bliss GUI — Prinbles
- Free Icon Pack — gvesster
- Casual Game Buttons Vol. 01 — Vektyr
- BlueStone Mobile UI — Evil

Fonts
- Gmarket Sans — Gmarket
- Yangjin — Kim Yangjin
```

`Paper UI Asset Pack for Games`는 제작자 표기가 요구되므로 `Lynda Mc Donald` 표기를 반드시 유지한다. 나머지는 아래 라이선스 메모를 확인한다.

## 확인된 외부 UI 에셋

### 1. Free Icon Pack v3.1 (Basic)

- 제작자: gvesster
- 실제 사용: 55개
- 로컬 위치: `Assets/99. Assets/Free Icon Pack v3.1 (Basic)`
- 원본: https://gvesster.itch.io/free-icon-pack
- 라이선스: 개인·상업 프로젝트 사용 가능, 크레딧 선택 사항, 원본 또는 수정본 재판매 금지
- 로컬 근거: `License.txt` 존재
- 판정: 확정

사용 범위는 Cash, Coin, Ingot, Trophy, Medal, Calendar, Clock, Cart, Settings, Plus/Minus, Checkmark, Info, X 등의 UI 아이콘이다.

### 2. Simple Vector UI Pack

- 제작자: PlayPug
- 실제 사용: 원본 폴더 3개 + 별도 복사본 추정 8개
- 로컬 위치: `Assets/99. Assets/simple_vector_ui_pack_v1.0_free`
- 원본: https://playpug.itch.io/simple-vector-ui-pack
- 라이선스: CC0, 개인·상업 사용과 수정 가능, 크레딧 불필요
- 로컬 근거: 제작자·CC0가 기재된 `license.txt` 존재
- 판정: 원본 폴더 3개 확정, 아래 8개는 파일명과 제품 구성 기준 높은 확률

실제 원본 폴더 파일:

- `checkbox_dig_checked_uw_4x.png`
- `checkbox_dig_unchecked_disabled_uw_4x.png`
- `checkbox_dig_unchecked_uw_4x.png`

별도 복사본으로 보이는 파일:

- `button_depth_uw_4x.png`
- `button_depth_pressed_uw_4x.png`
- `button_depth_focus_uw_4x.png`
- `button_depth_disabled_uw_4x.png`
- `button_depth_border_uw_4x.png`
- `button_depth_border_pressed_uw_4x.png`
- `button_depth_border_focus_uw_4x.png`
- `button_depth_border_disabled_uw_4x.png`

### 3. Bliss GUI

- 제작자: Prinbles
- 실제 사용: 5개
- 로컬 위치: `Assets/99. Assets/Prinbles_GUI_Asset_Bliss (1.0.0)`
- 원본: https://prinbles.itch.io/bliss
- 라이선스: 개인·상업 프로젝트 사용 가능, 원본 재판매 금지, 크레딧 선택 사항
- 로컬 근거: `readme.txt`, 제작자 페이지 연결 HTML 존재
- 판정: 확정

실제 사용 파일:

- `@previews/Button.png`
- `@previews/Panel.png`
- `@previews/ProgressBar.png`
- `asset/png/Button/Rect-Medium/Hover/Background.png`
- `asset/png/Panel/Body/Rounded/Background.png`

주의: `@previews` 이미지가 실제 UI에 연결되어 있다. 배포 전 미리보기 전체 이미지가 의도한 사용 단위인지 확인하는 것이 좋다.

### 4. Paper UI Asset Pack for Games

- 제작자 표기: Lynda Mc Donald
- 배포자명: LoudEyes
- 실제 사용: 원본 폴더 3개 + 별도 복사본 추정 2개
- 로컬 위치: `Assets/99. Assets/Paper`
- 원본: https://loudeyes.itch.io/paper-ui-pack-for-games
- 라이선스: 개인·상업 프로젝트 사용 가능, `Lynda Mc Donald` 크레딧 필수, 원본 파일 재배포·재판매 금지
- 판정: 폴더 파일·이미지 형태·원본 미리보기 기준 높은 확률

실제 사용 파일:

- `Paper/dialog box 10.png`
- `Paper/paper 02.png`
- `Paper/paper 07.png`
- `!/paper 04.png`
- `!/paper 05.png`

### 5. Casual Game Buttons Vol. 01

- 제작자: Vektyr
- 실제 사용: 1개
- 로컬 파일: `Assets/99. Assets/Buttons/CGB01-blue_S_pillShaped_btn.png`
- 원본: https://realvektyr.itch.io/casual-game-buttons-vol-01
- 라이선스: 개인·상업 프로젝트에서 사용·수정 가능, 원본 소스나 경미한 수정본 재판매 금지
- 판정: 파일명 `CGB01`, 규격 `128x128`, 제품 구성과 이미지 스타일 기준 높은 확률

### 6. BlueStone Mobile UI

- 제작자/퍼블리셔: Evil
- 실제 사용 추정: 10개
- 프로젝트 내 원본 흔적: `Assets/99. Assets/BlueStoneUI_SpriteSheet.png`
- 원본: https://assetstore.unity.com/packages/2d/gui/bluestone-mobile-ui-44000
- 라이선스: Unity Asset Store Standard EULA 대상
- 판정: 원본 스프라이트시트 이름, `Blue` 파일명, 동일한 청색 테두리 스타일 기준 높은 확률

관련 파일 추정:

- `09_IconArrowLeft (3)Blue.png`
- `09_IconArrowRigh (3)Blue.png`
- `checkbox empty.png`
- `checkbox tick.png`
- `Color 12_BigBlue.png`
- `icons arrow down.png`
- `icons arrow left.png`
- `icons arrow right.png`
- `Panel1Blue.png`
- `RectangleButtonL_X2Blue.png`

주의: 개별 PNG가 스프라이트시트에서 추출된 것인지 원본 패키지 파일인지 로컬 기록만으로 완전히 확정할 수 없다. Unity Asset Store 구매·다운로드 기록에서 패키지명을 한 번 확인한다.

## 실제 사용 폰트

### Gmarket Sans Medium

- 실제 사용 에셋: `Assets/06. Fonts/GmarketSansMedium SDF.asset`
- 공식 출처: https://corp.gmarket.com/fonts/
- 라이선스: SIL Open Font License, 개인·기업의 영리·비영리 목적 사용 가능
- 판정: 확정

### 양진체 v0.93

- 제작자: 김양진
- 실제 사용 에셋: `Yangin_v0.93.asset`, 검정/흰색 외곽선 Material
- 라이선스 확인: https://noonnu.cc/font_page/330
- 라이선스: 개인·기업 상업 사용 가능, 폰트 파일 수정·판매 금지, 배포 상태 그대로 사용
- 판정: 확정

## GPT 생성 후 프로젝트 내부 가공 이미지

다음 7개는 사용자가 GPT로 생성한 뒤 프로젝트 UI 용도에 맞게 사용한 이미지다.

- `Assets/99. Assets/ConvenienceIcons.png`
- `Assets/99. Assets/FacilityIcons.png`
- `Assets/99. Assets/ThemeIcons_2.png`
- `Assets/Resources/Icons/ConvenienceIcon.png`
- `Assets/Resources/Icons/EmployeeIcon.png`
- `Assets/Resources/Icons/FacilityIcon.png`
- `Assets/Resources/Icons/ResearchIcon.png`

외부 에셋 제작자 크레딧 대상에서는 제외하고, 필요하면 크레딧에 `AI-assisted UI icons` 또는 사용한 생성 도구명을 별도로 표기한다.

## 보관 중이지만 현재 두 씬에서 확인되지 않은 대표 항목

- `Prinbles_GUI_Asset_Solid (1.0.0)`
- `DungGeunMo SDF.asset`
- `BlueStoneUI_SpriteSheet.png` 원본 시트 자체

위 항목은 폴더에 존재하지만 현재 `Play`/`Title` 씬의 재귀 의존성에서는 직접 사용 이미지로 잡히지 않았다. 다만 BlueStone에서 잘라낸 개별 PNG가 사용된 것으로 추정된다.

## 배포 전 확인할 일

1. Unity Asset Store 계정에서 `BlueStone Mobile UI` 취득 내역을 확인한다.
2. `Paper UI Asset Pack for Games — Lynda Mc Donald`는 반드시 크레딧에 넣는다.
3. 배포 플랫폼에 AI 생성물 공개 규칙이 있으면 7개 생성·가공 아이콘을 해당 기준에 맞춰 표기한다.
4. 출시 시점에 각 배포 페이지의 최신 라이선스를 다시 저장하거나 캡처한다.

이 문서는 프로젝트 참조와 공개 배포 페이지를 대조한 작업용 기록이며 법률 자문은 아니다.
