using System.Collections;
using System.Collections.Generic;
using DIKUArcade.DataStructures;

namespace DIKUArcade.Entities {
    public sealed class EntityContainer<T> : IEnumerable where T: Entity {
        private readonly DoubleBufferedList<T> entities;

        public EntityContainer(int size) {
            entities = new DoubleBufferedList<T>((uint)size);
        }

        public EntityContainer() : this(50) { }

        public void AddEntity(T obj) {
            entities.Add(obj);
        }

        /// <summary>
        /// Delegate method for iterating through an EntityContainer.
        /// This function should return true if the object should be
        /// removed from the EntityContainer.
        /// </summary>
        /// <param name="obj">Generic object of type T</param>
        public delegate void IteratorMethod(T obj);

        /// <summary>Iterate through all Entities in this EntityContainer.</summary>
        /// <remarks>This method can modify objects during iteration!
        /// If this functionality is undesired, iterate then through this
        /// EntityContainer using a 'foreach'-loop (from IEnumerable).</remarks>
        public void Iterate(IteratorMethod iterator) {
            entities.MutatingIterator(e =>
            {
                iterator(e);
                return !e.IsDeleted();
            });
        }
        
        /// <summary>Iterate through all Entities in this EntityContainer.</summary>
        /// <remarks>This method cannot modify objects during iteration, but is much faster than <see cref="Iterate"/>> 
        /// when entity count is large, because it utilises parallelization. As a consequence, the given delegate has to be thread-safe.
        /// If you need to perform thread-unsafe operations, consider using a foreach'-loop instead.</remarks>
        public void ImmutableIterate(IteratorMethod iterator) {
            entities.ParallelImmutableIterator(e => iterator(e));
        }


        /// <summary>
        /// Render all entities in this EntityContainer
        /// </summary>
        public void RenderEntities() {
            foreach (var obj in entities) {
                obj.Image.Render(obj.Shape);
            }
        }

        /// <summary>
        /// Remove all entities from this container
        /// </summary>
        public void ClearContainer() {
            entities.Clear();
        }

        /// <summary>
        /// Count the number of entities in the EntityContainer
        /// </summary>
        public int CountEntities() {
            return entities.Count;
        }
        
        // IEnumerable interface:
        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<Entity> GetEnumerator() {
            return entities.GetEnumerator();
        }

        #endregion

    }
}