public interface IPlacable 
{

    // for item that can be placed on grid, e.g. turret, wall, spring board, etc.

    void Place();

    // for item that can be rotated when it's placed on grid, e.g. turret, wall and spring board which will rotate 90 degree CW.
    void Rotate();

    // when remove it will turn into crate, and can be picked up by player, but it will not be placed on grid until player place it again, e.g. turret, wall and spring board.
    void Remove();
}
