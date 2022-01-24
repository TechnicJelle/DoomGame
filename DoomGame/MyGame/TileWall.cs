﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GXPEngine.Core;

namespace GXPEngine.MyGame
{
	public class TileWall : Tile
	{
		private readonly WallSide[] sides;

		//TODO: Add texture for on the minimap

		public TileWall(int col, int row, string filename) : base()
		{
			sides = new WallSide[4];
			for (int i = 0; i < sides.Length; i++)
			{
				sides[i] = new WallSide(filename, i, col + 0.5f, row + 0.5f);
			}
		}

		/// <param name="visibility">If the entire tile is visible or not</param>
		public void SetVisibility(bool visibility)
		{
			foreach (WallSide side in sides)
				side.SetVisibility(visibility);
		}

		public IEnumerable<WallSide> FindVisibleSides()
		{
			return sides.Where(side => !(side.normal.Dot(Player.heading) > 0.5f)); //TODO: base this threshold on the FOV
		}

		// ReSharper disable once InconsistentNaming
		[SuppressMessage("ReSharper", "UseDeconstruction")]
		public static (Vector2 intersection, float dist) DDA(Vector2 rayStart, Vector2 rayEnd, float maxDistance = 32.0f)
		{
			(int x, int y) cellEnd = (Mathf.Floor(rayEnd.x), Mathf.Floor(rayEnd.y));
			Vector2 rayDir = Vector2.Sub(new Vector2(cellEnd.x, cellEnd.y), Player.position).Normalize();

			Vector2 rayUnitStepSize = new Vector2(
				Mathf.Sqrt(1 + (rayDir.y / rayDir.x) * (rayDir.y / rayDir.x)),
				Mathf.Sqrt(1 + (rayDir.x / rayDir.y) * (rayDir.x / rayDir.y)));

			(int x, int y) mapCheck = (Mathf.Floor(rayStart.x), Mathf.Floor(rayStart.y));
			Vector2 rayLength1D = new Vector2();

			(int x, int y) step;

			// Establish Starting Conditions
			if (rayDir.x < 0)
			{
				step.x = -1;
				rayLength1D.x = (rayStart.x - mapCheck.x) * rayUnitStepSize.x;
			}
			else
			{
				step.x = 1;
				rayLength1D.x = (mapCheck.x + 1 - rayStart.x) * rayUnitStepSize.x;
			}

			if (rayDir.y < 0)
			{
				step.y = -1;
				rayLength1D.y = (rayStart.y - mapCheck.y) * rayUnitStepSize.y;
			}
			else
			{
				step.y = 1;
				rayLength1D.y = (mapCheck.y + 1 - rayStart.y) * rayUnitStepSize.y;
			}

			bool tileFound = false;
			float distance = 0.0f;
			while (!tileFound && distance < maxDistance)
			{
				// Walk along shortest path
				if (rayLength1D.x < rayLength1D.y)
				{
					mapCheck.x += step.x;
					distance = rayLength1D.x;
					rayLength1D.x += rayUnitStepSize.x;
				}
				else
				{
					mapCheck.y += step.y;
					distance = rayLength1D.y;
					rayLength1D.y += rayUnitStepSize.y;
				}

				// Test tile at new test point
				if (!(rayLength1D.x >= 0) || !(rayLength1D.x < MyGame.level.tilesColumns) ||
				    !(rayLength1D.y >= 0) || !(rayLength1D.y < MyGame.level.tilesRows)) continue;
				if (MyGame.level.GetTileAtPosition(mapCheck.x, mapCheck.y).GetType() != typeof(TileWall))
				{
					tileFound = true;
				}
			}

			// Calculate intersection location
			Vector2 intersection = null;
			if (tileFound)
			{
				intersection = Vector2.Add(rayStart, rayDir).Mult(distance);
				Minimap.DebugFill(0, 0, 255);
				Minimap.DebugCircle(intersection.x, intersection.y, 4);
			}

			Minimap.DebugLine(rayStart.x, rayStart.y, rayEnd.x, rayEnd.y);
			return (intersection, distance);
		}
	}
}