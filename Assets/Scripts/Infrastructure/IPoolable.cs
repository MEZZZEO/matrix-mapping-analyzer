namespace Infrastructure
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}