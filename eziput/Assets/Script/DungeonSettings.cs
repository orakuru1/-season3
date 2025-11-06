using UnityEngine;

[System.Serializable]
public class DungeonSettings
{
    public RouteType routeType = RouteType.Safe;
    public int baseRoomCount = 8;
    public float enemySpawnRate = 0.05f;
    public float treasureSpawnRate = 0.03f;
    public float trapSpawnRate = 0.02f;

    public void ApplyRouteSettings(RouteType route)
    {
        routeType = route;
        if (route == RouteType.Safe)
        {
            baseRoomCount = 6;
            enemySpawnRate = 0.03f;
            treasureSpawnRate = 0.02f;
            trapSpawnRate = 0.01f;
        }
        else
        {
            baseRoomCount = 12;
            enemySpawnRate = 0.08f;
            treasureSpawnRate = 0.06f;
            trapSpawnRate = 0.04f;
        }
    }
}

public enum RouteType { Safe, Danger }
