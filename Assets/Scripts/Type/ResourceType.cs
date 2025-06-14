using UnityEngine;

public class ResourceType
{
    public enum TYPE
    {
        Wood,
        Iron,
        Food,
        Tech,
    }

    /// <summary>
    /// ResourceType.TYPE Enum의 유효한 멤버들 중 랜덤한 하나의 정수(int) 값을 반환합니다.
    /// </summary>
    public static int GetRandomTypeId()
    {
        // 1. ResourceType.TYPE Enum의 모든 멤버 값을 배열로 가져옵니다.
        TYPE[] allTypes = (TYPE[])TYPE.GetValues(typeof(TYPE));

        // 2. 만약 Enum에 'None'이나 'Max'처럼 랜덤 선택에서 제외하고 싶은 특수 멤버가 있다면
        //    여기서 필터링하는 로직을 추가할 수 있습니다.
        //    현재 ResourceType.TYPE 정의에는 그런 멤버가 없으므로 모든 Enum 멤버가 유효하다고 간주합니다.

        if (allTypes.Length == 0)
        {
            Debug.LogWarning("ResourceType.TYPE enum has no defined members. Cannot get a random type ID.");
            return -1; // 또는 0 (Wood) 등 적절한 기본값으로 처리
        }

        // 3. 필터링된 (또는 모든) 멤버들 중에서 랜덤 인덱스를 선택합니다.
        int randomIndex = Random.Range(0, allTypes.Length);

        // 4. 선택된 인덱스의 Enum 멤버를 가져와 int로 캐스팅하여 반환합니다.
        return (int)allTypes[randomIndex];
    }

    /// <summary>
    /// ResourceType.TYPE Enum의 유효한 멤버들 중 랜덤한 하나의 ResourceType.TYPE 값을 반환합니다.
    /// (필요하다면 추가)
    /// </summary>
    public static TYPE GetRandomType()
    {
        return (TYPE)GetRandomTypeId(); // GetRandomTypeId()를 사용하여 int 값을 얻은 후 TYPE으로 캐스팅
    }
}
