using Godot;
using Raele.GodotUtils;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconState : GodotUtils.StateMachine.BaseState
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.Flags, "X:1,Y:2")] public byte ResetVelocityOnEnter = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => field ??= this.RequireAncestor<SuperconBody2D>();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconInputMapping InputMapping => this.Character.InputMapping;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		if ((this.ResetVelocityOnEnter & 1) != 0)
		{
			this.Character.VelocityX = 0;
		}
		if ((this.ResetVelocityOnEnter & 2) != 0)
		{
			this.Character.VelocityY = 0;
		}
	}
}
