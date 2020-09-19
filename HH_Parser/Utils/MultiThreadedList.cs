using System.Collections.Generic;

namespace HH_Parser
{
    public class MultiThreadedList<T> : List<T>
    {
        private readonly object obj = new object();
        private int currentElementIndex = -1;

        public T GetNextElement()
        {
            lock (this.obj)
            {
                this.currentElementIndex++;
                if (this.currentElementIndex >= this.Count)
                {
                    return default(T);
                }

                return this[this.currentElementIndex];
            }
        }
    }
}
