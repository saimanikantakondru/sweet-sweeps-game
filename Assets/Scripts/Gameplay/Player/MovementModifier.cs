namespace SweetSweeps.Gameplay.Player
{
    public struct MovementModifier
    {
        public float SpeedMultiplier;
        public float AccelerationMultiplier;
        public float DecelerationMultiplier;
        public float DirectionChangeMultiplier;
        public float JumpForceMultiplier;

        public static MovementModifier Identity => new MovementModifier
        {
            SpeedMultiplier = 1f,
            AccelerationMultiplier = 1f,
            DecelerationMultiplier = 1f,
            DirectionChangeMultiplier = 1f,
            JumpForceMultiplier = 1f
        };

        public static MovementModifier Multiply(MovementModifier a, MovementModifier b) =>
            new MovementModifier
            {
                SpeedMultiplier = a.SpeedMultiplier * b.SpeedMultiplier,
                AccelerationMultiplier = a.AccelerationMultiplier * b.AccelerationMultiplier,
                DecelerationMultiplier = a.DecelerationMultiplier * b.DecelerationMultiplier,
                DirectionChangeMultiplier = a.DirectionChangeMultiplier * b.DirectionChangeMultiplier,
                JumpForceMultiplier = a.JumpForceMultiplier * b.JumpForceMultiplier
            };
    }
}