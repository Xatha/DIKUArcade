﻿using DIKUArcade.Math;

namespace DIKUArcade.Entities {
    /// <summary>
    /// Similar to DynamicEntity, but does not contain direction information,
    /// since a static object os not meant to be affected by game physics.
    /// </summary>
    public class StationaryEntity : Entity {
        public StationaryEntity(int posX, int posY, int width, int height) {
            Position = new Vec2F(posX, posY);
            Extent = new Vec2F();
        }

        public StationaryEntity(Vec2F pos, Vec2F extent) {
            Position = pos;
            Extent = extent;
        }

        public static explicit operator DynamicEntity(StationaryEntity sta) {
            return new DynamicEntity(sta.Position, sta.Extent);
        }
    }
}