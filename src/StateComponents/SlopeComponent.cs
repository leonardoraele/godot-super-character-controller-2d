using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class SlopeComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Slope Up Resistance")]
	[Export] public float SlopeDecelerationPxPSecSqr = 0f;
	[Export] public float SlopeMaxSpeedPxPSec = float.PositiveInfinity;

	[ExportGroup("Slope Down Acceleration")]
	[Export] public float SlideAccelerationPxPSecSqr = 0f;
	[Export] public float SlideMaxSpeedPxPSec = float.PositiveInfinity;

	[ExportGroup("Options")]
	[Export(PropertyHint.Range, "0,1,0.05")] public float NormalizationRate = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);

		if (this.Character.IsOnSlope && this.Character.Velocity.Length() > Mathf.Epsilon)
		{
			float currentVelocity = this.Character.Velocity.Length();
			float normalizedVelocity = Mathf.Lerp(
				(this.Character.Velocity * Vector2.Right).Project(this.Character.GetFloorNormal().Rotated(Mathf.Pi / 2)).Length(),
				currentVelocity,
				this.NormalizationRate
			);
			float newVelocity = this.Character.Velocity.Dot(Vector2.Up) >= 0
				// Moving upward on the slope
				? Mathf.MoveToward(
					normalizedVelocity,
					Math.Min(this.Character.Velocity.Length(), this.SlopeMaxSpeedPxPSec),
					this.SlopeDecelerationPxPSecSqr
						// * Math.Abs(this.Character.GetFloorNormal().Dot(Vector2.Right))
						* (float) delta
				)
				// Moving downward on the slope
				: Mathf.MoveToward(
					normalizedVelocity,
					Math.Max(this.Character.Velocity.Length(), this.SlideMaxSpeedPxPSec),
					this.SlideAccelerationPxPSecSqr
						// * Math.Abs(this.Character.GetFloorNormal().Dot(Vector2.Right))
						* (float) delta
				);
			Vector2 floorDirection = this.Character.GetFloorNormal().Rotated(Mathf.Pi / 2);
			this.Character.Velocity = floorDirection
				* newVelocity
				* Math.Sign(this.Character.Velocity.Dot(floorDirection)) switch {
					1 => 1,
					-1 => -1,
					_ => 1
				};
		}
	}
}
