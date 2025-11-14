using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class MultidirectionalComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedPxPSec = 200f;
	[Export] public float AccelerationPxPSecSqr = 400f;
	[Export] public float SoftDecelerationPxPSecSqr = 400f;
	// [Export] public float HardDecelerationPxPSecSqr = 800f; // TODO Implement this once we have multidirectional facing direction
	// [Export] public float TurnSpeedPiRadPSec = float.PositiveInfinity; // TODO Implement this once we have multidirectional facing direction

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		float currentVelocityPxPSec = this.Character.Velocity.Length();
		float targetVelocityPxPSec = this.InputManager.MovementInput.Length() * this.MaxSpeedPxPSec;
		float accelerationPxPSecSqr = targetVelocityPxPSec > currentVelocityPxPSec ? this.AccelerationPxPSecSqr
			: this.SoftDecelerationPxPSecSqr;
		float newVelocity = Mathf.MoveToward(
			this.Character.Velocity.Length(),
			targetVelocityPxPSec,
			accelerationPxPSecSqr * (float) delta
		);
		this.Character.Velocity = this.InputManager.MovementInput.Normalized() * newVelocity;
	}
}
