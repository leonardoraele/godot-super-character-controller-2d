#if TOOLS
using Godot;
using Raele.Supercon2D.StateComponents;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		Texture2D stateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_neutral.png");
		Texture2D genericStateIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_bg.png");
		Texture2D gravityIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_gravity.png");
		Texture2D singleAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_single_axis_control.png");
		Texture2D animatedSprite = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animated_sprite_3.png");
		Texture2D binaryIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_binary.png");
		Texture2D buttonIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_button.png");
		Texture2D multiAxisControlIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_multi_axis_control.png");
		Texture2D forceIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_force_2.png");
		Texture2D slopeIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_slope.png");
		Texture2D impulseIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_impulse.png");
		Texture2D presetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_preset.png");
		Texture2D facingIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_facing.png");
		Texture2D animationPlayIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_play.png");
		Texture2D animationParamIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_animation_param.png");
		Texture2D gateIdon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_t_gate.png");
		Texture2D velocityResetIcon = GD.Load<Texture2D>($"res://addons/{nameof(Supercon2D)}/icons/character_body_velocity_reset.png");

		// Core Types
		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), stateIcon);
		this.AddCustomType(nameof(SuperconStateLayer), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconStateLayer)}.cs"), stateIcon);

		// State Components
		this.AddCustomType(nameof(SuperconStateComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconStateComponent)}.cs"), genericStateIcon);
		this.AddCustomType(nameof(PlayAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PlayAnimationComponent)}.cs"), animationPlayIcon);
		this.AddCustomType(nameof(AnimationParamComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(AnimationParamComponent)}.cs"), animationParamIcon);
		this.AddCustomType(nameof(CustomTriggerComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(CustomTriggerComponent)}.cs"), binaryIcon);
		this.AddCustomType(nameof(ForceComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ForceComponent)}.cs"), forceIcon);
		this.AddCustomType(nameof(GravityComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(GravityComponent)}.cs"), gravityIcon);
		this.AddCustomType(nameof(ImpulseComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(ImpulseComponent)}.cs"), impulseIcon);
		this.AddCustomType(nameof(InputActionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(InputActionComponent)}.cs"), buttonIcon);
		this.AddCustomType(nameof(MultiAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(MultiAxisControlComponent)}.cs"), multiAxisControlIcon);
		this.AddCustomType(nameof(PresetMovementComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(PresetMovementComponent)}.cs"), presetIcon);
		this.AddCustomType(nameof(SingleAxisControlComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SingleAxisControlComponent)}.cs"), singleAxisControlIcon);
		this.AddCustomType(nameof(SlopeComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SlopeComponent)}.cs"), slopeIcon);
		this.AddCustomType(nameof(SpriteAnimationComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(SpriteAnimationComponent)}.cs"), animatedSprite);
		this.AddCustomType(nameof(TransitionGateComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(TransitionGateComponent)}.cs"), gateIdon);
		this.AddCustomType(nameof(VelocityResetComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateComponents)}/{nameof(VelocityResetComponent)}.cs"), velocityResetIcon);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconState));

		this.RemoveCustomType(nameof(PlayAnimationComponent));
		this.RemoveCustomType(nameof(AnimationParamComponent));
		this.RemoveCustomType(nameof(CustomTriggerComponent));
		this.RemoveCustomType(nameof(ForceComponent));
		this.RemoveCustomType(nameof(GravityComponent));
		this.RemoveCustomType(nameof(ImpulseComponent));
		this.RemoveCustomType(nameof(InputActionComponent));
		this.RemoveCustomType(nameof(MultiAxisControlComponent));
		this.RemoveCustomType(nameof(PresetMovementComponent));
		this.RemoveCustomType(nameof(SingleAxisControlComponent));
		this.RemoveCustomType(nameof(SlopeComponent));
		this.RemoveCustomType(nameof(SpriteAnimationComponent));
		this.RemoveCustomType(nameof(TransitionGateComponent));
		this.RemoveCustomType(nameof(VelocityResetComponent));
	}
}
#endif
