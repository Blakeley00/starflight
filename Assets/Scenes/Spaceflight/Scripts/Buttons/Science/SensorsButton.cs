﻿
public class SensorsButton : ShipButton
{
	public override string GetLabel()
	{
		return "Sensors";
	}

	public override bool Execute()
	{
		// get to the player data
		PlayerData playerData = DataController.m_instance.m_playerData;

		switch ( playerData.m_starflight.m_location )
		{
			case Starflight.Location.InOrbit:

				// show the system display
				m_spaceflightController.m_displayController.ChangeDisplay( m_spaceflightController.m_displayController.m_sensorsDisplay );

				// play the scanning sound
				SoundController.m_instance.PlaySound( SoundController.Sound.Scanning );

				return true;

			case Starflight.Location.DockingBay:
			case Starflight.Location.JustLaunched:

				SoundController.m_instance.PlaySound( SoundController.Sound.Error );

				m_spaceflightController.m_buttonController.UpdateButtonSprites();

				break;

			default:

				SoundController.m_instance.PlaySound( SoundController.Sound.Error );

				m_spaceflightController.m_spaceflightUI.ChangeMessageText( "Not yet implemented." );

				m_spaceflightController.m_buttonController.UpdateButtonSprites();

				break;
		}

		return false;
	}

	public override bool Update()
	{
		return true;
	}
}
