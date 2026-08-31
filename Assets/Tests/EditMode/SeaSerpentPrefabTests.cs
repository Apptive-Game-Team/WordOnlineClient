using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordOnline.Tests
{
    public class SeaSerpentPrefabTests
    {
        [Test]
        public void SeaSerpentUsesSubmergedAndAttackFrames()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/Prefabs/SeaSerpent.prefab");

            Assert.IsNotNull(prefab, "Resources/Prefabs/SeaSerpent.prefab must be loadable.");
            SpriteRenderer renderer = prefab.GetComponentsInChildren<SpriteRenderer>(true)
                .Single(candidate => candidate.sprite != null && candidate.sprite.name == "SeaSerpent");
            MonoBehaviour swap = prefab.GetComponents<MonoBehaviour>()
                .Single(component => component.GetType().Name == "AttackSpriteSwapController");

            Assert.IsNotNull(renderer);
            Assert.AreEqual("SeaSerpent", renderer.sprite.name);
            Assert.IsNotNull(swap, "Sea Serpent needs an attack-frame swap controller.");

            SerializedProperty swapSprite = new SerializedObject(swap).FindProperty("swapSprite");
            Assert.IsNotNull(swapSprite);
            Assert.AreEqual("SeaSerpentAttacking", ((Sprite)swapSprite.objectReferenceValue).name);
        }

        [Test]
        public void HydroPumpPrefabHasALinePresenterAndVisibleSprite()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/Projectiles/SeaSerpentHydroPump.prefab");
            Assert.IsNotNull(prefab);
            MonoBehaviour line = prefab.GetComponents<MonoBehaviour>()
                .Single(component => component.GetType().Name == "LineProjectile");
            SerializedObject serializedLine = new SerializedObject(line);
            Assert.IsNotNull(serializedLine.FindProperty("actualObject").objectReferenceValue);
            Assert.IsNotNull(serializedLine.FindProperty("spriteRenderer").objectReferenceValue);

            SpriteRenderer renderer = prefab.GetComponentInChildren<SpriteRenderer>();
            Assert.IsNotNull(renderer.sprite);
            Assert.AreEqual("SeaSerpentHydroPump", renderer.sprite.name);
            Assert.Greater(renderer.sortingOrder, 20);
        }
    }
}
