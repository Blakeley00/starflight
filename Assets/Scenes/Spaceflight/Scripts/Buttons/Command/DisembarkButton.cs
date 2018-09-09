﻿
public class DisembarkButton : ShipButton
{
	public override string GetLabel()
	{
		return "Disembark";
	}

	public override bool Execute()
	{
		// get to the player data
		PlayerData playerData = DataController.m_instance.m_playerData;

		switch ( playerData.m_starflight.m_location )
		{
			case Starflight.Location.DockingBay:

				// switch to the starport location
				playerData.m_starflight.m_location = Starflight.Location.Starport;

				// play the update sound
				SoundController.m_instance.PlaySound( SoundController.Sound.Update );

				// calling this causes the scene to change
				m_spaceflightController.SwitchMode();

				break;

			case Starflight.Location.JustLaunched:
			case Starflight.Location.InOrbit:
			case Starflight.Location.StarSystem:
			case Starflight.Location.Hyperspace:

				SoundController.m_instance.PlaySound( SoundController.Sound.Error );

				m_spaceflightController.m_spaceflightUI.ChangeMessageText( "We can't disembark in space!" );

				m_spaceflightController.m_buttonController.UpdateButtonSprites();

				break;
		}

		return false;
	}
}
