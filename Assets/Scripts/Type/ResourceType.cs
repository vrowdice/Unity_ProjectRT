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
    /// ResourceType.TYPE Enum�� ��ȿ�� ����� �� ������ �ϳ��� ����(int) ���� ��ȯ�մϴ�.
    /// </summary>
    public static int GetRandomTypeId()
    {
        // 1. ResourceType.TYPE Enum�� ��� ��� ���� �迭�� �����ɴϴ�.
        TYPE[] allTypes = (TYPE[])TYPE.GetValues(typeof(TYPE));

        // 2. ���� Enum�� 'None'�̳� 'Max'ó�� ���� ���ÿ��� �����ϰ� ���� Ư�� ����� �ִٸ�
        //    ���⼭ ���͸��ϴ� ������ �߰��� �� �ֽ��ϴ�.
        //    ���� ResourceType.TYPE ���ǿ��� �׷� ����� �����Ƿ� ��� Enum ����� ��ȿ�ϴٰ� �����մϴ�.

        if (allTypes.Length == 0)
        {
            Debug.LogWarning("ResourceType.TYPE enum has no defined members. Cannot get a random type ID.");
            return -1; // �Ǵ� 0 (Wood) �� ������ �⺻������ ó��
        }

        // 3. ���͸��� (�Ǵ� ���) ����� �߿��� ���� �ε����� �����մϴ�.
        int randomIndex = Random.Range(0, allTypes.Length);

        // 4. ���õ� �ε����� Enum ����� ������ int�� ĳ�����Ͽ� ��ȯ�մϴ�.
        return (int)allTypes[randomIndex];
    }

    /// <summary>
    /// ResourceType.TYPE Enum�� ��ȿ�� ����� �� ������ �ϳ��� ResourceType.TYPE ���� ��ȯ�մϴ�.
    /// (�ʿ��ϴٸ� �߰�)
    /// </summary>
    public static TYPE GetRandomType()
    {
        return (TYPE)GetRandomTypeId(); // GetRandomTypeId()�� ����Ͽ� int ���� ���� �� TYPE���� ĳ����
    }
}
