
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

			foreach (var it in data)
			{
				var point = series.Points.Add(it.Size);
				point.Label = it.Name;
				point.ToolTip = it.Name + " ~ " + Kit.FormatSize(it.Size);
			}

			using (var chart = new Chart())
			{
				chart.Dock = DockStyle.Fill;

				using (var area = new ChartArea())
				{
					chart.ChartAreas.Add(area);
					chart.Series.Add(series);

					using (var form = new Form())
					{
						form.Text = title;
						form.Size = new Size(600, 600);
						form.StartPosition = FormStartPosition.CenterParent;

						form.Controls.Add(chart);

						// mouse click
						chart.MouseClick += (sender, e) =>
						{
							var hit = chart.HitTest(e.X, e.Y);

							// pick a result item or switch chart modes
							if (e.Button == MouseButtons.Left)
							{
								if (hit.ChartElementType == ChartElementType.DataPoint)
								{
									if (hit.PointIndex >= 0 && series.Points[hit.PointIndex].Label.Length > 0)
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
						};

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

						form.ShowDialog(window);
						return result;
					}
				}
			}
		}
	}
}
