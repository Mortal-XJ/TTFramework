//----------------------
// Developer Mortal
// Date 2023 - - 
// Script Overview 
//----------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public abstract class StateMachine : MonoBehaviour
    {
        private List<StateNode> _tempStateList = new List<StateNode>();
        private List<StateNode> _reachedState = new List<StateNode>();
        private Dictionary<string, StateNode> _allState = new Dictionary<string, StateNode>();

        private bool _isExitStateMachine = false;

        protected virtual void Start()
        {
            FirstAddState();
        }

        protected void Update()
        {
            DetectionState();//检查状态
            ExecutionUpdateState();//执行状态
            UpdateAddState();//检测是否有新加入的状态机
            ExamineIsExitStateMachine();//检测是否结束状态机
        }

        protected void FixedUpdate()
        {
            ExecutionFixedUpdateState();
        }

        protected virtual void OnDestroy()
        {
            _reachedState.Clear();
            _allState.Clear();
        }


        /// <summary>
        /// 首次绑定状态
        /// </summary>
        protected abstract void FirstAddState();

        void ExecutionUpdateState()
        {
            for (int i = 0; i < _reachedState.Count; i++)
            {
                _reachedState[i].Update();
            }
        }

        void ExecutionFixedUpdateState()
        {
            for (int i = 0; i < _reachedState.Count; i++)
            {
                _reachedState[i].FixedUpdate();
            }
        }

        /// <summary>
        /// 检测状态
        /// </summary>
        void DetectionState()
        {
            foreach (var state in _allState)
            {
                if (state.Value.DetectCondition())
                {
                    if (_reachedState.Contains(state.Value)) continue;
                    state.Value.Enter();
                    _reachedState.Add(state.Value);
                }
                else
                {
                    if (!_reachedState.Contains(state.Value)) continue;
                    state.Value.Exit();
                    _reachedState.Remove(state.Value);
                }
            }
        }

        void UpdateAddState()
        {
            if (_tempStateList.Count>0)
            {
                for (int i = 0; i < _tempStateList.Count; i++)
                {
                    StateNode stateNode = _tempStateList[i];
                    _allState.Add(stateNode.GetType().Name,stateNode);
                }
                _tempStateList.Clear();
            }
        }
        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="stateNode"></param>
        public void PushState(StateNode stateNode)
        {
            if (!_allState.ContainsKey(stateNode.GetType().Name))
                _tempStateList.Add(stateNode);
            else
                Log.Warning($"已经存在该状态 ：{stateNode.GetType().Name}", Color.red);
        }

        /// <summary>
        /// 移除这个状态
        /// </summary>
        /// <param name="stateNode"></param>
        public void PullState(StateNode stateNode)
        {
            if (!_allState.ContainsValue(stateNode))
            {
                Log.Warning($"不存在该状态 ：{stateNode.GetType().Name}", Color.red);
                return;
            }

            _allState.Remove(stateNode.GetType().Name);
            if (_reachedState.Contains(stateNode))
            {
                _reachedState.Remove(stateNode);
                stateNode.Exit();
            }
        }
        /// <summary>
        /// 移除这个状态
        /// </summary>
        /// <param name="stateNode"></param>
        public void PullState(string stateNodeName)
        {
            if (!_allState.ContainsKey(stateNodeName))
            {
                Log.Warning($"不存在该状态 ：{stateNodeName.GetType().Name}", Color.red);
                return;
            }

            _allState.Remove(stateNodeName.GetType().Name);
            for (int i = 0; i < _reachedState.Count; i++)
            {
                StateNode stateNode = _reachedState[i];
                if (stateNode.GetType().Name == stateNodeName)
                {
                    stateNode.Exit();
                    _reachedState.Remove(stateNode);
                    break;
                }
            }
        }
      
        /// <summary>
        /// 退出状态机
        /// </summary>
        public void ExitStateMachine()
        {
            _isExitStateMachine = true;
        }
        void ExamineIsExitStateMachine()
        {
            if (!_isExitStateMachine)return;
            for (int i = 0; i < _reachedState.Count; i++)
            {
                _reachedState[i].Exit();
            }
            Destroy(this);
        }

        /// <summary>
        /// 根据名字获取状态机里面的状态
        /// </summary>
        /// <param name="stateNodeName"></param>
        /// <param name="stateNode"></param>
        /// <returns>是否获取成功</returns>
        public bool GetState(string stateNodeName, out StateNode stateNode)
        {
            if (_allState.ContainsKey(stateNodeName))
            {
                stateNode = _allState[stateNodeName];
                return true;
            }
            else
            {
                stateNode = default;
                return false;
            }
        }
    }
}