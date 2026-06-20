// IFreezable.cs — any enemy or object that can be frozen implements this
public interface IFreezable
{
    void Freeze(float duration, bool instantKill);
    void Unfreeze();
}