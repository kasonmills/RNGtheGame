// In your Godot MapController script:
void OnPlayerMovedToNode(MapNode node)
{
    if (node.HasEnemy)
    {
        gameManager.TriggerCombatEncounter();
    }
    else if (node.HasLoot)
    {
        gameManager.TriggerLootEvent();
    }
}