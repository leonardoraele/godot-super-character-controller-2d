using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class ForceComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum ForceTypeEnum
	{
		FixedDirection,
		FacingDirection,
		MovementResistance,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Vector2 Direction = Vector2.Down;
	[Export] public float AccelerationPxPSecSqr = 600f;
	[Export] public float MaxSpeedPxPSec = 600f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		this.Character.Accelerate(
			this.Direction.Normalized() * this.MaxSpeedPxPSec,
			this.AccelerationPxPSecSqr * (float) delta
		);
	}
}
