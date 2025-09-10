# Unity_ProjectRT

## 🎮 프로젝트 개요

Unity_ProjectRT는 Unity 기반의 **전략/시뮬레이션 게임** 프로젝트입니다.  
복잡한 게임 로직을 체계적으로 관리하며 확장 가능하도록 설계된 **모듈형 게임 시스템**을 제공합니다.

### 🎯 핵심 특징
- **모듈형 아키텍처**: 조건, 효과, 자원, UI 등이 독립적으로 설계되어 유지보수성 극대화
- **확장 가능한 시스템**: 새로운 기능 추가가 용이한 인터페이스 기반 설계
- **데이터 중심 설계**: ScriptableObject를 활용한 유연한 게임 데이터 관리
- **실시간 전투 시스템**: 유닛 기반 전투와 스킬 시스템 구현

---

## 🚀 주요 시스템

### 🏗️ 건물 & 자원 관리
- **4가지 기본 자원**: Wood(나무), Iron(철), Food(음식), Tech(기술)
- **건물 생산 시스템**: 건물별 자원 생산량 자동 계산
- **토큰 시스템**: Wealth Token, Exchange Token을 통한 고급 자원 관리

### ⚔️ 전투 시스템
- **유닛 기반 전투**: 근접 공격, 원거리 공격, 스킬 시스템
- **AI 컨트롤러**: 자동 이동, 공격, 스킬 사용
- **이펙트 시스템**: 공격 이펙트, 스킬 이펙트, 피격 이펙트

### 🏛️ 팩션 & 연구
- **다중 팩션**: Cat, Wolf 등 다양한 팩션과 고유 특성
- **연구 시스템**: 팩션별 고유 연구와 공통 연구
- **호감도 시스템**: 팩션과의 관계 관리

### 📋 요청 & 이벤트
- **동적 요청 시스템**: 전투, 정복, 생산, 비축 등 다양한 요청 타입
- **랜덤 이벤트**: 긍정적/부정적 이벤트로 게임플레이 변화
- **시간 기반 진행**: 날짜별 자동 진행되는 게임 로직

### 🎨 UI & UX
- **모듈형 UI**: 패널 기반 UI 시스템으로 확장성 확보
- **실시간 업데이트**: 자원, 날짜, 요청 상태 실시간 표시
- **애니메이션**: UI 전환과 피드백 애니메이션

---

## 🛠️ 시스템 요구사항

### 개발 환경
- **Unity**: 2021.3 LTS 이상
- **플랫폼**: Windows, Android 지원
- **언어**: C# (.NET Framework 4.x)

### 권장 사양
- **RAM**: 8GB 이상
- **저장공간**: 2GB 이상
- **그래픽**: DirectX 11 지원

---

## 🎮 게임 플레이

### 시작하기
1. Unity 2021.3 이상 버전으로 프로젝트를 엽니다
2. Assets/Scenes/Main.unity 씬을 실행합니다
3. 게임을 시작하여 자원 관리와 건물 건설을 시작합니다

### 기본 게임플레이
- **자원 수집**: 건물을 건설하여 자원을 자동 생산
- **연구 진행**: 팩션별 연구를 통해 새로운 기술 습득
- **요청 완료**: 다양한 요청을 완료하여 보상 획득
- **전투 참여**: 전투 요청을 통해 전투 시스템 체험

---

## 📁 프로젝트 구조

`
Assets/
 Scripts/                    # 핵심 스크립트
    Battle/                # 전투 시스템
    Common/                # 공통 유틸리티
    Condition/             # 조건 검사 시스템
    Data/                  # 게임 데이터 모델
    Effect/                # 효과 처리 시스템
    Interface/             # 인터페이스 정의
    Manager/               # 게임 매니저
    Message/               # 메시지 시스템
    Type/                  # 커스텀 타입
    Unit/                  # 유닛 시스템
    UI/                    # UI 시스템
 Datas/                     # 게임 데이터 에셋
 Prefebs/                   # 프리팹
 Scenes/                    # 게임 씬
 Image/                     # 이미지 리소스
`

---

## 🔧 개발 가이드

### 새로운 건물 추가
1. Assets/Datas/Building/에 새로운 BuildingData 에셋 생성
2. BuildingType.cs에 새로운 건물 타입 추가
3. UI에서 건물 선택 및 건설 로직 구현

### 새로운 팩션 추가
1. Assets/Datas/Faction/에 새로운 FactionData 에셋 생성
2. FactionType.cs에 새로운 팩션 타입 추가
3. 팩션별 고유 연구 데이터 설정

### 새로운 유닛 추가
1. Assets/Datas/Units/에 새로운 유닛 데이터 생성
2. 유닛 프리팹 생성 및 AI 컨트롤러 설정
3. 전투 시스템에 유닛 등록

---

## 🎯 주요 클래스

### GameManager
게임의 핵심 로직을 관리하는 싱글톤 클래스
- 자원 관리 (Wood, Iron, Food, Tech)
- 토큰 관리 (Wealth, Exchange)
- 날짜 진행 및 이벤트 처리
- UI 매니저 연동

### GameDataManager
모든 게임 데이터를 중앙에서 관리
- 팩션, 연구, 건물 데이터 로드
- 이벤트 및 효과 관리
- 요청 시스템 관리

### MainUIManager
메인 UI의 모든 요소를 관리
- 자원 표시 및 업데이트
- 패널 전환 및 상태 관리
- 사용자 입력 처리

---

## 🐛 알려진 이슈

- 일부 UI 요소에서 한국어 폰트 렌더링 문제
- Android 빌드 시 일부 이펙트 성능 이슈
- 대량의 건물 건설 시 프레임 드롭 현상

---

## 📝 라이선스

이 프로젝트는 교육 및 학습 목적으로 제작되었습니다.

---

## 🤝 기여하기

프로젝트 개선에 참여하고 싶으시다면:

1. **Fork** 이 저장소
2. **Feature Branch** 생성 (git checkout -b feature/AmazingFeature)
3. **Commit** 변경사항 (git commit -m "Add some AmazingFeature")
4. **Push** 브랜치 (git push origin feature/AmazingFeature)
5. **Pull Request** 생성

### 기여 가이드라인
- 코드 스타일: C# 표준 네이밍 컨벤션 준수
- 주석: 모든 public 메서드에 XML 문서 주석 추가
- 테스트: 새로운 기능 추가 시 테스트 코드 작성 권장

---

## 📞 문의

프로젝트 관련 문의사항이나 제안사항이 있으시면 언제든지 연락주세요!

**이슈 리포트**: [GitHub Issues](https://github.com/your-repo/issues)  
**기능 제안**: [GitHub Discussions](https://github.com/your-repo/discussions)

---

<div align="center">

**⭐ 이 프로젝트가 도움이 되었다면 Star를 눌러주세요! ⭐**

Made with ❤️ by Unity Game Developers

</div>
