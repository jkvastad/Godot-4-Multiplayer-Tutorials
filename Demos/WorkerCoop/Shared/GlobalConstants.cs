namespace Shared;

public static class GlobalConstants
{
    #region scene paths    
    
    public static readonly string PLAYER_SEAT_VIEW_PATH = "res://Client/Viewers/PlayerSeatView/player_seat_view.tscn";
    public static readonly string WORKER_GRID_VIEW_PATH = "res://Client/Viewers/WorkerGridView/worker_grid_view.tscn";
    public static readonly string GRID_BOX_VIEW_PATH = "res://Client/Viewers/WorkerGridView/grid_box_view.tscn";
    public static readonly string WORKER_GRID_HUD_PATH = "res://Client/Viewers/WorkerGridView/GridHud/worker_grid_hud.tscn";
    public static readonly string GRID_WORKER_VIEW_PATH = "res://Client/Viewers/WorkerGridView/GridWorker/grid_worker_view.tscn";
    public static readonly string SESSION_MENU_PATH = "res://Client/GameSession/SessionMenu/session_menu.tscn";
    #endregion


    #region Enums
    public enum UpdateType
    {
        Add,
        Remove
    }
    #endregion
}

