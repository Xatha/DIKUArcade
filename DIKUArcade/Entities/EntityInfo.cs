﻿using DIKUArcade.Strategies;

namespace DIKUArcade.Entities
{
    public class EntityInfo
    {
        public Entity Entity;
        public MovementStrategy MovementStrategy;
        
        public EntityInfo(Entity entity, MovementStrategy strat)
        {
            Entity = entity;
            MovementStrategy = strat;
        }
    }
}