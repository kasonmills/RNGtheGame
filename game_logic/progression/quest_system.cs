using System;

namespace GameLogic.Progression;

public class QuestSystem
{
    public string CurrentQuest { get; private set; }
    public bool IsQuestCompleted { get; private set; }

    public QuestSystem()
    {
        CurrentQuest = string.Empty;
        IsQuestCompleted = false;
    }

    public void StartQuest(string questName)
    {
        CurrentQuest = questName;
        IsQuestCompleted = false;
        Console.WriteLine($"Quest started: {CurrentQuest}");
    }

    public void CompleteQuest()
    {
        if (!string.IsNullOrEmpty(CurrentQuest))
        {
            IsQuestCompleted = true;
            Console.WriteLine($"Congratulations! You've completed the quest: {CurrentQuest}");
            CurrentQuest = string.Empty;
        }
        else
        {
            Console.WriteLine("No active quest to complete.");
        }
    }
}