using System.Linq;
using Godot;
using Godot.Collections;

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

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AnimatedSprite2D? AnimatedSprite
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }} = null;
	[Export(PropertyHint.Enum)] public string Animation = "";
	[Export] public FlipHEnum FlipH = FlipHEnum.IfFacingLeft;
	[Export(PropertyHint.Range, "0.25,4,0.05,or_greater,or_less")] public float AnimationSpeedScale = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public bool ShouldFlipH => this.FlipH switch
	{
		FlipHEnum.Never => false,
		FlipHEnum.Always => true,
		FlipHEnum.IfFacingLeft => this.Character.FacingDirection < 0,
		_ => false,
	};

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------



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
		if (Engine.IsEditorHint() && this.AnimatedSprite == null)
		{
			this.AnimatedSprite = this.Character.GetChildren().OfType<AnimatedSprite2D>().FirstOrDefault();
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
		if (property["name"].AsString() == nameof(this.Animation))
		{
			property["hint_string"] = this.AnimatedSprite?.SpriteFrames?.GetAnimationNames().Join(",") ?? "";
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		this.AnimatedSprite?.Play(this.Animation);
		this.AnimatedSprite?.FlipH = this.ShouldFlipH;
	}

	public override void _ProcessActive(double delta)
	{
		base._ProcessActive(delta);
		if (this.FlipH == FlipHEnum.IfFacingLeft)
		{
			this.AnimatedSprite?.FlipH = this.ShouldFlipH;
		}
	}
}
