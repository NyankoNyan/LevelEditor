using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2
{
    public class Operation : IOperation
    {
        private readonly List<Instruction> _instructions = new();

        public IOperation Do(Action callback)
        {
            Assert.IsNotNull(callback);
            _instructions.Add(new Instruction() {
                iType = InstructionType.Do,
                callback = callback
            });
            return this;
        }

        public IOperation Wait(YieldInstruction wait)
        {
            _instructions.Add(new Instruction() {
                iType = InstructionType.Wait,
                wait = wait
            });
            return this;
        }

        public IOperation Call(IOperation operation)
        {
            _instructions.Add(new() {
                iType = InstructionType.Call,
                operation = operation
            });
            return this;
        }

        public IOperation CallSelf()
        {
            _instructions.Add(new() {
                iType = InstructionType.Call,
                operation = this
            });
            return this;
        }

        public IOperation Break(ConditionDelegate cond)
        {
            _instructions.Add(new() {
                iType = InstructionType.Break,
                cond = cond
            });
            return this;
        }

        public IEnumerator Exec()
        {
            bool tailRecursion = _instructions.Count > 0
                                 && _instructions[^1].iType == InstructionType.Call
                                 && _instructions[^1].operation == this;
            do {
                bool breakFor = false;
                for (int i = 0; i < _instructions.Count; i++) {
                    bool last = i == _instructions.Count - 1;
                    if (last && tailRecursion) {
                        break;
                    }

                    var instruction = _instructions[i];

                    switch (instruction.iType) {
                        case InstructionType.Do:
                            instruction.callback();
                            break;

                        case InstructionType.Wait:
                            yield return instruction.wait;
                            break;

                        case InstructionType.Call:
                            yield return instruction.operation.Exec();
                            break;

                        case InstructionType.Break:
                            if (instruction.cond()) {
                                tailRecursion = false;
                                breakFor = true;
                            }

                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    if (breakFor) {
                        break;
                    }
                }
            } while (tailRecursion);
        }

        private enum InstructionType
        {
            Do, Wait, Call, Break
        }

        private struct Instruction
        {
            public InstructionType iType;
            public Action callback;
            public YieldInstruction wait;
            public IOperation operation;
            public ConditionDelegate cond;
        }
    }
}