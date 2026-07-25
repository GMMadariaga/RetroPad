namespace RetroPad.Core.Entities;

public class SessionState
{
    public List<TabState> Tabs { get; set; } = [];
    public int ActiveTabIndex { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public bool HasTabs => Tabs.Count > 0;

    public TabState? ActiveTab => ActiveTabIndex >= 0 && ActiveTabIndex < Tabs.Count
        ? Tabs[ActiveTabIndex]
        : null;

    public void AddTab(TabState tab)
    {
        Tabs.Add(tab);
        ActiveTabIndex = Tabs.Count - 1;
    }

    public void RemoveTab(int index)
    {
        if (index < 0 || index >= Tabs.Count) return;
        Tabs.RemoveAt(index);
        if (ActiveTabIndex >= Tabs.Count)
            ActiveTabIndex = Math.Max(0, Tabs.Count - 1);
    }
}
