/* BMPFontEnums.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// The texture channel where the character image is found.
    /// </summary>
    public enum CharacterTextureChannel : byte
    {
        Blue = 1,
        Green = 2,
        Red = 4,
        Alpha = 8,
        All = 15
    }

    public enum FontChannelInformation : byte
    {
        GlyphData = 0,
        Outline = 1,
        GlyphAndOutline = 2,
        Zero = 3,
        One = 4
    }
}
