﻿
using UnityEngine;

public class ManeuverButton : ShipButton
{
	// keep track of the ship's current movement (for inertia)
	private Vector3 m_inertiaVector;

	// keep track of the skybox rotation
	private Matrix4x4 m_skyboxRotation = Matrix4x4.identity;

	public override string GetLabel()
	{
		return "Maneuver";
	}

	public override bool Execute()
	{
		if ( m_spaceflightController.m_inDockingBay )
		{
			m_spaceflightController.m_uiSoundController.Play( UISoundController.UISound.Error );

			m_spaceflightController.m_messages.text = "Standing by to launch.";

			m_spaceflightController.m_buttonController.UpdateButtonSprites();

			return false;
		}

		// clear out the just launched flag
		m_spaceflightController.m_justLaunched = false;

		// turn off the map overlay
		m_spaceflightController.m_mapRawImage.color = new Color( 1.0f, 1.0f, 1.0f );

		// show the ship on the map
		m_spaceflightController.m_ship.SetActive( true );

		// show the star
		m_spaceflightController.m_star.SetActive( true );

		// show the lens flare
		m_spaceflightController.m_lensFlare.SetActive( true );

		// reset the inertia vector
		m_inertiaVector = Vector3.zero;

		// update the skybox rotation on the material
		Material skyboxMaterial = RenderSettings.skybox;
		skyboxMaterial.SetMatrix( "_Rotation", m_skyboxRotation );

		// change to the system display
		m_spaceflightController.m_displayController.ChangeDisplay( m_spaceflightController.m_displayController.m_systemDisplay );

		// start playing music
		if ( m_spaceflightController.m_inHyperspace )
		{
			MusicController.m_instance.ChangeToTrack( MusicController.Track.Hyperspace );
		}
		else
		{
			MusicController.m_instance.ChangeToTrack( MusicController.Track.StarSystem );
		}

		return true;
	}

	public override bool Update()
	{
		// check if we want to stop maneuvering
		if ( m_spaceflightController.m_inputManager.GetSubmitDown() )
		{
			//		m_spaceflightController.m_buttonController.UpdateButtons();
		}
		else
		{
			// get the controller stick position
			float x = m_spaceflightController.m_inputManager.m_x;
			float z = m_spaceflightController.m_inputManager.m_y;

			// create our 3d move vector from the controller position
			Vector3 moveVector = new Vector3( x, 0.0f, z );

			// check if the move vector will actually move the ship (controller is not centered)
			if ( moveVector.magnitude > 0.5f )
			{
				// normalize the move vector to a length of 1.0 - so the ship will move the same distance in any direction
				moveVector.Normalize();

				// figure out how much acceleration to apply
				Vector3 normalizedInertiaVector = Vector3.Normalize( m_inertiaVector );
				float acceleration = Vector3.Dot( normalizedInertiaVector, moveVector );
				acceleration = Mathf.Clamp( acceleration * 30.0f, 0.1f, 30.0f );

				// update the inertia vector
				m_inertiaVector = Vector3.Slerp( m_inertiaVector, moveVector * acceleration, Time.deltaTime * 3.0f );
			}
			else
			{
				// slow the ship to a stop
				m_inertiaVector = Vector3.Slerp( m_inertiaVector, Vector3.zero, Time.deltaTime );
			}

			// calculate the new position of the ship
			Vector3 shipPosition = m_spaceflightController.m_player.transform.position + m_inertiaVector;

			// make sure the ship stays on the zero plane
			shipPosition.y = 0.0f;

			// update the ship model
			m_spaceflightController.m_player.transform.position = shipPosition;

			// check if the ship is moving
			if ( m_inertiaVector.magnitude > 0.001f )
			{
				// rotate the ship towards the direction we want to move in
				m_spaceflightController.m_ship.transform.rotation = Quaternion.LookRotation( m_inertiaVector, Vector3.up );

				// rotate the skybox accordingly
				Vector3 currentRightVector = m_spaceflightController.m_ship.transform.rotation * Vector3.right;
				Quaternion rotation = Quaternion.AngleAxis( m_inertiaVector.magnitude * Time.deltaTime, currentRightVector );
				m_skyboxRotation = Matrix4x4.Rotate( rotation ) * m_skyboxRotation;

				Material skyboxMaterial = RenderSettings.skybox;
				skyboxMaterial.SetMatrix( "_Rotation", m_skyboxRotation );
			}
		}

		// returning true prevents the default spaceflight update from running
		return true;
	}
}
