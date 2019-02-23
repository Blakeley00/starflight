﻿
public class StarportNoticesButton : ShipButton
{
	public override string GetLabel()
	{
		return "Starport Notices";
	}

	public override bool Execute()
	{
		// get to the player data
		var playerData = DataController.m_instance.m_playerData;

		// change the viewport label
		SpaceflightController.m_instance.m_viewport.UpdateLabel( "Ships Log - Starport Notices" );

		// show the ships log
		SpaceflightController.m_instance.m_shipsLog.Show( playerData.m_shipsLog.m_starportNotices );

		return false;
	}
}
