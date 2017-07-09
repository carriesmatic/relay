using System;

namespace Relay
{
	public enum TileType {
		Floor,
		Wall
	}
	/**
	 * A Tile in Relay. Each tile has a tile type. This class is immutable.
	 */
	public class Tile
	{
		private readonly TileType type;

		public Tile (TileType type)
		{
			this.type = type;
		}
	}
}

