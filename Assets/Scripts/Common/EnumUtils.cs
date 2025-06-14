using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Random.Range�� ����ϱ� ���� �ʿ�

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
        // 1. Enum�� ��� ��ȿ�� ���� �����ɴϴ�.
        List<T> allValues = GetAllEnumValues<T>();

        // 2. 'None' (0���� �����ϴ� ���)�� 'Max' (�������� ��ġ�ϴ� ���)�� ���͸��մϴ�.
        //    �̰��� �������� �̸� ��Ģ�� ������, Enum ���ǿ� ���� �޶��� �� �ֽ��ϴ�.
        //    Enum�� 0���� �����ϰ� 'None'�� 0�� ���, ù ��° ��Ҹ� ����.
        //    Enum�� ������ ��Ұ� 'Max'�̰� �װ� �� ������ ��Ÿ���� ���, ������ ��Ҹ� ����.
        //    �� ��Ȯ�� ���͸��� �ʿ��ϴٸ� T.ToString()���� �̸��� Ȯ���ϰų�, Ư�� ���� ���� ���ؾ� �մϴ�.
        List<T> usableValues = new List<T>(allValues); // ���纻 ����

        // T�� Enum�̹Ƿ�, T�� ���� int�� ĳ�����Ͽ� 0�� ������ ���� ���غ��ϴ�.
        // �̰��� Enum ���ǿ� ���� �޶��� �� �ִ� '����'�� �������ϴ�.
        // ���� None = 0�� �ƴ� ���� �ְ�, Max�� ���� ���� �ֱ� �����Դϴ�.
        // ���� ������ ����� Enum ���� �� 'None'�� 'Max'�� ������
        // ���� ����� Enum ����鸸���� `GetAllEnumValues`�� �θ��� ���Դϴ�.
        // ������ ���⼭�� ���׸��̹Ƿ� �Ϲ����� ������ �����ڽ��ϴ�.

        // None/Max ���͸� (���� �Ϲ����� ���Ͽ� ���� ����)
        if (usableValues.Count > 0)
        {
            // Enum ���� 0�� 'None'�� ���� (���� ù ��° ��Ұ� None�̰� ���� 0�̶��)
            if (Convert.ToInt32(usableValues[0]) == 0 && usableValues[0].ToString().Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                // .RemoveAt(0) ���� Skip(1)�� �� �����մϴ�.
                usableValues = usableValues.Skip(1).ToList();
            }

            // Enum ���� �������̰� 'Max'�� ��� (���� �� ������ ��Ÿ��)
            if (usableValues.Count > 0) // Skip �Ŀ��� ��Ұ� �����ִ��� Ȯ��
            {
                T lastValue = usableValues[usableValues.Count - 1];
                // ������ ����� �̸��� 'Max'�̰� �ش� ���� Enum�� ������ ��Ÿ�� ��
                if (lastValue.ToString().Equals("Max", StringComparison.OrdinalIgnoreCase))
                {
                    // Max�� ������ Enum ���� ������ ��Ÿ���� ��츸 ����
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
            return -1; // �Ǵ� 0 �� ������ �⺻��
        }

        // 3. ���͸��� ����� �߿��� ���� �ε����� �����մϴ�.
        int randomIndex = UnityEngine.Random.Range(0, usableValues.Count);

        // 4. ���õ� Enum ����� int�� ĳ�����Ͽ� ��ȯ�մϴ�.
        return Convert.ToInt32(usableValues[randomIndex]);
    }

    /// <summary>
    /// Returns a random value from the specified Enum type.
    /// Excludes 'None' and 'Max' if they exist as the first/last elements.
    /// </summary>
    public static T GetRandomEnumValue<T>() where T : Enum
    {
        int randomInt = GetRandomEnumValueInt<T>();
        if (randomInt == -1) // ���� ó��
        {
            return default(T); // Enum�� �⺻�� (���� 0�� �ش��ϴ� ���) ��ȯ
        }
        return (T)Enum.ToObject(typeof(T), randomInt);
    }
}