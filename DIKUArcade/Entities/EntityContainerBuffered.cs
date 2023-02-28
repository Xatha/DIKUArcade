using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DIKUArcade.DataStructures;
using DIKUArcade.Graphics;

namespace DIKUArcade.Entities {
    public class EntityContainerBuffered : IEnumerable {
        private DoubleBufferedList<Entity> entities;

        public EntityContainerBuffered(uint size) {
            entities = new DoubleBufferedList<Entity>(size);
        }

        public EntityContainerBuffered() : this(50) { }

        public void AddStationaryEntity(StationaryShape ent, IBaseImage img) {
            entities.Add(new Entity(ent, img));
        }

        public void AddDynamicEntity(DynamicShape ent, IBaseImage img) {
            entities.Add(new Entity(ent, img));
        }

        /// <summary>
        /// Delegate method for iterating through an EntityContainer.
        /// This function should return true if the Entity should be
        /// removed from the EntityContainer.
        /// </summary>
        /// <param name="entity"></param>
        public delegate void IteratorMethod(Entity entity);

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
        /// because it utilises parallelization. As a consequence, the given delegate has to be thread-safe.
        /// If you need to perform thread-unsafe operations, consider using a foreach'-loop instead.</remarks>
        public void ImmutableIterate(IteratorMethod iterator) {
            entities.ParallelImmutableIterator(e => iterator(e));
        }

        /// <summary>
        /// Render all entities in this EntityContainer
        /// </summary>
        public void RenderEntities() {
            foreach (Entity entity in entities) {
                entity.Image.Render(entity.Shape);
            }
        }

        /// <summary>
        /// Render all entities in this EntityContainer
        /// </summary>
        public void RenderEntities(Camera camera) {
            foreach (Entity entity in entities) {
                entity.Image.Render(entity.Shape, camera);
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