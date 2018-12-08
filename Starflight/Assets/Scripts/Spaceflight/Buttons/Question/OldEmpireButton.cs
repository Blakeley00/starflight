﻿
public class OldEmpireButton : ShipButton
{
	public override string GetLabel()
	{
		return "Old Empire";
	}

	public override bool Execute()
	{
		m_spaceflightController.m_encounter.AddComm( GD_Comm.Subject.OldEmpire, true );

		m_spaceflightController.m_buttonController.ChangeButtonSet( ButtonController.ButtonSet.Comm );

		return false;
	}
}
