public class NetworkTimer
{
    float timer;
    public float MinTimeBetweenTicks { get; }
    public int CurrentTick { get; private set; }

    public NetworkTimer(float serverTickTime)
    {
        MinTimeBetweenTicks = 1f / serverTickTime;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;
    }

    public bool ShouldTick()
    {
        if (timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            CurrentTick++;
            return true;
        }
        return false;
    }
}
