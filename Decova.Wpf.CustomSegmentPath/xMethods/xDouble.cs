using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decova.Wpf
{
	static class xDouble
	{
		public static bool IsWithinInclusive(this double num, double minOrMax, double maxOrMin)
		{
			if (minOrMax >= maxOrMin && num <= minOrMax && num >= maxOrMin)
				return true;

			if (minOrMax <= maxOrMin && num >= minOrMax && num <= maxOrMin)
				return true;

			return false;
		}
	}
}
