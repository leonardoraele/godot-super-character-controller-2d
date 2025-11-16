using System;
using Godot;
using Raele.Supercon2D.StateComponents;

namespace Raele.Supercon2D.StateTransitions;

public partial class ConditionalStateTransition : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum SelfOption
	{
		Character,
		StateMachine,
		State,
		ThisNode,
		TreeRoot,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.MultilineText)] public string Expression = "";
	[Export] public SuperconState? TransitionOnTrue;

	[ExportGroup("Evaluation Options")]
	[Export] public SelfOption Self = SelfOption.Character;
	/// <summary>
	/// This value will be available in the expression's context as the 'context' variable.
	/// </summary>
	[Export] public Variant ContextVar = new Variant();

	[ExportGroup("More Options")]
	/// <summary>
	/// Minimum duration, in milliseconds, that the condition must be true before the transition is triggered.
	/// </summary>
	[Export] public uint MinDurationMs = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private float ConditionSatisfiedMoment = float.PositiveInfinity;
	private Expression CompiledExpression = new();
	private GodotObject ResolvedSelf => this.Self switch
	{
		SelfOption.Character => this.Character,
		SelfOption.StateMachine => this.StateMachine,
		SelfOption.State => this.State,
		SelfOption.ThisNode => this,
		SelfOption.TreeRoot => this.GetTree().Root,
		_ => this,
	};

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.CompileExpression();
	}

	public override void _EnterState()
	{
		base._EnterState();
		if (OS.IsDebugBuild())
		{
			this.CompileExpression();
		}
		this.ConditionSatisfiedMoment = float.PositiveInfinity;
	}

	public override void _ProcessActive(double delta)
	{
		if (this.TransitionOnTrue == null)
		{
			return;
		}
		if (this.TestExpression())
		{
			this.ConditionSatisfiedMoment = Math.Min(this.ConditionSatisfiedMoment, Time.GetTicksMsec());
			if (this.ConditionSatisfiedMoment + this.MinDurationMs <= Time.GetTicksMsec())
			{
				this.StateMachine.QueueTransition(this.TransitionOnTrue);
			}
		}
		else
		{
			this.ConditionSatisfiedMoment = float.PositiveInfinity;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void CompileExpression()
	{
		Error error = this.CompiledExpression.Parse(this.Expression, ["context"]);
		if (error != Error.Ok)
		{
			GD.PrintErr($"[{this.GetType().FullName}] Failed to parse expression '{this.Expression}': {this.CompiledExpression.GetErrorText()}");
		}
	}

	private bool TestExpression()
	{
		Variant result;
		try
		{
			result = this.CompiledExpression.Execute([this.ContextVar], this.ResolvedSelf);
		} catch
		{
			result = new Variant();
		}
		if (this.CompiledExpression.HasExecuteFailed())
		{
			GD.PrintErr($"[{this.GetType().FullName}] Failed to execute expression '{this.Expression}': {this.CompiledExpression.GetErrorText()}");
			return false;
		} else if (result.VariantType != Variant.Type.Bool)
		{
			GD.PrintErr($"[{this.GetType().FullName}] Expression '{this.Expression}' did not evaluate to a boolean value.");
			return false;
		}
		return result.AsBool();
	}
}
