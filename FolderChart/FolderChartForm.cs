
// FarNet module FolderChart
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FolderChart;

// Used the parameter `IWin32Window window` and pass `new WindowWrapper(Far.Api.UI.MainWindowHandle)` for `ShowDialog`.
// With the new pseudo non-modal form this is not good, Far is kind of locked.

static class FolderChartForm
{
	public static void Show(string title, IEnumerable<FolderItem> data, Action<string> action)
	{
		using var series = new Series();
		series.ChartType = SeriesChartType.Pie;
		series.SmartLabelStyle.Enabled = true;

		foreach (var it in data)
		{
			var point = series.Points.Add(it.Size);
			point.Label = it.Name;
			point.ToolTip = Kit.FormatSize(it.Size, it.Name);
		}

		using var chart = new Chart();
		chart.Dock = DockStyle.Fill;

		using var area = new ChartArea();
		chart.ChartAreas.Add(area);
		chart.Series.Add(series);

		using var form = new Form();
		form.Text = title;
		form.Size = new Size(600, 600);
		form.TopMost = true;
		form.StartPosition = FormStartPosition.CenterParent;

		form.Controls.Add(chart);

		// handle clicks
		void click(MouseEventArgs e, bool isDoubleClick)
		{
			var hit = chart.HitTest(e.X, e.Y);

			// pick a result item or switch chart modes
			if (e.Button == MouseButtons.Left)
			{
				if (hit.ChartElementType == ChartElementType.DataPoint)
				{
					if (hit.PointIndex >= 0 && series.Points[hit.PointIndex].Label.Length > 0)
					{
						action(series.Points[hit.PointIndex].Label);
						if (isDoubleClick)
							form.Close();
					}
				}
				else
				{
					if (series.ChartType == SeriesChartType.Pie)
						series.ChartType = SeriesChartType.Bar;
					else
						series.ChartType = SeriesChartType.Pie;
				}
			}
			// remove an item from the chart
			else if (e.Button == MouseButtons.Right)
			{
				if (hit.ChartElementType == ChartElementType.DataPoint)
				{
					if (hit.PointIndex >= 0)
						series.Points.RemoveAt(hit.PointIndex);
				}
			}
		}

		// mouse clicks
		chart.MouseClick += (sender, e) => { click(e, false); };
		chart.MouseDoubleClick += (sender, e) => { click(e, true); };

		// mouse move: highlight an item
		chart.MouseMove += (sender, e) =>
		{
			var hit = chart.HitTest(e.X, e.Y);

			foreach (var point in series.Points)
			{
				point.BackSecondaryColor = Color.Black;
				point.BackHatchStyle = ChartHatchStyle.None;
			}

			if (hit.ChartElementType == ChartElementType.DataPoint && hit.PointIndex >= 0)
			{
				form.Cursor = Cursors.Hand;
				var point = series.Points[hit.PointIndex];
				point.BackSecondaryColor = Color.White;
				point.BackHatchStyle = ChartHatchStyle.DiagonalCross;
			}
			else
			{
				form.Cursor = Cursors.Default;
			}
		};

		form.ShowDialog();
	}
}
