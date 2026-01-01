using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Raele.MyProject;

namespace Raele.Supercon2D;

[Tool][GlobalClass]
public partial class SuperconState : Node2D, SuperconStateMachine.IState, IDiscreteIntervalProcessor
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public ProcessModeEnum ProcessModeWhenActive = ProcessModeEnum.Inherit;
	[Export] public ProcessModeEnum ProcessModeWhenInactive = ProcessModeEnum.Disabled;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public ISuperconStateMachineOwner? StateMachineOwner => ISuperconStateMachineOwner.GetOrNull(this);

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.StateMachineOwner?.StateMachine.ActiveState == this;
	public bool IsPreviousActiveState => this.StateMachineOwner?.StateMachine.PreviousActiveState == this;
	public SuperconInputMapping? InputMapping => this.StateMachineOwner?.Character?.InputMapping;
	public TimeSpan ActiveTimeSpan => this.IsActive
		? this.StateMachineOwner?.StateMachine.ActiveStateDuration ?? TimeSpan.Zero
		: TimeSpan.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateEnteredEventHandler(SuperconStateMachine.Transition transition);
	[Signal] public delegate void StateExitedEventHandler(SuperconStateMachine.Transition transition);

	public event Action<Variant, CancellationTokenSource>? WillStartEvent;
	event Action<Variant> IDiscreteIntervalProcessor.StartedEvent
	{
		add
		{
			if (value.Target is not GodotObject godotObject)
				throw new NotSupportedException("Only methods on GodotObject targets are supported.");
			this.Connect(SignalName.StateEntered, new Callable(godotObject, value.Method.Name));
		}
		remove
		{
			if (value.Target is not GodotObject godotObject)
				throw new NotSupportedException("Only methods on GodotObject targets are supported.");
			this.Disconnect(SignalName.StateEntered, new Callable(godotObject, value.Method.Name));
		}
	}
	public event Action<Variant, CancellationTokenSource>? FinishRequestedEvent;
	event Action<Variant> IDiscreteIntervalProcessor.FinishedEvent
	{
		add
		{
			if (value.Target is not GodotObject godotObject)
				throw new NotSupportedException("Only methods on GodotObject targets are supported.");
			this.Connect(SignalName.StateExited, new Callable(godotObject, value.Method.Name));
		}
		remove
		{
			if (value.Target is not GodotObject godotObject)
				throw new NotSupportedException("Only methods on GodotObject targets are supported.");
			this.Disconnect(SignalName.StateExited, new Callable(godotObject, value.Method.Name));
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	async Task IDiscreteIntervalProcessor.Start(Variant argument)
	{
		CancellationTokenSource cts = new();
		this.WillStartEvent?.Invoke(argument, cts);
		if (cts.IsCancellationRequested)
			throw new TaskCanceledException();
		this.QueueTransition(argument);
	}
	async Task IDiscreteIntervalProcessor.Finish(Variant reason)
	{
		if (!this.IsActive)
			throw new InvalidOperationException("Cannot finish a state that is not active.");
		CancellationTokenSource cts = new();
		this.FinishRequestedEvent?.Invoke(reason, cts);
		if (cts.IsCancellationRequested)
			throw new TaskCanceledException();
		this.StateMachineOwner?.ResetState();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (!Engine.IsEditorHint())
		{
			this.ProcessMode = this.ProcessModeWhenInactive;
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
	// 		.ToArray();

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	void SuperconStateMachine.IState.EnterState(SuperconStateMachine.Transition transition)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		try
		{
			this.EmitSignalStateEntered(transition);
		}
		finally
		{
			if (!transition.IsCanceled)
			{
				this.ProcessMode = this.ProcessModeWhenActive;
			}
		}
	}

	void SuperconStateMachine.IState.ExitState(SuperconStateMachine.Transition transition)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		try
		{
			this.EmitSignalStateExited(transition);
		}
		finally
		{
			if (!transition.IsCanceled)
			{
				this.ProcessMode = this.ProcessModeWhenInactive;
			}
		}
	}

	public void QueueTransition() => this.StateMachineOwner?.StateMachine.QueueTransition(this);
	public void QueueTransition(Variant data) => this.StateMachineOwner?.StateMachine.QueueTransition(this, data);
}
