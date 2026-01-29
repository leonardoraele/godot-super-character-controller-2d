// using System;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

/// <summary>
/// Allows the player to control the character using directional input to move it along a plane. e.g. To move the
/// character over the floor or climbing a wall.
///
/// You can set different speed values for forward, lateral, and backward movement.
///
/// This component does not not update the character's rotation direction. For that, use the FacingComponent.
/// </summary>
[GlobalClass]
public partial class PlaneControlComponent : SuperconStateComponent3D
{
	//==================================================================================================================
		#region STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EXPORTS
	//==================================================================================================================

	[Export] public PlaneOptionsEnum MovementPlane = PlaneOptionsEnum.Floor;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxForwardSpeed = 5f;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxLateralSpeed = 5f;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxBackwardSpeed = 5f;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxVerticalSpeed = 5f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Acceleration = 10f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float DragDeceleration = 15f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float BrakeDeceleration = 50f;
	[Export(PropertyHint.None, "0,360,radians_as_degrees,suffix:°/s")] public float TurnSpeed = 50f;
	[Export] public SpeedAboveMaxBehavior SpeedAboveMax = SpeedAboveMaxBehavior.DragIfInputIsNeutral;

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region FIELDS
	//==================================================================================================================



	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region COMPUTED PROPERTIES
	//==================================================================================================================

	public Plane? ResolveGlobalMovementPlane()
		=> this.MovementPlane switch
		{
			PlaneOptionsEnum.Floor => this.Character?.IsOnFloor() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.Wall => this.Character?.IsOnWall() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.Ceiling => this.Character?.IsOnCeiling() == true
				&& this.Character.GetLastSlideCollision() is KinematicCollision3D collision
					? new Plane(collision.GetNormal(), collision.GetPosition())
					: null,
			PlaneOptionsEnum.PlaneXZ => Plane.PlaneXZ with { D = this.Character?.GlobalPosition.Length() ?? 0},
			PlaneOptionsEnum.PlaneXY => Plane.PlaneXY with { D = this.Character?.GlobalPosition.Length() ?? 0},
			PlaneOptionsEnum.PlaneYZ => Plane.PlaneYZ with { D = this.Character?.GlobalPosition.Length() ?? 0},
			_ => null,
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EVENTS & SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler();

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region INTERNAL TYPES
	//==================================================================================================================

	public enum PlaneOptionsEnum : sbyte {
		Floor = 16,
		Wall = 20,
		Ceiling = 24,
		PlaneXZ = 64,
		PlaneXY = 65,
		PlaneYZ = 66,
	}

	public enum SpeedAboveMaxBehavior : sbyte
	{
		Clamp = 0,
		DragIfInputIsNeutral = 24,
		AlwaysDrag = 32,
		Brake = 64,
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	// public override string[] _GetConfigurationWarnings()
	// 	=> (base._GetConfigurationWarnings() ?? [])
	// 		.AppendIf(false "This node is not configured correctly. Did you forget to assign a required field?")
	// 		.ToArray();

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof():
	// 			break;
	// 	}
	// }

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.Character == null)
			return;
		if (this.ResolveGlobalMovementPlane() is not Plane plane)
			return;
		Vector3 currentVelocity = this.Character.Velocity.Project(plane);
		double currentSpeed = currentVelocity.Length();
		bool isMoving = currentSpeed > Mathf.Epsilon;
		Vector3 currentDirection = isMoving
			? currentVelocity.Normalized()
			: this.Character.GlobalBasis.Forward;
		Vector2 rawInput = this.Character.InputController?.RawMovementInput ?? Vector2.Zero;
		float inputStrength = rawInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = hasInput
			? Basis.LookingAt(plane.Normal) * new Vector3(rawInput.X, rawInput.Y, 0)
			: Vector3.Zero;
		Vector3 newGlobalDirection = isMoving && hasInput
				? currentDirection.MoveToward(inputDirection, this.TurnSpeed * delta)
			: isMoving ? currentDirection
			: hasInput ? inputDirection
			: this.Character.Basis.Forward;
		Vector3 newLocalDirection = this.Character.GlobalBasis * newGlobalDirection;
		float maxSpeed = new Vector3(
				newLocalDirection.X * this.MaxLateralSpeed,
				newLocalDirection.Y * this.MaxVerticalSpeed,
				newLocalDirection.Z * (
					newLocalDirection.Z < 0
						? this.MaxForwardSpeed
						: this.MaxBackwardSpeed
				)
			)
			.Length();
		float targetSpeed = maxSpeed * inputStrength;
		float acceleration = targetSpeed > currentSpeed - Mathf.Epsilon ? this.Acceleration
			: inputStrength.IsZeroApprox() ? this.DragDeceleration
			: this.BrakeDeceleration;
		float newSpeed = (float) currentSpeed.MoveToward(targetSpeed, acceleration * delta);
		this.Character.Velocity = newGlobalDirection * newSpeed
			+ this.Character.Velocity.Project(plane.Normal);
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region METHODS
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
