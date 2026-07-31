namespace GameScene.ServedObjectComponent.OnAttack
{
    /// <summary>
    /// Shows an attack frame while the object attacks. Replaces the hand-written per-creature
    /// attack presenters: assign the attack sprite on the prefab and the creature animates, with no
    /// object-type string anywhere in code.
    /// </summary>
    public sealed class AttackSpriteSwapController : TimedSpriteSwapController
    {
        protected override void Subscribe(ServedObject owner)
        {
            owner.OnAttack += Play;
        }

        protected override void Unsubscribe(ServedObject owner)
        {
            owner.OnAttack -= Play;
        }
    }
}
