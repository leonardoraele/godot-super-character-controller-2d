using System;
using Godot;
using Godot.Collections;
using Raele.GodotUtils;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Node2D, SuperconStateMachine.IState, IActivity
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	IActivity.ActivityData IActivity.InstanceFields { get; } = new();
	public ISuperconStateMachineOwner? StateMachineOwner => ISuperconStateMachineOwner.GetOrNull(this);

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.AsActivity().IsActive;
	public TimeSpan ActiveTimeSpan => this.AsActivity().ActiveTimeSpan;
	public bool IsPreviousActiveState => this.StateMachineOwner?.StateMachine.PreviousActiveState == this;
	// TOOD Do we really need this class here?
	public SuperconInputMapping? InputMapping => this.StateMachineOwner?.Character?.InputMapping;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Obsolete("Use WillStart instead.")][Signal] public delegate void StateWillEnterEventHandler(SuperconStateMachine.Transition transition, GodotCancellationController cancellationController);
	[Obsolete("Use Started instead.")][Signal] public delegate void StateEnteredEventHandler(SuperconStateMachine.Transition transition);
	[Obsolete("Use WillFinish instead.")][Signal] public delegate void StateWillExitEventHandler(SuperconStateMachine.Transition transition, GodotCancellationController cancellationController);
	[Obsolete("Use Finished instead.")][Signal] public delegate void StateExitedEventHandler(SuperconStateMachine.Transition transition);

	[Signal] public delegate void WillStartEventHandler(string mode, Variant argument, GodotCancellationController controller);
	[Signal] public delegate void StartedEventHandler(string mode, Variant argument);
	[Signal] public delegate void WillFinishEventHandler(string reason, Variant details, GodotCancellationController controller);
	[Signal] public delegate void FinishedEventHandler(string reason, Variant details);

	event Action<string, Variant, GodotCancellationController> IActivity.EventWillStart
	{
		add => this.Connect(SignalName.WillStart, value.ToCallable());
		remove => this.Disconnect(SignalName.WillStart, value.ToCallable());
	}
	event Action<string, Variant> IActivity.EventStarted
	{
		add => this.Connect(SignalName.Started, value.ToCallable());
		remove => this.Disconnect(SignalName.Started, value.ToCallable());
	}
	event Action<string, Variant, GodotCancellationController> IActivity.EventWillFinish
	{
		add => this.Connect(SignalName.WillFinish, value.ToCallable());
		remove => this.Disconnect(SignalName.WillFinish, value.ToCallable());
	}
	event Action<string, Variant> IActivity.EventFinished
	{
		add => this.Connect(SignalName.Finished, value.ToCallable());
		remove => this.Disconnect(SignalName.Finished, value.ToCallable());
	}

	public void InvokeEventWillStart(string mode, Variant argument, GodotCancellationController controller)
		=> this.EmitSignalWillStart(mode, argument, controller);
	public void InvokeEventStarted(string mode, Variant argument)
		=> this.EmitSignalStarted(mode, argument);
	public void InvokeEventWillFinish(string reason, Variant details, GodotCancellationController controller)
		=> this.EmitSignalWillFinish(reason, details, controller);
	public void InvokeEventFinished(string reason, Variant details)
		=> this.EmitSignalFinished(reason, details);

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public void Start(string mode = "", Variant argument = new Variant())
		=> this.AsActivity().Start(mode, argument);
	public void Finish(string reason = "", Variant details = new Variant())
		=> this.AsActivity().Finish(reason, details);

	public override Array<Dictionary> _GetPropertyList()
		=> this.AsActivity().HandleGetPropertyList();
	public override void _ValidateProperty(Dictionary property)
		=> this.AsActivity().HandleValidateProperty(property);
	public override Variant _Get(StringName property)
		=> this.AsActivity().HandleGet(property);
	public override bool _Set(StringName property, Variant value)
		=> this.AsActivity().HandleSet(property, value);
	public override void _EnterTree()
		=> this.AsActivity().HandleEnterTree();
	public override void _ExitTree()
		=> this.AsActivity().HandleExitTree();
	public override void _Ready()
		=> this.AsActivity().HandleReady();
	public override void _Process(double delta)
		=> this.AsActivity().HandleProcess(delta);
	public override void _PhysicsProcess(double delta)
		=> this.AsActivity().HandlePhysicsProcess(delta);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Start($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);
	void SuperconStateMachine.IState.ExitState(SuperconStateMachine.Transition transition)
		=> this.AsActivity().Finish($"{nameof(SuperconStateMachine)}.{nameof(SuperconStateMachine.Transition)}", transition);

	public void QueueTransition() => this.StateMachineOwner?.StateMachine.QueueTransition(this);
	public void QueueTransition(Variant data) => this.StateMachineOwner?.StateMachine.QueueTransition(this, data);
}
