using GameScene.Dto;
using NUnit.Framework;
using UnityEngine;

namespace WordOnline.Tests
{
    public class StormStagChargeImpactRulesTests
    {
        [TestCase("StormStagCharge3")]
        [TestCase("StormStagCharge4")]
        public void HighAccelerationChargePlaysDedicatedImpact(string effect)
        {
            Assert.That(StormStagChargeImpactRules.ShouldPlay(new[] { effect }), Is.True);
        }

        [Test]
        public void TierTwoChargeKeepsTheGenericImpact()
        {
            Assert.That(
                StormStagChargeImpactRules.ShouldPlay(new[] { "StormStagCharge2" }),
                Is.False);
        }

        [Test]
        public void UnrelatedEffectsKeepTheGenericImpact()
        {
            Assert.That(
                StormStagChargeImpactRules.ShouldPlay(new[] { "Overcharge", "Shock" }),
                Is.False);
        }

        [Test]
        public void MissingEffectsKeepTheGenericImpact()
        {
            Assert.That(StormStagChargeImpactRules.ShouldPlay(null), Is.False);
        }

        [TestCase("StormStagCharge3")]
        [TestCase("StormStagCharge4")]
        public void HighAccelerationChargeReplacesElectricShot(string effect)
        {
            Assert.That(
                StormStagChargeImpactRules.ShouldSuppressProjectile("ElectricShot", new[] { effect }),
                Is.True);
        }

        [Test]
        public void TierTwoChargeKeepsElectricShot()
        {
            Assert.That(
                StormStagChargeImpactRules.ShouldSuppressProjectile(
                    "ElectricShot",
                    new[] { "StormStagCharge2" }),
                Is.False);
        }

        [Test]
        public void OtherProjectileTypesAreNeverSuppressed()
        {
            Assert.That(
                StormStagChargeImpactRules.ShouldSuppressProjectile(
                    "ElectricAbsorb",
                    new[] { "StormStagCharge4" }),
                Is.False);
        }

        [Test]
        public void DedicatedImpactPrefabIsLoadable()
        {
            GameObject prefab = Resources.Load<GameObject>(
                $"Prefabs/Effects/{StormStagChargeImpactRules.EffectResourceName}");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<SpriteRenderer>(), Is.Not.Null);
            Assert.That(prefab.GetComponent("StormStagChargeImpactEffect"), Is.Not.Null);
        }

        [Test]
        public void DedicatedImpactSpriteFitsTheBigAssetTier()
        {
            GameObject prefab = Resources.Load<GameObject>(
                $"Prefabs/Effects/{StormStagChargeImpactRules.EffectResourceName}");
            Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;

            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.texture.width, Is.LessThanOrEqualTo(256));
            Assert.That(sprite.texture.height, Is.LessThanOrEqualTo(256));
        }
    }
}
