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

        public IEnumerator Exec()
        {
            foreach (var instruction in _instructions) {
                switch (instruction.iType) {
                    case InstructionType.Do:
                        instruction.callback();
                        break;

                    case InstructionType.Wait:
                        yield return instruction.wait;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private enum InstructionType
        { Do, Wait }

        private struct Instruction
        {
            public InstructionType iType;
            public Action callback;
            public YieldInstruction wait;
        }
    }
}