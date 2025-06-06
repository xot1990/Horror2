using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    public bool open;
    public ParticleSystem Psys1;
    public ParticleSystem Psys2;

    void Start()
    {
        open = false;
        if (Psys1 != null) Psys1.Stop();
        if (Psys2 != null) Psys2.Stop();
    }

    public void Use()
    {
        open = !open;
        if (open)
        {
            if (Psys1 != null) Psys1.Play();
            if (Psys2 != null) Psys2.Play();
        }
        else
        {
            if (Psys1 != null) Psys1.Stop();
            if (Psys2 != null) Psys2.Stop();
        }
    }
}