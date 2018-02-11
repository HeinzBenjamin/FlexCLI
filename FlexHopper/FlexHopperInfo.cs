using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FlexHopper
{
    public class FlexHopperInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FlexHopper";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return FlexHopper.Properties.Resources.engine;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Fast GPU-based physics simulation.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("95724da4-6b17-4ad8-9904-b01cfdf2274f");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Benjamin Felbrich";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "flexhopper@felbrich.com";
            }
        }
    }
}
