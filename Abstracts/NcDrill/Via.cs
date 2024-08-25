using Clipper2Lib;

namespace GerberParser.Abstracts.NcDrill;

public abstract class Via
{
   public Path64 Path {  get;}

   public long Finished_hole_size {  get; }
}
