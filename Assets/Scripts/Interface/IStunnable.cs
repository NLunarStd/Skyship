public interface IStunnable
{
    // for character that can be stunned by player, and will be unable to move or attack for a certain duration when it's stunned, e.g. enemy, player, etc.
     void Stun(float duration);
}
