using System;
using System.Linq;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[Tool]
public partial class AnimationComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	public enum FlipHEnum
	{
		Never,
		Always,
		IfFacingLeft,
	}

	public enum PlayWhenEnum
	{
		StateEnter,
		StateExit,
		ExpressionIsTrue,
		StateEnterIfExpressionIsTrue,
		StateExitIfExpressionIsTrue,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export(PropertyHint.Enum)] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.IfFacingLeft;
	[Export(PropertyHint.Range, "0.25,4,0.05,or_greater,or_less")] public float AnimationSpeedScale = 1f;
	[Export] public PlayWhenEnum PlayAnimationWhen
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); } }
		= PlayWhenEnum.StateEnter;
	[Export] public Node? Self;
	[Export(PropertyHint.Expression)] public string Expression = "";
	[Export] public Variant ContextVar = new Variant();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Expression ExpressionParser = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private bool ShouldFlipH => this.FlipH switch
	{
		FlipHEnum.Never => false,
		FlipHEnum.Always => true,
		FlipHEnum.IfFacingLeft => this.Character.FacingDirection < 0,
		_ => false,
	};


	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum Type {
	// 	Value1,
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
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
		if (this.PlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			if (this.ExpressionParser.Parse(this.Expression, ["context"]) != Error.Ok)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)}] Failed to parse expression: \"{this.Expression}\"");
			}
		}
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
				property["hint_string"] = this.AnimatedSprite?.SpriteFrames?.GetAnimationNames().Join(",") ?? "";
				break;
			case nameof(this.Expression):
			case nameof(this.Self):
			case nameof(this.ContextVar):
				property["usage"] = this.PlayAnimationWhen switch
				{
					PlayWhenEnum.ExpressionIsTrue
						or PlayWhenEnum.StateEnterIfExpressionIsTrue
						or PlayWhenEnum.StateExitIfExpressionIsTrue
						=> (int) PropertyUsageFlags.Default,
					_ => (int) PropertyUsageFlags.NoEditor,
				};
				break;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		if (
			this.PlayAnimationWhen == PlayWhenEnum.StateEnter
			|| this.PlayAnimationWhen == PlayWhenEnum.StateEnterIfExpressionIsTrue
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
	}

	public override void _ExitState()
	{
		if (
			this.PlayAnimationWhen == PlayWhenEnum.StateExit
			|| this.PlayAnimationWhen == PlayWhenEnum.StateExitIfExpressionIsTrue
			&& this.EvaluateUserExpression()
		)
		{
			this.Play();
		}
		base._ExitState();
	}

	public override void _ProcessActive(double delta)
	{
		base._ProcessActive(delta);
		if (this.PlayAnimationWhen == PlayWhenEnum.ExpressionIsTrue)
		{
			if (this.AnimatedSprite?.Animation != this.Animation && this.EvaluateUserExpression())
			{
				this.Play();
			}
		}
		if (this.FlipH == FlipHEnum.IfFacingLeft)
		{
			this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void Play()
	{
		this.AnimatedSprite?.Play(this.Animation);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		this.AnimatedSprite?.SpeedScale = this.AnimationSpeedScale;
	}

	private bool EvaluateUserExpression()
	{
		try {
			Variant result = this.ExpressionParser.Execute([this.ContextVar], this.Self);
			if (result.VariantType != Variant.Type.Bool)
			{
				GD.PrintErr($"[{nameof(AnimationComponent)}] Condition did not evaluate to a boolean: \"{this.Expression}\"");
				return false;
			}
			return result.AsBool();
		} catch (Exception e) {
			GD.PrintErr($"[{nameof(AnimationComponent)}] Failed to evaluate condition: \"{this.Expression}\"", e);
			return false;
		}
	}
}
