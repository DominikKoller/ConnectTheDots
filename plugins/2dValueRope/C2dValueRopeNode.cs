//@author: motzi
//@help: creates a tube-like strip of triangles with adjustable thickness

#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

//using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Rope", Version = "Binsize", Category = "2D", Help = "Creates rope-like triangle strip vertices with adjustable thickness from given path coordinates", Tags = "", Author = "motzi")]
	#endregion PluginInfo
	public class Rope2d : IPluginEvaluate
	{
		#region fields & pins
		[Input("Points", DefaultValue = 1.0)]
		public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Thickness", DefaultValue = 0.1)]
		public ISpread<ISpread<double>> FWidth;
		
		[Output("Vertices")]
		public ISpread<ISpread<Vector2D>> FOutput;
		
		[Output("Texture Coordinates")]
		public ISpread<ISpread<Vector2D>> FTexCoords;
		
		[Output("Factor", Visibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<double>> IFactor;

		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			FInput.SliceCount = FWidth.SliceCount = FInput.CombineWith(FWidth);
			// SET OUTPUT SLICECOUNTS first order!!
			
			for(int i=0; i<FInput.SliceCount; i++)
			{
				//FInput[i].SliceCount = FWidth[i].SliceCount = FInput[i].CombineWith(FWidth[i]);
			
			// no vertices
			if(FInput[i].SliceCount < 1 || FWidth[i].SliceCount < 1)
			{
				FOutput[i].SliceCount = 0;
				FTexCoords[i].SliceCount = 0;
				IFactor[i][0] = 0;
				continue;
			}
			// Only one vertex
			else if(FInput[i].SliceCount == 1)
			{
				FOutput[i].SliceCount = FInput[i].SliceCount;
				FOutput[i][0] = FInput[i][0];
				
				FTexCoords[i].SliceCount = 1;
				FTexCoords[i][0] = new Vector2D(0,0);
				IFactor[i][0] = 1;
				continue;
			}

			IFactor[i][0] = 1.0 /(FInput[i].SliceCount-1);
			
			FOutput[i].SliceCount = FInput[i].SliceCount * 2;
			FTexCoords[i].SliceCount = FInput[i].SliceCount * 2;
			
			Vector2D hn;
			int j, inIndex, outIndex, outTexCoord = 0;
			
			
			for (j = 1; j < FInput[i].SliceCount; j++)
			{
				inIndex = FInput[i].SliceCount - j;
				outIndex = (j-1) * 2;
				outTexCoord = j-1;
				hn = GetHalfNormal(FInput[inIndex], FInput[inIndex-1], FWidth[inIndex-1]);
				FOutput[outIndex] = FInput[inIndex] - hn;
				FTexCoords[outIndex] = new Vector2D(IFactor[0] * outTexCoord,1);
				
				FOutput[outIndex + 1] = FInput[inIndex] + hn;
				FTexCoords[outIndex + 1] = new Vector2D(IFactor[0] * outTexCoord,0);
			}
			
			// last point
			--j;
			inIndex = 1;
			outIndex = j*2;
			hn = GetHalfNormal(FInput[inIndex], FInput[inIndex-1], FWidth[inIndex-1]);
			FOutput[outIndex] = FInput[inIndex - 1] - hn;
			FTexCoords[outIndex] = new Vector2D(1,1);
			
			FOutput[outIndex+1] = FInput[inIndex - 1] + hn;
			FTexCoords[outIndex+1] = new Vector2D(1,0);
		}
			
		}
			
		
		
		private Vector2D GetHalfNormal(Vector2D vertexOne, Vector2D vertexTwo, double width)
		{
			Vector2D halfnormal = vertexTwo - vertexOne;
			halfnormal = ~halfnormal;
			halfnormal = new Vector2D(-halfnormal.y  * width * 0.5, halfnormal.x * width * 0.5);
			
			return halfnormal;
		}
	}
}
