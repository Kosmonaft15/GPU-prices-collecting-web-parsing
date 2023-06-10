using Microsoft.VisualBasic.FileIO;
using System;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace Сбор_цен_GPU
{
    public partial class Form1 : Form
    {
        //передвижение графика мышкой
        private bool isLeftButtonPressed = false;
        private Point mouseDown = Point.Empty;

        public Form1()
        {
            InitializeComponent();

            //загрузка данных для графика
            List<(string GPUname, DateOnly date, int pricemin, int pricemax)> GPU_price_data = new();
            using (TextFieldParser parser = new TextFieldParser(@"d:\\result2.txt"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    GPU_price_data.Add((fields[0], DateOnly.Parse(fields[1]), int.Parse(fields[2]), int.Parse(fields[3])));
                }
            }

            this.Text = "Загруженно: " + GPU_price_data.Count.ToString() + " записей";

            //рисование графика
            //создаем элемент Chart
            Chart priceChart = new Chart();
            //кладем его на форму и растягиваем на все окно.
            priceChart.Parent = this;
            priceChart.Dock = DockStyle.Fill;
            //добавляем в Chart область для рисования графиков, их может быть
            //много, поэтому даем ей имя.
            priceChart.ChartAreas.Add(new ChartArea("Цены GPU"));
            //Создаем и настраиваем набор точек для рисования графика, в том
            //не забыв указать имя области на которой хотим отобразить этот
            //набор точек.
            Legend legend = new Legend("Legend1");
            priceChart.Legends.Add(legend);
            Series SeriesOfPointMin = new Series("4090 min");
            Series SeriesOfPointMax = new Series("4090 max");
            SeriesOfPointMin.ChartType = SeriesChartType.Spline;
            SeriesOfPointMax.ChartType = SeriesChartType.Spline;
            SeriesOfPointMin.YValuesPerPoint = 1;
            SeriesOfPointMax.YValuesPerPoint = 1;
            SeriesOfPointMin.ChartArea = "Цены GPU";
            SeriesOfPointMax.ChartArea = "Цены GPU";
            SeriesOfPointMin.Legend = "Legend1";
            SeriesOfPointMax.Legend = "Legend1";
            SeriesOfPointMin.Name = "min";
            SeriesOfPointMax.Name = "max";
            SeriesOfPointMin.Color = Color.Green;
            SeriesOfPointMax.Color = Color.Red;

            //mySeriesOfPoint.IsValueShownAsLabel = true;
            //делает оси маштабируемыми
            priceChart.ChartAreas["Цены GPU"].AxisX.ScaleView.Zoomable = true;
            priceChart.ChartAreas["Цены GPU"].AxisY.ScaleView.Zoomable = true;
            priceChart.Titles.Add("Цена 4090"); //заголовок графиков          
            priceChart.MouseWheel += PriceChart_MouseWheel; //обработчик прокрутки мышки
            priceChart.MouseMove += PriceChart_MouseMove;  //прицел курсора
            priceChart.MouseUp += PriceChart_MouseUp;    //перемещение графика мышкой
            priceChart.MouseDown += PriceChart_MouseDown;  //перемещение графика мышкой                         
            
            //набор точек для графиков
            foreach (var item in GPU_price_data.Where(a => a.GPUname == "Nvidia RTX 4090"))
            {
                SeriesOfPointMin.Points.AddXY(item.date.ToDateTime(TimeOnly.MinValue), item.pricemin);
                SeriesOfPointMax.Points.AddXY(item.date.ToDateTime(TimeOnly.MinValue), item.pricemax);
            }

            //Добавляем созданные наборы точек в Chart
            priceChart.Series.Add(SeriesOfPointMin);
            priceChart.Series.Add(SeriesOfPointMax);
        }

        private void PriceChart_MouseDown(object? sender, MouseEventArgs e)
        //перемещение графика мышкой
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isLeftButtonPressed = true;
                mouseDown = e.Location;

            }
        }

        private void PriceChart_MouseUp(object? sender, MouseEventArgs e)
        //перемещение графика мышкой
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isLeftButtonPressed = false;
                mouseDown = Point.Empty;
            }
        }

        private void PriceChart_MouseMove(object? sender, MouseEventArgs e)
        //обработчик прицела курсора, перемещение графика мышкой
        {
            var chart = (Chart)sender;

            //обработчик прицела курсора
            {
                Point mousePoint = new Point(e.X, e.Y);

                chart.ChartAreas["Цены GPU"].CursorX.SetCursorPixelPosition(mousePoint, true);
                chart.ChartAreas["Цены GPU"].CursorY.SetCursorPixelPosition(mousePoint, true);
            }

            //перемещение графика мышкой
            {
                if (isLeftButtonPressed)
                {
                    var result = chart.HitTest(e.X, e.Y);

                    if (result.ChartElementType == (ChartElementType.PlottingArea))
                    {
                        var oldXValueX = result.ChartArea.AxisX.PixelPositionToValue(mouseDown.X);
                        var newXValueX = result.ChartArea.AxisX.PixelPositionToValue(e.X);
                        var oldXValueY = result.ChartArea.AxisY.PixelPositionToValue(mouseDown.Y);
                        var newXValueY = result.ChartArea.AxisY.PixelPositionToValue(e.Y);

                        chart.ChartAreas["Цены GPU"].AxisX.ScaleView.Position += oldXValueX - newXValueX;
                        chart.ChartAreas["Цены GPU"].AxisY.ScaleView.Position += oldXValueY - newXValueY;
                        mouseDown.X = e.X;
                        mouseDown.Y = e.Y;
                    }
                }
            }
        }

        private void PriceChart_MouseWheel(object? sender, MouseEventArgs e)
        //обработчик зума
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas["Цены GPU"].AxisX;
            var yAxis = chart.ChartAreas["Цены GPU"].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;
                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 2.5;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 2.5;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2.5;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2.5;
                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }
    }
}