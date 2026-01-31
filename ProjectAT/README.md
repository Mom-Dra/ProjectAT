# ProjectAT

Unity 기반의 전술 액션 게임 프로젝트입니다. 멀티플레이어를 지원하며, 플레이어와 적 AI 간의 전투 시스템을 핵심으로 합니다.

## 핵심 특징

- **멀티플레이어 지원**: Unity Netcode for GameObjects를 활용한 네트워크 멀티플레이어
- **전술적 AI 시스템**: State Pattern 기반의 적 AI (순찰, 추적, 공격, 엄폐, 수색)
- **엄폐 시스템**: 전술적 전투를 위한 동적 엄폐물 시스템
- **모듈화된 엔티티 구조**: Component 기반의 플레이어 및 AI 엔티티 설계
- **무기 시스템**: State Machine 기반의 총기 및 투척 무기 시스템
- **스킬 시스템**: 타겟팅 기반의 전술 스킬 (지정 사격, 수류탄, 회복)
- **성능 최적화**: Object Pool Pattern을 활용한 리소스 관리

## 기술 스택

| 카테고리 | 기술 |
|----------|------|
| **Engine** | Unity 6000.1.13f1 |
| **Language** | C# |
| **Networking** | Unity Netcode for GameObjects 2.4.4 |
| **Rendering** | Universal Render Pipeline (URP) 17.1.0 |
| **Input** | Unity Input System 1.14.0 |
| **AI** | Unity Behavior Tree 1.0.12, NavMesh |
| **Camera** | Cinemachine 3.1.4 |
| **Effects** | Visual Effect Graph 17.1.0 |

## 프로젝트 구조

```
Assets/Scripts/
├── Abstract/           # 인터페이스 및 추상 클래스
│   └── IDamageable, IHealable, IAttackable, IThrowable
│
├── Entities/           # 엔티티 시스템
│   ├── Player/         # 플레이어 모듈 (Controller, Movement, Combat, Skill)
│   └── Weapon/         # 무기 상태 관리
│
├── Enemy/              # 적 AI 시스템 (State Pattern)
│   ├── IEnemyState.cs  # AI 상태 패턴
│   └── Squad.cs        # 분대 시스템
│
├── Weapon/             # 무기 시스템
│   ├── Gun.cs          # 총기 구현
│   └── IGunState.cs    # 총기 상태 머신
│
├── Skills/             # 스킬 시스템
│   └── DesignatedFire, ThrowGrenade, UseBandage
│
├── Cover/              # 엄폐 시스템
├── View/               # 시야 시스템
├── Network/            # 네트워킹 (ObjectPool)
├── UI/                 # UI 컴포넌트
└── Utils/              # 유틸리티 (Singleton, PoolManager, Extensions)
```

## 아키텍처 패턴

- **State Pattern**: Enemy AI, Gun 상태 관리
- **Singleton Pattern**: Manager 클래스 (PoolManager, CanvasManager)
- **Object Pool Pattern**: 성능 최적화용 오브젝트 풀링
- **Component-Based Entity System**: 모듈화된 엔티티 구조
- **Interface-Based Design**: IDamageable, IAttackable 등

## 상세 문서

프로젝트의 아키텍처, 코드 컨벤션, 시스템 설명 등 상세한 정보는 [AGENTS.md](AGENTS.md)를 참고하세요.
