using GameScene.Dto.Projectile;
using Global.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace WordOnline.Tests
{
    public class ProjectileTargetTests
    {
        [Test]
        public void PositionTargetResolvesToPositionSubtype()
        {
            const string json = @"{""targetType"":""position"",""x"":1.0,""y"":2.0,""z"":3.0}";

            ProjectileTarget target = JsonCodec.Deserialize<ProjectileTarget>(json);

            Assert.IsInstanceOf<PositionProjectileTarget>(target);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), ((PositionProjectileTarget)target).ToVector3());
        }

        [Test]
        public void ReferenceTargetResolvesToReferenceSubtype()
        {
            const string json = @"{""targetType"":""reference"",""id"":77}";

            ProjectileTarget target = JsonCodec.Deserialize<ProjectileTarget>(json);

            Assert.IsInstanceOf<ReferenceProjectileTarget>(target);
            Assert.AreEqual(77, ((ReferenceProjectileTarget)target).id);
            Assert.AreEqual("reference", target.targetType);
        }

        [Test]
        public void UnknownTargetTypeResolvesToNull()
        {
            const string json = @"{""targetType"":""orbit"",""id"":77}";

            Assert.IsNull(JsonCodec.Deserialize<ProjectileTarget>(json));
        }

        [Test]
        public void MissingTargetTypeResolvesToNull()
        {
            const string json = @"{""id"":77}";

            Assert.IsNull(JsonCodec.Deserialize<ProjectileTarget>(json));
        }

        [Test]
        public void NullTargetStaysNull()
        {
            const string json = @"{""type"":""ArrowShot"",""start"":null,""end"":null,""duration"":0.2}";

            ProjectileDto projectile = JsonCodec.Deserialize<ProjectileDto>(json);

            Assert.IsNull(projectile.start);
            Assert.IsNull(projectile.end);
        }
    }
}
