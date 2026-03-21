# ProjectK

> Unity6 12th **TeamProject · Team 4**  
> Reference: **Escape from Duckov / SuperVive / PUBG**

## 프로젝트 소개
### Unity6 엔진을 활용한 **탑뷰 시점의 캐주얼 배틀로얄** 게임입니다.  
플레이어 **시야 제한(암흑 시야)**, **인벤토리/장비 슬롯**, **FSM 기반 게임 흐름 제어**, **오브젝트 풀링을 포함한 Netcode 멀티플레이** 구조를 설계·구현했습니다.  
싱글에서 **로컬/근거리 네트워크 멀티플레이**로 확장했으며, **컴포넌트 단위 개발**과 **데이터·정책 분리**로 기능 교체 및 테스트 효율을 높였습니다.  
- **팀 구성** : 오융택(팀장), 박광호, 이정균, 이재훈  
- **개발 기간** : 2025.05.12 ~ 2025.05.29 (18일)  
- **기획 의도** : Unity Netcode를 활용한 **캐쥬얼 배틀로얄 게임 개발**  
- **개발 목표**
  - **Unity Netcode** 기반 **Host–Client 멀티플레이** 구조 구현
  - **플레이어 시야 제한**으로 긴장감 있는 전투 연출
  - **인벤토리/장비 슬롯** 시스템 구축
  - **엄폐물 배치**로 전략성 강화
  - **FSM**으로 전체 게임 흐름 제어
- **시연 성과** : 발표 당일 동일 LAN 환경에서 **20인 동시 플레이** 검증 (Host 1 + Client 19)

## 핵심 기능
- **멀티플레이(Netcode)** : Host 기준 이동·상태 동기화, `NetworkVariable`로 게임 상태·타이머·생존 인원 실시간 공유, 시스템 이벤트 브로드캐스팅
- **플레이어 시야 제한** : Raycast FOV 메시 + Stencil Buffer 3셰이더 파이프라인으로 암흑 시야 구현
- **플레이어 FSM** : `Idle / Walk / Dodge / Die` 상태 전이, 체력·스태미나 관리 및 UI 연동
- **총기 FSM** : `None / Attack / Reload` 상태 전이, 장착 아이템 파라미터로 연사속도·장전시간·탄창 동적 적용
- **아이템 파밍/사용** : 누적 확률 감소 방식 랜덤 드롭박스, 소비형/장착형 분기 처리
- **인벤토리·총기 부착물 슬롯** : SubType 배열을 슬롯 타입 테이블로 사용하는 정책 분리 구조
- **오브젝트 풀링** : 총알·이펙트·사운드 재사용으로 성능 안정화 및 GC 최소화

## 수행 역할
 
| 이름 | 역할 | 담당 업무 |
|------|------|-----------|
| **오융택** | 팀장 | <div style="white-space:nowrap">게임 기획 · 업무 분배 · 전체 아키텍처 설계 · 플레이어 시스템 구현(시야/FSM) · GameManager · InputManager</div> |
| **박광호** | 팀원 | <div style="white-space:nowrap">UI 구현(HUD·인벤토리·시스템) · 플레이어 초기 구현 · 이펙트/사운드 시스템</div> |
| **이정균** | 팀원 | <div style="white-space:nowrap">인벤토리 구현 · 아이템·드롭박스 시스템 · 총기 내부 로직 · 멀티플레이 동기화</div> |
| **이재훈** | 팀원 | <div style="white-space:nowrap">레벨 디자인 · 월드 디자인 · 폴리싱 및 QA</div> |

## 실행 방법 (Windows 빌드)
1. `ProjectK.zip` 다운로드  
2. 압축 해제 → `ProjectK.exe` 실행 (최초 실행 시 방화벽 허용)  
3. 게임 플레이 : PC1에서 **Host** 시작 → PC2에서 **Client** 접속 *(동일 네트워크 필수)*

## 기술 스택
- **엔진** : Unity 6 (6.0.34f1)  
- **언어** : C#  
- **네트워크** : Unity Netcode for GameObjects  
- **버전관리** : GitHub Desktop  
- **데이터** : CSV

## 시연 자료
- **프로젝트(배포)** : [`ProjectK.zip`](https://github.com/Unity-Bootcamp-12/ProjectK/releases/download/%EB%B0%B0%ED%8B%80%EB%A1%9C%EC%96%84%EC%A7%AC%EB%BD%95v1.0.0/ProjectK.zip)
- **Presentation**
  - [`배틀로얄짬뽕_4조.pdf`](https://drive.google.com/file/d/1BFWQLrpwrAkx4iI_O8Isnkq3zDCFhuWF/view?usp=drive_link)
  - [`배틀로얄짬뽕_4조.pptx`](https://www.canva.com/design/DAGoy-LzSOc/ESZ7xtQ94T1grkF-UB3t2w/view?utm_content=DAGoy-LzSOc&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=h00d00bec48)

## 저장소
- **GitHub** : [https://github.com/Unity-Bootcamp-12/ProjectK](https://github.com/Unity-Bootcamp-12/ProjectK)

---

## 📬 Contact Us
- **공용 계정** : projectk12th@gmail.com / 010-4810-7201 / [GitHub](https://github.com/Unity-Bootcamp-12/ProjectK)  
- **오융택** : yungtaekoh@gmail.com / 010-4810-7201 / [GitHub](https://github.com/cyphen156)  
- **이정균** : wjdrbsdl@gmail.com / 010-2933-1655 / [GitHub](https://github.com/wjdrbsdl)  
- **박광호** : pkh2956@naver.com / 010-6237-2956 / [GitHub](https://github.com/doncici77)  
- **이재훈** : anrocksa@gmail.com / 010-2595-5993 / [GitHub](https://github.com/LJH0521-91)

## Rules

### Asset Management

모든 외부 에셋은 아래 폴더에 저장하여 관리합니다:

- `DownLoadAssets/`  
  1. stylize  
  2. PolySamuraiPack  
  3. CharacterPack  
  4. polygonParticlePack

### Coding Standard

코딩 스탠다드는 다음 기준을 따릅니다:
1. [Pope Kim의 C# 코딩 스탠다드](https://docs.popekim.com/ko/coding-standards/csharp)  
2. **Unity Basic C# 스타일 가이드**  
   - Unity 라이프사이클 순서 (`Awake → OnEnable/Reset → Start`)  
   - 직관적인 네이밍 및 컴포넌트 접근 방식 준수

### 변수 초기화 규칙

인스펙터 활성화 시 Unity가 값을 덮어쓸 수 있으므로, **필요한 경우가 아니라면 선언부 초기화를 지양**합니다.  
초기화는 아래 기준을 따릅니다:

1. `Awake()` — **인스턴스 생성 시점 초기화**  
2. `OnEnable()` / `Reset()` — **활성화/비활성화 시 초기화**  
3. `Start()` — **외부 참조 객체 초기화 및 연결**
