/// <summary>
/// Defines the different modes for spawning enemies.
/// Aka, how they spawn within the wave type
/// </summary>
public enum SpawnType
{
    Fixed,              // Spawn exactly at one of the fixed positions.
    AreaAroundPosition, // Spawn within a radius around one of the provided reference points.
    AreaAroundPlayers   // Spawn within a radius around one or more players.
}