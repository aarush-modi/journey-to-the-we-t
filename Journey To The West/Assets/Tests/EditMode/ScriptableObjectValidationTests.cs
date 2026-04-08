using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
public class ScriptableObjectValidationTests
{
    [Test]
    public void AllEnemyData_HaveValidStats()
    {
        var guids = AssetDatabase.FindAssets("t:EnemyData");
        Assert.IsNotEmpty(guids, "No EnemyData assets found — add enemies or skip this test");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            Assert.IsNotNull(enemy, $"Failed to load EnemyData at {path}");
            Assert.Greater(enemy.maxHP, 0, $"{enemy.name} has invalid maxHP: {enemy.maxHP}");
            Assert.GreaterOrEqual(enemy.baseGoldDrop, 0, $"{enemy.name} has negative baseGoldDrop");
            Assert.IsFalse(string.IsNullOrEmpty(enemy.enemyName),
                $"{enemy.name} has empty enemyName");
        }
    }

    [Test]
    public void AllSkillData_HaveValidConfig()
    {
        var guids = AssetDatabase.FindAssets("t:SkillData");
        Assert.IsNotEmpty(guids, "No SkillData assets found — add skills or skip this test");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var skill = AssetDatabase.LoadAssetAtPath<SkillData>(path);
            Assert.IsNotNull(skill, $"Failed to load SkillData at {path}");
            Assert.IsFalse(string.IsNullOrEmpty(skill.skillName),
                $"{skill.name} has empty skillName");
            Assert.GreaterOrEqual(skill.goldCost, 0,
                $"{skill.name} has negative goldCost: {skill.goldCost}");
            Assert.IsNotNull(skill.skillPrefab,
                $"{skill.name} has no skillPrefab assigned");
        }
    }

    [Test]
    public void AllQuestData_HaveValidNames()
    {
        var guids = AssetDatabase.FindAssets("t:QuestData");
        Assert.IsNotEmpty(guids, "No QuestData assets found — add quests or skip this test");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            Assert.IsNotNull(quest, $"Failed to load QuestData at {path}");
            Assert.IsFalse(string.IsNullOrEmpty(quest.questName),
                $"{quest.name} has empty questName");
        }
    }

    [Test]
    public void AllItemData_HaveValidNames()
    {
        var guids = AssetDatabase.FindAssets("t:ItemData");
        Assert.IsNotEmpty(guids, "No ItemData assets found — add items or skip this test");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            Assert.IsNotNull(item, $"Failed to load ItemData at {path}");
            Assert.IsFalse(string.IsNullOrEmpty(item.itemName),
                $"{item.name} has empty itemName");
        }
    }
}
