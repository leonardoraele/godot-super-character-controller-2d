using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class GravityComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxFallSpeedPxPSec = float.PositiveInfinity;
	[Export] public float UpwardGravityMultiplier = 5f;
	[Export] public float DownwardGravityMultiplier = 5f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2 GravityDirection;
	public float GravityMagnitudePxPSecSq;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public float GravityMultiplier => this.Character.Velocity.Dot(this.GravityDirection) < 0
		? this.UpwardGravityMultiplier
		: this.DownwardGravityMultiplier;

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.GravityDirection = ProjectSettings.GetSetting("physics/2d/default_gravity_vector").AsVector2().Normalized();
		this.GravityMagnitudePxPSecSq = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	}

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		this.Character.ApplyForce(
			this.GravityDirection * this.GravityMagnitudePxPSecSq * (float) delta * this.GravityMultiplier,
			this.MaxFallSpeedPxPSec
		);
	}
}
