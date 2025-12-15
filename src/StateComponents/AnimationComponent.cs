using System;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class AnimationComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimationPlayer? AnimationPlayer;
	[Export(PropertyHint.Enum)] public string Animation = "";

	[ExportGroup("Playback Options")]
	[Export] public bool PlayBackwards = false;
	[Export(PropertyHint.None, "suffix:s")] public float BeginSeekSec = 0f;
	[ExportSubgroup("Speed Scaling")]
	[Export] public SpeedScaleModeEnum SpeedScaleMode = SpeedScaleModeEnum.FixedValue;
	[Export(PropertyHint.None, "suffix:px/s")] public float MinSpeed = 0;
	[Export(PropertyHint.None, "suffix:px/s")] public float MaxSpeed = float.PositiveInfinity;
	[Export(PropertyHint.Range, "0.05,8,or_greater,or_less")] public float MinSpeedScale = 1f;
	[Export(PropertyHint.Range, "0.05,8,or_greater,or_less")] public float MaxSpeedScale = 1f;
	[Export(PropertyHint.Range, "0.05,8,or_greater,or_less")] public float SpeedScale = 1f;

	[ExportGroup("Blending")]
	[Export(PropertyHint.GroupEnable)] public bool BlendEnabled;
	[Export(PropertyHint.None, "suffix:ms")] public float BlendTimeMs = 200f;

	[ExportGroup("Timing", "Timing")]
	[Export] public PlayWhenEnum TimingPlayWhen = PlayWhenEnum.StateEnter;
	[Export(PropertyHint.Expression)] public string TimingExpression = "";
	[ExportSubgroup("Expression Options")]
	[Export] public Node? TimingSelf = null;
	[Export] public Variant TimingContextVar = new Variant();
	[Export(PropertyHint.None, "suffix:ms")] public float TimingMinDurationMs = 0f;

	[ExportGroup("State Transition", "Transition")]
	[Export] public SuperconState? TransitionOnAnimationEnd = null;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression? TimingExpressionParser;
	private float TimingDurationAccumulatedTimeMs = 0f;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private int PlayBackwardsInt => this.PlayBackwards ? -1 : 1;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum PlayWhenEnum :  byte
	{
		StateEnter = 1,
		StateExit = 2,
		ExpressionIsTrue = 3,
	}

	public enum SpeedScaleModeEnum : byte
	{
		FixedValue = 1,
		Velocity = 2,
		HorizontalVelocity = 3,
		VerticalVelocity = 4,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint() && this.AnimationPlayer == null)
		{
			this.AnimationPlayer = this.Character.GetChildren().OfType<AnimationPlayer>().FirstOrDefault();
		}
		if (this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			this.TimingExpressionParser = new();
			if (this.TimingExpressionParser.Parse(this.TimingExpression, ["context"]) is Error error && error != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Error parsing expression. Error: {error}");
				this.TimingExpressionParser = null;
			}
		}
		this.AnimationPlayer?.AnimationFinished += _ => this.OnAnimationfinished();
	}

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
	// 		.Concat(true ? [] : ["Some warning"])
	// 		.ToArray();

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.Animation):
				property["hint_string"] = this.AnimationPlayer?.GetAnimationList().Join(",") ?? "";
				break;
			case nameof(this.TimingPlayWhen):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.TimingSelf):
			case nameof(this.TimingExpression):
			case nameof(this.TimingMinDurationMs):
				property["usage"] = this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.TimingContextVar):
				property["usage"] = this.TimingPlayWhen == PlayWhenEnum.ExpressionIsTrue
					? (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.NilIsVariant
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.SpeedScaleMode):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.SpeedScale):
				property["usage"] = this.SpeedScaleMode == SpeedScaleModeEnum.FixedValue
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
			case nameof(this.MinSpeed):
			case nameof(this.MaxSpeed):
			case nameof(this.MinSpeedScale):
			case nameof(this.MaxSpeedScale):
				property["usage"] = this.SpeedScaleMode != SpeedScaleModeEnum.FixedValue
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.NoEditor;
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OTHER OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconEnter(SuperconStateMachine.Transition transition)
	{
		base._SuperconEnter(transition);
		if (this.TimingPlayWhen == PlayWhenEnum.StateEnter)
		{
			this.Activate();
		}
	}

	public override void _SuperconExit(SuperconStateMachine.Transition transition)
	{
		base._SuperconExit(transition);
		if (this.TimingPlayWhen == PlayWhenEnum.StateExit)
		{
			this.Activate();
		}
	}

	public override void _SuperconProcess(double delta)
	{
		base._SuperconProcess(delta);
		if (Engine.IsEditorHint())
		{
			return;
		}
		if (this.TestTimingExpression((float) delta))
		{
			this.Activate();
		}
		if (this.SpeedScaleMode != SpeedScaleModeEnum.FixedValue)
		{
			this.AnimationPlayer?.SpeedScale = this.GetCurrentFrameSpeedScale() * this.PlayBackwardsInt;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Activate()
	{
		if (this.AnimationPlayer?.IsPlaying() == true && this.AnimationPlayer?.CurrentAnimation == this.Animation)
		{
			return;
		}
		this.AnimationPlayer?.Play("RESET");
		this.AnimationPlayer?.Advance(0f); // Force reset immediately
		this.AnimationPlayer?.Play(
			this.Animation,
			this.BlendEnabled ? this.BlendTimeMs * this.PlayBackwardsInt : default,
			this.GetInitialSpeedScale() * this.PlayBackwardsInt,
			this.PlayBackwards
		);
		if (!Mathf.IsZeroApprox(this.BeginSeekSec))
		{
			this.AnimationPlayer?.Seek(this.BeginSeekSec, update: true);
		}
	}

	private bool TestTimingExpression(float delta)
	{
		if (this.TimingExpressionParser == null)
		{
			return false;
		}
		Variant result;
		try
		{
			result = this.TimingExpressionParser.Execute([this.TimingContextVar], this.TimingSelf);
		}
		catch (Exception e)
		{
			GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Exception while evaluating timing expression. Exception: {e}");
			return false;
		}
		if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{nameof(AnimationComponent)} at {this.GetPath()}] Timing expression did not evaluate to a boolean. Returned value: {result} ({result.VariantType})");
			return false;
		}
		if (!result.AsBool())
		{
			this.TimingDurationAccumulatedTimeMs = 0f;
			return false;
		}
		this.TimingDurationAccumulatedTimeMs += delta * 1000;
		return this.TimingDurationAccumulatedTimeMs >= this.TimingMinDurationMs;
	}

	private void OnAnimationfinished()
	{
		if (
			!this.State.IsActive
			|| this.AnimationPlayer?.CurrentAnimation != this.Animation
			|| this.TransitionOnAnimationEnd == null
		)
		{
			return;
		}
		this.StateMachine?.QueueTransition(this.TransitionOnAnimationEnd);
	}

	private float GetInitialSpeedScale() => this.SpeedScaleMode == SpeedScaleModeEnum.FixedValue ? this.SpeedScale : 1f;
	private float GetCurrentFrameSpeedScale()
	{
		if (this.SpeedScaleMode == SpeedScaleModeEnum.FixedValue)
		{
			return this.SpeedScale;
		}
		float velocity = this.SpeedScaleMode switch
		{
			SpeedScaleModeEnum.HorizontalVelocity => Math.Abs(this.Character.Velocity.X),
			SpeedScaleModeEnum.VerticalVelocity => Math.Abs(this.Character.Velocity.Y),
			SpeedScaleModeEnum.Velocity => this.Character.Velocity.Length(),
			_ => 0,
		};
		return Math.Clamp((velocity - this.MinSpeed) / (this.MaxSpeed - this.MinSpeed), 0, 1)
			* (this.MaxSpeedScale - this.MinSpeedScale)
			+ this.MinSpeedScale;
	}
}
