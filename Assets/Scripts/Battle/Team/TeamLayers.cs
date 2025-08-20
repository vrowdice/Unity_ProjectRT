using UnityEngine;

public enum TeamSide { Neutral = 0, Ally = 1, Enemy = 2 }

public static class TeamLayers
{
    // Project Settings > Tags and Layers 에 다음 두 레이어가 있어야 함
    public const string AllyLayerName = "AllyUnit";
    public const string EnemyLayerName = "EnemyUnit";

    public static int AllyLayer => LayerMask.NameToLayer(AllyLayerName);
    public static int EnemyLayer => LayerMask.NameToLayer(EnemyLayerName);

    public static int GetUnitLayer(TeamSide team) =>
        (team == TeamSide.Ally) ? AllyLayer : (team == TeamSide.Enemy ? EnemyLayer : 0);

    public static LayerMask GetEnemyMask(TeamSide myTeam)
        => (myTeam == TeamSide.Ally) ? (1 << EnemyLayer) :
           (myTeam == TeamSide.Enemy) ? (1 << AllyLayer) : 0;

    public static LayerMask GetAllyMask(TeamSide myTeam)
        => (myTeam == TeamSide.Ally) ? (1 << AllyLayer) :
           (myTeam == TeamSide.Enemy) ? (1 << EnemyLayer) : 0;
}
