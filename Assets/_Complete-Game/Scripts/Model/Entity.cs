using System;

namespace Relay
{
	/**
	 * The base class for all entities in Relay - the user, animals, homes, etc.
	 */
	public class Entity
	{
		int x, y;

		public Entity (int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}
}

