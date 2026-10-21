using UnityEngine;

/// <summary>
/// Base abstract class for all enemy states
/// Egoera guztientzako oinarrizko klase abstraktua
/// </summary>
public abstract class EnemyState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine stateMachine;

    public EnemyState(EnemyAI enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    /// <summary>
    /// Called when entering this state
    /// Egoera hau sartzen denean deitzen da
    /// </summary>
    public virtual void Enter()
    {
        // Override in child classes
    }

    /// <summary>
    /// Called every frame while in this state
    /// Frame bakoitzean deitzen da egoera honetan dagoen bitartean
    /// </summary>
    public virtual void Update()
    {
        // Override in child classes
    }

    /// <summary>
    /// Called every fixed frame (physics)
    /// Frame finkatu bakoitzean deitzen da (fisika)
    /// </summary>
    public virtual void FixedUpdate()
    {
        // Override in child classes
    }

    /// <summary>
    /// Called when exiting this state
    /// Egoera honetatik irteten denean deitzen da
    /// </summary>
    public virtual void Exit()
    {
        // Override in child classes
    }
}
