namespace CgfConverter.Renderers;

public interface IRenderer
{
    /// <summary>
    /// Renders a terrain into a file.
    /// </summary>
    /// <returns>Number of files created.</returns>
    public int Render();
}
