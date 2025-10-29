# RNGtheGame

This is the file skeleton to base a lot of my project on.

RNG-The-Game/

    Assets/
        Characters/
            ability_base.cs
            Player/
                Player.cs
                PlayerStats.cs
                PlayerAbilities.cs
                PlayerInventory.cs
            Enemies/
                EnemyBase.cs
                EnemyStats.cs
                EnemyAbilities.cs
                EnemyAI.cs
            NPCs/
                NPCBase.cs
                Shopkeeper.cs
        Combat/
            CombatManager.cs
            TurnManager.cs
            Ability.cs
            DamageCalculator.cs
        Items/
            Weapon.cs
            Armor.cs
            LootTable.cs
            ItemBase.cs
        Maps/
            MapManager.cs
            MapNode.cs
            Cities/
                City1.tscn
                City1.cs
                City2.tscn
        UI/
            UIManager.cs
            Menus/
                MainMenu.cs
                PauseMenu.cs
                SettingsMenu.cs
            HUD/
                HealthBar.cs
                InventoryHUD.cs
                CombatHUD.cs
        Utilities/
            RNGManager.cs
            DifficultyScaler.cs
            SaveSystem.cs
            AudioManager.cs

    Scenes/
        MainScene.tscn
        CombatScene.tscn
        MapScene.tscn
        UIScene.tscn
        TestScene.tscn

    Resources/
        Sprites/
            Characters/
            Enemies/
            Items/
            Maps/
        Audio/
            Music/
            SoundEffects/
        Fonts/
        Shaders/
        Prefabs/
            Weapons/
            Armors/
            Enemies/

    Docs/
        DesignDocument.md ?
        RNGAlgorithms.md ?
        ToDoList.md
        README.md ?

    Tests/
        TestCombat.cs
        TestRNG.cs
        TestSaveSystem.cs
        TestPlayerStats.cs

    .gitignore
    project.godot
    main.csproj
    README.md
