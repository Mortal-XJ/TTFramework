//----------------------
// Developer Mortal
// Date 2023 - 10 - 11 
// Script Overview 状态机的状态节点
//----------------------

namespace GameFrame
{
    public abstract class StateNode
    {
        protected StateMachine StateMachine { get; set; }

        public StateNode(StateMachine stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public abstract bool DetectCondition();
        
        public virtual void Enter(){}
        public virtual void Update(){}
        public virtual void FixedUpdate(){}
        public virtual void Exit(){}
    }
}