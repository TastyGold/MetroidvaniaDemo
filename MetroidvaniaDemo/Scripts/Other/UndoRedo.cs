using System;
using System.Collections.Generic;

namespace UndoRedo
{
    public class ActionHistory
    {
        //Data
        private readonly List<IAction> pastActions; //first index: earliest action, last index: most recent action
        private readonly List<IAction> futureActions; //first index: next redo, last index: final redo
        public int maxHistoryLength = 20;

        public ActionHistory()
        {
            pastActions = new List<IAction>();
            futureActions = new List<IAction>();
        }

        //Methods
        public void RecordAction(IAction action)
        {
            Console.WriteLine($"Recorded action: {action}");
            pastActions.Add(action);
            if (pastActions.Count > 20) pastActions.RemoveAt(0);
            ClearRedo();
        }
        public void RecordAndExectute(IAction action)
        {
            action.Execute();
            RecordAction(action);
        }
        public void UndoLastAction()
        {
            int lastIndex = pastActions.Count - 1;

            if (lastIndex >= 0)
            {
                pastActions[lastIndex].Unexecute();
                futureActions.Insert(0, pastActions[lastIndex]);
                pastActions.RemoveAt(lastIndex);
            }
        }
        public void RedoNextAction()
        {
            if (futureActions.Count > 0)
            {
                futureActions[0].Execute();
                pastActions.Add(futureActions[0]);
                futureActions.RemoveAt(0);
            }
        }
        public void ClearAll()
        {
            pastActions.Clear();
            futureActions.Clear();
        }
        public void ClearRedo()
        {
            futureActions.Clear();
        }
    }

    public interface IAction
    {
        public void Execute();
        public void Unexecute();
    }
}