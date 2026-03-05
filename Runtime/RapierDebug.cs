namespace RapierPhysics
{
    public static class RapierDebug
    {
        public static ulong GetPhysicsWorldHash()
        {
            return RapierBindings.GetPhysicsWorldHash();
        }
    }
}