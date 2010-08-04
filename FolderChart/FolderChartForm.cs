
/*
FarNet module FolderChart
Copyright (c) 2010 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

static class FolderChartForm
{
	public static string Show(string title, IEnumerable<FolderItem> data, IWin32Window window)
	{
		string result = null;
		
		using (var series = new Series())
		{
			series.ChartType = SeriesChartType.Pie;
			series.SmartLabelStyle.Enabled = true;
			series["CollectedColor"] = "White";
			series["CollectedThreshold"] = "1";
			series["CollectedLabel"] = series["CollectedToolTip"] = "...";

			foreach (var it in data)
			{
				var point = series.Points.Add(it.Size);
				point.Label = it.Name;
				point.ToolTip = it.Name + " ~ " + Kit.FormatSize(it.Size);
			}

			series.Sort(PointSortOrder.Ascending);

			using (var chart = new Chart())
			{
				chart.Dock = DockStyle.Fill;

				using (var area = new ChartArea())
				{
					chart.ChartAreas.Add(area);
					chart.Series.Add(series);

					using (var form = new Form())
					{
						form.Size = new Size(600, 600);
						form.StartPosition = FormStartPosition.CenterParent;
						form.Text = title;
						form.Controls.Add(chart);

						// Pick the result folder or switch charts
						chart.MouseClick += (sender, e) =>
						{
							var hit = chart.HitTest(e.X, e.Y);
							if (hit.ChartElementType == ChartElementType.DataPoint)
							{
								if (hit.PointIndex >= 0)
								{
									form.Close();
									result = series.Points[hit.PointIndex].Label;
								}
							}
							else
							{
								if (series.ChartType == SeriesChartType.Pie)
									series.ChartType = SeriesChartType.Bar;
								else
									series.ChartType = SeriesChartType.Pie;
							}
						};

						// Highlight the active folder
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

						form.ShowDialog(window);
						return result;
					}
				}
			}
		}
	}
}
