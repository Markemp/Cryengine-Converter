namespace CgfConverter.Renderers;

public interface IRenderer
{
    /// <summary>
    /// Renders scene data to output files.
    /// </summary>
    /// <returns>Number of files created.</returns>
    public int Render();
}
