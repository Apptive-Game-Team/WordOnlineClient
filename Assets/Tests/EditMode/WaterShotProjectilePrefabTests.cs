using NUnit.Framework;
using UnityEngine;

namespace WordOnline.Tests
{
    public class WaterShotProjectilePrefabTests
    {
        [Test]
        public void WaterShotProjectileIsVisibleAboveSummonedCreatures()
        {
            GameObject prefab = Resources.Load<GameObject>("Projectiles/WaterShot");

            Assert.IsNotNull(prefab, "Resources/Projectiles/WaterShot.prefab must be loadable.");
            Assert.AreEqual("WaterShot", prefab.name);

            SpriteRenderer renderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
            Assert.IsNotNull(renderer, "WaterShot requires a SpriteRenderer.");
            Assert.IsTrue(renderer.enabled, "WaterShot's SpriteRenderer must be enabled.");
            Assert.IsNotNull(renderer.sprite, "WaterShot requires a visible sprite.");
            Assert.Greater(renderer.sortingOrder, 20,
                "WaterShot must render above summoned creatures, which use sorting orders up to 20.");
        }
    }
}
