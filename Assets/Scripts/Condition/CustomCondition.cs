using System;

public class CustomCondition : ICondition
{
    private readonly Func<bool> m_predicate;

    public CustomCondition(Func<bool> predicate)
    {
        m_predicate = predicate;
    }

    public bool IsSatisfied()
    {
        return m_predicate();
    }
}