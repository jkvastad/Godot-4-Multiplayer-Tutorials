using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkerCoop.Utilities
{
    public class ListId<T> : List<T> where T : class, IHasId
    {        
        public T? GetById(int id)
        {
            return this.SingleOrDefault(x => x.Id == id); // T must be class, default always null
        }

        public new void Add(T t)
        {
            if (Exists(oldT => oldT.Id == t.Id))
                throw new ArgumentException($"{nameof(ListId<T>)} already has element with id {t.Id}");

            base.Add(t);
        }

        public bool Remove(int id)
        {
            T? t = GetById(id);
            if (t is not null)
                return Remove(t);
            return false;
        }
    }
}