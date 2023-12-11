using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Linq.Expressions;

namespace VNFramework
{
    public static partial class GraphicsTools
    {
        public static class TileRenderer
        {
            public struct Tileset
            {
                public Tileset(TAtlasInfo? tileAtlas = null, Dictionary<int, Point> tileLookup = null, Vector2? tileOffset = null, Vector2? tileOrigin = null, Vector2? drawScale = null, Color? background = null)
                {
                    TileAtlas = tileAtlas ?? null;
                    TileLookup = tileLookup ?? null;
                    TileOffset = tileOffset ?? (TileAtlas != null ? VNFUtils.ConvertPoint(((TAtlasInfo)TileAtlas).DivDimensions) : new Vector2(20, 20));
                    TileOrigin = tileOrigin ?? new Vector2(0, 0);
                    DrawScale = drawScale ?? new Vector2(1, 1);
                    Background = background ?? new Color(0, 0, 0, 0);
                }
                public TAtlasInfo? TileAtlas;
                public Dictionary<int, Point> TileLookup;
                public Vector2 TileOffset;
                public Vector2 TileOrigin;
                public Vector2 DrawScale;
                public Color Background;
            }
            public static Texture2D RenderTiles(Tileset tileSet, int[,] tiles)
            {
                return RenderTiles(tileSet, tiles, null);
            }
            public static Texture2D RenderTiles(Tileset tileSet, int[,] tiles, Texture2D inputTexture)
            {
                if (tileSet.TileAtlas == null)
                {
                    throw new VNFUtils.EventHorizonException("Exception in TileRenderer pipeline: Tileset did not provide a texture atlas.");
                }
                GraphicsDevice graphicsDevice = Shell.PubGD;
                Point gridDims = new Point(tiles.GetLength(0), tiles.GetLength(1));
                Vector2 textureSize = new Vector2(gridDims.X * tileSet.TileOffset.X, gridDims.Y * tileSet.TileOffset.Y);
                RenderTarget2D renderPane = new RenderTarget2D(graphicsDevice, (int)Math.Ceiling(textureSize.X), (int)Math.Ceiling(textureSize.Y), false,
                        graphicsDevice.PresentationParameters.BackBufferFormat,
                        DepthFormat.Depth24);
                if (inputTexture != null && inputTexture.Bounds.Size != new Point((int)Math.Ceiling(textureSize.X), (int)Math.Ceiling(textureSize.Y))) { return inputTexture; }
                graphicsDevice.SetRenderTarget(renderPane);
                graphicsDevice.Clear(tileSet.Background);
                SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
                Point location = new Point(0, 0);
                Point atlasCoordinates = new Point(0, 0);
                TAtlasInfo atlas = (TAtlasInfo)tileSet.TileAtlas;
                Texture2D currentTexture = atlas.Atlas;
                Point size = atlas.FrameSize() * VNFUtils.ConvertVector(tileSet.DrawScale);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                if(inputTexture != null) { spriteBatch.Draw(inputTexture, new Vector2(), Color.White); }
                for(int x = 0; x < gridDims.X; x++)
                {
                    for(int y = 0; y < gridDims.Y; y++)
                    {
                        currentTexture = atlas.Atlas;
                        atlasCoordinates = tileSet.TileLookup[tiles[x, y]];
                        location.X = (int)(x * tileSet.TileOffset.X);
                        location.Y = (int)(y * tileSet.TileOffset.Y);
                        spriteBatch.Draw(currentTexture, new Rectangle(location, size), new Rectangle(new Point((atlas.SourceRect.Width / atlas.DivDimensions.X) * atlasCoordinates.X, (atlas.SourceRect.Height / atlas.DivDimensions.Y) * atlasCoordinates.Y), atlas.FrameSize()), Color.White);
                    }
                }
                spriteBatch.End();
                graphicsDevice.SetRenderTarget(null);
                Texture2D tileTexture = VNFUtils.GetFromRT(renderPane);
                return tileTexture;
            }
        }
    }
}
