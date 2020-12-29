using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;

namespace AzLog
{
	public class ColorFilterColors
	{
		private Dictionary<Color, string> m_colorsToNames = new Dictionary<Color, string>();
		private Dictionary<string, Color> m_namesToColors = new Dictionary<string, Color>();

		private Dictionary<string, (Color, Color)> m_colorPairs = new Dictionary<string, (Color, Color)>();

		/*----------------------------------------------------------------------------
			%%Function:AddColor
			%%Qualified:AzLog.ColorFilterColors.AddColor
		
			create forward and backward mappings for colors. All names get mapped
			to their color. The other direction, only static, non-system colors get
			mapped back (since the same RGB value could semantically be different
			system colors)
		----------------------------------------------------------------------------*/
		public void AddColor(Color color, string primaryName, string secondaryName)
		{
			if (!m_colorsToNames.ContainsKey(color))
			{
				if (color.IsNamedColor && !color.IsSystemColor)
				{
					m_colorsToNames.Add(color, primaryName);
				}
			}

			if (!m_namesToColors.ContainsKey(primaryName.ToLower()))
				m_namesToColors.Add(primaryName.ToLower(), color);
			
			if (color.IsNamedColor && !color.IsSystemColor)
			{
				string sArgb = $"{color.ToArgb():x8}";
				if (!m_namesToColors.ContainsKey(sArgb))
					m_namesToColors.Add(sArgb, color);
			}

			if (secondaryName != null && !m_namesToColors.ContainsKey(secondaryName.ToLower()))
				m_namesToColors.Add(secondaryName.ToLower(), color);
		}

		/*----------------------------------------------------------------------------
			%%Function:AddPair
			%%Qualified:AzLog.ColorFilterColors.AddPair

		----------------------------------------------------------------------------*/
		public void AddPair(string sName, Color fore, Color back)
		{
			m_colorPairs.Add(sName, (back, fore));
		}

		/*----------------------------------------------------------------------------
			%%Function:GetColorName
			%%Qualified:AzLog.ColorFilterColors.GetColorName

		----------------------------------------------------------------------------*/
		public string GetColorName(Color color)
		{
			if (m_colorsToNames.ContainsKey(color))
				return m_colorsToNames[color];

			return $"{color.ToArgb():X8}";
		}

		/*----------------------------------------------------------------------------
			%%Function:FGetColor
			%%Qualified:AzLog.ColorFilterColors.FGetColor

		----------------------------------------------------------------------------*/
		public bool FGetColor(string s, out Color color)
		{
			color = Color.Black;

			if (m_namesToColors.ContainsKey(s.ToLower()))
			{
				color = m_namesToColors[s.ToLower()];
				return true;
			}

			if (s.Length == 7 && s[0] == '#')
				s = s.Substring(1);
			
			if (s.Length == 6)
			{
				s = $"FF{s}";
			}
			
			if (s.Length < 8
			    || (s[0] == '#' && s.Length < 9))
			{
				return false;
			}

			if (s[0] == '#')
				s = s.Substring(1);

			try
			{
				color = Color.FromArgb(Int32.Parse(s, NumberStyles.HexNumber));
				// see if we have a proper name for this color
				if (m_namesToColors.ContainsKey(color.Name.ToLower()))
					color = m_namesToColors[color.Name.ToLower()];
				
				return true;
			}
			catch (Exception exc)
			{
				return false;
			}
		}

		/*----------------------------------------------------------------------------
			%%Function:BuildStaticColorsForTest
			%%Qualified:AzLog.ColorFilterColors.BuildStaticColorsForTest

			Build some static color tables for tests
		----------------------------------------------------------------------------*/
		public static ColorFilterColors BuildStaticColorsForTest()
		{
			Dictionary<string, (Color, Color)> builtins = new Dictionary<string, (Color, Color)>()
			{
				{"Blue", (Color.Blue, SystemColors.Control)},
				{"Red", (Color.Red, SystemColors.ControlText)},
				{"Light Green", (Color.LightGreen, SystemColors.Control)},
				{"Custom1", (Color.FromArgb(129, 129, 0), SystemColors.ControlText)},
				{"Custom2", (Color.White, Color.FromArgb(128, 129, 129, 0))}
			};
			
			ColorFilterColors colors = new ColorFilterColors();
			
			foreach (string key in builtins.Keys)
			{
				Color back, fore;

				(back, fore) = builtins[key];
				colors.AddColor(fore, fore.ToString().Substring(6), null);
				colors.AddColor(back, key, back.ToString().Substring(6));
				colors.AddPair(key, fore, back);
			}

			return colors;
		}

		[Test]
		public static void TestColorLookup_ByName()
		{
			ColorFilterColors colors = BuildStaticColorsForTest();

			Assert.IsTrue(colors.FGetColor("Blue", out Color color));
			Assert.AreEqual(Color.Blue, color);
			
			Assert.IsTrue(colors.FGetColor(Color.Blue.ToString().Substring(6), out color));
			Assert.AreEqual(Color.Blue, color);
		}

		[Test]
		public static void TestColorLookup_ByName_SystemColor()
		{
			ColorFilterColors colors = BuildStaticColorsForTest();

			Assert.IsTrue(colors.FGetColor("[Control]", out Color color));
			Assert.AreEqual(SystemColors.Control, color);
		}

		[Test]
		public static void TestColorLookup_ByCommonRgb()
		{
			ColorFilterColors colors = BuildStaticColorsForTest();

			Assert.IsTrue(colors.FGetColor("0000FF", out Color color));
			Assert.AreEqual(Color.Blue, color);

			Assert.IsTrue(colors.FGetColor("#0000FF", out color));
			Assert.AreEqual(Color.Blue, color);

			Assert.IsTrue(colors.FGetColor("FF0000FF", out color));
			Assert.AreEqual(Color.Blue, color);

			Assert.IsTrue(colors.FGetColor("#FF0000FF", out color));
			Assert.AreEqual(Color.Blue, color);
		}

		[Test]
		public static void TestColorLookup_ByRgb_CustomColor()
		{
			ColorFilterColors colors = BuildStaticColorsForTest();

			Assert.IsTrue(colors.FGetColor("818100", out Color color));
			Assert.AreEqual(Color.FromArgb(129, 129, 0), color);
		}

		[Test]
		public static void TestColorLookup_ByRgb_CustomColor_ZeroAlpha()
		{
			ColorFilterColors colors = BuildStaticColorsForTest();

			Assert.IsTrue(colors.FGetColor("#00818100", out Color color));
			Assert.AreEqual(Color.FromArgb(0, 129, 129, 0), color);
		}
	}
}