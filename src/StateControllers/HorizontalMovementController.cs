using System;
using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class HorizontalMovementController : StateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AutomaticMovementDirection : byte
	{
		Manual = 0,
		Left = 1,
		Right = 2,
		Idle = 3,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedPxPSec = 600f;
	[Export] public float AccelerationPxPSecSqr = 1200f;
	/// <summary>
	/// Deceleration when no input is given.
	/// </summary>
	[Export] public float SoftDecelerationPxPSecSqr = 1200f;
	/// <summary>
	/// Deceleration when current input is lower than the current speed or opposite to current movement direction.
	/// </summary>
	[Export] public float HardDecelerationPxPSecSqr = 2400f;

	[ExportGroup("Options")]
	/// <summary>
	/// Which directions the player is allowed to move the character. If either is disabled, the player cannot move the
	/// character to that direction.
	/// </summary>
	// [Export(PropertyHint.Flags, "1:Left,2:Right")] public byte Direction = 3;
	/// <summary>
	/// If either direction is set, the character will move automatically to that direction as if the player was
	/// constantly inputting the directional movement to that direction. Actual player input is ignored in this case.
	/// </summary>
	[Export] public AutomaticMovementDirection AutomaticMovement = AutomaticMovementDirection.Manual;
	/// <summary>
	/// If true, inverts the direction of movement.
	/// (useful to implement things like status effects that reverse controls)
	/// </summary>
	[Export] public bool InvertDirections = false;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ResolvedInput
		=> this.AutomaticMovement switch
			{
				AutomaticMovementDirection.Manual => this.Character.InputManager.MovementInput,
				AutomaticMovementDirection.Left => Vector2.Left,
				AutomaticMovementDirection.Right => Vector2.Right,
				_ => Vector2.Zero,
			}
			* (this.InvertDirections ? -1 : 1);

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		float targetVelocityX = this.MaxSpeedPxPSec * this.ResolvedInput.X;
		double accelerationX =
			Math.Abs(this.Character.Velocity.X) < float.Epsilon
			|| Math.Abs(targetVelocityX) > Math.Abs(this.Character.Velocity.X)
				&& Math.Sign(targetVelocityX) == Math.Sign(this.Character.Velocity.X)
				? this.AccelerationPxPSecSqr * delta
			: Math.Abs(this.ResolvedInput.X) < float.Epsilon ? this.SoftDecelerationPxPSecSqr * delta
			: this.HardDecelerationPxPSecSqr * delta;
		this.Character.AccelerateX(targetVelocityX, (float) accelerationX);
	}
}
