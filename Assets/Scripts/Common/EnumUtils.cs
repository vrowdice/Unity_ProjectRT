using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Random.Range를 사용하기 위해 필요

public static class EnumUtils
{
    /// <summary>
    /// Returns all values of a given Enum type as a List.
    /// </summary>
    public static List<T> GetAllEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Returns a random integer value from the specified Enum type.
    /// Excludes 'None' and 'Max' if they exist as the first/last elements.
    /// </summary>
    public static int GetRandomEnumValueInt<T>() where T : Enum
    {
        // 1. Enum의 모든 유효한 값을 가져옵니다.
        List<T> allValues = GetAllEnumValues<T>();

        // 2. 'None' (0으로 시작하는 경우)과 'Max' (마지막에 위치하는 경우)를 필터링합니다.
        //    이것은 관습적인 이름 규칙을 따르며, Enum 정의에 따라 달라질 수 있습니다.
        //    Enum이 0부터 시작하고 'None'이 0인 경우, 첫 번째 요소를 제외.
        //    Enum의 마지막 요소가 'Max'이고 그게 총 개수를 나타내는 경우, 마지막 요소를 제외.
        //    더 정확한 필터링이 필요하다면 T.ToString()으로 이름을 확인하거나, 특정 값을 직접 비교해야 합니다.
        List<T> usableValues = new List<T>(allValues); // 복사본 생성

        // T가 Enum이므로, T의 값을 int로 캐스팅하여 0과 마지막 값과 비교해봅니다.
        // 이것은 Enum 정의에 따라 달라질 수 있는 '추측'에 가깝습니다.
        // 만약 None = 0이 아닐 수도 있고, Max가 없을 수도 있기 때문입니다.
        // 가장 안전한 방법은 Enum 정의 시 'None'과 'Max'를 제외한
        // 실제 사용할 Enum 멤버들만으로 `GetAllEnumValues`를 부르는 것입니다.
        // 하지만 여기서는 제네릭이므로 일반적인 패턴을 따르겠습니다.

        // None/Max 필터링 (가장 일반적인 패턴에 대한 추정)
        if (usableValues.Count > 0)
        {
            // Enum 값이 0인 'None'을 제외 (만약 첫 번째 요소가 None이고 값이 0이라면)
            if (Convert.ToInt32(usableValues[0]) == 0 && usableValues[0].ToString().Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                // .RemoveAt(0) 보다 Skip(1)이 더 안전합니다.
                usableValues = usableValues.Skip(1).ToList();
            }

            // Enum 값이 마지막이고 'Max'인 경우 (보통 총 개수를 나타냄)
            if (usableValues.Count > 0) // Skip 후에도 요소가 남아있는지 확인
            {
                T lastValue = usableValues[usableValues.Count - 1];
                // 마지막 요소의 이름이 'Max'이고 해당 값이 Enum의 개수를 나타낼 때
                if (lastValue.ToString().Equals("Max", StringComparison.OrdinalIgnoreCase))
                {
                    // Max가 순수한 Enum 값의 개수를 나타내는 경우만 제외
                    if (Convert.ToInt32(lastValue) == (allValues.Count - (allValues[0].ToString().Equals("None", StringComparison.OrdinalIgnoreCase) ? 1 : 0)))
                    {
                        usableValues.RemoveAt(usableValues.Count - 1);
                    }
                }
            }
        }


        if (usableValues.Count == 0)
        {
            Debug.LogWarning($"No usable enum values found for type {typeof(T).Name} after filtering. Returning -1.");
            return -1; // 또는 0 등 적절한 기본값
        }

        // 3. 필터링된 멤버들 중에서 랜덤 인덱스를 선택합니다.
        int randomIndex = UnityEngine.Random.Range(0, usableValues.Count);

        // 4. 선택된 Enum 멤버를 int로 캐스팅하여 반환합니다.
        return Convert.ToInt32(usableValues[randomIndex]);
    }

    /// <summary>
    /// Returns a random value from the specified Enum type.
    /// Excludes 'None' and 'Max' if they exist as the first/last elements.
    /// </summary>
    public static T GetRandomEnumValue<T>() where T : Enum
    {
        int randomInt = GetRandomEnumValueInt<T>();
        if (randomInt == -1) // 에러 처리
        {
            return default(T); // Enum의 기본값 (보통 0에 해당하는 멤버) 반환
        }
        return (T)Enum.ToObject(typeof(T), randomInt);
    }
}