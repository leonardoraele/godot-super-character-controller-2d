using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool][GlobalClass]
public partial class VelocityResetComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AxisEnum Axis = AxisEnum.Both;
	[Export] public ModeEnum Mode = ModeEnum.Instant;
	[Export(PropertyHint.ExpEasing, "attenuation")] public float Easing = 1f;
	[Export(PropertyHint.None, "suffix:ms")] public int Duration = 200;
	[Export(PropertyHint.Range, "0.01,1")] public float LerpWeight = 0.05f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 InitialVelocity = Vector2.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool HorizontalAffected => this.Axis == AxisEnum.Both || this.Axis == AxisEnum.Horizontal;
	public bool VerticalAffected => this.Axis == AxisEnum.Both || this.Axis == AxisEnum.Vertical;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AxisEnum : byte {
		Both,
		Horizontal,
		Vertical,
	}

	public enum ModeEnum : byte {
		Instant,
		Ease,
		Lerp,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? ["This node is not configured correctly. Is any mandatory property empty?"] : [])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Mode):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.Easing):
			case nameof(this.Duration):
				property["usage"] = this.Mode == ModeEnum.Ease
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.LerpWeight):
				property["usage"] = this.Mode == ModeEnum.Lerp
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	public override void _SuperconStart()
	{
		base._SuperconStart();
		this.InitialVelocity = this.Character?.Velocity ?? Vector2.Zero;
		this.SetPhysicsProcess(true);
	}

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		if (this.State?.ActiveDuration.TotalMilliseconds > this.Duration - Mathf.Epsilon)
		{
			this.ZeroOutVelocity();
			return;
		}
		switch (this.Mode)
		{
			case ModeEnum.Instant:
				this.ProcessInstant();
				break;
			case ModeEnum.Ease:
				this.ProcessEase();
				break;
			case ModeEnum.Lerp:
				this.ProcessLerp();
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void ProcessInstant()
	{
		this.ZeroOutVelocity();
	}

	private void ProcessEase()
	{
		if (this.State == null)
		{
			return;
		}
		if (this.HorizontalAffected)
			this.Character?.VelocityX = this.InitialVelocity.X * Mathf.Ease((float) this.State.ActiveDuration.TotalMilliseconds / this.Duration, this.Easing);
		if (this.VerticalAffected)
			this.Character?.VelocityY = this.InitialVelocity.Y * Mathf.Ease((float) this.State.ActiveDuration.TotalMilliseconds / this.Duration, this.Easing);
	}

	private void ProcessLerp()
	{
		if (this.HorizontalAffected)
			this.Character?.VelocityX = Mathf.Lerp(this.Character.VelocityX, 0f, this.LerpWeight);
		if (this.VerticalAffected)
			this.Character?.VelocityY = Mathf.Lerp(this.Character.VelocityY, 0f, this.LerpWeight);
	}

	private void ZeroOutVelocity()
	{
		if (this.HorizontalAffected)
			this.Character?.VelocityX = 0f;
		if (this.VerticalAffected)
			this.Character?.VelocityY = 0f;
		this.SetPhysicsProcess(false);
	}
}
