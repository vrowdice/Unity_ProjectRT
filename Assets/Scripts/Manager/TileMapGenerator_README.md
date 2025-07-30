# 절차적 맵 생성기 (TileMapGenerator) 사용법

## 개요
셀룰러 오토마타(Cellular Automata)를 사용하여 절차적으로 맵을 생성하는 시스템입니다.

## 주요 기능
- **셀룰러 오토마타**: 주변 타일의 상태를 기반으로 자연스러운 형태 생성
- **높이 맵 & 바이옴 맵**: 두 개의 독립적인 맵 데이터 생성
- **스무딩**: 여러 번의 반복을 통해 부드러운 형태 생성
- **시드 기반 생성**: 동일한 시드로 같은 맵 재생성 가능

## 설정 방법

### 1. 기본 설정
1. 빈 GameObject 생성
2. `TileMapGenerator` 스크립트 추가
3. Grid 컴포넌트 추가 (2D Grid)

### 2. Inspector 설정
```
Grid Settings:
- Grid: 2D Grid 컴포넌트 참조

Tile Prefabs:
- Tile Prefabs: 바이옴별 타일 프리팹 배열
  - [0]: 일반 블록 프리팹
  - [1]: 높은 블록 프리팹

Generation Settings:
- Seed: 맵 생성 시드 (빈 값이면 랜덤)
- Use Random Seed: 랜덤 시드 사용 여부
- Size: 맵 크기 (X, Y)
- Normal Block Percent: 일반 블록 비율 (0-100)
- Normal Biome Percent: 일반 바이옴 비율 (0-100)

Smoothing:
- Smoothing Factor: 스무딩 반복 횟수 (기본값: 5)
```

## 권장 설정값
- **Size**: 50x50 (테스트용), 100x100 (실제 사용)
- **Normal Block Percent**: 45 (균형잡힌 분포)
- **Normal Biome Percent**: 45 (균형잡힌 분포)
- **Smoothing Factor**: 5 (자연스러운 형태)

## 사용법

### 1. 자동 생성
- 게임 시작 시 자동으로 맵 생성 및 타일 배치

### 2. 수동 생성
- Inspector에서 "Generate New Map" 컨텍스트 메뉴 사용
- 코드에서 `GenerateNewMap()` 메서드 호출

### 3. 맵 데이터 접근
```csharp
TileMapGenerator generator = GetComponent<TileMapGenerator>();
int[,] heights = generator.MapHeights;  // 높이 맵 데이터
int[,] biomes = generator.MapBiomes;    // 바이옴 맵 데이터
```

## 알고리즘 설명

### 셀룰러 오토마타 규칙
1. 각 타일의 주변 8개 타일 확인
2. 주변 타일 중 1인 타일 개수 계산
3. 개수에 따라 현재 타일 상태 결정:
   - 4개 초과: 1로 변경
   - 4개 미만: 0으로 변경
   - 4개: 현재 상태 유지

### 스무딩 효과
- 1회: 기본적인 군집 형성
- 3-5회: 자연스러운 형태
- 10회 이상: 과도한 스무딩 (단조로운 형태)

## 팁
1. **성능**: 큰 맵에서는 스무딩 횟수를 줄이세요
2. **다양성**: 시드를 변경하여 다양한 맵 생성
3. **확장성**: 새로운 바이옴 타입 추가 가능
4. **최적화**: 타일 프리팹은 간단한 구조로 유지

## 참고 자료
- [Unity] Procedural Cave Generation (E01. Cellular Automata)
- Sebastian Lague의 절차적 맵 생성 튜토리얼 