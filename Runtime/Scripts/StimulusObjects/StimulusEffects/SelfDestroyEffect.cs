namespace BCIEssentials.StimulusEffects
{
    public class SelfDestroyEffect: StimulusEffect
    {
        public override void SetOn()
        {
            Destroy();
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}