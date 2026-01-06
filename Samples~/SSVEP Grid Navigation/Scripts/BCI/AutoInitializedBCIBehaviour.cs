using BCIEssentials.Behaviours;

public class AutoInitializedBCIBehaviour : BCIBehaviour
{
    private void Start() => Initialize();
}