﻿
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CrewAssignmentController : PanelController
{
	enum State
	{
		MenuBar, AssignPersonnel
	}

	// public stuff we want to set using the editor
	public TextMeshProUGUI m_positionValuesText;
	public TextMeshProUGUI m_nameText;
	public TextMeshProUGUI m_skillValuesText;
	public TextMeshProUGUI m_messageText;
	public Image m_upArrowImage;
	public Image m_downArrowImage;
	public Button m_assignButton;
	public Button m_exitButton;
	public GameObject m_selectionXform;
	public GameObject m_personnelFileXform;
	public GameObject m_messageXform;
	public GameObject m_enabledArrowsGameObject;
	public GameObject m_disabledArrowsGameObject;
	public GameObject m_bottomPanelGameObject;

	// private stuff we don't want the editor to see
	private StarportController m_starportController;
	private InputManager m_inputManager;
	private State m_currentState;
	private int m_currentPositionIndex;
	private int m_currentPersonnelId;
	private Vector3 m_baseSelectionOffsetMin;
	private Vector3 m_baseSelectionOffsetMax;
	private float m_ignoreControllerTimer;

	// constant values
	private const int c_numPositions = 6;
	private const int c_numSkills = 5;

	// this is called by unity before start
	private void Awake()
	{
		// get access to the starport controller
		m_starportController = GetComponent<StarportController>();

		// get access to the input manager
		m_inputManager = GetComponent<InputManager>();

		// reset the ignore controller timer
		m_ignoreControllerTimer = 0.0f;

		// remember the base selection offset
		RectTransform rectTransform = m_selectionXform.GetComponent<RectTransform>();
		m_baseSelectionOffsetMin = rectTransform.offsetMin;
		m_baseSelectionOffsetMax = rectTransform.offsetMax;
	}

	// this is called by unity once at the start of the level
	private void Start()
	{
		// hide the ui
		m_panelGameObject.SetActive( false );
	}

	// this is called by unity every frame
	private void Update()
	{
		// update the ignore controller timer
		m_ignoreControllerTimer = Mathf.Max( 0.0f, m_ignoreControllerTimer - Time.deltaTime );

		// call the proper update function based on our current state
		switch ( m_currentState )
		{
			case State.AssignPersonnel:
			{
				UpdateControllerForAssignPersonnelState();
				break;
			}
		}
	}

	// controller updates for when we are currently selecting a race
	public void UpdateControllerForAssignPersonnelState()
	{
		// get access to the personnel player data
		Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

		// get the controller stick position
		float x = m_inputManager.GetRawX();
		float y = m_inputManager.GetRawY();

		// keep track if we have centered both x and y
		bool xIsCentered = false;
		bool yIsCentered = false;

		// check if we moved the stick left
		if ( x <= -0.5f )
		{
			if ( m_ignoreControllerTimer == 0.0f )
			{
				m_ignoreControllerTimer = 0.3f;

				ChangeCurrentPersonnelId( ( m_currentPersonnelId + personnel.m_personnelList.Count - 1 ) % personnel.m_personnelList.Count );

				UpdateDisplay();
			}
		}
		else if ( x >= 0.5f ) // check if we moved the stick right
		{
			if ( m_ignoreControllerTimer == 0.0f )
			{
				m_ignoreControllerTimer = 0.3f;

				ChangeCurrentPersonnelId( ( m_currentPersonnelId + 1 ) % personnel.m_personnelList.Count );

				UpdateDisplay();
			}
		}
		else // we have centered the stick
		{
			xIsCentered = true;
		}

		// change position only if x is centered
		if ( xIsCentered )
		{
			// check if we moved the stick down
			if ( y <= -0.5f )
			{
				if ( m_ignoreControllerTimer == 0.0f )
				{
					m_ignoreControllerTimer = 0.3f;

					if ( m_currentPositionIndex < ( c_numPositions - 1 ) )
					{
						ChangeCurrentPositionIndex( m_currentPositionIndex + 1 );

						UpdateDisplay();
					}
				}
			}
			else if ( y >= 0.5f ) // check if we have moved the stick up
			{
				if ( m_ignoreControllerTimer == 0.0f )
				{
					m_ignoreControllerTimer = 0.3f;

					if ( m_currentPositionIndex > 0 )
					{
						ChangeCurrentPositionIndex( m_currentPositionIndex - 1 );

						UpdateDisplay();
					}
				}
			}
			else // we have centered the stick
			{
				yIsCentered = true;
			}
		}

		// check if we have centered the stick
		if ( xIsCentered && yIsCentered )
		{
			m_ignoreControllerTimer = 0.0f;
		}

		// check if we have pressed the cancel button
		if ( m_inputManager.GetCancelDown() )
		{
			SwitchToMenuBarState();

			GetComponent<UISoundController>().Play( UISoundController.UISound.Deactivate );
		}
	}

	// call this to show the personnel ui
	public override void Show()
	{
		// reset the current state
		SwitchToMenuBarState();

		// start the opening animation
		StartOpeningUI();
	}

	// call this to hide the personnel ui
	public override void Hide()
	{
		// lose the focus
		LoseFocus();

		// start the closing animation
		StartClosingUI();
	}

	// call this to take control
	public void TakeFocus()
	{
		// turn on controller navigation of the UI
		EventSystem.current.sendNavigationEvents = true;

		// switch to the default view
		SwitchToMenuBarState();

		// cancel the ui sounds
		GetComponent<UISoundController>().CancelSounds();
	}

	// call this to give up control
	public void LoseFocus()
	{
		// turn off controller navigation of the UI
		EventSystem.current.sendNavigationEvents = false;
	}

	// this is called when the ui has finished animating to the open state
	public override void FinishedOpeningUI()
	{
		// take the focus
		TakeFocus();
	}

	// this is called when the ui has finished animating to the close state
	public override void FinishedClosingUI()
	{
		// give the focus back to the starport controller
		m_starportController.TakeFocus();
	}

	// call this to switch to the menu bar state
	private void SwitchToMenuBarState()
	{
		// change the current state
		m_currentState = State.MenuBar;

		// hide the selection bar
		m_selectionXform.SetActive( false );

		// hide the enabled arrows
		m_enabledArrowsGameObject.SetActive( false );

		// show the disabled arrows
		m_disabledArrowsGameObject.SetActive( false );

		// enable the exit button
		m_exitButton.interactable = true;

		// update the assigned crewmember list
		UpdateAssignedCrewmemberList();

		// get access to the personnel player data
		Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

		// check if we have at least one living crewmember in personnel
		if ( personnel.AnyLiving() )
		{
			// hide the crewmember panel
			m_bottomPanelGameObject.SetActive( false );

			// show the personnel file
			m_personnelFileXform.SetActive( true );

			// hide the message
			m_messageXform.SetActive( false );

			// activate the assign button
			m_assignButton.interactable = true;

			// select the assign button
			m_assignButton.Select();
		}
		else
		{
			// show the crewmember panel
			m_bottomPanelGameObject.SetActive( true );

			// hide the personnel file
			m_personnelFileXform.SetActive( false );

			// show the message
			m_messageXform.SetActive( true );

			// update the message
			m_messageText.text = "Report to Personnel: There are no living crewmembers on file";

			// inactivate the assign button
			m_assignButton.interactable = false;

			// select the exit button
			m_exitButton.Select();
		}
	}

	// call this to switch to the select race state
	private void SwitchToAssignPersonnelState()
	{
		// change the current state
		m_currentState = State.AssignPersonnel;

		// start with the captain
		ChangeCurrentPositionIndex( 0 );

		// show the crewmember panel
		m_bottomPanelGameObject.SetActive( true );

		// show the selection xform
		m_selectionXform.SetActive( true );

		// get access to the personnel player data
		Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

		// show the enabled arrows only if we have more than one personnel on file
		if ( personnel.m_personnelList.Count > 1 )
		{
			m_enabledArrowsGameObject.SetActive( true );
		}
		else
		{
			m_disabledArrowsGameObject.SetActive( true );
		}

		// disable the assign and exit buttons
		m_assignButton.interactable = false;
		m_exitButton.interactable = false;

		// update the display
		UpdateDisplay();
	}

	// update the assigned crewmember list
	private void UpdateAssignedCrewmemberList()
	{
		// get access to the crew assignment player data
		CrewAssignment crewAssignment = PersistentController.m_instance.m_playerData.m_crewAssignment;

		// get access to the personnel player data
		Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

		// start with an empty text string
		m_positionValuesText.text = "";

		// go through each position
		for ( int positionIndex = 0; positionIndex < c_numPositions; positionIndex++ )
		{
			// get the personnel id for the assigned crewmember
			int personnelId = crewAssignment.GetFileId( positionIndex );

			// check if the position is assigned
			if ( personnelId != -1 )
			{
				// find the personnel with that file id
				Personnel.PersonnelFile personnelFile = personnel.GetPersonnel( personnelId );

				// add the crewmember's name
				m_positionValuesText.text += personnelFile.m_name;
			}
			else
			{
				// add the not assigned text
				m_positionValuesText.text += "[Not Assigned]";
			}

			if ( positionIndex < ( c_numPositions - 1 ) )
			{
				m_positionValuesText.text += Environment.NewLine;
			}
		}
	}

	private void ChangeCurrentPositionIndex( int positionIndex )
	{
		// update the current position index
		m_currentPositionIndex = positionIndex;

		// get access to the crew assignment player data
		CrewAssignment crewAssignment = PersistentController.m_instance.m_playerData.m_crewAssignment;

		// check if we have don't have someone assigned to this position
		if ( !crewAssignment.IsAssigned( m_currentPositionIndex ) )
		{
			// automatically select the first personnel file
			ChangeCurrentPersonnelId( 0, true );
		}
		else
		{
			// get the current file id for this position
			int fileId = crewAssignment.GetFileId( m_currentPositionIndex );

			// get access to the personnel player data
			Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

			// update the current personnel id
			m_currentPersonnelId = personnel.GetPersonnelId( fileId );
		}

		// play a sound
		GetComponent<UISoundController>().Play( UISoundController.UISound.Update );
	}

	private void ChangeCurrentPersonnelId( int personnelId, bool forceUpdate = false )
	{
		// don't do anything if we aren't changing the file index to a different one
		if ( ( personnelId != m_currentPersonnelId ) || forceUpdate )
		{
			// update the current personnel id
			m_currentPersonnelId = personnelId;

			// get access to the crew assignment player data
			CrewAssignment crewAssignment = PersistentController.m_instance.m_playerData.m_crewAssignment;

			// get access to the personnel player data
			Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

			// get the personnel file
			Personnel.PersonnelFile personnelFile = personnel.m_personnelList[ m_currentPersonnelId ];

			// assign this personnel to this position
			crewAssignment.Assign( m_currentPositionIndex, personnelFile.m_fileId );

			// update the assigned crewmember list
			UpdateAssignedCrewmemberList();

			// play a sound
			GetComponent<UISoundController>().Play( UISoundController.UISound.Update );
		}
	}

	private void UpdateDisplay()
	{
		// show the up arrow only if we are not at the first position index
		m_upArrowImage.gameObject.SetActive( m_currentPositionIndex != 0 );

		// show the down arrow only if we are not at the last position index
		m_downArrowImage.gameObject.SetActive( m_currentPositionIndex != ( c_numPositions - 1 ) );

		// put the position selection box in the right place
		float offset = m_currentPositionIndex * m_positionValuesText.renderedHeight / c_numPositions;

		RectTransform rectTransform = m_selectionXform.GetComponent<RectTransform>();
		rectTransform.offsetMin = m_baseSelectionOffsetMin + new Vector3( 0.0f, -offset, 0.0f );
		rectTransform.offsetMax = m_baseSelectionOffsetMax + new Vector3( 0.0f, -offset, 0.0f );

		// get access to the personnel player data
		Personnel personnel = PersistentController.m_instance.m_playerData.m_personnel;

		// get the personnel file
		Personnel.PersonnelFile personnelFile = personnel.m_personnelList[ m_currentPersonnelId ];

		// update the crewmember name
		if ( personnelFile.m_vitality > 0 )
		{
			m_nameText.text = personnelFile.m_name + " - " + personnelFile.m_vitality + "% vitality";
		}
		else
		{
			m_nameText.text = personnelFile.m_name + " - DEAD";
		}

		// update the skill values
		m_skillValuesText.text = "";

		for ( int skillIndex = 0; skillIndex < c_numSkills; skillIndex++ )
		{
			m_skillValuesText.text += personnelFile.GetSkill( skillIndex ).ToString();

			if ( skillIndex < ( c_numSkills - 1 ) )
			{
				m_skillValuesText.text += Environment.NewLine;
			}
		}
	}

	// this is called if we clicked on the assign button
	public void AssignClicked()
	{
		// switch to the create select race state
		SwitchToAssignPersonnelState();

		// play a ui sound
		GetComponent<UISoundController>().Play( UISoundController.UISound.Activate );
	}

	// this is called if we clicked on the exit button
	public void ExitClicked()
	{
		// close this ui
		Hide();

		// play a ui sound
		GetComponent<UISoundController>().Play( UISoundController.UISound.Deactivate );
	}
}
