using System;

namespace Relay
{
	/**
	 * Mutable class that represents an entire level. This is the M in MVC.
	 * Unity GameObjects (which are the V and C) should communicate and synchronize state with Levels.
	 */
	public class Level
	{
		public static readonly int LEVEL_WIDTH = 10;
		public static readonly int LEVEL_HEIGHT = 7;

		/**
		 * Row major tile array. tiles[0][0] is the top-left tile, tiles[0][9] is the top-right tile
		 * tiles[6][0] is the bottom-left tile, tiles[6][9] is the bottom-right tile.
		 */
		Tile[][] tiles;

		/**
		 * All the entities that live in this Level. 
		 */
		Entity[] entities;

		public Level (Tile[][] tiles, Entity[] entities)
		{
			this.tiles = tiles;
			this.entities = entities;
		}

//		public static Level instantiateFromASCIIFormat(string ascii) {
//			// TODO fill in
//		}
	}
}

